using BAExamApp.Dtos.Emails;
using BAExamApp.Dtos.Students;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BAExamApp.Business.Interfaces.Services;
public interface ISendMailService
{
    /// <summary>
    /// Yeni oluşturulan öğrenci hesabının bilgilerinin gönderilmesi için kullanırlır.
    /// </summary>
    /// <param name="email">Mailin gönderileceği mail adresi</param>
    /// <param name="url">Login sayfasının url adresi</param>
    /// <returns></returns>
    Task SendEmailNewStudent(string email, string url);

    /// <summary>
    /// Yeni oluşturulan eğitmen hesabının bilgilerinin gönderilmesi için kullanırlır.
    /// </summary>
    /// <param name="email">Mailin gönderileceği mail adresi</param>
    /// <param name="url">Login sayfasının url adresi</param>
    /// <returns></returns>
    Task SendEmailNewTrainer(string email, string url);

    /// <summary>
    /// Yeni oluşturulan admin hesabının bilgilerinin gönderilmesi için kullanırlır.
    /// </summary>
    /// <param name="email">Mailin gönderileceği mail adresi</param>
    /// <param name="url">Login sayfasının url adresi</param>
    /// <returns></returns>
    Task SendEmailNewAdmin(string email, string url);

    /// <summary>
    /// Giriş yapan kullanıcının iki adımlı doğrulamadaki güvenlik kodunun gönderilmesi için kullanılır.
    /// </summary>
    /// <param name="email">Mailin gönderileceği mail adresi</param>
    /// <returns>Gönderilen güvenlik kodu</returns>
    Task<int> SendEmailVerificationCode(string email);


    /// <summary>
    /// Belirtilen mail adresine sınav raporu göndermek için kullanılır.
    /// </summary>
    /// <param name="email">Mailin gönderileceği mail adresi</param>
    /// <returns></returns>
    Task SendAfterExamMail(string email, string studentFullName, string examName, int totalTimeSpent, int? studentPoint);

    /// <summary>
    /// Öğrencinin oluşturulan yeni sınava girebilmesi için gerekli bilgileri ve linki gönderir. 
    /// </summary>
    /// <param name="student">Öğrenci bilgileirini içerir</param>
    /// <param name="url">Sınav sayfasının url adresi</param>
    /// <param name="examId">Öğrencinin gireceği sınavın ID'si</param>
    /// <returns></returns>
    Task SendEmailToStudentNewExam(string emailAdress, string url, Guid studentExamId);


    Task<string> GetStudentEmailById(Guid studentId);

    Task SendEmailToTrainerNewExam(string trainerEmailAdress, List<string> studentContents);
    




}