<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/137757184/20.1.5%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T830571)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
[![](https://img.shields.io/badge/💬_Leave_Feedback-feecdd?style=flat-square)](#does-this-example-address-your-development-requirementsobjectives)
<!-- default badges end -->

# How to Implement OData v4 Service with XPO (.NET Framework)

> **Note**:
> It is much easier to use the **[Web API Service](https://docs.devexpress.com/eXpressAppFramework/403394/backend-web-api-service)** with integrated authorization & CRUD operations based on ASP.NET Core OData 8.0 (OData v4) powered by EF Core and XPO ORM library instead. For more information, see [A 1-Click Solution for CRUD Web API Services with Role-based Access Control via EF Core & XPO (FREE)](https://community.devexpress.com/blogs/news/archive/2022/06/20/a-one-click-solution-for-role-based-access-control-asp-net-core-web-api-services-via-entity-framework-core-and-xpo-v22-1.aspx).

This example describes how to implement an OData v4 service with XPO and .NET Framework 4.5. This example is an ASP.NET MVC 5 Web API project and provides a simple REST API for data access. For the .NET Core-based example, refer to [How to Implement OData v4 Service with XPO (.NET Core)](https://github.com/DevExpress-Examples/XPO_how-to-implement-odata4-service-with-xpo-netcore).

## Steps to Implement

1. Create a new **ASP.NET Web Application** project and select the **Web API** project template (refer to the **Create the Visual Studio Project** section in [this example](https://docs.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/odata-v4/create-an-odata-v4-endpoint) for details.
2. Install the following nuget packages:
	* DevExpress.Xpo
	* Microsoft.AspNet.OData
3. Define your data model - implement persistent classes and initialize the data layer. If you are new to XPO, refer to the following articles to learn how to do this: [Create Persistent Class](https://docs.devexpress.com/XPO/2077/create-a-data-model/create-a-persistent-object), [Map to Existing Tables](https://docs.devexpress.com/CoreLibraries/3264/devexpress-orm-tool/concepts/basics-of-creating-persistent-objects-for-existing-data-tables).
4. Add files from the **CS\OdataService\Helpers** folder in this example to your project ([Quick Tip: Add files to Visual Studio projects the easy way](https://blogs.msdn.microsoft.com/davidklinems/2007/12/18/quick-tip-add-files-to-visual-studio-projects-the-easy-way/)).
5. Modify the `Application_Start()` method declared in the *Global.asax* file: register the model body validator class and initialize the [Data Access Layer](https://docs.devexpress.com/CoreLibraries/2121/devexpress-orm-tool/feature-center/connecting-to-a-data-store/data-access-layer).

```cs
protected void Application_Start() {
	GlobalConfiguration.Configuration.Services.Replace(typeof(IBodyModelValidator), new CustomBodyModelValidator());
	GlobalConfiguration.Configure(WebApiConfig.Register);
	XpoDefault.DataLayer = ConnectionHelper.CreateDataLayer(AutoCreateOption.SchemaAlreadyExists, true);
}

public class CustomBodyModelValidator : DefaultBodyModelValidator {
	readonly ConcurrentDictionary<Type, bool> persistentTypes = new ConcurrentDictionary<Type, bool>();
	public override bool ShouldValidateType(Type type) {
		return persistentTypes.GetOrAdd(type, t => !typeof(IXPSimpleObject).IsAssignableFrom(t));
	}
}
```

6. Modify the *WebApiConfig.cs* file: create an ODataModelBuilder instance and register an EntitySet for each persistent class (refer to the [WebApiConfig.cs](CS/ODataService/App_Start/WebApiConfig.cs) file in this repository to learn how to automatically register all persistent classes):

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
7. Add OData controllers to the Controllers folder. An OData controller is a class inherited from the Microsoft.AspNet.OData.ODataController class. Each controller represents a separate data model class created on the third step.
8. Implement the required methods in controllers (e.g., `Get`, `Post`, `Put`, `Patch`, `Delete`, etc.). For reference, use existing controllers in this example. For example: **CS\ODataService\Controllers\CustomersController.cs**.
<!-- feedback -->
## Does this example address your development requirements/objectives?

[<img src="https://www.devexpress.com/support/examples/i/yes-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=XPO_how-to-implement-odata4-service-with-xpo&~~~was_helpful=yes) [<img src="https://www.devexpress.com/support/examples/i/no-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=XPO_how-to-implement-odata4-service-with-xpo&~~~was_helpful=no)

(you will be redirected to DevExpress.com to submit your response)
<!-- feedback end -->
