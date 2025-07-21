using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SetLight.UI.Startup))]
namespace SetLight.UI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
