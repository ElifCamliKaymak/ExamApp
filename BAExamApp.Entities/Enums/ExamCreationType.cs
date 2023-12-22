using System.ComponentModel.DataAnnotations;

namespace BAExamApp.Entities.Enums;
public enum ExamCreationType
{
    [Display(Name = "Tüm öğrenciler için aynı sorular, aynı sıra ile gelicek")]
    SameForEveryone = 1,
    [Display(Name = "Tüm öğrenciler için aynı sorular, farklı sıra ile gelicek")]
    SameQuestionsDifferentOrder = 2,
    [Display(Name = "Tüm öğrenciler için farklı sorular gelicek")]
    DifferentQuestions = 3,
}
