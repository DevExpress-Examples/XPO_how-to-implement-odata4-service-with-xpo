using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using DevExpress.Xpo;
using WebApplication1.Models;
using ODataService.Helpers;
using System.Web.OData;
using System.Web.OData.Routing;

namespace WebApplication1.Controllers {
    public class ActionsController : ODataController {

        [ODataRoute("InitializeDatabase")]
        public IHttpActionResult InitializeDatabase() {
            DemoDataHelper.CleanupDatabase();
            DemoDataHelper.CreateDemoData();
            return Ok();
        }

        [HttpGet]
        [ODataRoute("TotalSalesByYear(year={year})")]
        public IHttpActionResult TotalSalesByYear(int year) {
            using(UnitOfWork uow = new UnitOfWork()) {
                decimal result = uow.Query<Order>()
                    .Where(o => o.OrderDate.Value.Year == year)
                    .Sum(o => o.OrderDetails.Sum(d => d.Quantity * d.UnitPrice));
                return Ok(result);
            }
        }
    }
}