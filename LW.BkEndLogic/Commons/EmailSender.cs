using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using LW.BkEndLogic.Commons.Interfaces;
using Azure.Communication.Email;
using Azure;
using Microsoft.Extensions.Logging;

namespace LW.BkEndLogic.Commons
{
	public class EmailSender : IEmailSender
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<EmailSender> _logger;
		public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
		{
			_configuration = configuration;
			_logger = logger;
		}
		public bool SendEmail(string[] emailTo, string subject, string body)
		{
			EmailClient emailClient = new EmailClient(_configuration["AzureCommServ:ConnString"]);
			var emailContent = new EmailContent(subject)
			{
				Html = body,
			};
			var toRecipients = new List<EmailAddress>();
			foreach (var email in emailTo)
			{
				toRecipients.Add(new EmailAddress(email));
			}
			EmailRecipients emailRecipients = new EmailRecipients(toRecipients);

			var emailMessage = new EmailMessage(_configuration["AzureCommServ:Sender"], emailRecipients, emailContent);

			try
			{
				EmailSendOperation emailSendOperation = emailClient.Send(WaitUntil.Completed, emailMessage);
				_logger.LogInformation($"Email Sent. Status = {emailSendOperation.Value.Status}");

				/// Get the OperationId so that it can be used for tracking the message for troubleshooting
				string operationId = emailSendOperation.Id;
				_logger.LogInformation($"Email operation id = {operationId}");
				return true;
			}
			catch (RequestFailedException ex)
			{
				/// OperationID is contained in the exception message and can be used for troubleshooting purposes
				_logger.LogWarning($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
				return false;
			}

		}

	}
}
