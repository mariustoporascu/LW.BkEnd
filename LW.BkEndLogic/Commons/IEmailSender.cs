using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndLogic.Commons
{
	public interface IEmailSender
	{
		bool SendEmail(string[] emailTo, string subject, string body);
	}
	public class EmailSender : IEmailSender
	{
		private readonly IConfiguration _configuration;
		public EmailSender(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		public bool SendEmail(string[] emailTo, string subject, string body)
		{
			MailMessage mailMessage = new MailMessage();
			mailMessage.From = new MailAddress(_configuration["Smtp:Email"]);
			mailMessage.To.Add(new MailAddress(emailTo[0]));
			if (emailTo.Length > 1)
				for (int i = 1; i < emailTo.Length; i++)
					mailMessage.Bcc.Add(new MailAddress(emailTo[i]));

			mailMessage.Subject = $"{_configuration["Brand"]} - {subject}";
			mailMessage.IsBodyHtml = true;
			mailMessage.Body = body;

			var client = ConfigureClient();

			try
			{
				client.Send(mailMessage);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}
		private SmtpClient ConfigureClient()
		{
			SmtpClient client = new SmtpClient();
			client.Credentials = new System.Net.NetworkCredential(_configuration["Smtp:Email"], _configuration["Smtp:Pass"]);
			client.Host = _configuration["Smtp:Host"];
			client.Port = int.Parse(_configuration["Smtp:Port"]);
			client.EnableSsl = true;
			client.Timeout = 30000;
			client.UseDefaultCredentials = false;
			return client;
		}
	}
}
