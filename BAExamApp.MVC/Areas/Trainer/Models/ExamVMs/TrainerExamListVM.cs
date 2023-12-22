using System.ComponentModel.DataAnnotations;

namespace BAExamApp.MVC.Areas.Trainer.Models.ExamVMs;

public class TrainerExamListVM
{
    public Guid Id { get; set; }

    [Display(Name ="Exam_Name")]
    public string Name { get; set; }

    [Display(Name = "Exam_Date")]
    public DateTime ExamDateTime { get; set; } = DateTime.Now;

    [Display(Name = "Exam_Duration")]
    public TimeSpan ExamDuration { get; set; }

    [Display(Name = "Classroom")]
    public string ClassroomName { get; set; }
}