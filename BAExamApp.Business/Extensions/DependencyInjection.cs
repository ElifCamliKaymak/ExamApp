using BAExamApp.Business.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BAExamApp.Business.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<IClassroomService, ClassroomService>();
        services.AddScoped<IClassroomProductService, ClassroomProductService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IExamEvaluatorService, ExamEvaluatorService>();
        services.AddScoped<IExamRuleService, ExamRuleService>();
        services.AddScoped<IExamRuleSubtopicService, ExamRuleSubtopicService>();
        services.AddScoped<IExamClassroomsService, ExamClassroomsService>();
        services.AddScoped<IGroupTypeService, GroupTypeService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductSubjectService, ProductSubjectService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IQuestionAnswerService, QuestionAnswerService>();
        services.AddScoped<IQuestionDifficultyService, QuestionDifficultyService>();
        services.AddScoped<IQuestionFeedbackService, QuestionFeedbackService>();
        services.AddScoped<IQuestionRevisionService, QuestionRevisionService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IStudentAnswerService, StudentAnswerService>();
        services.AddScoped<IStudentClassroomService, StudentClassroomService>();
        services.AddScoped<IStudentExamService, StudentExamService>();
        services.AddScoped<IStudentQuestionService, StudentQuestionService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ISubtopicService, SubtopicService>();
        services.AddScoped<ITalentService, TalentService>();
        services.AddScoped<ITechnicalUnitService, TechnicalUnitService>();
        services.AddScoped<ITestExamService, TestExamService>();
        services.AddScoped<ITestExamQuestionService, TestExamQuestionService>();
        services.AddScoped<ITestExamTesterService, TestExamTesterService>();
        services.AddScoped<ITrainerService, TrainerService>();
        services.AddScoped<ITrainerClassroomService, TrainerClassroomService>();
        services.AddScoped<ITrainerProductService, TrainerProductService>();
        services.AddScoped<ITrainerTalentService, TrainerTalentService>();
        services.AddScoped<IExamAnalysisService, ExamAnalysisService>();

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISendMailService, SendMailService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
