using AutoMapper;
using BAExamApp.Dtos.TecnicalUnits;
using BAExamApp.MVC.Areas.Admin.Models.TechnicalUnitVMs;

namespace BAExamApp.MVC.Areas.Admin.Controllers;

public class TechnicalUnitController : AdminBaseController
{
    private readonly ITechnicalUnitService _technicalUnitService;
    private readonly IMapper _mapper;
    public TechnicalUnitController(ITechnicalUnitService technicalUnitService, IMapper mapper)
    {
        _technicalUnitService = technicalUnitService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _technicalUnitService.GetAllAsync();
        var TechnicalUnitList = _mapper.Map<IEnumerable<AdminTechnicalUnitListVM>>(result.Data);

        return View(TechnicalUnitList);
    }
    [HttpPost]
    public async Task<IActionResult> Create(AdminTechnicalUnitCreateVM technicalUnitCreateVM)
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

        var addResult = await _technicalUnitService.AddAsync(_mapper.Map<TechnicalUnitCreateDto>(technicalUnitCreateVM));
        if (!addResult.IsSuccess)
        {
            NotifyErrorLocalized(addResult.Message);
            return RedirectToAction(nameof(Index));
        }

        NotifySuccessLocalized(addResult.Message);
        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Update(Guid id)
    {
        var technicalUnitFoundResult = await _technicalUnitService.GetByIdAsync(id);
        if (!technicalUnitFoundResult.IsSuccess)
        {
            NotifyErrorLocalized(technicalUnitFoundResult.Message);
            return RedirectToAction(nameof(Index));
        }

        var technicalUnitUpdateVM = _mapper.Map<AdminTechnicalUnitUpdateVM>(technicalUnitFoundResult.Data);
        return View(technicalUnitUpdateVM);
    }


    [HttpPost]
    public async Task<IActionResult> Update(AdminTechnicalUnitUpdateVM technicalUnitUpdateVM)
    {
        if (!ModelState.IsValid)
        {
            return View(technicalUnitUpdateVM);
        }

        var technicalUnitUpdateDto = _mapper.Map<TechnicalUnitUpdateDto>(technicalUnitUpdateVM);
        var technicalUnitUpdateResult = await _technicalUnitService.UpdateAsync(technicalUnitUpdateDto);
        if (!technicalUnitUpdateResult.IsSuccess)
        {
            NotifyErrorLocalized(technicalUnitUpdateResult.Message);
            return View(technicalUnitUpdateVM);
        }
        
        NotifySuccessLocalized(technicalUnitUpdateResult.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleteResult = await _technicalUnitService.DeleteAsync(id);

        if (!deleteResult.IsSuccess)
        {
            return Content("<script>setTimeout(function() { location.reload(); }, 2000);</script>");
        }
        else
        {
            return Json(deleteResult);
        }
    }
}