Imports System
Imports System.Linq
Imports System.Web.Http
Imports System.Web.OData.Batch
Imports System.Web.OData.Builder
Imports System.Web.OData.Extensions
Imports WebApplication1.Models

Namespace WebApplication1
    Public NotInheritable Class WebApiConfig

        Private Sub New()
        End Sub

        Public Shared Sub Register(ByVal config As HttpConfiguration)
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(Nothing)
            Dim modelBuilder As ODataModelBuilder = CreateODataModelBuilder()

            Dim batchHandler As ODataBatchHandler = New DefaultODataBatchHandler(GlobalConfiguration.DefaultServer)

            config.MapODataServiceRoute(routeName:= "ODataRoute", routePrefix:= Nothing, model:= modelBuilder.GetEdmModel(), batchHandler:= batchHandler)
        End Sub

        Private Shared Function CreateODataModelBuilder() As ODataModelBuilder
            Dim builder As ODataModelBuilder = New ODataConventionModelBuilder()
            Dim customers = builder.EntitySet(Of Customer)("Customers")
            Dim orders = builder.EntitySet(Of Order)("Orders")
            Dim products = builder.EntitySet(Of Product)("Products")
            Dim orderDetails = builder.EntitySet(Of OrderDetail)("OrderDetails")

            customers.EntityType.HasKey(Function(t) t.CustomerID)
            orders.EntityType.HasKey(Function(t) t.OrderID)
            products.EntityType.HasKey(Function(t) t.ProductID)
            orderDetails.EntityType.HasKey(Function(t) t.OrderDetailID)

            builder.Action("InitializeDatabase")

            builder.Function("TotalSalesByYear").Returns(Of Decimal)().Parameter(Of Integer)("year")

            Return builder
        End Function
    End Class
End Namespace
