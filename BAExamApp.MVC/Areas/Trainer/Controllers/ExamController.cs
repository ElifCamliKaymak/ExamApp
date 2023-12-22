using AutoMapper;
using BAExamApp.Business.Constants;
using BAExamApp.Dtos.ExamClassrooms;
using BAExamApp.Dtos.Exams;
using BAExamApp.Dtos.StudentExams;
using BAExamApp.Entities.DbSets;
using BAExamApp.Entities.Enums;
using BAExamApp.MVC.Areas.Trainer.Models.ExamVMs;
using BAExamApp.MVC.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Claims;

namespace BAExamApp.MVC.Areas.Trainer.Controllers;

public class ExamController : TrainerBaseController
{

    private readonly ITrainerService _trainerService;
    private readonly IExamService _examService;
    private readonly IExamRuleService _examRuleService;
    private readonly IExamRuleSubtopicService _examRuleSubjectService;
    private readonly IClassroomService _classroomService;
    private readonly IQuestionService _questionService;
    private readonly IMapper _mapper;
    private readonly IStudentExamService _examStudentService;
    private readonly IStudentService _studentService;
    private readonly IStudentClassroomService _studentClassroomService;
    private readonly IStudentQuestionService _studentQuestionService;
    private readonly IStudentExamService _studentExamService;
    private readonly ISendMailService _sendMailService;
    private readonly IHttpContextAccessor? _contextAccessor;
    private readonly IUserService _userService;

