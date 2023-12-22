using System.ComponentModel.DataAnnotations;

namespace BAExamApp.MVC.Areas.Admin.Models.SubtopicVMs;
public class AdminSubtopicListVM
{
    public Guid Id { get; set; }

    [Display(Name ="Subtopic")]
    public string Name { get; set; }

    public bool IsActive { get; set; }
}
