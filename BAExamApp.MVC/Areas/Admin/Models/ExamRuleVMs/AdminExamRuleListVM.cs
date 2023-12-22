using BAExamApp.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace BAExamApp.MVC.Areas.Admin.Models.ExamRuleVMs;

public class AdminExamRuleListVM
{
    public Guid Id { get; set; }
    [Display(Name = "Exam_Rule_Name")]
    public string Name { get; set; }

   
}