using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndModel
{
	public enum StatusEnum
	{
		NoStatus = 0,
		Approved = 1,
		Rejected = 2,
		WaitingForApproval = 3,
		Processing = 4,
		CompletedProcessing = 5,
		FailedProcessing = 6,
	}
}
