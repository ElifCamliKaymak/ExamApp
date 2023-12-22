namespace BAExamApp.Entities.DbSets;

public class Subtopic : AuditableEntity
{
    public Subtopic()
    {
        ExamRuleSubtopics = new HashSet<ExamRuleSubtopic>();
        Questions = new HashSet<Question>();
    }
    public string Name { get; set; }

    //Navigation Prop.
    public Guid SubjectId { get; set; }
    public virtual Subject? Subject { get; set; }

    public virtual ICollection<ExamRuleSubtopic> ExamRuleSubtopics { get; set; }
    public virtual ICollection<Question> Questions { get; set; }
}
