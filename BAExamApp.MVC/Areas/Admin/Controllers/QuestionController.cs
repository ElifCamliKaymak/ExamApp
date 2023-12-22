using AutoMapper;
using BAExamApp.Dtos.QuestionRevisions;
using BAExamApp.Dtos.Questions;
using BAExamApp.Entities.Enums;
using BAExamApp.MVC.Areas.Admin.Models.QuestionRevisionVMs;
using BAExamApp.MVC.Areas.Admin.Models.QuestionVMs;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BAExamApp.MVC.Areas.Admin.Controllers;

public class QuestionController : AdminBaseController
{

    private readonly IQuestionService _questionService;
    private readonly ISubjectService _subjectService;
    private readonly IMapper _mapper;
    private readonly IAdminService _adminService;
    private readonly IQuestionRevisionService _questionRevisionService;
    private readonly ITrainerService _trainerService;

    public QuestionController(IQuestionService questionService, ISubjectService subjectService, IMapper mapper, IAdminService adminService, IQuestionRevisionService questionRevisionService, ITrainerService trainerService)
    {
        _questionService = questionService;
        _subjectService = subjectService;
        _mapper = mapper;
        _adminService = adminService;
        _questionRevisionService = questionRevisionService;
        _trainerService = trainerService;
    }


    [HttpGet]
    public async Task<IActionResult> QuestionList(State state)
    {
        ViewData["Question_State"] = (int)state;
        var questionListResult = await _questionService.GetAllByStateAsync(state);

        var questionList = _mapper.Map<IEnumerable<AdminQuestionListVM>>(questionListResult.Data);

        return View(questionList);
    }

    [HttpGet]
    public async Task<IActionResult> QuestionDetails(Guid id)
    {
        var questionDetailResult = await _questionService.GetDetailsByIdForAdminAsync(id);
        if (!questionDetailResult.IsSuccess) return NotFound();

        var questionMapped = _mapper.Map<AdminQuestionDetailsVM>(questionDetailResult.Data);

        return PartialView("_QuestionDetailsPartial", questionMapped);
    }

    [HttpGet]
    public async Task<IActionResult> Review(Guid id)
    {
        ViewBag.TrainerList = await GetTrainersAsync();
        var questionDetailResult = await _questionService.GetDetailsByIdForAdminAsync(id);

        return View(_mapper.Map<AdminQuestionReviewVM>(questionDetailResult.Data));
    }

    [HttpPost]
    public async Task<IActionResult> Approve(Guid id)
    {
        var updateStateResult = await _questionService.UpdateStateAsync(id, State.Approved);
        if (updateStateResult.IsSuccess) NotifySuccessLocalized(updateStateResult.Message);
        else NotifyErrorLocalized(updateStateResult.Message);

        return RedirectToAction(nameof(QuestionList), new { state = State.Approved });
    }

    [HttpPost]
    public async Task<IActionResult> Test(Guid id)
    {
        var updateStateResult = await _questionService.UpdateStateAsync(id, State.Test);
        if (updateStateResult.IsSuccess) NotifySuccessLocalized(updateStateResult.Message);
        else NotifyErrorLocalized(updateStateResult.Message);

        return RedirectToAction(nameof(QuestionList), new { state = State.Test });
    }

    [HttpPost]
    public async Task<IActionResult> Reject(Guid id, [FromForm]string? rejectComment )
    {
        var updateStateResult = await _questionService.UpdateStateWithCommentAsync(id, State.Rejected, rejectComment);
        if (updateStateResult.IsSuccess)
            NotifySuccessLocalized(updateStateResult.Message);
        else
            NotifyErrorLocalized(updateStateResult.Message);

        return RedirectToAction(nameof(QuestionList), new { state = State.Rejected });
    }

    [HttpPost]
    public async Task<IActionResult> Review(AdminQuestionReviewVM model)
    {
        if (ModelState.IsValid)
        {
            var currentAdmin = await _adminService.GetByIdentityIdAsync(UserIdentityId);
            if (currentAdmin.IsSuccess)
            {
                var questionRevision = new QuestionRevisionCreateDto() { QuestionId = model.Id, RequestDescription = model.RequestDescription, RequestedTrainerId = model.TrainerID, RequesterAdminId = currentAdmin.Data.Id };

                var result = await _questionRevisionService.AddAsync(questionRevision);
                if (!result.IsSuccess)
                {
                    NotifyErrorLocalized(result.Message);
                }
                else
                {
                    var questionresult = await _questionService.UpdateStateAsync(model.Id, State.Reviewed);
                    if (!questionresult.IsSuccess)
                    {
                        NotifyErrorLocalized(questionresult.Message);
                    }
                }
            }
            NotifyErrorLocalized(currentAdmin.Message);
            return RedirectToAction(nameof(QuestionList), new { state = State.Awaited });
        }
        ViewBag.TrainerList = await GetTrainersAsync();
        var questionDetailResult = await _questionService.GetDetailsByIdForAdminAsync(model.Id);
        var mappedQuestion = _mapper.Map<AdminQuestionReviewVM>(questionDetailResult.Data);
        mappedQuestion.TrainerID = model.TrainerID;
        mappedQuestion.RequestDescription = model.RequestDescription;

        return View(mappedQuestion);
    }

    [HttpGet]
    public async Task<IActionResult> QuestionRevisionList(Guid id)
    {
        var questionRevisions = await _questionRevisionService.GetAllByQuestionId(id);

        if (questionRevisions.Any())
        {
            var questionRevisionsVM = _mapper.Map<IEnumerable<QuestionRevisionListVM>>(questionRevisions);
            return View(questionRevisionsVM);
        }
        else
        {
            // Revize işlemi yoksa, bir bilgi mesajı döndür
            return Content("Revize işlemi yoktur");
        }
    }

    private async Task<SelectList> GetTrainersAsync()
    {
        var trainers = await _trainerService.GetAllAsync();
        return new SelectList(trainers.Data.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.FullName,
        }), "Value", "Text");
    }
}