using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.Practices.Unity.Utility;

namespace UnityDemo
{
    public class AccessCheckCallHandler : ICallHandler
    {
        private string[] allowedRoles;

        public AccessCheckCallHandler(params string[] allowedRoles)
        {
            Guard.ArgumentNotNull(allowedRoles, "allowedRules");
            this.allowedRoles = allowedRoles;
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
        {
            if (this.allowedRoles.Length > 0)
            {
                IPrincipal currentPrincipal = Thread.CurrentPrincipal;

                if (currentPrincipal != null)
                {
                    bool allowed = allowedRoles.Any(currentPrincipal.IsInRole);
                    if (!allowed)
                    {
                        // short circuit the call
                        return input.CreateExceptionMethodReturn(
                            new UnauthorizedAccessException(
                                "User NOT allowed to invoke the method."));
                    }
                }
            }

            return getNext()(input, getNext);
        }

        public int Order { get; set; }
    }
}
