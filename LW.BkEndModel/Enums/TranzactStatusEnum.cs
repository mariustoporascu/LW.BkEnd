using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndModel.Enums
{
    public enum TranzactStatusEnum
    {
        NoStatus = 0,
        Transfered = 1,
        FailedTransfer = 2,
        Withdrawed = 3,
        FailedWithdraw = 4,
        WaitingForPayment = 5,
        PaymentGenerated = 6,
    }
}
