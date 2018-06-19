using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Validation;
using System.Web.ModelBinding;
using System.Web.Routing;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using ODataService.Helpers;
using WebApplication1.Models;

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
            readonly HashSet<Type> persistentTypes;
            public CustomBodyModelValidator() {
                persistentTypes = new HashSet<Type>(ConnectionHelper.GetPersistentTypes());
            }
            public override bool ShouldValidateType(Type type) {
                return !persistentTypes.Contains(type);
            }
        }
    }
}
