namespace BAExamApp.Entities.DbSets;

public class Subject : AuditableEntity
{
    public Subject()
    {
        Subtopics = new HashSet<Subtopic>();
        ProductSubjects = new HashSet<ProductSubject>();
    }
    public string Name { get; set; } = null!;

    //Navigation Prop.
    public virtual ICollection<Subtopic> Subtopics { get; set; }
    public virtual ICollection<ProductSubject> ProductSubjects { get; set; }
}
