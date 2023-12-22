using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAExamApp.Business.Services;
public class ExamAnalysisService : IExamAnalysisService
{
    private readonly IExamRepository _examRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISubtopicRepository _subtopicRepository;

    public ExamAnalysisService(IExamRepository examRepository, IStudentRepository studentRepository, ISubtopicRepository subtopicRepository)
    {
        _examRepository = examRepository;
        _studentRepository = studentRepository;
        _subtopicRepository = subtopicRepository;

    }

    public async Task<IDictionary<string, double>> AnalysisStudentPerformanceAsync(Guid studentId, Guid examId)
    {
        var exam = await _examRepository.GetByIdAsync(examId);
        var student = await _studentRepository.GetByIdAsync(studentId);

        if (exam == null || student == null)
        {
            throw new InvalidOperationException("Öğrenci ya da Sınav bulunamadı!");
        }

        var studentExam = exam.StudentExams.FirstOrDefault(se => se.StudentId == student.Id);

        if (studentExam == null)
        {
            throw new InvalidOperationException("Öğrenci bu sınava girmedi");
        }

        var subtopicPerformances = new Dictionary<string, double>();

        foreach (var studentQuestion in studentExam.StudentQuestions)
        {
            var question = studentQuestion.Question;

            if (question != null && question.Subtopic != null)
            {
                var subtopicName = question.Subtopic.Name;

                if (!string.IsNullOrEmpty(subtopicName))
                {
                    if (!subtopicPerformances.ContainsKey(subtopicName))
                    {
                        subtopicPerformances[subtopicName] = CalculateSubtopicPerformance(studentExam, subtopicName);
                    }
                }
            }
        }

        return subtopicPerformances;
    }





    private double CalculateSubtopicPerformance(StudentExam studentExam, string subtopic)
    {
        var questionsInSubtopic = studentExam.StudentQuestions
            .Where(sq => sq.Question != null && sq.Question.Subtopic != null && sq.Question.Subtopic.Name == subtopic)
            .ToList();

        int correctQuestions = questionsInSubtopic
            .Count(sq => IsQuestionCorrect(sq));

        int totalQuestions = questionsInSubtopic.Count;

        double subtopicPerformance = 0;
        if (totalQuestions > 0)
        {
            subtopicPerformance = (correctQuestions / (double)totalQuestions) * 100;
        }

        return subtopicPerformance;
    }

    private bool IsQuestionCorrect(StudentQuestion studentQuestion)
    {
        
        int correctSelected = 0;
        int totalCorrectAnswers = studentQuestion.StudentAnswers
            .Count(sa => sa.QuestionAnswer != null && sa.QuestionAnswer.IsRightAnswer);

        foreach (var answer in studentQuestion.StudentAnswers)
        {
            if (answer.QuestionAnswer != null)
            {
                if (answer.QuestionAnswer.IsRightAnswer && answer.IsSelected)
                {
                    
                    correctSelected++;
                }
                else if (!answer.QuestionAnswer.IsRightAnswer && answer.IsSelected)
                {
                   
                    return false;
                }
            }
        }

        
        return correctSelected == totalCorrectAnswers;
    }



}






