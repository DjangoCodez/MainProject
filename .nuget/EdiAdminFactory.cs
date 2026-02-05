using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using SoftOne.EdiAdmin.Business.Interfaces;
using SoftOne.EdiAdmin.Business.Util.MessageParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business
{
    public class EdiAdminFactory
    {
        private static EdiAdminFactory instance;
        public static EdiAdminFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EdiAdminFactory();
                }
                
                return instance;
            }
        }
        private IWindsorContainer container;
        //static EdiAdminFactory()
        //{
        //    //application starts...
        //    var container = new WindsorContainer();
        //    // adds and configures all components using WindsorInstallers from executing assembly
        //    container.Install(new RepositoriesInstaller());
 
        //    // clean up, application exits
        //    //container.Dispose();
        //}

        private EdiAdminFactory()
        {
            this.container = new WindsorContainer();
            this.container.Install(new RepositoriesInstaller());
        }

        public T GetEdiAdminManager<T>() where T : IEdiAdminManager
        {
             var manager = container.Resolve<T>();

            return manager;
        }
    }

    public class RepositoriesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<EdiAdminManager>().LifestyleTransient());
            container.Register(Component.For<EdiAdminFolderWatcherManager>().LifestyleSingleton());
            container.Register(Component.For<IMessageParser>().ImplementedBy<MessageParser>().LifestyleTransient());
            
            

            //container.Register(Classes.FromThisAssembly()
            //                .Where(Component.IsInSameNamespaceAs<MessageParser>())
            //                .WithService.DefaultInterfaces()
            //                .LifestyleTransient());

            //container.Register(Component.For<IEdiAdminManager>().LifestyleSingleton());
            //Classes.FromThisAssembly()
            //                .Where(Component.IsInSameNamespaceAs<EdiAdminManager>())
            //                .WithService.DefaultInterfaces()
            //                .LifestyleSingleton());

            //container.Register(Component.For<EdiAdminFolderWatcherManager>().LifestyleSingleton());
        }
    }
}
