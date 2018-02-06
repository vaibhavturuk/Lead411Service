using Ninject.Modules;
using RepositoryLayer.Repositories;
using RepositoryLayer.Repositories.Interfaces;
using ServiceLayer.Interfaces;
using ServiceLayer.Services;

namespace ServiceLayer
{
    public class DependencyResolver : NinjectModule
    {
        public override void Load()
        {
            Bind<IAccountRespsitory>().To<AccountRepository>();
            Bind<IUserRepository>().To<UserRepository>();

            Bind<IAuthentication>().To<GoogleAuthentication>().WhenInjectedExactlyInto<GoogleService>();
            
            Bind<IMailProcess>().To<GoogleMailProcess>().WhenInjectedExactlyInto<GoogleService>();
           
            Bind<IOperation>().To<GoogleOperation>().WhenInjectedExactlyInto<GoogleService>();

            Bind<IOperation>().To<GoogleOperation>().WhenInjectedExactlyInto<GoogleMailProcess>();

            Bind<IGoogleService>().To<GoogleService>();
        }
    }
}