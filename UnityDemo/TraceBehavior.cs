using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace UnityDemo
{
    class TraceBehavior : IInterceptionBehavior, IDisposable
    {
        private TraceSource source;

        public TraceBehavior(TraceSource source)
        {
            if (source == null) throw new ArgumentNullException("source");
            this.source = source;
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            this.source.TraceInformation(
                "Invoking {0}, with input: {1}",
                input.MethodBase.ToString(), GetMethodParamInfos(input));

            IMethodReturn methodReturn = getNext().Invoke(input, getNext);

            if (methodReturn.Exception == null)
            {
                this.source.TraceInformation(
                    "Successfully finished {0}",
                    input.MethodBase.ToString());
            }
            else
            {
                this.source.TraceInformation(
                    "Finished {0} with exception {1}: {2}",
                    input.MethodBase.ToString(),
                    methodReturn.Exception.GetType().Name,
                    methodReturn.Exception.Message);
            }

            this.source.Flush();

            return methodReturn;
        }

        private string GetMethodParamInfos(IMethodInvocation input)
        {
            var sb = new StringBuilder();

            if(input.Inputs!=null && input.Inputs.Count>0)
            {
                for(var i=0;i<input.Inputs.Count;i++)
                {
                    sb
                        .Append("parameterName:")
                        .Append(input.Inputs.ParameterName(i))
                        .Append(", parameterValue:")
                        .Append(input.Inputs[i]).AppendLine();
                }
            }

            return sb.ToString();
        }

        public bool WillExecute
        {
            get { return true; }
        }

        public void Dispose()
        {
            this.source.Close();
        }
    }
}
