using FluentAssertions;
using LW.BkEndLogic.Commons;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace LW.BkEndTests
{
	public class TestMail
	{

		[Fact]
		public void CheckRandomCuiResult()
		{
			var result = new EmailSender(new ConfigurationBuilder().AddJsonFile("appsettings.Production.json").Build())
				.SendEmail(new string[] { "mariustoporascu@topodvlp.ro" },
				"Welcome to Azure Communication Service Email APIs.",
				"<html><body><h1>Quick send email test</h1><br/><h4>This email message is sent from Azure Communication Service Email.</h4><p>This mail was sent using .NET SDK!!</p></body></html>");
			result.Should().BeTrue();
		}
	}
}
