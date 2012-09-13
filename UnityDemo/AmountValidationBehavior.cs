using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace UnityDemo
{
    public class AmountValidationBehavior : IInterceptionBehavior
    {
        private decimal maxAmount;

        public AmountValidationBehavior(decimal maxAmount)
        {
            this.maxAmount = maxAmount;
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            if (input.Inputs.Count > 0)
            {
                foreach (var inputValue in input.Inputs)
                {
                    if (inputValue is Decimal)
                    {
                        if ((Decimal)inputValue > maxAmount)
                        {
                            MessageBox.Show(
                              string.Format("Amount of {0} is beyond max limit of {1}",
                                inputValue, maxAmount),
                              "Limit Exceeded");
                            return input.CreateExceptionMethodReturn(
                              new InvalidOperationException("Limit Exceeded"));
                        }
                    }
                }
            }
            return getNext()(input, getNext);
        }

        public bool WillExecute
        {
            get { return true; }
        }
    }
}
