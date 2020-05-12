using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FiveWonders.WebUI.Startup))]
namespace FiveWonders.WebUI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
