using AutoMapper;
using BAExamApp.Core.Enums;
using BAExamApp.Core.Utilities.Results.Concrete;
using BAExamApp.Dtos.TrainerProducts;
using BAExamApp.Entities.DbSets;
using BAExamApp.MVC.Areas.Admin.Models.ClassroomVMs;
using BAExamApp.MVC.Areas.Admin.Models.ExamEvaluatorVMs;
using BAExamApp.MVC.Areas.Admin.Models.ExamVMs;
using BAExamApp.MVC.Areas.Trainer.Models.QuestionVMs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BAExamApp.MVC.Areas.Admin.Controllers;

public class ExamController : AdminBaseController
{
    private readonly IExamService _examService;
    private readonly IMapper _mapper;
    private readonly IClassroomProductService _classroomProductService;
    private readonly ITrainerProductService _trainerProductService;
    private readonly IExamEvaluatorService _examEvaluatorService;
    private readonly IStudentExamService _studentExamService;
    private readonly IClassroomService _classroomService;
    private readonly IExamRuleService _examRuleService;
    public ExamController(IExamService examService, IMapper mapper, IClassroomProductService classroomProductService, ITrainerProductService trainerProductService, IExamEvaluatorService examEvaluatorService, IStudentExamService studentExamService, IClassroomService classroomService,IExamRuleService examRuleService)
    {
        _examService = examService;
        _mapper = mapper;
        _classroomProductService = classroomProductService;
        _trainerProductService = trainerProductService;
        _examEvaluatorService = examEvaluatorService;
        _studentExamService = studentExamService;
        _classroomService = classroomService;
        _examRuleService = examRuleService;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        //var exams = await _examService.GetAllAsync();
        //var examList = _mapper.Map<List<AdminExamListVM>>(exams.Data);
        var classes = await _classroomService.GetAllAsync();
        var rules = await _examRuleService.GetAllAsync();
        ViewBag.className = classes.Data.Select(x=> new SelectListItem { Text=x.Name, Value=x.Id.ToString() }).ToList();
        ViewBag.ruleName = rules.Data.Select(x=> new SelectListItem { Text=x.Name,Value=x.Id.ToString() }).ToList();
        return View(new List<AdminExamListVM>());
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var exam = await _examService.GetDetailsByIdAsync(id);
        if (!exam.IsSuccess)
        {
            NotifyErrorLocalized(exam.Message);
            return RedirectToAction(nameof(Index));
        }

        var examDetailVM = _mapper.Map<AdminExamDetailVM>(exam.Data);

        var studentExam = await _studentExamService.GetAllByExamIdAsync(id);

        if (!studentExam.IsSuccess)
        {
            NotifyError(studentExam.Message);
            return RedirectToAction(nameof(Index));
        }

        var studentExamVm = _mapper.Map<IEnumerable<StudentExamDetailForAdminVM>>(studentExam.Data);

        var combinedExamDetailsVM = new AdminCombinedExamDetailsVM 
        { 
            ExamDetail = examDetailVM,
            StudentExamDetails = studentExamVm
        };

        return View(combinedExamDetailsVM);
    }

    [HttpGet]
    public async Task<IActionResult> AddEvaluators(Guid id)
    {
        var exam = await _examService.GetDetailsByIdAsync(id);

        if (!exam.IsSuccess)
        {
            NotifyErrorLocalized(exam.Message);
            return RedirectToAction(nameof(Index));
        }

        var examAddEvaluators = _mapper.Map<AdminExamEvaluatorCreateVM>(exam.Data);

        examAddEvaluators.TrainerIds = new List<Guid>();

        foreach (var examEvaluator in exam.Data.ExamEvaluators)
        {
            examAddEvaluators.TrainerIds.Add(examEvaluator.EvaluatorId);
        }

        ViewBag.TrainerList = await GetTrainersAsync(id);

        return View(examAddEvaluators);
    }

    [HttpPost]
    public async Task<IActionResult> AddEvaluators(AdminExamEvaluatorCreateVM viewModel)
    {
        if (ModelState.IsValid)
        {
            var selectedTrainers = viewModel.TrainerIds;

            var addTrainerResponse = await _examEvaluatorService.UpdateExamEvaluatorsAsync(selectedTrainers, viewModel.Id);
            if (addTrainerResponse.IsSuccess)
            {
                NotifySuccessLocalized(addTrainerResponse.Message);
                return RedirectToAction(nameof(Index));
            }
            NotifyErrorLocalized(addTrainerResponse.Message);
        }

        ViewBag.TrainerList = await GetTrainersAsync(viewModel.Id);

        return View(viewModel);
    }

    [HttpGet]
    private async Task<List<SelectListItem>> GetTrainersAsync(Guid id)
    {
        var exam = await _examService.GetByIdAsync(id);

        if (exam.Data.ClassroomId != null)
        {
            var classroomProductResult = await _classroomProductService.GetProductIdListByClassroomIdAsync((Guid)exam.Data.ClassroomId);
            var trainerList = new List<TrainerProductListDto>();

            if (classroomProductResult.IsSuccess)
            {
                foreach (var productId in classroomProductResult.Data)
                {
                    var trainers = await _trainerProductService.GetAllTrainersByProductIdAsync(productId);
                    if (trainers.IsSuccess)
                    {
                        trainerList.AddRange(trainers.Data);
                    }
                }
            }

            return trainerList.Select(x => new SelectListItem()
            {
                Value = x.TrainerId.ToString(),
                Text = x.FullName
            }).ToList();
        }

        return new List<SelectListItem>();
    }

    ///// <summary>
    ///// Sınav detaylarını ExamStudentId'ye göre bulur ve getirir.
    ///// </summary>
    ///// <param name="id"></param>
    ///// <returns>List<ExamStudentResultDto></returns>
    //[HttpGet]
    //public async Task<IActionResult> GetExamDetails(Guid id)
    //{
    //    var examsOfStudent = await _studentAnswerService.GetStudentAnswerById(id);

    //    //var studentAnswers = _mapper.Map<List<StudentAnswer>>(examsOfStudent);
    //    return View(examsOfStudent);
    //}

    //[HttpGet]
    //public IActionResult GetExamStudentWithParameters()
    //{
    //    Expression<Func<StudentExam, object>>[] expressions = { examStudent => examStudent.Exam, examStudent => examStudent.Student };
    //    return View(_examStudentService.GetExamStudentWithParameters(expressions));
    //}

    [HttpGet]
    public async Task<IActionResult> GetExamsByStatus(string selectedClassroomId, string selectedRulenameId, string datetimePickerStart, string datetimePickerEnd,bool isActiveExams)
    {
        var exams = await _examService.GetExamsByFiltered(selectedClassroomId,selectedRulenameId,datetimePickerStart,datetimePickerEnd,isActiveExams);
        var examList = _mapper.Map<List<AdminExamListVM>>(exams.Data);
        return PartialView(Url.Content("~/Areas/Admin/Views/Shared/PartialViews/_ExamListPartial.cshtml"), examList);
    }
}
