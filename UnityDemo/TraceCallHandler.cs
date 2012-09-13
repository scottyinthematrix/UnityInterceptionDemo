using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.Practices.Unity.Utility;

namespace UnityDemo
{
    public class TraceCallHandler : ICallHandler, IDisposable
    {
        private TraceSource source;

        public TraceCallHandler(TraceSource source)
        {
            Guard.ArgumentNotNull(source, "source");
            this.source = source;
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
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

            if (input.Inputs != null && input.Inputs.Count > 0)
            {
                for (var i = 0; i < input.Inputs.Count; i++)
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

        private int order;
        public int Order
        {
            get
            {
                return order;
            }
            set
            {
                order = value;
            }
        }

        public void Dispose()
        {
            this.source.Close();
        }

    }

    class TraceCallHandlerAttribute : HandlerAttribute
    {
        private string sourceName;

        public TraceCallHandlerAttribute(string sourceName)
        {
            Guard.ArgumentNotNull(sourceName, "sourceName");
            this.sourceName = sourceName;
        }

        public override ICallHandler CreateHandler(IUnityContainer container)
        {
            return new TraceCallHandler(new TraceSource(this.sourceName));
        }
    }
}
