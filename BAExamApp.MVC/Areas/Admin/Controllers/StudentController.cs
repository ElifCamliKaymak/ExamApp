using AutoMapper;
using BAExamApp.Dtos.Emails;
using BAExamApp.Dtos.Students;
using BAExamApp.MVC.Areas.Admin.Models.ExamVMs;
using BAExamApp.MVC.Areas.Admin.Models.StudentVMs;
using BAExamApp.MVC.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BAExamApp.MVC.Areas.Admin.Controllers;

public class StudentController : AdminBaseController
{
    private readonly IStudentService _studentService;
    private readonly ICityService _cityService;
    private readonly ISendMailService _sendMailService;
    private readonly IEmailService _emailService;
    private readonly IStudentClassroomService _studentClassroomService;
    private readonly IMapper _mapper;
    private readonly IStudentExamService _studentExamService;
    private readonly ITrainerClassroomService _trainerClassroomService;
    private readonly IClassroomService _classroomService;

    public StudentController(IStudentService studentService, ICityService cityService, IMapper mapper, ISendMailService sendMailService, IEmailService emailService, IStudentClassroomService studentClassroomService, IStudentExamService studentExamService, ITrainerClassroomService trainerClassroomService, IClassroomService classroomService)
    {
        _studentService = studentService;
        _cityService = cityService;
        _mapper = mapper;
        _sendMailService = sendMailService;
        _emailService = emailService;
        _studentClassroomService = studentClassroomService;
        _studentExamService = studentExamService;
        _trainerClassroomService = trainerClassroomService;
        _classroomService = classroomService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool showAllActiveStudents = false)
    {
        ViewBag.Cities = await GetCitiesAsync();

        List<AdminStudentListVM> students = new List<AdminStudentListVM>();
        
        if(showAllActiveStudents)
        {
            var activeStudents = await _studentService.GetActiveStudentsAsync();
            students = _mapper.Map<List<AdminStudentListVM>>(activeStudents.Data);
        }
        
        ViewBag.ShowAllActiveStudents = showAllActiveStudents;

        return View(students);
    }

