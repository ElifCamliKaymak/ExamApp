using BAExamApp.Dtos.StudentExams;

namespace BAExamApp.Business.Profiles;

public class StudentExamProfile : Profile
{
    public StudentExamProfile()
    {
        CreateMap<StudentExam, StudentExamDto>()
            .ForMember(dest=>dest.StudentFullName, opt=>opt.MapFrom(src=>src.Student.FullName));

        CreateMap<StudentExam, StudentExamListDto>()
            .ForMember(dest => dest.ExamName, opt => opt.MapFrom(src => src.Exam.Name))
            .ForMember(dest => dest.ExamDateTime, opt => opt.MapFrom(src => src.Exam.ExamDateTime))
            .ForMember(dest => dest.ExamDuration, opt => opt.MapFrom(src => src.Exam.ExamDuration))
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName))
            .ForMember(dest => dest.EvaluatorName, opt => opt.MapFrom(src => src.Exam.ExamEvaluators.FirstOrDefault().Trainer.FullName))
            .ForMember(dest => dest.MaxScore, opt => opt.MapFrom(src => src.Exam.MaxScore))
            .ForMember(dest => dest.ClassroomNames, opt => opt.MapFrom(src => src.Student.StudentClassrooms.Select(ec => ec.Classroom.Name).Distinct().ToList()));
        
        CreateMap<StudentExam, StudentExamListForTrainerDto>()
            .ForMember(dest => dest.ExamName, opt => opt.MapFrom(src => src.Exam.Name))
            .ForMember(dest => dest.ExamDateTime, opt => opt.MapFrom(src => src.Exam.ExamDateTime))
            .ForMember(dest => dest.MaxScore, opt => opt.MapFrom(src => src.Exam.MaxScore));


        CreateMap<StudentExam, StudentExamsAdminDto>()
            .ForMember(dest => dest.ClassroomNames, opt => opt.MapFrom(src => src.Exam.ExamClassrooms.Select(ec => ec.Classroom.Name).ToList()))
            .ForMember(dest => dest.ExamName, opt => opt.MapFrom(src => src.Exam.Name))
            .ForMember(dest => dest.MaxScore, opt => opt.MapFrom(src => src.Exam.MaxScore))
            .ForMember(dest => dest.ExamDateTime, opt => opt.MapFrom(src => src.Exam.ExamDateTime))
            .ForMember(dest => dest.StudentFullName, opt => opt.MapFrom(src => src.Student.FullName));

        CreateMap<StudentExam,StudentExamsDetailsDto>()
            .ForMember(dest => dest.ClassroomName, opt => opt.MapFrom(src => src.Exam.ExamClassrooms.Select(ec => ec.Classroom.Name)))
            .ForMember(dest => dest.ExamName, opt => opt.MapFrom(src => src.Exam.Name))
            .ForMember(dest => dest.MaxScore, opt => opt.MapFrom(src => src.Exam.MaxScore))
            .ForMember(dest => dest.ExamDateTime, opt => opt.MapFrom(src => src.Exam.ExamDateTime))
            .ForMember(dest => dest.StudentFullName, opt => opt.MapFrom(src => src.Student.FullName));


        CreateMap<StudentExamCreateDto, StudentExam>();
        CreateMap<StudentExamUpdateDto, StudentExam>();
    }
}
