using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using WebAPIPost.Common;

namespace WebAPIPost
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configuration.ParameterBindingRules.Insert(0, ArgumentsParameterBinding.HookupParameterBinding);

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
