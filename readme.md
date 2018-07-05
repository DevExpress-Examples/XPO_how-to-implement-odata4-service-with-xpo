How to implement OData4 service with XPO
========================================

This example describes how to implement an OData4 service with XPO. This example is an ASP.NET MVC 5 Web API project and provides simple REST API for data access.

Steps to implement:

1. Create a new **ASP.NET Web Application** project and select the **Web API** project template (refer to the **Create the Visual Studio Project** section in [this example](https://docs.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/odata-v4/create-an-odata-v4-endpoint) for details.
2. Install the following nuget packages:
	* DevExpress.Xpo
	* Microsoft.AspNet.OData
3. Define your data model - implement persistent classes and initialize the data layer. If you are new to XPO, refer to the following articles to learn how to do this: [Create Persistent Class](https://docs.devexpress.com/CoreLibraries/2256/devexpress-orm-tool/getting-started/tutorial-1-your-first-data-aware-application-with-xpo), [Map to Existing Tables](https://docs.devexpress.com/CoreLibraries/3264/devexpress-orm-tool/concepts/basics-of-creating-persistent-objects-for-existing-data-tables).
4. Copy files from the **CS\OdataService\Helpers** folder in this example to your project.
5. Modify the `Application_Start()` method declared in the *Global.asax* file: register the model body validator class and initialize the [Data Access Layer](https://docs.devexpress.com/CoreLibraries/2121/devexpress-orm-tool/feature-center/connecting-to-a-data-store/data-access-layer).

	```cs
	protected void Application_Start() {
		GlobalConfiguration.Configuration.Services.Replace(typeof(IBodyModelValidator), new CustomBodyModelValidator());
		GlobalConfiguration.Configure(WebApiConfig.Register);
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
	```

6. Modify the *WebApiConfig.cs* file: create an ODataModelBuilder instance and register an EntitySet for each persistent class:

	```cs
	public static void Register(HttpConfiguration config) {
		config.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
		ODataModelBuilder modelBuilder = CreateODataModelBuilder();

		ODataBatchHandler batchHandler =
			new DefaultODataBatchHandler(GlobalConfiguration.DefaultServer);

		config.MapODataServiceRoute(
			routeName: "ODataRoute",
			routePrefix: null,
			model: modelBuilder.GetEdmModel(),
			batchHandler: batchHandler);
	}

	static ODataModelBuilder CreateODataModelBuilder() { 

	  // Include persistent classes to the EdmModel:
		ODataModelBuilder builder = new ODataConventionModelBuilder();
		var customers = builder.EntitySet<Customer>("Customers");
		customers.EntityType.HasKey(t => t.CustomerID);
		// ..

	  // Include custom actions and functions into the EdmModel.
		builder.Function("TotalSalesByYear")
			.Returns<decimal>()
			.Parameter<int>("year");

		return builder;
	}
	```
7. Add OData controllers to the Controllers folder. An OData controller is a class inherited from the System.Web.OData.ODataController class. Each controller represents a separate data model class created on the third step.
8. Implement the required methods in controllers (e.g., `Get`, `Post`, `Put`, `Path`, `Delete`, etc.). For reference, use existing controllers in this example. For example: **CS\ODataService\Controllers\CustomersController.cs**.
