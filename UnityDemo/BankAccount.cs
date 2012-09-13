using System;

namespace UnityDemo.BusinessLogic
{
    public class BankAccount
    {
        private decimal balance;

        public virtual decimal GetCurrentBalance()
        {
            return balance;
        }

        public virtual void Deposit(decimal depositAmount)
        {
            balance += depositAmount;
        }

        public virtual void Withdraw(decimal withdrawAmount)
        {
            if (withdrawAmount > balance)
            {
                throw new ArithmeticException();
            }
            balance -= withdrawAmount;
        }
    }
}
