﻿using BAExamApp.Dtos.QuestionAnswers;
using BAExamApp.Entities.Enums;

namespace BAExamApp.Dtos.Questions;

public class QuestionCreateDto
{
    public string Content { get; set; }
    public QuestionType QuestionType { get; set; }
    public string? Image { get; set; }
    public string Target { get; set; }
    public string Gains { get; set; }
    public Guid SubtopicId { get; set; }
    public Guid QuestionDifficultyId { get; set; }
    public List<QuestionAnswerCreateDto> QuestionAnswers { get; set; }
}