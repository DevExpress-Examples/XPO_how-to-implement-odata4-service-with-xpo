using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using DevExpress.Xpo;
using WebApplication1.Models;
using ODataService.Helpers;

namespace WebApplication1.Controllers {
    public class OrderDetailsController : ODataController {

        private UnitOfWork Session;        

        [EnableQuery]
        public IQueryable<OrderDetail> Get() {
            Session = ConnectionHelper.CreateSession();
            return Session.Query<OrderDetail>().AsWrappedQuery();
        }

        [EnableQuery]
        public SingleResult<OrderDetail> Get([FromODataUri] int key) {
            Session = ConnectionHelper.CreateSession();
            IQueryable<OrderDetail> result = Session.Query<OrderDetail>().AsWrappedQuery().Where(t => t.OrderDetailID == key);
            return SingleResult.Create(result);
        }

        [HttpPut]
        public IHttpActionResult Put([FromODataUri] int key, OrderDetail orderDetail) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            if(key != orderDetail.OrderDetailID) {
                return BadRequest();
            }
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                OrderDetail existing = uow.GetObjectByKey<OrderDetail>(key);
                if(existing == null) {
                    OrderDetail entity = new OrderDetail(uow) {
                        Order = orderDetail.Order,
                        Quantity = orderDetail.Quantity,
                        UnitPrice = orderDetail.UnitPrice
                    };
                    uow.CommitChanges();
                    return Created(entity);
                } else {
                    existing.Quantity = orderDetail.Quantity;
                    existing.UnitPrice = orderDetail.UnitPrice;
                    uow.CommitChanges();
                    return Updated(existing);
                }
            }
        }

        [HttpPatch]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<OrderDetail> orderDetail) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            var result = ApiHelper.Patch<OrderDetail, int>(key, orderDetail);
            if(result != null) {
                return Updated(result);
            }
            return NotFound();
        }

        [HttpDelete]
        public IHttpActionResult Delete([FromODataUri] int key) {
            return StatusCode(ApiHelper.Delete<OrderDetail, int>(key));
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