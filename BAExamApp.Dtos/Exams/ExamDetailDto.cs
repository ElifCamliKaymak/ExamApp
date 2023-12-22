using BAExamApp.Dtos.ExamEvaluators;

namespace BAExamApp.Dtos.Exams;

public class ExamDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime ExamDateTime { get; set; }
    public TimeSpan ExamDuration { get; set; }
    public string? Description { get; set; }
    public decimal MaxScore { get; set; }
    public decimal BonusScore { get; set; }
    public string ExamRuleName { get; set; }
    public string? ClassroomName { get; set; }
    public List<ExamEvaluatorListForExamDetailsDto> ExamEvaluators { get; set; }
}
