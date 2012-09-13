using System;

namespace UnityDemo.BusinessLogic
{
    public class BankAccount : IBankAccount
    {
        private decimal balance;

        [TraceCallHandler("interception")]
        public decimal GetCurrentBalance()
        {
            return balance;
        }

        [TraceCallHandler("interception")]
        public void Deposit(decimal depositAmount)
        {
            balance += depositAmount;
        }

        [TraceCallHandler("interception")]
        public void Withdraw(decimal withdrawAmount)
        {
            if (withdrawAmount > balance)
            {
                throw new ArithmeticException();
            }
            balance -= withdrawAmount;
        }
    }
}
