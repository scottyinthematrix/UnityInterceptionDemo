using System;

namespace UnityDemo.BusinessLogic
{
    public /*sealed*/ class BankAccount /*: MarshalByRefObject*/ : IBankAccount
    {
        private decimal balance;

        //[TraceCallHandler("interception")]
        public virtual decimal GetCurrentBalance()
        {
            return balance;
        }

        //[TraceCallHandler("interception")]
        public virtual void Deposit(decimal depositAmount)
        {
            balance += depositAmount;
        }

        //[TraceCallHandler("interception")]
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
