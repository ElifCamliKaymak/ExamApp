using BAExamApp.Dtos.Emails;
using BAExamApp.Dtos.Students;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using SmtpClient = System.Net.Mail.SmtpClient;

namespace BAExamApp.Business.Services
{


    public class SendMailService : ISendMailService
    {
        private readonly IOptions<EmailConfigurationDto> _configuration;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentExamRepository _studentExamRepository;
        private readonly ITrainerRepository _trainerRepository;

        public SendMailService(IStudentExamRepository studentExamRepository, ITrainerRepository tarnierRepository, IStudentRepository studentRepository, IOptions<EmailConfigurationDto> configuration)
        {
            _configuration = configuration;
            _studentRepository = studentRepository;
            _trainerRepository = tarnierRepository;
            _studentExamRepository = studentExamRepository;
        }

        /// <summary>
        /// E-posta'ya gönderilmek üzere 6 haneli rastgele bir sayı üretir.
        /// </summary>
        /// <returns>Gönderilen güvenlik kodu</returns>
        private int GenerateVerificationCode()
        {
            Random code = new Random();
            return code.Next(100000, 999999);
        }

        /// <summary>
        /// Gönderilecek mesajın içeriğinin oluşturulması için kullanılır.
        /// </summary>
        /// <param name="message">Gönderilecek mailin içeriği</param>
        /// <returns>Gönderilecek Mail İçeriği</returns>
        private async Task<MailMessage> CreateEmailContent(MailMessageDto message)
        {
            var emailMessage = new MailMessage();
            emailMessage.From = new MailAddress(_configuration.Value.From);
            emailMessage.To.Add(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = message.Content;

            return emailMessage;
        }

        /// <summary>
        /// Oluşturulan mesajın gönderilmesi işlemi gerçekleştirilir.
        /// </summary>
        /// <param name="message">Gönderilecek mesaj</param>
        private async Task SendMail(MailMessageDto message)
        {
            var mailMessage = await CreateEmailContent(message);

            using (var client = new SmtpClient(_configuration.Value.SmtpServer, _configuration.Value.Port))
            {
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential(_configuration.Value.From, _configuration.Value.Password);
                client.EnableSsl = true;

                client.Send(mailMessage);

            }
        }

        /// <summary>
        /// Giriş yapan kullanıcının iki adımlı doğrulamadaki güvenlik kodunun gönderilmesi için kullanılır.
        /// </summary>
        /// <param name="email">Mailin gönderileceği mail adresi</param>
        /// <returns>Gönderilen güvenlik kodu</returns>
        public async Task<int> SendEmailVerificationCode(string email)
        {
            int verificationCode = GenerateVerificationCode();
            MailMessageDto message = new MailMessageDto(email, "Bilge Adam Giriş Şifresi", $"Giriş Şifreniz {verificationCode}");

            await SendMail(message);
            return verificationCode;
        }

        /// <summary>
        /// Yeni oluşturulan öğrenci hesabının bilgilerinin gönderilmesi için kullanırlır.
        /// </summary>
        /// <param name="email">Mailin gönderileceği mail adresi</param>
        /// <param name="url">Login sayfasının url adresi</param>
        /// <returns></returns>
        public async Task SendEmailNewStudent(string email, string url)
        {
            MailMessageDto message = new MailMessageDto(email, "Bilge Adam'a Hoşgeldiniz", $"Yeni hesabınızla giriş yapmak için aşağıdaki linke tıklayabilirsiniz.\n{url} \n Giriş Bilgileriniz \nEmail : {email} \nŞifre : newPassword+0");
            await SendMail(message);
        }
        /// <summary>
        /// Yeni oluşturulan eğitmen hesabının bilgilerinin gönderilmesi için kullanırlır.
        /// </summary>
        /// <param name="email">Mailin gönderileceği mail adresi</param>
        /// <param name="url">Login sayfasının url adresi</param>
        /// <returns></returns>
        public async Task SendEmailNewTrainer(string email, string url)
        {
            MailMessageDto message = new MailMessageDto(email, "Bilge Adam'a Hoşgeldiniz", $"Yeni hesabınızla giriş yapmak için aşağıdaki linke tıklayabilirsiniz.\n{url} \n Giriş Bilgileriniz \nEmail : {email} \nŞifre : newPassword+0");
            await SendMail(message);
        }

        /// <summary>
        /// Yeni oluşturulan admin hesabının bilgilerinin gönderilmesi için kullanırlır.
        /// </summary>
        /// <param name="email">Mailin gönderileceği mail adresi</param>
        /// <param name="url">Login sayfasının url adresi</param>
        /// <returns></returns>
        public async Task SendEmailNewAdmin(string email, string url)
        {
            MailMessageDto message = new MailMessageDto(email, "Bilge Adam'a Hoşgeldiniz", $"Yeni hesabınızla giriş yapmak için aşağıdaki linke tıklayabilirsiniz.\n{url} \n Giriş Bilgileriniz \nEmail : {email} \nŞifre : newPassword+0");
            await SendMail(message);
        }

        /// <summary>
        /// Belirtilen mail adresine sınav raporu göndermek için kullanılır.
        /// </summary>
        /// <param name="email">Mailin gönderileceği mail adresi</param>
        /// <returns></returns>
        public async Task SendAfterExamMail(string email, string studentFullName, string examName, int totalTimeSpent, int? studentPoint)
        {
            TimeSpan timeElapsed = TimeSpan.FromSeconds(totalTimeSpent);
            string timeElapsedFormatted = timeElapsed.ToString(@"m\:ss");

            MailMessageDto message = new MailMessageDto(email, $"{examName} adlı öğrencinin sınavı hakkında", $"{studentFullName} isimli öğrenci {examName} sınavını {timeElapsedFormatted} sürede tamamlamıştır. Öğrenci notu: {studentPoint}");
            await SendMail(message);
        }

        /// <summary>
        /// Öğrencinin oluşturulan yeni sınava girebilmesi için gerekli bilgileri ve linki gönderir. 
        /// </summary>
        /// <param name="student">Öğrenci bilgileirini içerir</param>
        /// <param name="url">Sınav sayfasının url adresi</param>
        /// <returns></returns>

        public async Task SendEmailToStudentNewExam(string emailAdress, string url, Guid studentExamId)
        {
            MailMessageDto message = new MailMessageDto(emailAdress, "Yeni Sınavanız Oluşturuldu.", $"Sınava giriş yapmak için aşağıdaki linke tıklayabilirsiniz.\n{url}/{studentExamId}");
            await SendMail(message);
        }




        public async Task<string> GetStudentEmailById(Guid studentId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            return student?.Email;
        }



        public async Task SendEmailToTrainerNewExam(string trainerEmailAdress, List<string> studentContents)
        {

            if (studentContents != null && !string.IsNullOrEmpty(trainerEmailAdress))
            {
                var concatenatedContents = string.Join("\n", studentContents);
                var trainerMessage = new MailMessageDto(trainerEmailAdress, "Yeni Sınav Oluşturuldu", $"Aşağıdaki öğrencilere mail gönderildi:\n{concatenatedContents}");
                await SendMail(trainerMessage);
            }
            else
            {
                throw new Exception("Öğrenci bilgisi bulunamadı veya eğitmen mail adresi eksik.");
            }
        }
    }
}