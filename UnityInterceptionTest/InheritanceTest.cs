using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnityInterceptionTest
{
    [TestClass]
    public class InheritanceTest
    {
        #region utils
        public interface IDoSomething
        {
            string DoSomething();
        }

        public class BaseClass : MarshalByRefObject, IDoSomething
        {
            public virtual string DoSomething()
            {
                return "doing something from base.";
            }
        }

        public class DerivedClass : BaseClass
        {
            // NOTE note the different behavior when we do NOT override the base virtual method here
            //public override string DoSomething()
            //{
            //    return "it's in derived now: " + base.DoSomething();
            //}
        }

        public class SimpleObject
        {
            public List<string> Result { get; set; }
        }
        #endregion

        private IUnityContainer InitializeContainer<TInterceptor>()
            where TInterceptor : IInterceptor
        {
            var container = new UnityContainer();
            container.AddNewExtension<Interception>();

            //container.RegisterType<IDoSomething, BaseClass>("base");
            //container.RegisterType<IDoSomething, DerivedClass>("derived");
            container.RegisterInstance(typeof(SimpleObject), "result", new SimpleObject { Result = new List<string>() }, new ContainerControlledLifetimeManager());

            IInterceptionBehavior behavior = new DelegateInterceptionBehavior((mi, next) =>
            {
                var result = container.Resolve<SimpleObject>("result");
                var runMethodResult = next()(mi, next);
                result.Result.Add(string.Format("invoked by {0}:{1} at {2}", mi.MethodBase.DeclaringType, mi.MethodBase.Name, DateTime.Now.ToLongTimeString()));
                return runMethodResult;
            });

            if (typeof(TInterceptor) == typeof(InterfaceInterceptor))
            {
                container.RegisterType<IDoSomething, BaseClass>(
                    "base",
                    new Interceptor<TInterceptor>(),
                    new InterceptionBehavior(behavior));
                container.RegisterType<IDoSomething, DerivedClass>(
                    "derived",
                    new Interceptor<TInterceptor>(),
                    new InterceptionBehavior(behavior));
            }
            else
            {
                container.RegisterType<BaseClass>(
                    new Interceptor<TInterceptor>(),
                    new InterceptionBehavior(behavior));
                container.RegisterType<BaseClass, DerivedClass>(
                    "derived",
                    new Interceptor<TInterceptor>(),
                    new InterceptionBehavior(behavior));
            }

            return container;
        }

        #region TransparentProxyInterceptor

        [TestMethod]
        public void CanInterceptOnBaseClass4Proxy()
        {
            var container = InitializeContainer<TransparentProxyInterceptor>();

            var derived = container.Resolve<BaseClass>();
            Assert.IsNotNull(derived);
            Assert.IsInstanceOfType(derived, typeof(BaseClass));

            var str = derived.DoSomething();
            Assert.AreEqual("doing something from base.", str);

            var result = container.Resolve<SimpleObject>("result");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.Count > 0);

            //container.Dispose();
        }

        [TestMethod]
        public void CanInterceptOnDerivedClass4Proxy()
        {
            var container = InitializeContainer<TransparentProxyInterceptor>();

            var derived = container.Resolve<BaseClass>("derived");
            Assert.IsNotNull(derived);
            Assert.IsInstanceOfType(derived, typeof(DerivedClass));

            var str = derived.DoSomething();
            Assert.AreNotEqual("doing something from base.", str);

            var result = container.Resolve<SimpleObject>("result");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.Count > 0);

            //container.Dispose();
        }

        #endregion

        #region InterfaceInterception
        [TestMethod]
        public void CanInterceptOnBaseClass4Interface()
        {
            var container = InitializeContainer<InterfaceInterceptor>();

            var @base = container.Resolve<IDoSomething>("base");
            Assert.IsNotNull(@base);
            //Assert.IsInstanceOfType(@base, typeof(BaseClass));

            var str = @base.DoSomething();
            Assert.AreEqual("doing something from base.", str);

            var result = container.Resolve<SimpleObject>("result");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.Count > 0);

            //container.Dispose();
        }

        [TestMethod]
        public void CanInterceptOnDerivedClass4Interface()
        {
            var container = InitializeContainer<InterfaceInterceptor>();

            var derived = container.Resolve<IDoSomething>("derived");
            Assert.IsNotNull(derived);
            //Assert.IsInstanceOfType(derived, typeof(DerivedClass));

            var str = derived.DoSomething();
            Assert.AreEqual("doing something from base.", str);

            var result = container.Resolve<SimpleObject>("result");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.Count > 0);

            //container.Dispose();
        }

        #endregion

        #region VirtualMethodInterception

        [TestMethod]
        public void CanInterceptOnBaseClass4VirtualMethod()
        {
            var container = InitializeContainer<VirtualMethodInterceptor>();

            var @base = container.Resolve<BaseClass>();
            Assert.IsNotNull(@base);
            //Assert.IsInstanceOfType(@base, typeof(BaseClass));

            var str = @base.DoSomething();
            Assert.AreEqual("doing something from base.", str);

            var result = container.Resolve<SimpleObject>("result");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.Count > 0);

            //container.Dispose();
        }

        [TestMethod]
        public void CanInterceptOnDerivedClass4VirtualMethod()
        {
            var container = InitializeContainer<VirtualMethodInterceptor>();

            var derived = container.Resolve<BaseClass>("derived");
            Assert.IsNotNull(derived);
            //Assert.IsInstanceOfType(derived, typeof(DerivedClass));

            var str = derived.DoSomething();
            Assert.AreEqual("doing something from base.", str);

            var result = container.Resolve<SimpleObject>("result");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.Count > 0);

            //container.Dispose();
        }

        #endregion
    }
}
