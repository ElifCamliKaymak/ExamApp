using BAExamApp.Dtos.ExamEvaluators;

namespace BAExamApp.Business.Services;
public class ExamEvaluatorService : IExamEvaluatorService
{
    private readonly IExamEvaluatorRepository _examsEvaluatorsRepository;
    private readonly IMapper _mapper;

    public ExamEvaluatorService(IExamEvaluatorRepository examsEvaluatorsRepository, IMapper mapper)
    {
        _examsEvaluatorsRepository = examsEvaluatorsRepository;
        _mapper = mapper;
    }

    public async Task<IDataResult<List<ExamEvaluatorListDto>>> GetAllByExamIdAsync(Guid id)
    {
        var examEvaluators = await _examsEvaluatorsRepository.GetAllAsync(x => x.ExamId == id);

        return new SuccessDataResult<List<ExamEvaluatorListDto>>(_mapper.Map<List<ExamEvaluatorListDto>>(examEvaluators), Messages.FoundSuccess);
    }

    public async Task<IDataResult<List<ExamEvaluatorListDto>>> GetAllByTrainerIdAsync(Guid id)
    {

        var examEvaluators = await _examsEvaluatorsRepository.GetAllAsync(x => x.TrainerId == id);

        return new SuccessDataResult<List<ExamEvaluatorListDto>>(_mapper.Map<List<ExamEvaluatorListDto>>(examEvaluators), Messages.FoundSuccess);

    }

    public async Task<IDataResult<ExamEvaluatorDto>> AddAsync(ExamEvaluatorCreateDto examEvaluatorCreateDto)
    {
        var hasExamEvaluator = await _examsEvaluatorsRepository.AnyAsync(x => x.ExamId == examEvaluatorCreateDto.ExamId && x.TrainerId == examEvaluatorCreateDto.TrainerId);

        if (hasExamEvaluator)
        {
            return new ErrorDataResult<ExamEvaluatorDto>(Messages.AddFailAlreadyExists);
        }

        var examEvaluator = _mapper.Map<ExamEvaluator>(examEvaluatorCreateDto);

        await _examsEvaluatorsRepository.AddAsync(examEvaluator);
        await _examsEvaluatorsRepository.SaveChangesAsync();


        return new SuccessDataResult<ExamEvaluatorDto>(_mapper.Map<ExamEvaluatorDto>(examEvaluator), Messages.AddSuccess);
    }

    public async Task<IDataResult<List<ExamEvaluatorDto>>> AddRangeAsync(List<ExamEvaluatorCreateDto> examEvaluatorsCreateDto)
    {
        var examEvaluators = new List<ExamEvaluator>();

        foreach (var examEvaluatorCreateDto in examEvaluatorsCreateDto)
        {
            var hasExamEvaluator = await _examsEvaluatorsRepository.AnyAsync(x => x.ExamId == examEvaluatorCreateDto.ExamId && x.TrainerId == examEvaluatorCreateDto.TrainerId);

            if (hasExamEvaluator)
            {
                return new ErrorDataResult<List<ExamEvaluatorDto>>(Messages.AddFailAlreadyExists);
            }

            var examEvaluator = _mapper.Map<ExamEvaluator>(examEvaluatorCreateDto);

            await _examsEvaluatorsRepository.AddAsync(examEvaluator);
            examEvaluators.Add(examEvaluator);
        }
        await _examsEvaluatorsRepository.SaveChangesAsync();

        return new SuccessDataResult<List<ExamEvaluatorDto>>(_mapper.Map<List<ExamEvaluatorDto>>(examEvaluators), Messages.AddSuccess);
    }

    public async Task<IResult> DeleteAsync(Guid id)
    {
        var examEvaluator = await _examsEvaluatorsRepository.GetByIdAsync(id);

        if (examEvaluator is null)
        {
            return new ErrorDataResult<ExamEvaluatorDto>(Messages.ExamEvaluatorNotFound);
        }

        await _examsEvaluatorsRepository.DeleteAsync(examEvaluator);
        await _examsEvaluatorsRepository.SaveChangesAsync();

        return new SuccessResult(Messages.DeleteSuccess);
    }

    public async Task<IResult> DeleteRangeAsync(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            var examEvaluator = await _examsEvaluatorsRepository.GetByIdAsync(id);

            if (examEvaluator is null)
            {
                return new ErrorDataResult<ExamEvaluatorDto>(Messages.ExamEvaluatorNotFound);
            }

            await _examsEvaluatorsRepository.DeleteAsync(examEvaluator);
        }
        await _examsEvaluatorsRepository.SaveChangesAsync();

        return new SuccessResult(Messages.DeleteSuccess);
    }

    public async Task<IResult> UpdateExamEvaluatorsAsync(List<Guid> trainerIds, Guid examId)
    {
        var existingTrainers = await _examsEvaluatorsRepository.GetAllAsync(x => x.ExamId == examId);

        var trainersToRemove = existingTrainers.Where(trainer => !trainerIds.Contains(trainer.TrainerId)).ToList();
        foreach (var trainerToRemove in trainersToRemove)
        {
            await _examsEvaluatorsRepository.DeleteAsync(trainerToRemove);
        }
        List<Guid> trainersToAdd = new List<Guid>();
        if (trainerIds != null)
        {
            trainersToAdd = trainerIds.Where(trainerId => !existingTrainers.Any(s => s.TrainerId == trainerId)).ToList();

        }
        foreach (var trainerIdToAdd in trainersToAdd)
        {
            var newTrainer = new ExamEvaluator
            {
                ExamId = examId,
                TrainerId = trainerIdToAdd
            };
            await _examsEvaluatorsRepository.AddAsync(newTrainer);
        }
        await _examsEvaluatorsRepository.SaveChangesAsync();

        return new SuccessResult("Trainer güncelleme işlemi başarıyla tamamlandı.");
    }
}
