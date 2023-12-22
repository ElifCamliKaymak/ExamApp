using BAExamApp.Dtos.Questions;
using BAExamApp.Entities.DbSets;
using BAExamApp.Entities.Enums;
using System.Linq.Expressions;

namespace BAExamApp.Business.Services;

public class QuestionService : IQuestionService
{
    private readonly ITrainerRepository _trainerRepository;
    private readonly IQuestionRevisionRepository _questionRevisionRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IMapper _mapper;
    private readonly ITrainerService _trainerService;

    public QuestionService(IQuestionRepository questionRepository, IMapper mapper, ITrainerService trainerService, ITrainerRepository trainerRepository, IQuestionRevisionRepository questionRevisionRepository)
    {
        _questionRepository = questionRepository;
        _mapper = mapper;
        _trainerService = trainerService;
        _trainerRepository = trainerRepository;
        _questionRevisionRepository = questionRevisionRepository;
    }
    public async Task<IDataResult<QuestionDto>> GetByIdAsync(Guid id)
    {
        var question = await _questionRepository.GetByIdAsync(id);

        if (question == null)
        {
            return new ErrorDataResult<QuestionDto>(Messages.QuestionNotFound);
        }

        return new SuccessDataResult<QuestionDto>(_mapper.Map<QuestionDto>(question), Messages.QuestionFoundSuccess);
    }

    public async Task<IDataResult<List<QuestionListDto>>> GetAllAsync()
    {
        var questions = await _questionRepository.GetAllAsync();

        return new SuccessDataResult<List<QuestionListDto>>(_mapper.Map<List<QuestionListDto>>(questions), Messages.ListedSuccess);
    }

    public async Task<IDataResult<List<QuestionListDto>>> GetAllByStateAsync(State state)
    {
        var questions = await _questionRepository.GetAllAsync(x => x.State == state);

        return new SuccessDataResult<List<QuestionListDto>>(_mapper.Map<List<QuestionListDto>>(questions), Messages.ListedSuccess);
    }

    public async Task<IDataResult<List<QuestionListDto>>> GetAllByStateAndTrainerIdAsync(string trainerIdentityId, State state)
    {
        var trainer = await _trainerRepository.GetByIdentityIdAsync(trainerIdentityId);
        var questionRevisionList = await _questionRevisionRepository.GetAllAsync(qr =>
            qr.RequestedTrainerId == trainer.Id);

        var questionList = new List<Question>();
        foreach (var questionRevision in questionRevisionList)
        {
            var question = await _questionRepository.GetByIdAsync(questionRevision.QuestionId);
            if (question.State == state && questionRevision.RevisionConclusion == null)
                questionList.Add(question);
            else if (question.State == state && questionRevision.RevisionConclusion != null)
                questionList.Add(question);
        }

        var questions = await _questionRepository.GetAllAsync(question => 
            question.CreatedBy == trainerIdentityId && 
            question.State == state);

        foreach (var question in questions)
        {
            if (question.Status == Core.Enums.Status.Added)
                questionList.Add(question);
            else
            {
                var result = await _questionRevisionRepository.GetAllAsync(qr=>
                    qr.RequestedTrainerId.Equals(trainer.Id) && 
                    qr.QuestionId.Equals(question.Id));

                foreach(var questionRevision in result)
                {
                    if (questionRevision.Status == Core.Enums.Status.Added && 
                        questionRevision.RequestedTrainerId == trainer.Id)
                            questionList.Add(question);
                    else if (questionRevision.Status == Core.Enums.Status.Modified && 
                        questionRevision.ModifiedBy == trainerIdentityId)
                            questionList.Add(question);
                }
            }
        }

        return new SuccessDataResult<List<QuestionListDto>>(_mapper.Map<List<QuestionListDto>>(questionList), Messages.ListedSuccess);
    }

    public async Task<IDataResult<List<QuestionListDto>>> GetAllByFilterAsync(QuestionFilterDto questionFilterDto)
    {
        var expressionList = new List<Expression<Func<Question, bool>>>
        {
            x => x.State == State.Approved
        };

        if (questionFilterDto.QuestionDifficultyId != null)
            expressionList.Add(x => x.QuestionDifficultyId == questionFilterDto.QuestionDifficultyId);

        if (questionFilterDto.SubtopicId != null)
            expressionList.Add(x => x.SubtopicId == questionFilterDto.SubtopicId);

        var firstExpression = expressionList[0];

        var body = firstExpression.Body;
        var parameters = firstExpression.Parameters.ToArray();

        foreach (var expression in expressionList.Skip(1))
        {
            var nextBody = Expression.Invoke(expression, parameters);
            body = Expression.AndAlso(body, nextBody);
        }

        var finalExpression = Expression.Lambda<Func<Question, bool>>(body, parameters);

        var questions = await _questionRepository.GetAllAsync(finalExpression);

        return new SuccessDataResult<List<QuestionListDto>>(_mapper.Map<List<QuestionListDto>>(questions), Messages.ListedSuccess);
    }

