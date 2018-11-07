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
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace WebApplication1.Controllers {

    public class DocumentsController : ODataController {

        private UnitOfWork Session;

        [EnableQuery]
        public IQueryable<BaseDocument> Get() {
            Session = ConnectionHelper.CreateSession();
            return Session.Query<BaseDocument>().AsWrappedQuery();
        }

        [EnableQuery]
        public SingleResult<BaseDocument> Get([FromODataUri] int key) {
            Session = ConnectionHelper.CreateSession();
            var result = Session.Query<BaseDocument>().AsWrappedQuery().Where(t => t.ID == key);
            return SingleResult.Create(result);
        }

        [HttpPost, HttpPut]
        public IHttpActionResult CreateRef([FromODataUri]int key, string navigationProperty, [FromBody] Uri link) {
            return StatusCode(ApiHelper.CreateRef<BaseDocument, int>(Request, key, navigationProperty, link));
        }

        [HttpDelete]
        public IHttpActionResult DeleteRef([FromODataUri] int key, [FromODataUri] int relatedKey, string navigationProperty) {
            return StatusCode(ApiHelper.DeleteRef<BaseDocument, int, int>(key, relatedKey, navigationProperty));
        }

        [HttpDelete]
        public IHttpActionResult Delete([FromODataUri] int key) {
            return StatusCode(ApiHelper.Delete<BaseDocument, int>(key));
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if(disposing) {
                if(Session != null) {
                    Session.Dispose();
                    Session = null;
                }
            }
        }
    }
}