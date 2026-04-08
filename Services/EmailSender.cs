using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

public class EmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
{
    try 
    {
        var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential("bao1310zzz@gmail.com", "hfnaivracsmqujsv")
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("bao1310zzz@gmail.com", "Ăn Vặt Foodie"),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);

        await client.SendMailAsync(mailMessage);
        Console.WriteLine("====> MAIL ĐÃ GỬI THÀNH CÔNG ĐẾN: " + email);
    }
    catch (Exception ex)
    {
        Console.WriteLine("====> LỖI GỬI MAIL: " + ex.Message);
    }
}
}