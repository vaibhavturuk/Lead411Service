using Ninject;
using Ninject.Modules;

namespace ControllerWebJob
{
    public class ControllerJobBindings : NinjectModule
    {
        public override void Load()
        {
            Bind<ServiceLayer.Interfaces.ICommonService>().To<ServiceLayer.Services.CommonService>();
            Bind<ServiceLayer.Interfaces.IGoogleService>().To<ServiceLayer.Services.GoogleService>();
            Bind<ServiceLayer.Interfaces.IAuthentication>().To<ServiceLayer.Services.GoogleAuthentication>();
            Bind<ServiceLayer.Interfaces.IMailProcess>().To<ServiceLayer.Services.GoogleMailProcess>();
            Bind<ServiceLayer.Interfaces.IOperation>().To<ServiceLayer.Services.GoogleOperation>();
            Bind<RepositoryLayer.Repositories.Interfaces.IAccountRespsitory>().To<RepositoryLayer.Repositories.AccountRepository>();
        }
    }
}