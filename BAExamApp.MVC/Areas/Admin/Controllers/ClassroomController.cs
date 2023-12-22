using AutoMapper;
using BAExamApp.Dtos.ClassroomProducts;
using BAExamApp.Dtos.Classrooms;
using BAExamApp.Dtos.StudentClassrooms;
using BAExamApp.Dtos.TrainerClassrooms;
using BAExamApp.Entities.DbSets;
using BAExamApp.MVC.Areas.Admin.Models.ClassroomVMs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BAExamApp.MVC.Areas.Admin.Controllers;

public class ClassroomController : AdminBaseController
{
    private readonly IBranchService _branchService;
    private readonly IClassroomService _classroomService;
    private readonly IClassroomProductService _classroomProductService;
    private readonly IGroupTypeService _groupTypeService;
    private readonly IProductService _productService;
    private readonly IStudentService _studentService;
    private readonly IStudentClassroomService _studentClassroomService;
    private readonly ITrainerClassroomService _trainerClassroomService;
    private readonly ITrainerService _trainerService;
    private readonly IExamService _examService;
    private readonly IMapper _mapper;
    public ClassroomController(IClassroomService classroomService, IMapper mapper, IGroupTypeService groupTypeService, IProductService productService, IStudentService studentService, ITrainerService trainerService, IBranchService branchService, IStudentClassroomService studentClassroomService, ITrainerClassroomService trainerClassroomService, IClassroomProductService classroomProductService, IExamService examService)
    {
        _branchService = branchService;
        _classroomService = classroomService;
        _classroomProductService = classroomProductService;
        _groupTypeService = groupTypeService;
        _productService = productService;
        _studentClassroomService = studentClassroomService;
        _studentService = studentService;
        _trainerClassroomService = trainerClassroomService;
        _trainerService = trainerService;
        _examService = examService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool showAllData = false)
    {
        List<AdminClassroomListVM> classroomList;
        var resultAll = await _classroomService.GetAllAsync();
        var resultActive = await _classroomService.GetActiveAsync();
        ViewBag.ProductList = await GetProducts();
        ViewBag.GroupTypeList = await GetGroupTypesAsync();
        ViewBag.BranchList = await GetBranchs();
        ViewBag.ClassList = await GetClasses();

        if (showAllData)
        {
            classroomList = _mapper.Map<List<AdminClassroomListVM>>(resultAll.Data);
        }
        else
        {
            classroomList = new List<AdminClassroomListVM>();
        }

        ViewBag.ShowAllData = showAllData;
        return View(classroomList);
    }

