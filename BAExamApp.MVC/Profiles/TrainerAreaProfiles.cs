using AutoMapper;
using BAExamApp.Dtos.Classrooms;
using BAExamApp.Dtos.ExamClassrooms;
using BAExamApp.Dtos.Exams;
using BAExamApp.Dtos.QuestionAnswers;
using BAExamApp.Dtos.QuestionRevisions;
using BAExamApp.Dtos.Questions;
using BAExamApp.Dtos.StudentClassrooms;
using BAExamApp.Dtos.StudentExams;
using BAExamApp.Dtos.Students;
using BAExamApp.Dtos.Trainers;
using BAExamApp.MVC.Areas.Trainer.Models.ClassroomVMs;
using BAExamApp.MVC.Areas.Trainer.Models.ExamVMs;
using BAExamApp.MVC.Areas.Trainer.Models.QuestionAnswerVMs;
using BAExamApp.MVC.Areas.Trainer.Models.QuestionRevisionVMs;
using BAExamApp.MVC.Areas.Trainer.Models.QuestionVMs;
using BAExamApp.MVC.Areas.Trainer.Models.StudentClassroomVMs;
using BAExamApp.MVC.Areas.Trainer.Models.StudentExamVMs;
using BAExamApp.MVC.Areas.Trainer.Models.StudentVMs;
using BAExamApp.MVC.Areas.Trainer.Models.TrainerVMs;

namespace BAExamApp.MVC.Profiles
{
    public class TrainerAreaProfiles : Profile
    {
        public TrainerAreaProfiles()
        {
            //Question Controller

            //List
            CreateMap<QuestionListDto, TrainerQuestionListVM>();
            CreateMap<QuestionRevisionListDto, TrainerQuestionRevisionListVM>();

            //Create
            CreateMap<TrainerQuestionCreateVM, QuestionCreateDto>();
            CreateMap<QuestionCreateDto, TrainerQuestionsCreateVM>().ReverseMap();
            CreateMap<TrainerQuestionAnswerCreateVM, QuestionAnswerCreateDto>();
            CreateMap<QuestionDto, TrainerQuestionCreateVM>();
            CreateMap<QuestionAnswerDto, TrainerQuestionAnswerCreateVM>();

            //Details
            CreateMap<QuestionDetailsDto, TrainerQuestionDetailsVM>();

            //Update
            CreateMap<QuestionDto, TrainerQuestionUpdateVM>();
            CreateMap<QuestionAnswerDto, TrainerQuestionAnswerUpdateVM>();
            CreateMap<TrainerQuestionUpdateVM, QuestionUpdateDto>();
            CreateMap<TrainerQuestionAnswerUpdateVM, QuestionAnswerUpdateDto>();

            //Classroom Controller

            //List
            CreateMap<ClassroomListDto, TrainerClassroomListVM>();
            CreateMap<ClassroomDetailsDto, TrainerClassroomDetailsVM>();

            //Student Controller

            //Details
            CreateMap<StudentDetailsForTrainerDto, TrainerStudentDetailsVM>();

            CreateMap<StudentExamsDetailsDto, StudentExamsForTrainerVM>();

            //StudentClassroom Details
            CreateMap<StudentClassroomListForClassroomDetailsDto, TrainerStudentClassroomListForClassroomDetailsVM>();

            //StudentExam List
            CreateMap<StudentExamListForTrainerDto, TrainerStudentExamListVM>();


            //The mapping process has been carried out to list the excuses of students who did not take the exam
            CreateMap<StudentExamListDto, StudentExamStatusForTrainerVM>();

            //Trainer Controller

            //Details
            CreateMap<TrainerDetailsForTrainerDto, TrainerTrainerDetailVM>();

            //Update
            CreateMap<TrainerDto, TrainerTrainerUpdateVM>();
            CreateMap<TrainerTrainerUpdateVM, TrainerUpdateDto>();

            //Exam
            CreateMap<ExamListDto, TrainerExamListVM>()
                .ForMember(dest => dest.ClassroomName , opt => opt.MapFrom(src => String.Join(", ", src.ClassroomNames)));
            CreateMap<ExamDetailDto, TrainerExamDetailVM>();

            CreateMap<TrainerExamCreateVM, ExamCreateDto>()
                .ForMember(dest => dest.ExamClassroomsIds, opt => opt.MapFrom(src => src.ExamClassroomsIds))
                .ForMember(dest => dest.ExamType, opt => opt.MapFrom(src => src.ExamType));


            //ExamEvaluators List ***** ExamRule'a sınav tipi eklendikten sonra eklenecek. *****
            //CreateMap<ExamEvaluatorListDto, TrainerExamEvaluatorListVM>();
        }
    }
}
