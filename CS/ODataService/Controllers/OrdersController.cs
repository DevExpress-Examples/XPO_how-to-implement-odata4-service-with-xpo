using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using DevExpress.Xpo;
using WebApplication1.Models;
using ODataService.Helpers;

namespace WebApplication1.Controllers {
    public class OrdersController : ODataController {

        private UnitOfWork Session;

        [EnableQuery]
        public IQueryable<Order> Get() {
            Session = ConnectionHelper.CreateSession();
            return Session.Query<Order>().AsWrappedQuery();
        }

        [EnableQuery]
        public SingleResult<Order> Get([FromODataUri] int key) {
            Session = ConnectionHelper.CreateSession();
            IQueryable<Order> result = Session.Query<Order>().AsWrappedQuery().Where(t => t.OrderID == key);
            return SingleResult.Create(result);
        }

        [EnableQuery]
        public SingleResult<Customer> GetCustomerID([FromODataUri] int key) {
            Session = ConnectionHelper.CreateSession();
            var result = Session.Query<Order>().AsWrappedQuery().Where(m => m.OrderID == key).Select(m => m.Customer);
            return SingleResult.Create(result);
        }

        [EnableQuery]
        public IQueryable<OrderDetail> GetOrderDetails([FromODataUri] int key) {
            Session = ConnectionHelper.CreateSession();
            return Session.Query<OrderDetail>().AsWrappedQuery().Where(t => t.Order.OrderID == key);
        }

        [HttpPost]
        public IHttpActionResult Post(Order order) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                Order entity = new Order(uow) {
                    OrderID = order.OrderID,
                    OrderDate = order.OrderDate,
                    OrderStatus = order.OrderStatus
                };
                uow.CommitChanges();
                return Created(entity);
            }
        }

        [HttpPut]
        public IHttpActionResult Put([FromODataUri] int key, Order order) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            if(key != order.OrderID) {
                return BadRequest();
            }
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                Order existing = uow.GetObjectByKey<Order>(key);
                if(existing == null) {
                    Order entity = new Order(uow) {
                        OrderID = order.OrderID,
                        OrderDate = order.OrderDate,
                        OrderStatus = order.OrderStatus
                    };
                    uow.CommitChanges();
                    return Created(entity);
                } else {
                    existing.OrderDate = order.OrderDate;
                    uow.CommitChanges();
                    return Updated(existing);
                }
            }
        }

        [HttpPatch]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<Order> order) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            var result = ApiHelper.Patch<Order, int>(key, order);
            if(result != null) {
                return Updated(result);
            }
            return NotFound();
        }

        [HttpDelete]
        public IHttpActionResult Delete([FromODataUri] int key) {
            return StatusCode(ApiHelper.Delete<Order, int>(key));
        }

        [HttpPost]
        [HttpPut]
        [ODataRoute("Orders({key})/OrderDetails")]
        public IHttpActionResult AddToOrderDetails([FromODataUri] int key, OrderDetail orderDetail) {
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                Order order = uow.GetObjectByKey<Order>(key);
                if(order == null) {
                    return NotFound();
                }
                OrderDetail existing = order.OrderDetails.FirstOrDefault(d => d.OrderDetailID == orderDetail.OrderDetailID);
                if(existing == null) {
                    OrderDetail entity = new OrderDetail(uow) {
                        Quantity = orderDetail.Quantity,
                        UnitPrice = orderDetail.UnitPrice,
                    };
                    order.OrderDetails.Add(entity);
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

        [HttpDelete]
        public IHttpActionResult DeleteRef([FromODataUri] int key, [FromODataUri] int relatedKey, string navigationProperty) {
            return StatusCode(ApiHelper.DeleteRef<Order, int, int>(key, relatedKey, navigationProperty));
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