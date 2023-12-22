using BAExamApp.Dtos.QuestionAnswers;

namespace BAExamApp.Business.Profiles;

public class QuestionAnswerProfile : Profile
{
    public QuestionAnswerProfile()
    {
        CreateMap<QuestionAnswer, QuestionAnswerDto>();
        CreateMap<QuestionAnswerCreateDto, QuestionAnswer>();
        CreateMap<QuestionAnswerUpdateDto, QuestionAnswer>();
    }
}