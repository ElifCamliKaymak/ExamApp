namespace BAExamApp.Entities.Configurations;

public class SubtopicConfiguration : AuditableEntityConfiguration<Subtopic>
{
    public override void Configure(EntityTypeBuilder<Subtopic> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(256);

        builder.HasOne(x => x.Subject).WithMany(x => x.Subtopics).HasForeignKey(x => x.SubjectId);
    }
}