    public ExamController(ITrainerService trainerService, IMapper mapper, IExamService examService, IExamRuleSubtopicService examRuleSubjectService, IExamRuleService examRuleService, IClassroomService classroomService, IQuestionService questionService, IStudentExamService examStudentService, IStudentService studentService, IStudentClassroomService studentClassroomService, IStudentQuestionService studentQuestionService, ISendMailService sendMailService, IStudentExamService studentExamService, IHttpContextAccessor? contextAccessor, IUserService userService)
    {
        _trainerService = trainerService;
        _examService = examService;
        _examRuleService = examRuleService;
        _examRuleSubjectService = examRuleSubjectService;
        _mapper = mapper;
        _classroomService = classroomService;
        _questionService = questionService;
        _examStudentService = examStudentService;
        _studentService = studentService;
        _studentQuestionService = studentQuestionService;
        _sendMailService = sendMailService;
        _studentExamService = studentExamService;
        _contextAccessor = contextAccessor;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _examService.GetByIdentityIdAsync(UserIdentityId);

        return View(_mapper.Map<List<TrainerExamListVM>>(result.Data));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var examResult = await _examService.GetDetailsByIdAsync(id);

        if (examResult.IsSuccess)
        {
            return View(_mapper.Map<TrainerExamDetailVM>(examResult.Data));
        }

        NotifyErrorLocalized(examResult.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var examDeleteResponse = await _examService.DeleteAsync(id);

        if (examDeleteResponse.IsSuccess)
            NotifySuccessLocalized(examDeleteResponse.Message);
        else
            NotifyErrorLocalized(examDeleteResponse.Message);

        return Json(examDeleteResponse);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var examRules = await GetExamRules();

        if (examRules.Count > 1)
        {
            ViewBag.ExamRuleList = examRules;
            ViewBag.ClassroomList = await GetClassroomByTrainerId(UserIdentityId);
            ViewBag.ExamCreationTypes = GetExamCreationTypes();
            ViewBag.ExamTypes = GetExamTypes();
            var trainerExamCreateVM = new TrainerExamCreateVM();

            return View(trainerExamCreateVM);
        }

        NotifyWarningLocalized(Messages.PleaseAddExamRuleBefore);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TrainerExamCreateVM examCreateVM)
    {
        if (ModelState.IsValid)
        {
            var uniqueStudentExamIds = new HashSet<Guid>();
            var studentExams = new List<StudentExamCreateDto>();
            var examClassrooms = new List<ExamClassroomsCreateDto>();
            var examCreateDto = _mapper.Map<ExamCreateDto>(examCreateVM);

            if (examCreateVM.forClassroom && examCreateVM.ExamClassroomsIds != null)
            {
                foreach (var classroomId in examCreateVM.ExamClassroomsIds)
                {
                    var students = await _studentService.GetAllByClassroomIdAsync(classroomId);
                    examClassrooms.Add(new() { ClassroomId = classroomId });
                    if (students.Data != null && students.Data.Count > 0)
                    {
                        foreach (var student in students.Data)
                        {
                            var exam = await _studentExamService.GetAllExamsByStudentIdAsync(student.Id);

                            if (uniqueStudentExamIds.Add(student.Id))
                            {
                                studentExams.Add(new() { StudentId = student.Id });
                            }
                        }
                    }
                    else
                        NotifyErrorLocalized(students.Message);
                }
            }
            else
            {
                if (examCreateVM.StudentIds != null && examCreateVM.StudentIds.Count > 0)
                {
                    foreach (var studentId in examCreateVM.StudentIds)
                    {
                        if (uniqueStudentExamIds.Add(studentId))
                        {
                            studentExams.Add(new() { StudentId = studentId });
                        }
                    }
                    examClassrooms = examCreateVM.ExamClassroomsIds.Select(id => new ExamClassroomsCreateDto { ClassroomId = id }).ToList();
                }
                else
                {
                    NotifyErrorLocalized(Messages.NoAvailableStudent);
                    return RedirectToAction(nameof(Create));
                }
            }
            examCreateDto.StudentExams = studentExams;
            examCreateDto.ExamClassroomsIds = examClassrooms;
            var examRule = (await _examRuleService.GetByIdAsync(examCreateVM.ExamRuleId)).Data;
            var questionListResult = await _studentQuestionService.CreateQuestionPoolForExamRuleSubtopicsAsync(examRule.ExamRuleSubtopics);

            var trainerId = _contextAccessor?.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var trainerEmail = await _userService.GetEmailByUserId(trainerId);

            List<string> emailsAndLinks = new List<string>();

            if (questionListResult.IsSuccess)
            {
                var examResult = await _examService.AddAsync(examCreateDto);

                if (examResult.IsSuccess)
                {
                    var studentQuestionResult = await _studentQuestionService.AddRangeForExamIdAsync(examResult.Data.Id);

                    if (studentQuestionResult.IsSuccess)
                    {
                        var studentExamResult = await _studentExamService.GetAllByExamIdAsync(examResult.Data.Id);

                        string link = Url.Action("StartExam", "exam", new { Area = "Student" }, Request.Scheme);

                        if (studentExamResult.IsSuccess)
                        {
                            foreach (var studentExam in studentExamResult.Data)
                            {
                                var studentResult = await _studentService.GetByIdAsync(studentExam.StudentId);
                                if (studentResult.IsSuccess)
                                {
                                    await _sendMailService.SendEmailToStudentNewExam(studentResult.Data.Email, link, studentExam.Id);
                                    var email = await _sendMailService.GetStudentEmailById(studentExam.StudentId);
                                    emailsAndLinks.Add($"{email}" + "  " + $"{link}/{studentExam.Id}");
                                }

                            }
                            await _sendMailService.SendEmailToTrainerNewExam(trainerEmail, emailsAndLinks);


                        }
                        NotifySuccessLocalized(examResult.Message);
                        return RedirectToAction(nameof(Index));
                    }

                    NotifyErrorLocalized(studentQuestionResult.Message);
                }
                else
                {
                    NotifyErrorLocalized(examResult.Message);
                }
            }
            else
            {
                NotifyErrorLocalized(questionListResult.Message);
            }

            NotifySuccess(Messages.ExamCreatedSuccessfully);
        }
        ViewBag.ExamRuleList = await GetExamRules();
        ViewBag.ClassroomList = await GetClassroomByTrainerId(UserIdentityId);
        ViewBag.ExamTypes = GetExamTypes();
        ViewBag.ExamCreationTypes = GetExamCreationTypes();
        return View(examCreateVM);
    }

    private async Task<List<SelectListItem>> GetExamRules()
    {
        var getExamRulesResult = await _examRuleService.GetAllAsync();

        var examRulesList = getExamRulesResult.Data.Select(x => new SelectListItem()
        {
            Value = x.Id.ToString(),
            Text = x.Name
        }).ToList();
        return examRulesList;
    }

    private async Task<SelectList> GetClassroomByTrainerId(string id)
    {
        var getClassrooms = await _classroomService.GetAllByIdentityIdAsync(id);

        return new SelectList(getClassrooms.Data.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Name
        }), "Value", "Text");
    }

    [HttpGet]
    public async Task<SelectList> GetStudentsByClassroom(Guid classroomId)
    {
        var filteredStudents = await _studentService.GetAllByClassroomIdAsync(classroomId);
        return new SelectList(filteredStudents.Data.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.FirstName + " " + x.LastName
        }), "Value", "Text");
    }

    private List<SelectListItem> GetExamCreationTypes()
    {
        return Enum.GetValues(typeof(ExamCreationType)).Cast<ExamCreationType>().
             Select(x => new SelectListItem
             {
                 Text = x.GetType().GetMember(x.ToString()).First().GetCustomAttribute<DisplayAttribute>().GetName(),
                 Value = ((int)x).ToString()
             }).ToList();
    }

    [HttpGet]
    public async Task<SelectList> GetExamRulesByExamType(string examTypeId)
    {
        return new SelectList((await _examRuleService.GetExamRulesByExamTypeAsync(examTypeId)).Data, "Id", "Name");
    }

    [HttpGet]
    private List<SelectListItem> GetExamTypes()
    {
        return Enum.GetValues(typeof(ExamType)).Cast<ExamType>().
            Select(x => new SelectListItem
            {
                Text = x.GetType().GetMember(x.ToString()).First().GetCustomAttribute<DisplayAttribute>()!.GetName(),
                Value = ((int)x).ToString()
            }).ToList();
    }
}