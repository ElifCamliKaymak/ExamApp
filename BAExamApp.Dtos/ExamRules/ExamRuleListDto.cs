using BAExamApp.Entities.Enums;

namespace BAExamApp.Dtos.ExamRules;

public class ExamRuleListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ExamCreationType ExamCreationType { get; set; }
}