    [HttpPost]
    public async Task<IActionResult> GetStudents(AdminStudentListVM adminStudentListVM) 
    {
        var getStudentResponse = await _studentService.GetStudentsByNameOrSurnameOrMailAdressAsync(adminStudentListVM.FirstName, adminStudentListVM.LastName, adminStudentListVM.Email);
        var studentList = _mapper.Map<List<AdminStudentListVM>>(getStudentResponse.Data);
        return View("Index", studentList);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(new AdminStudentCreateVM()
        {
            Cities = await GetCitiesAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(AdminStudentCreateVM studentCreateVM, IFormCollection collection)
    {
        if (!ModelState.IsValid)
        {
            studentCreateVM.Cities = await GetCitiesAsync();
            return View(studentCreateVM);
        }

        var studentCreateDto = _mapper.Map<StudentCreateDto>(studentCreateVM);
        if (studentCreateVM.NewImage != null)
        {
            studentCreateDto.Image = await studentCreateVM.NewImage.FileToStringAsync();
        }

        // Servise göndermeden önce düzeltme işleminin yapıldığı yer

        studentCreateDto.FirstName = StringExtensions.TitleFormat(studentCreateVM.FirstName);
        studentCreateDto.LastName = StringExtensions.TitleFormat(studentCreateVM.LastName);

        var addSutdentresult = await _studentService.AddAsync(studentCreateDto);
        if (addSutdentresult.IsSuccess)
        {
            string link = Url.Action("index", "login", new { Area = "" }, Request.Scheme);
            await _sendMailService.SendEmailNewStudent(addSutdentresult.Data.Email, link);
            NotifySuccess($"{studentCreateVM.FirstName} {studentCreateVM.LastName} kişisi başarıyla eklendi, Mail adresine mail gönderildi.");
        }

        if (!addSutdentresult.IsSuccess)
        {
            NotifyErrorLocalized("Öğrenci eklenirken bir hatayla karşılaşıldı. " + addSutdentresult.Message.ToString());
        }

        var studentOtherEmailList = new List<EmailCreateDto>();
        var otherEmailsList = collection["otherEmails"].ToList();
        var identityId = addSutdentresult.Data.IdentityId;
        foreach (var studentOtherEmail in otherEmailsList)
        {
            studentOtherEmailList.Add(new EmailCreateDto() { EmailAddress = studentOtherEmail, IdentityId = identityId });
        }
        var addEmailResult = await _emailService.AddRangeAsync(studentOtherEmailList);

        if (!addEmailResult.IsSuccess)
        {
            NotifyErrorLocalized(addEmailResult.Message);
            return RedirectToAction(nameof(Index));
        }


        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Update(Guid id)
    {
        var getStudentResult = await _studentService.GetByIdAsync(id);

        if (!getStudentResult.IsSuccess)
        {
            NotifyErrorLocalized(getStudentResult.Message);
            return RedirectToAction(nameof(Index));
        }
        var studentDto = getStudentResult.Data;
        var studentUpdateVM = _mapper.Map<AdminStudentUpdateVM>(studentDto);
        studentUpdateVM.OtherEmails = (await _emailService.GetEmailAddressesByIdentityIdAsync(getStudentResult.Data.IdentityId)).Data;
        studentUpdateVM.Cities = await GetCitiesAsync(studentUpdateVM.CityId);
        return View(studentUpdateVM);
    }

    [HttpPost]
    public async Task<IActionResult> Update(AdminStudentUpdateVM model, IFormCollection collection)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updateStudent = _mapper.Map<StudentUpdateDto>(model);

        if (model.NewImage != null)
        {
            updateStudent.Image = await model.NewImage.FileToStringAsync();
        }

        updateStudent.FirstName = StringExtensions.TitleFormat(model.FirstName);
        updateStudent.LastName = StringExtensions.TitleFormat(model.LastName);
        var updateStudentResult = await _studentService.UpdateAsync(updateStudent);

        if (!updateStudentResult.IsSuccess)
        {
            NotifyErrorLocalized(updateStudentResult.Message);
            return RedirectToAction(nameof(Update));
        }

        var otherEmailsList = collection["otherEmails"].ToList();
        var studentOtherEmailList = new List<EmailCreateDto>();
        var identityId = updateStudentResult.Data.IdentityId;

        foreach (var studentOtherEmail in otherEmailsList)
        {
            studentOtherEmailList.Add(new EmailCreateDto() { EmailAddress = studentOtherEmail, IdentityId = identityId });
        }
        var addEmailResult = await _emailService.UpdateRangeAsync(studentOtherEmailList, identityId);

        NotifySuccessLocalized(updateStudentResult.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var getStudent = await _studentService.GetStudentDetailsByIdAsync(id);
        if (getStudent.IsSuccess)
        {
            var studentDetailsVM = _mapper.Map<AdminStudentDetailVM>(getStudent.Data);
            studentDetailsVM.OtherEmails = (await _emailService.GetEmailAddressesByIdentityIdAsync(getStudent.Data.IdentityId)).Data;
            studentDetailsVM.Classrooms = (await _studentClassroomService.GetStudetClassroomIdentityIdAsync(getStudent.Data.Id)).Data;
            return View(studentDetailsVM);
        }
        NotifyErrorLocalized(getStudent.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete([FromQuery(Name = "id")] Guid id)
    {
        var deleteResult = await _studentService.DeleteAsync(id);
        if (deleteResult.IsSuccess)
            NotifySuccessLocalized(deleteResult.Message);
        else
            NotifyErrorLocalized(deleteResult.Message);

        return Json(deleteResult);

    }

    /// <summary>
    /// Şehirleri liste olarak getirir
    /// </summary>
    /// <param name="cityId"></param>
    /// <returns>Parametre ile kullanılırsa parametre verisine göre seçili dönüş yapar, para metre yok ise seçilmeden dönüş yapar </returns>
    private async Task<SelectList> GetCitiesAsync(Guid? cityId = null)
    {
        var cityList = (await _cityService.GetAllAsync()).Data;
        return new SelectList(cityList.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Name,
            Selected = x.Id == (cityId != null ? cityId.Value : cityId)
        }).OrderBy(x => x.Text), "Value", "Text");
    }


    [HttpGet]
    public async Task<IActionResult> StudentExams(Guid id)
    {
        var getStudentExams = await _studentExamService.GetAllExamsWithDetailsByIdAsync(id);
        if (getStudentExams.IsSuccess)
        {
            

            var studentExamsVM = _mapper.Map<IEnumerable<StudentExamsForAdminVM>>(getStudentExams.Data);

            return View(studentExamsVM);
        }
        NotifyErrorLocalized(getStudentExams.Message);
        return RedirectToAction(nameof(Index));
    }


}