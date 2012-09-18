using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnityInterceptionTest
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

    [TestClass]
    public class InheritanceTest
    {

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

        #region PolicyInjectionBehavior

        private IUnityContainer InitializeContainer4Policy()
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();

            var simpleObject = new SimpleObject { Result = new List<string>() };
            container.RegisterInstance<SimpleObject>("result", simpleObject, new ContainerControlledLifetimeManager());

            container.RegisterType<BaseClass>(
                    new Interceptor<VirtualMethodInterceptor>(),
                    new InterceptionBehavior<PolicyInjectionBehavior>()
                );

            container.RegisterType<BaseClass, DerivedClass>(
                    "derived",
                    new Interceptor<VirtualMethodInterceptor>(),
                    new InterceptionBehavior<PolicyInjectionBehavior>()
                );

            //var interception = container.Configure<Interception>();
            //var policy = interception.AddPolicy("policy4Base");

            //policy.AddMatchingRule<TypeMatchingRule>(new InjectionConstructor(
            //                                             new InjectionParameter(typeof(BaseClass))));

            //policy.AddCallHandler<TestCallHandler>(
            //            new ContainerControlledLifetimeManager(),
            //            new InjectionConstructor(
            //                simpleObject));

            // NOTE i think policy is based on type, one policy per type
            container.Configure<Interception>()
                .AddPolicy("policy4Base")
                    .AddMatchingRule<TypeMatchingRule>(
                        new InjectionConstructor(
                            new InjectionParameter(typeof(BaseClass))),                                                                       
                        new InjectionConstructor(
                            new InjectionParameter(typeof(DerivedClass))))
                    //.AddMatchingRule<TypeMatchingRule>(
                    //    new InjectionConstructor(
                    //        new InjectionParameter(typeof(DerivedClass))))
                    .AddCallHandler<TestCallHandler>(
                        new ContainerControlledLifetimeManager(),
                        new InjectionConstructor(
                            simpleObject));

            //container.Configure<Interception>()
            //    .AddPolicy("policy4Derived")
            //        .AddMatchingRule<TypeMatchingRule>(
            //            new InjectionConstructor(
            //                new InjectionParameter(typeof(DerivedClass))))
            //        .AddCallHandler<TestCallHandler>(
            //            new ContainerControlledLifetimeManager(),
            //            new InjectionConstructor(
            //                simpleObject));

            return container;
        }
        [TestMethod]
        public void CanInterceptOnBaseClass4Policy()
        {
            var container = InitializeContainer4Policy();

            var @base = container.Resolve<BaseClass>();
            Assert.IsNotNull(@base);

            var result = container.Resolve<SimpleObject>("result");

            var str = @base.DoSomething();
            Assert.AreEqual("doing something from base.", str);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.Count > 0);
        }

        [TestMethod]
        public void CanInterceptOnDerivedClass4Policy()
        {
            var container = InitializeContainer4Policy();

            var derived = container.Resolve<BaseClass>("derived");
            Assert.IsNotNull(derived);

            var str = derived.DoSomething();
            //Assert.AreNotEqual("doing something from base.", str);

            var result = container.Resolve<SimpleObject>("result");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result.Count > 0);
        }

        #endregion
    }

    public class TestCallHandler : ICallHandler
    {
        private SimpleObject _simpleObject;
        public TestCallHandler(SimpleObject so)
        {
            this._simpleObject = so;
        }
        public IMethodReturn Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
        {
            var runMethodResult = getNext()(input, getNext);
            _simpleObject.Result.Add(string.Format("invoked by {0}:{1} at {2}", input.MethodBase.DeclaringType, input.MethodBase.Name, DateTime.Now.ToLongTimeString()));
            return runMethodResult;
        }

        public int Order { get; set; }
    }
}
