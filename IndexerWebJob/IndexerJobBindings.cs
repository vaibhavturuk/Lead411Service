using Ninject;
using Ninject.Modules;
using ServiceLayer.Interfaces;
using ServiceLayer.Services;
using RepositoryLayer.Repositories.Interfaces;
using RepositoryLayer.Repositories;
namespace IndexerWebJob
{
    public class IndexerJobBindings : NinjectModule
    {
        public override void Load()
        {
            Bind<ICommonService>().To<CommonService>();
            Bind<IAccountRespsitory>().To<AccountRepository>();

            Bind<IAuthentication>().To<GoogleAuthentication>().WhenInjectedExactlyInto<GoogleService>();

            Bind<IMailProcess>().To<GoogleMailProcess>().WhenInjectedExactlyInto<GoogleService>();

            Bind<IOperation>().To<GoogleOperation>().WhenInjectedExactlyInto<GoogleService>();

            Bind<IOperation>().To<GoogleOperation>().WhenInjectedExactlyInto<GoogleMailProcess>();

            Bind<IService>().To<GoogleService>().WhenInjectedExactlyInto<GoogleService>();
        }
    }
}
