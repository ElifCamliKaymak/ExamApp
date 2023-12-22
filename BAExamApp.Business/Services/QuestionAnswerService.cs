using BAExamApp.Dtos.ClassroomProducts;
using BAExamApp.Dtos.QuestionAnswers;

namespace BAExamApp.Business.Services;
public class QuestionAnswerService : IQuestionAnswerService
{
    private readonly IQuestionAnswerRepository _questionAnswerRepository;
    private readonly IMapper _mapper;

    public QuestionAnswerService(IQuestionAnswerRepository questionAnswerRepository, IMapper mapper)
    {
        _questionAnswerRepository = questionAnswerRepository;
        _mapper = mapper;
    }

    public async Task<IDataResult<QuestionAnswerDto>> GetById(Guid id)
    {
        var questionAnswer = await _questionAnswerRepository.GetByIdAsync(id);
        if (questionAnswer == null)
        {
            return new ErrorDataResult<QuestionAnswerDto>(Messages.QuestionAnswerNotFound);
        }

        return new SuccessDataResult<QuestionAnswerDto>(_mapper.Map<QuestionAnswerDto>(questionAnswer), Messages.FoundSuccess);
    }

    public async Task<IDataResult<QuestionAnswerDto>> AddAsync(QuestionAnswerCreateDto questionAnswerCreateDto)
    {
        var questionAnswer = _mapper.Map<QuestionAnswer>(questionAnswerCreateDto);

        await _questionAnswerRepository.AddAsync(questionAnswer);
        await _questionAnswerRepository.SaveChangesAsync();

        return new SuccessDataResult<QuestionAnswerDto>(_mapper.Map<QuestionAnswerDto>(questionAnswer), Messages.AddSuccess);
    }

    public async Task<IDataResult<List<QuestionAnswerDto>>> AddRangeAsync(List<QuestionAnswerCreateDto> questionAnswersCreateDto)
    {
        var questionAnswers = new List<QuestionAnswer>();

        foreach (var questionAnswerCreateDto in questionAnswersCreateDto)
        {
            var questionAnswer = _mapper.Map<QuestionAnswer>(questionAnswerCreateDto);

            await _questionAnswerRepository.AddAsync(questionAnswer);

            questionAnswers.Add(questionAnswer);
        }
        await _questionAnswerRepository.SaveChangesAsync();

        return new SuccessDataResult<List<QuestionAnswerDto>>(_mapper.Map<List<QuestionAnswerDto>>(questionAnswers), Messages.AddSuccess);
    }

    public async Task<IDataResult<List<QuestionAnswerDto>>> UpdateRangeAsync(List<QuestionAnswerCreateDto> questionAnswersUpdateDto)
    {
        if (questionAnswersUpdateDto.Count > 0)
        {
            var CurrentQuestionAnswers = await _questionAnswerRepository.GetAllAsync(x => x.QuestionId == questionAnswersUpdateDto[0].QuestionId);
            await DeleteRangeAsync(CurrentQuestionAnswers.Select(x=>x.Id).ToList());
        }

        return await AddRangeAsync(questionAnswersUpdateDto);
    }

    public async Task<IResult> DeleteAsync(Guid id)
    {
        var questionAnswer = await _questionAnswerRepository.GetByIdAsync(id);

        if (questionAnswer is null)
        {
            return new ErrorDataResult<ClassroomProductDto>(Messages.ClassroomProductNotFound);
        }

        await _questionAnswerRepository.DeleteAsync(questionAnswer);
        await _questionAnswerRepository.SaveChangesAsync();

        return new SuccessResult(Messages.DeleteSuccess);
    }

    public async Task<IResult> DeleteRangeAsync(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            var questionAnswer = await _questionAnswerRepository.GetByIdAsync(id);

            if (questionAnswer is null)
            {
                return new ErrorDataResult<ClassroomProductDto>(Messages.ClassroomProductNotFound);
            }

            await _questionAnswerRepository.DeleteAsync(questionAnswer);
        }

        await _questionAnswerRepository.SaveChangesAsync();

        return new SuccessResult(Messages.DeleteSuccess);
    }
}
