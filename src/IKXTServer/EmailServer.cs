using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace IKXTServer
{
    public class EmailServer
    {
        private SmtpClient Smtp;
        private string FromEmail;

        public EmailServer
            (
            string server_email = EmailSenderID,
            string server_host = EmailSenderSV,
            int server_port = EmailSenderPT,
            string server_pw = EmailSenderPW
            )
        {
            FromEmail = server_email;
            Smtp = new SmtpClient
            {
                Host = server_host,
                Port = server_port,
                Timeout = 1000000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(server_email, server_pw)
            };
        }

        public bool Send
            (
            string to,
            string subject,
            string body
            )
        {
            try
            {
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(FromEmail),
                    IsBodyHtml = true,
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8,
                    Subject = subject,
                    Body = body
                };
                mail.To.Add(new MailAddress(to));
                mail.Headers.Add("KXT", "安全管理");

                Smtp.Send(mail);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        private const string EmailSenderID = "3341118159@qq.com";
        private const string EmailSenderSV = "smtp.qq.com";
        private const int EmailSenderPT = 587;
        private const string EmailSenderPW = "tyzxrtuqasuudbed";
    }
}
