using LW.BkEndModel.Enums;
using LW.BkEndModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using LW.BkEndLogic.FirmaDiscUser;

namespace LW.BkEndTests
{
	public class IDbRepoFirmaTests
	{
		private readonly Mock<IDbRepoFirma> mockRepo;
		private readonly Guid testGuid = Guid.NewGuid();

		public IDbRepoFirmaTests()
		{
			mockRepo = new Mock<IDbRepoFirma>();
		}

		[Fact]
		public void GetAllDocumenteWFP_ReturnsExpectedResult()
		{
			var expected = new List<Documente>();
			mockRepo.Setup(repo => repo.GetAllDocumenteWFP(testGuid))
				.Returns(expected);
			var result = mockRepo.Object.GetAllDocumenteWFP(testGuid);
			Assert.Equal(expected, result);
		}

		[Fact]
		public void GetDocument_ReturnsExpectedResult()
		{
			var expected = new Documente();
			mockRepo.Setup(repo => repo.GetDocument(testGuid))
				.Returns(expected);
			var result = mockRepo.Object.GetDocument(testGuid);
			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task UpdateDocStatusAsync_ReturnsExpectedResult()
		{
			var expected = true;
			var documente = new Documente();
			var status = new StatusEnum();
			mockRepo.Setup(repo => repo.UpdateDocStatusAsync(documente, status))
				.ReturnsAsync(expected);
			var result = await mockRepo.Object.UpdateDocStatusAsync(documente, status);
			Assert.Equal(expected, result);
		}

		[Fact]
		public void GetDashboardInfo_ReturnsExpectedResult()
		{
			var expected = new object();
			mockRepo.Setup(repo => repo.GetDashboardInfo(testGuid))
				.Returns(expected);
			var result = mockRepo.Object.GetDashboardInfo(testGuid);
			Assert.Equal(expected, result);
		}

		[Fact]
		public void GetFirmaDiscountId_ReturnsExpectedResult()
		{
			var expected = Guid.NewGuid();
			mockRepo.Setup(repo => repo.GetFirmaDiscountId(testGuid))
				.Returns(expected);
			var result = mockRepo.Object.GetFirmaDiscountId(testGuid);
			Assert.Equal(expected, result);
		}

		[Fact]
		public void GetAllHybrids_ReturnsExpectedResult()
		{
			var expected = new List<Hybrid>();
			mockRepo.Setup(repo => repo.GetAllHybrids(testGuid))
				.Returns(expected);
			var result = mockRepo.Object.GetAllHybrids(testGuid);
			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task DeleteHybrid_ReturnsExpectedResult()
		{
			var expected = true;
			var firmaDiscountId = Guid.NewGuid();
			var groupId = Guid.NewGuid();
			mockRepo.Setup(repo => repo.DeleteHybrid(firmaDiscountId, groupId))
				.ReturnsAsync(expected);
			var result = await mockRepo.Object.DeleteHybrid(firmaDiscountId, groupId);
			Assert.Equal(expected, result);
		}
	}
}
