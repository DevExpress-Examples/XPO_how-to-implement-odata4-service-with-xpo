using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using DevExpress.Xpo;
using WebApplication1.Models;
using ODataService.Helpers;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace WebApplication1.Controllers {
    public class ProductController : ODataController {

        private UnitOfWork Session;
        
        [EnableQuery]
        public IQueryable<Product> Get() {
            Session = ConnectionHelper.CreateSession();
            return Session.Query<Product>();
        }

        [EnableQuery]
        public SingleResult<Product> Get([FromODataUri] int key) {
            Session = ConnectionHelper.CreateSession();
            var result = Session.Query<Product>().Where(t => t.ProductID == key);
            return SingleResult.Create(result);
        }

        [HttpPost]
        public IHttpActionResult Post(Product product) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                Product entity = new Product(uow) {
                    ProductName = product.ProductName,
                    Picture = product.Picture
                };
                uow.CommitChanges();
                return Created(entity);
            }
        }

        [HttpPut]
        public IHttpActionResult Put([FromODataUri] int key, Product product) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            if(key != product.ProductID) {
                return BadRequest();
            }
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                Product existing = uow.GetObjectByKey<Product>(key);
                if(existing == null) {
                    Product entity = new Product(uow) {
                        ProductName = product.ProductName,
                        Picture = product.Picture
                    };
                    uow.CommitChanges();
                    return Created(entity);
                } else {
                    existing.ProductName = product.ProductName;
                    existing.Picture = product.Picture;
                    uow.CommitChanges();
                    return Updated(product);
                }
            }
        }

        [HttpPatch]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<Product> product) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            var result = ApiHelper.Patch<Product, int>(key, product);
            if(result != null) {
                return Updated(result);
            }
            return NotFound();
        }

        [HttpDelete]
        public IHttpActionResult Delete([FromODataUri] int key) {
            return StatusCode(ApiHelper.Delete<Product, int>(key));
        }

        [HttpPost, HttpPut]
        public IHttpActionResult CreateRef([FromODataUri]int key, string navigationProperty, [FromBody] Uri link) {
            return StatusCode(ApiHelper.CreateRef<Product, int>(Request, key, navigationProperty, link));
        }

        [HttpDelete]
        public IHttpActionResult DeleteRef([FromODataUri] int key, [FromODataUri] int relatedKey, string navigationProperty) {
            return StatusCode(ApiHelper.DeleteRef<Product, int, int>(key, relatedKey, navigationProperty));
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