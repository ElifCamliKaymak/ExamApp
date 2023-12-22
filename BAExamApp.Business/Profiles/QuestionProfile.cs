using BAExamApp.Dtos.Questions;

namespace BAExamApp.Business.Profiles;

public class QuestionProfile : Profile
{
    public QuestionProfile()
    {
        CreateMap<Question, QuestionDto>()
            .ForMember(dest => dest.SubjectId, config => config.MapFrom(src => src.Subtopic.SubjectId));
        CreateMap<Question, QuestionListDto>()
            .ForMember(dest => dest.TimeGiven, config => config.MapFrom(src => src.QuestionDifficulty.TimeGiven))
            .ForMember(dest => dest.SubtopicName, config => config.MapFrom(src => src.Subtopic.Name))
            .ForMember(dest => dest.QuestionDifficultyName, config => config.MapFrom(src => src.QuestionDifficulty.Name));
        CreateMap<Question, QuestionDetailsDto>()
            .ForMember(dest => dest.TimeGiven, config => config.MapFrom(src => src.QuestionDifficulty.TimeGiven))
            .ForMember(dest => dest.SubjectName, config => config.MapFrom(src => src.Subtopic.Subject.Name))
            .ForMember(dest => dest.SubtopicName, config => config.MapFrom(src => src.Subtopic.Name))
            .ForMember(dest => dest.QuestionDifficultyName, config => config.MapFrom(src => src.QuestionDifficulty.Name));
        CreateMap<Question, QuestionDetailsForAdminDto>()
            .ForMember(dest => dest.SubtopicName, config => config.MapFrom(src => src.Subtopic.Name))
            .ForMember(dest => dest.QuestionDifficultyName, config => config.MapFrom(src => src.QuestionDifficulty.Name));
        CreateMap<QuestionCreateDto, Question>();
        CreateMap<QuestionUpdateDto, Question>();
    }
}
