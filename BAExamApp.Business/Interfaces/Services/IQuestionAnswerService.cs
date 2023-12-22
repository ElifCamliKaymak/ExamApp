using BAExamApp.Dtos.QuestionAnswers;

namespace BAExamApp.Business.Interfaces.Services;

public interface IQuestionAnswerService
{
    Task<IDataResult<QuestionAnswerDto>> GetById(Guid id);
    Task<IDataResult<QuestionAnswerDto>> AddAsync(QuestionAnswerCreateDto questionAnswerCreateDto);
    Task<IDataResult<List<QuestionAnswerDto>>> AddRangeAsync(List<QuestionAnswerCreateDto> questionAnswersCreateDto);
    Task<IDataResult<List<QuestionAnswerDto>>> UpdateRangeAsync(List<QuestionAnswerCreateDto> questionAnswersUpdateDto);
    Task<IResult> DeleteAsync(Guid id);
    Task<IResult> DeleteRangeAsync(List<Guid> ids);
}
