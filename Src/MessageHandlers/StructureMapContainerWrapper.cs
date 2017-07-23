//using System;
//using EasyNetQ;
//using StructureMap;
//using IContainer = EasyNetQ.IContainer;
//using IServiceProvider = EasyNetQ.IServiceProvider;

//namespace HansJuergenWeb.MessageHandlers
//{
//    public class StructureMapContainerWrapper : IContainer, IDisposable
//    {
//        private readonly Container _structuremapContainer;

//        public StructureMapContainerWrapper(Container structuremapContainer)
//        {
//            _structuremapContainer = structuremapContainer;
//        }

//        public TService Resolve<TService>() where TService : class
//        {
//            return _structuremapContainer.GetInstance<TService>();
//        }

//        public IServiceRegister Register<TService>(Func<IServiceProvider, TService> serviceCreator)
//            where TService : class
//        {
//            _structuremapContainer.Configure(x => x.For<TService>().Use(() => serviceCreator(this)).Singleton()
//            );
//            return this;
//        }

//        public IServiceRegister Register<TService, TImplementation>()
//            where TService : class
//            where TImplementation : class, TService
//        {
//            _structuremapContainer.Configure(x => x.For<TService>().Use<TImplementation>().Singleton()
//            );
//            return this;
//        }

//        public void Dispose()
//        {
//            _structuremapContainer.Dispose();
//        }
//    }
//}