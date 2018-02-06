using Ninject;
using Ninject.Modules;
using RepositoryLayer.Repositories;
using RepositoryLayer.Repositories.Interfaces;
using ServiceLayer.Interfaces;
using ServiceLayer.Interfaces.AdminPanel;
using ServiceLayer.Services;
using ServiceLayer.Services.AdminPanel;

namespace TestCases
{
    public class NinjectModel : NinjectModule
    {
        public override void Load()
        {
            Bind<Lead411.Controllers.AccountApiController>().ToSelf();

            Bind<Lead411.Controllers.AdminApiController>().ToSelf();

            Bind<ServiceLayer.Interfaces.IGoogleService>().To<ServiceLayer.Services.GoogleService>();

            Bind<ServiceLayer.Interfaces.ICommonService>().To<ServiceLayer.Services.CommonService>();
            Bind<IUserService>().To<UserService>();

            Bind<RepositoryLayer.Repositories.Interfaces.IAccountRespsitory>().To<RepositoryLayer.Repositories.AccountRepository>();
            Bind<IUserRepository>().To<UserRepository>();

            Bind<IAuthentication>().To<GoogleAuthentication>().WhenInjectedExactlyInto<GoogleService>();

            Bind<IMailProcess>().To<GoogleMailProcess>().WhenInjectedExactlyInto<GoogleService>();

            Bind<IOperation>().To<GoogleOperation>().WhenInjectedExactlyInto<GoogleService>();

            Bind<IOperation>().To<GoogleOperation>().WhenInjectedExactlyInto<GoogleMailProcess>();

            Bind<IService>().To<GoogleService>().WhenInjectedExactlyInto<GoogleService>();

            Bind<IGoogleService>().To<GoogleService>();
        }
    }
}
