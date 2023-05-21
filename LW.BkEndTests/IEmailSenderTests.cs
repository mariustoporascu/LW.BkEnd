using FluentAssertions;
using LW.BkEndLogic.Commons;
using LW.BkEndLogic.Commons.Interfaces;
using LW.BkEndLogic.FirmaDiscUser;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace LW.BkEndTests
{
	public class IEmailSenderTests
	{
		private readonly Mock<IEmailSender> _emailSenderMock;

		public IEmailSenderTests()
		{
			_emailSenderMock = new Mock<IEmailSender>();
		}

		[Fact]
		public void Test_SendEmail_Returns_True()
		{
			// Arrange
			string[] emailTo = new string[] { "mariustoporascu@topodvlp.ro" };
			string subject = "Test email";
			string body = "This is a test email.";
			_emailSenderMock.Setup(es => es.SendEmail(emailTo, subject, body)).Returns(true);

			// Act
			bool result = _emailSenderMock.Object.SendEmail(emailTo, subject, body);

			// Assert
			Assert.True(result);
			_emailSenderMock.Verify(es => es.SendEmail(emailTo, subject, body), Times.Once);
			_emailSenderMock.VerifyNoOtherCalls();
		}

		[Theory]
		[InlineData(null, "Test email", "This is a test email.")]
		[InlineData(new string[] { }, "Test email", "This is a test email.")]
		[InlineData(new string[] { "test@example.com" }, null, "This is a test email.")]
		[InlineData(new string[] { "test@example.com" }, "Test email", null)]
		public void Test_SendEmail_Throws_Exception(string[] emailTo, string subject, string body)
		{
			// Arrange
			_emailSenderMock.Setup(es => es.SendEmail(emailTo, subject, body)).Throws<ArgumentNullException>();

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => _emailSenderMock.Object.SendEmail(emailTo, subject, body));
			_emailSenderMock.Verify(es => es.SendEmail(emailTo, subject, body), Times.Once);
			_emailSenderMock.VerifyNoOtherCalls();
		}
	}
}
