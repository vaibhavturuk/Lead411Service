//using Microsoft.Owin;
using Owin;

//[assembly: OwinStartupAttribute(typeof(Lead411.Startup))]
namespace Lead411
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