    [HttpPost]
    public async Task<IActionResult> GetFilteredList(string name, string branchName, string groupType, DateTime openingDate, DateTime closedDate)
    {
        ViewBag.GroupTypeList = await GetGroupTypesAsync();
        ViewBag.BranchList = await GetBranchs();
        ViewBag.ClassList = await GetClasses();
        var getClassroomResponse = await _classroomService.GetFilteredByNameOrBranchNameOrGroupTypeOrOpeningDateOrClosedDateAsync(name, branchName, groupType, openingDate, closedDate);
        var classroomList = _mapper.Map<List<AdminClassroomListVM>>(getClassroomResponse.Data);

        return View("Index", classroomList);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var getClassroomResponse = await _classroomService.GetDetailsByIdForAdminAsync(id);

        if (getClassroomResponse.IsSuccess)
        {
            var getClassroomExams = await _examService.GetExamsByClassIdAsync(id);
            ViewBag.ClassroomExams = getClassroomExams.Data;

            return View(_mapper.Map<AdminClassroomDetailsVM>(getClassroomResponse.Data));
        }
        NotifyErrorLocalized(getClassroomResponse.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Create(AdminClassroomCreateVM classroomCreateVM)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(x => x.Errors);
            string errorMessages = null!;
            foreach (var error in errors)
            {
                errorMessages += " ," + error.ErrorMessage;
            }
            NotifyError(errorMessages);
            return RedirectToAction(nameof(Index));
        }
        var classroomCreateDto = _mapper.Map<ClassroomCreateDto>(classroomCreateVM);

        var classroomProducts = new List<ClassroomProductCreateDto>();

        foreach (var productId in classroomCreateVM.ProductIds)
        {
            classroomProducts.Add(new() { ProductId = productId });
        }

        classroomCreateDto.ClassroomProducts = classroomProducts;

        var createResult = await _classroomService.AddAsync(classroomCreateDto);

        if (!createResult.IsSuccess)
        {
            NotifyErrorLocalized(createResult.Message);
            return RedirectToAction(nameof(Index));
        }

        NotifySuccessLocalized(createResult.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Update(Guid id)
    {
        var getResult = await _classroomService.GetDetailsByIdForAdminAsync(id);
        if (!getResult.IsSuccess)
            return RedirectToAction(nameof(Index));

        var classroomUpdateVm = _mapper.Map<AdminClassroomUpdateVM>(getResult.Data);

        classroomUpdateVm.ProductList = await GetProducts();
        classroomUpdateVm.GroupTypeList = await GetGroupTypesAsync();
        classroomUpdateVm.BranchList = await GetBranchs();
        return View(classroomUpdateVm);
    }

    [HttpPost]
    public async Task<IActionResult> Update(AdminClassroomUpdateVM model)
    {
        if (!ModelState.IsValid)
        {
            model.ProductList = await GetProducts();
            model.GroupTypeList = await GetGroupTypesAsync();
            model.BranchList = await GetBranchs();
            return View(model);
        }

        var classroomDto = _mapper.Map<ClassroomUpdateDto>(model);
        var updateResult = await _classroomService.UpdateAsync(classroomDto);
        if (updateResult.IsSuccess)
        {
            NotifySuccessLocalized(updateResult.Message);
        }
        else
        {
            NotifyErrorLocalized(updateResult.Message);
        }

        return RedirectToAction(nameof(Index));

    }

    [HttpGet]
    public async Task<IActionResult> AddTrainer(Guid id)
    {
        AdminClassroomAddTrainerVM viewModel = new()
        {
            ClassroomId = id,
            Trainers = await GetTrainersAsync(id)
        };
        try
        {
            viewModel.AppointedTrainersId = (await _trainerClassroomService.GetTrainersWithSpesificClassroomIdAsync(id))
                .Data
                .Select(x => x.Id.ToString())
                .ToList();
        }
        catch (Exception)
        {
            viewModel.AppointedTrainersId = new List<string>();
        }
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddTrainer(AdminClassroomAddTrainerVM viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.Trainers = await GetTrainersAsync(viewModel.ClassroomId);
            return View(viewModel);
        }

        var addTrainerResponse = await _trainerClassroomService.AddTrainersToClassroomAsync(_mapper.Map<TraninerAddClassroomDto>(viewModel));
        if (addTrainerResponse.IsSuccess)
        {
            NotifySuccessLocalized(addTrainerResponse.Message);
        }
        else
        {
            NotifyErrorLocalized(addTrainerResponse.Message);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> AddStudent(Guid id)
    {
        AdminClassroomAddStudentVM viewModel = new()
        {
            ClassroomId = id,
            Students = await GetStudentsAsync(id)
        };
        try
        {
            viewModel.AppointedStudentsId = (await _studentService.GetStudentsWithSpesificClassroomIdAsync(id)).Data
            .Select(x => x.Id.ToString())
            .ToList();
        }
        catch (Exception)
        {
            viewModel.AppointedStudentsId = new List<string>();
        }
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddStudent(AdminClassroomAddStudentVM viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.Students = await GetStudentsAsync(viewModel.ClassroomId);
            return View(viewModel);
        }

        var addStudentResult = await _studentClassroomService.AddStudentToClassroomAsync(_mapper.Map<StudentAddToClassroomDto>(viewModel));

        if (addStudentResult.IsSuccess)
        {
            NotifySuccessLocalized(addStudentResult.Message);
        }
        else
        {
            NotifyErrorLocalized(addStudentResult.Message);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete([FromQuery(Name = "id")] Guid id)
    {
        var deleteResult = await _classroomService.DeleteAsync(id);
        if (!deleteResult.IsSuccess)
        {
            NotifyErrorLocalized(deleteResult.Message);
        }
        else
        {
            NotifySuccessLocalized(deleteResult.Message);
        }

        return Json(deleteResult);
    }

    private async Task<SelectList> GetGroupTypesAsync()
    {
        var groupTypeList = await _groupTypeService.GetAllAsync();
        return new SelectList(groupTypeList.Data.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Name
        }), "Value", "Text");

    }
    private async Task<SelectList> GetProducts(Guid? productId = null)
    {
        var productList = (await _productService.GetAllAsync()).Data;
        return new SelectList(productList.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Name,
            Selected = x.Id == (productId != null ? productId.Value : productId)
        }), "Value", "Text");

    }
    private async Task<SelectList> GetBranchs()
    {
        var branchList = await _branchService.GetAllAsync();
        return new SelectList(branchList.Data.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Name
        }), "Value", "Text");
    }
    private async Task<SelectList> GetClasses()
    {
        var classList = await _classroomService.GetAllAsync();
        return new SelectList(classList.Data.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Name
        }), "Value", "Text");
    }
    private async Task<List<SelectListItem>> GetTrainersAsync(Guid classroomId)
    {
        var getFreeTrainersResponse = await _trainerService.GetAllAsync();
        if (getFreeTrainersResponse.IsSuccess)
        {
            var trainerList = getFreeTrainersResponse.Data.Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.FirstName + " " + x.LastName,
            }).ToList();

            return trainerList;
        }
        return new List<SelectListItem>();
    }
    private async Task<List<SelectListItem>> GetStudentsAsync(Guid classroomId)
    {
        var getFreeStudentsResponse = await _studentService.GetStudentsWithoutSpesificClassroomIdAsync(classroomId);
        if (getFreeStudentsResponse.IsSuccess)
        {
            var studentList = getFreeStudentsResponse.Data.Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.FirstName + " " + x.LastName,
            }).ToList();

            return studentList;
        }
        return new List<SelectListItem>();
    }
}