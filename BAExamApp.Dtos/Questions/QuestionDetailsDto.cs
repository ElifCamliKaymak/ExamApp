using BAExamApp.Dtos.QuestionAnswers;

namespace BAExamApp.Dtos.Questions;

public class QuestionDetailsDto
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Content { get; set; }
    public int State { get; set; }
    public int QuestionType { get; set; }
    public string Target { get; set; }
    public string Gains { get; set; }
    public string SubtopicName { get; set; }
    public string SubjectName { get; set; }
    public string QuestionDifficultyName { get; set; }
    public TimeSpan TimeGiven { get; set; }
    public List<QuestionAnswerDto> QuestionAnswers { get; set; }
}