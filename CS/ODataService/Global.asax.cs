using System;
using System.Collections.Concurrent;
using System.Web.Http;
using System.Web.Http.Validation;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using ODataService.Helpers;

namespace WebApplication1
{
    public class WebApiApplication : System.Web.HttpApplication {
        protected void Application_Start() {
            GlobalConfiguration.Configuration.Services.Replace(typeof(IBodyModelValidator), new CustomBodyModelValidator());
            GlobalConfiguration.Configure(WebApiConfig.Register);
            ConnectionHelper.EnsureDatabaseCreated();
            XpoDefault.DataLayer = ConnectionHelper.CreateDataLayer(AutoCreateOption.SchemaAlreadyExists, true);
        }

        public class CustomBodyModelValidator : DefaultBodyModelValidator {
            readonly ConcurrentDictionary<Type, bool> persistentTypes = new ConcurrentDictionary<Type, bool>();
            public override bool ShouldValidateType(Type type) {
                return persistentTypes.GetOrAdd(type, t => !typeof(IXPSimpleObject).IsAssignableFrom(t));
            }
        }
    }
}
