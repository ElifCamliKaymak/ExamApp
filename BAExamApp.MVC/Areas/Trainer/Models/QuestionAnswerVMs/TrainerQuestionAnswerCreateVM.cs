namespace BAExamApp.MVC.Areas.Trainer.Models.QuestionAnswerVMs;

public class TrainerQuestionAnswerCreateVM
{
    public string Answer { get; set; }
    public bool IsRightAnswer { get; set; }
    public Guid QuestionId { get; set; }
}