    public async Task<IDataResult<List<QuestionListDto>>> GetAllByExamRuleSubtopicAsync(Guid questionDifficultyId, int questionType, Guid subtopicId)
    {
        var questions = await _questionRepository.GetAllAsync(x => x.QuestionDifficultyId == questionDifficultyId && (int)x.QuestionType == questionType && x.SubtopicId == subtopicId && x.State == State.Approved, true);

        return new SuccessDataResult<List<QuestionListDto>>(_mapper.Map<List<QuestionListDto>>(questions), Messages.ListedSuccess);
    }

    public async Task<IDataResult<QuestionDetailsDto>> GetDetailsByIdAsync(Guid id)
    {
        var question = await _questionRepository.GetByIdAsync(id);

        if (question is null)
        {
            return new ErrorDataResult<QuestionDetailsDto>(Messages.QuestionNotFound);
        }

        return new SuccessDataResult<QuestionDetailsDto>(_mapper.Map<QuestionDetailsDto>(question), Messages.FoundSuccess);
    }

    public async Task<IDataResult<QuestionDetailsForAdminDto>> GetDetailsByIdForAdminAsync(Guid id)
    {

        var question = await _questionRepository.GetByIdAsync(id);

        if (question is null)
        {
            return new ErrorDataResult<QuestionDetailsForAdminDto>(Messages.QuestionNotFound);
        }

        var creator = await _trainerService.GetByIdentityIdAsync(question.CreatedBy);
        if (creator.IsSuccess)
        {
            question.CreatedBy = creator.Data.FirstName + " " + creator.Data.LastName;
        }

        return new SuccessDataResult<QuestionDetailsForAdminDto>(_mapper.Map<QuestionDetailsForAdminDto>(question), Messages.FoundSuccess);
    }

    public async Task<IDataResult<QuestionDto>> AddAsync(QuestionCreateDto questionCreateDto)
    {
        var question = _mapper.Map<Question>(questionCreateDto);

        await _questionRepository.AddAsync(question);
        await _questionRepository.SaveChangesAsync();

        return new SuccessDataResult<QuestionDto>(_mapper.Map<QuestionDto>(question), Messages.AddSuccess);
    }

    public async Task<IDataResult<QuestionDto>> UpdateAsync(QuestionUpdateDto questionUpdateDto)
    {
        var question = await _questionRepository.GetByIdAsync(questionUpdateDto.Id);

        if (question is null)
            return new ErrorDataResult<QuestionDto>(Messages.QuestionNotFound);

        var updatedQuestion = _mapper.Map(questionUpdateDto, question);

        await _questionRepository.UpdateAsync(updatedQuestion);
        await _questionRepository.SaveChangesAsync();

        return new SuccessDataResult<QuestionDto>(_mapper.Map<QuestionDto>(updatedQuestion), Messages.UpdateSuccess);
    }

    public async Task<IResult> UpdateStateAsync(Guid id, State state)
    {
        var question = await _questionRepository.GetByIdAsync(id);

        if (question is null)
        {
            return new ErrorDataResult<QuestionDetailsForAdminDto>(Messages.QuestionNotFound);
        }
        question.State = state;


        await _questionRepository.UpdateAsync(question);
        await _questionRepository.SaveChangesAsync();

        return new SuccessDataResult<QuestionDto>(_mapper.Map<QuestionDto>(question), Messages.UpdateSuccess);
    }
    public async Task<IResult> UpdateStateWithCommentAsync(Guid id, State state, string? rejectComment)
    {
        var question = await _questionRepository.GetByIdAsync(id);

        if (question is null)
        {
            return new ErrorDataResult<QuestionDetailsForAdminDto>(Messages.QuestionNotFound);
        }
        question.State = state;
        question.RejectComment = rejectComment;

        await _questionRepository.UpdateAsync(question);
        await _questionRepository.SaveChangesAsync();

        return new SuccessDataResult<QuestionDto>(_mapper.Map<QuestionDto>(question), Messages.UpdateSuccess);
    }

    public async Task<IResult> DeleteAsync(Guid questionId)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);

        if (question is null)
        {
            return new ErrorResult(Messages.QuestionNotFound);
        }

        await _questionRepository.DeleteAsync(question);
        await _questionRepository.SaveChangesAsync();

        return new SuccessResult(Messages.DeleteSuccess);
    }

    public async Task<IResult> SetIsActive(Guid id, bool isActive)
    {
        var question = await _questionRepository.GetByIdAsync(id);

        if (question == null)
        {
            return new ErrorResult(Messages.QuestionNotFound);
        }

        question.IsActive = isActive;
        await _questionRepository.SaveChangesAsync();

        return question.IsActive switch
        {
            true => new SuccessResult(Messages.SetIsActiveTrue),
            false => new SuccessResult(Messages.SetIsActiveFalse)
        };
    }
}