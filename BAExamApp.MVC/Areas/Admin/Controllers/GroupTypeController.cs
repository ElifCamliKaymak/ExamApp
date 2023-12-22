using AutoMapper;
using BAExamApp.Dtos.GroupTypes;
using BAExamApp.MVC.Areas.Admin.Models.GroupTypeVMs;

namespace BAExamApp.MVC.Areas.Admin.Controllers;

public class GroupTypeController : AdminBaseController
{
    private readonly IGroupTypeService _groupTypeService;
    private readonly IMapper _mapper;
    public GroupTypeController(IGroupTypeService groupTypeService, IMapper mapper)
    {
        _groupTypeService = groupTypeService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var groupTypeGetResult = await _groupTypeService.GetAllAsync();
        var groupTypeList = _mapper.Map<List<AdminGroupTypeListVM>>(groupTypeGetResult.Data);
        return View(groupTypeList);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AdminGroupTypeCreateVM model)
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

        var addResult = await _groupTypeService.AddAsync(_mapper.Map<GroupTypeCreateDto>(model));
        if (!addResult.IsSuccess)
        {
            NotifyErrorLocalized(addResult.Message);
            return RedirectToAction(nameof(Index));
        }

        NotifySuccessLocalized(addResult.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var getGroupType = await _groupTypeService.GetByIdAsync(id);
        if (!getGroupType.IsSuccess)
        {
            NotifyErrorLocalized(getGroupType.Message);
            return RedirectToAction(nameof(Index));
        }

        return View(_mapper.Map<AdminGroupTypeDetailVM>(getGroupType.Data));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _groupTypeService.DeleteAsync(id);
        if (result.IsSuccess)
        {
            NotifySuccessLocalized(result.Message);
        }
        else
        {
            NotifyErrorLocalized(result.Message);
        }

        return Json(result);
    }

    public async Task<IActionResult> Update(Guid id)
    {
        var result = await _groupTypeService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            NotifyErrorLocalized(result.Message);
            return RedirectToAction(nameof(Index));
        }

        var groupTypeUpdateVM = _mapper.Map<AdminGroupTypeUpdateVM>(result.Data);

        return View(groupTypeUpdateVM);
    }


    [HttpPost]
    public async Task<IActionResult> Update(AdminGroupTypeUpdateVM groupTypeUpdateVM)
    {
        if (!ModelState.IsValid)
        {
            return View(groupTypeUpdateVM);
        }

        var groupTypeUpdateDto = _mapper.Map<GroupTypeUpdateDto>(groupTypeUpdateVM);
        var groupUpdateResponse = await _groupTypeService.UpdateAsync(groupTypeUpdateDto);
        if (!groupUpdateResponse.IsSuccess)
        {
            NotifyErrorLocalized(groupUpdateResponse.Message);
        }
        else
        {
            NotifySuccessLocalized(groupUpdateResponse.Message);
        }

        return RedirectToAction(nameof(Index));
    }
}