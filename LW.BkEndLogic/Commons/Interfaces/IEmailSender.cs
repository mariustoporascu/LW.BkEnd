namespace LW.BkEndLogic.Commons.Interfaces
{
	public interface IEmailSender
	{
		bool SendEmail(string[] emailTo, string subject, string body);
	}

}
