using System;
using System.Linq;
using System.Web.Http;
using WebApplication1.Models;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Builder;

namespace WebApplication1
{
    public static class WebApiConfig {
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
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            var documents = builder.EntitySet<BaseDocument>("Documents");
            var customers = builder.EntitySet<Customer>("Customers");
            var orders = builder.EntitySet<Order>("Orders");
            var contracts = builder.EntitySet<Contract>("Contracts");
            var products = builder.EntitySet<Product>("Products");
            var orderDetails = builder.EntitySet<OrderDetail>("OrderDetails");

            documents.EntityType.HasKey(t => t.ID);
            customers.EntityType.HasKey(t => t.CustomerID);
            products.EntityType.HasKey(t => t.ProductID);
            orderDetails.EntityType.HasKey(t => t.OrderDetailID);
            orders.EntityType.DerivesFrom<BaseDocument>();
            contracts.EntityType.DerivesFrom<BaseDocument>();

            builder.Action("InitializeDatabase");
            builder.Function("TotalSalesByYear")
                .Returns<decimal>()
                .Parameter<int>("year");

            return builder;
        }
    }
}
