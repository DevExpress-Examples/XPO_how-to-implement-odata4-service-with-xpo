How to implement OData4 service with XPO
========================================

This example describes how to implement an OData4 service with XPO. This example is an ASP.NET MVC 5 Web API project and provides simple REST API for data access.

Steps to implement:

1. Create a new **ASP.NET Web Application** project and select the **Web API** project template.
2. Install the following nuget packages:
	* DevExpress.Xpo
	* Microsoft.AspNet.OData
3. Define your data model - implement persistent classes and initialize the data layer.
4. Create a separate System.Web.OData.ODataController descendant for each persistent class.
5. Implement the required methods in controllers (e.g., `Get`, `Post`, `Put`, `Path`, `Delete`, etc.). 
6. Modify the `Application_Start()` method declared in the *Global.asax* file: register the model body validator class and place and create your data layer.

	```cs
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
	```

7. Modify the *WebApiConfig.cs* file: instantiate a ODataModelBuilder and pass your persistent classes to it:

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
		var orders = builder.EntitySet<Order>("Orders");
		var products = builder.EntitySet<Product>("Products");
		var orderDetails = builder.EntitySet<OrderDetail>("OrderDetails");
		customers.EntityType.HasKey(t => t.CustomerID);
		orders.EntityType.HasKey(t => t.OrderID);
		products.EntityType.HasKey(t => t.ProductID);
		orderDetails.EntityType.HasKey(t => t.OrderDetailID);

	  // Include custom actions and functions into the EdmModel.
		builder.Action("InitializeDatabase");
		builder.Function("TotalSalesByYear")
			.Returns<decimal>()
			.Parameter<int>("year");

		return builder;
	}
	```