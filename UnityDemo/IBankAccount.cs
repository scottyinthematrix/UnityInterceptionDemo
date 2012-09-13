using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityDemo
{
    public interface IBankAccount
    {
        void Deposit(decimal depositAmount);
        decimal GetCurrentBalance();
        void Withdraw(decimal withdrawAmount);

    }
}
