using System;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using DevExpress.Xpo;
using WebApplication1.Models;
using ODataService.Helpers;

namespace WebApplication1.Controllers {
    public class CustomersController : ODataController {

        private UnitOfWork Session;

        [EnableQuery]
        public IQueryable<Customer> Get() {
            Session = ConnectionHelper.CreateSession();
            return Session.Query<Customer>().AsWrappedQuery();
        }

        [EnableQuery]
        public SingleResult<Customer> Get([FromODataUri] string key) {
            Session = ConnectionHelper.CreateSession();
            IQueryable<Customer> result = Session.Query<Customer>().AsWrappedQuery().Where(t => t.CustomerID == key);
            return SingleResult.Create(result);
        }

        [EnableQuery]
        public IQueryable<Order> GetOrders([FromODataUri] string key) {
            Session = ConnectionHelper.CreateSession();
            return Session.Query<Customer>().AsWrappedQuery().Where(m => m.CustomerID == key).SelectMany(m => m.Orders);
        }


        [HttpPost]
        public IHttpActionResult Post(Customer customer) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                Customer entity = new Customer(uow) {
                    CustomerID = customer.CustomerID,
                    CompanyName = customer.CompanyName
                };
                uow.CommitChanges();
                return Created(entity);
            }
        }

        [HttpPut]
        public IHttpActionResult Put([FromODataUri] string key, Customer customer) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            if(key != customer.CustomerID) {
                return BadRequest();
            }
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                Customer existing = uow.GetObjectByKey<Customer>(key);
                if(existing == null) {
                    Customer entity = new Customer(uow) {
                        CustomerID = customer.CustomerID,
                        CompanyName = customer.CompanyName
                    };
                    uow.CommitChanges();
                    return Created(entity);
                } else {
                    existing.CustomerID = customer.CustomerID;
                    existing.CompanyName = customer.CompanyName;
                    uow.CommitChanges();
                    return Updated(customer);
                }
            }
        }

        [HttpPatch]
        public IHttpActionResult Patch([FromODataUri] string key, Delta<Customer> customer) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            var result = ApiHelper.Patch<Customer, string>(key, customer);
            if(result != null) {
                return Updated(result);
            }
            return NotFound();
        }

        [HttpDelete]
        public IHttpActionResult Delete([FromODataUri] string key) {
            return StatusCode(ApiHelper.Delete<Customer, string>(key));
        }

        [HttpPost, HttpPut]
        public IHttpActionResult CreateRef([FromODataUri]string key, string navigationProperty, [FromBody] Uri link) {
            return StatusCode(ApiHelper.CreateRef<Customer, string>(Request, key, navigationProperty, link));
        }

        [HttpDelete]
        public IHttpActionResult DeleteRef([FromODataUri] string key, [FromODataUri] int relatedKey, string navigationProperty) {
            return StatusCode(ApiHelper.DeleteRef<Customer, string, int>(key, relatedKey, navigationProperty));
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