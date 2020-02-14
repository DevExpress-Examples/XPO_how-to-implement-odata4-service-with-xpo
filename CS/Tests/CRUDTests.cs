using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Default;
using NUnit.Framework;
using WebApplication1.Models;

namespace Tests
{
    [TestFixture]
    public class CRUDOperationTests : ODataTestsBase {

        [Test]
        public void CreateObject() {
            Container container = GetODataContainer();
            Customer customer = new Customer() {
                CustomerID = "C0001",
                CompanyName = "Test Company"
            };
            container.AddToCustomer(customer);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.Created, response.First().StatusCode);

            container = GetODataContainer();
            Customer createdItem = container.Customer.Where(t => t.CustomerID == customer.CustomerID).First();
            Assert.AreEqual(customer.CustomerID, createdItem.CustomerID);
            Assert.AreEqual(customer.CompanyName, createdItem.CompanyName);
        }

        [Test]
        public void UpdateObject() {
            Container container = GetODataContainer();
            Customer customer = container.Customer.Where(t => t.CustomerID == "BSBEV").First();

            customer.CompanyName = "Test Company Renamed";
            container.UpdateObject(customer);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Customer updatedItem = container.Customer.Where(t => t.CustomerID == "BSBEV").First();
            Assert.AreEqual(customer.CustomerID, updatedItem.CustomerID);
            Assert.AreEqual(customer.CompanyName, updatedItem.CompanyName);
        }

        [Test]
        public void BatchUpdate() {
            Container container = GetODataContainer();
            var customers = container.Customer.ToList();
            foreach(var customer in customers) {
                customer.CompanyName += " -renamed";
                container.UpdateObject(customer);
            }
            var response = container.SaveChanges(Microsoft.OData.Client.SaveChangesOptions.BatchWithSingleChangeset);

            Assert.AreEqual(4, response.Count());
            foreach(var res in response) {
                Assert.AreEqual((int)HttpStatusCode.NoContent, res.StatusCode);
            }

            container = GetODataContainer();
            customers = container.Customer.ToList();
            foreach(var customer in customers) {
                Assert.IsTrue(customer.CompanyName.EndsWith(" -renamed"));
            }
        }

        [Test]
        public void DeleteObject() {
            Container container = GetODataContainer();
            Customer customer = container.Customer.Where(t => t.CustomerID == "WARTH").First();

            container.DeleteObject(customer);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);
        }

        [Test]
        public void CreateObjectWithBlobField() {
            byte[] pictureData = new byte[256 * 256 * 4];
            for(int i = 0; i < pictureData.Length; i++) {
                pictureData[i] = (byte)i;
            }
            Container container = GetODataContainer();
            Product product = new Product() {
                ProductName = "test product",
                Picture = pictureData
            };
            container.AddToProduct(product);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.Created, response.First().StatusCode);

            container = GetODataContainer();
            Product createdItem = container.Product.Where(t => t.ProductName == product.ProductName).First();

            Assert.IsNotNull(createdItem.Picture);
            Assert.AreEqual(product.Picture.Length, createdItem.Picture.Length);
        }

        [Test]
        public void UpdateObjectWithBlobField() {
            byte[] pictureData = new byte[256 * 256 * 4];
            for(int i = 0; i < pictureData.Length; i++) {
                pictureData[i] = (byte)i;
            }
            Container container = GetODataContainer();
            var product = container.Product.Where(t => t.ProductName == "Queso Cabrales").First();
            byte[] oldPicture = product.Picture;
            product.Picture = pictureData;
            container.UpdateObject(product);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Product updatedItem = container.Product.Where(t => t.ProductID == product.ProductID).First();
            Assert.IsNull(oldPicture);
            Assert.IsNotNull(updatedItem.Picture);
            Assert.AreEqual(pictureData.Length, updatedItem.Picture.Length);
        }

        [Test]
        public void AddRelatedObject() {
            Container container = GetODataContainer();
            Order order = container.Order.OrderByDescending(t => t.ID).First();

            OrderDetail detail = new OrderDetail() {
                Quantity = 105,
                UnitPrice = 201.37m
            };
            container.AddRelatedObject(order, "OrderDetails", detail);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.Created, response.First().StatusCode);

            container = GetODataContainer();
            OrderDetail createdItem = container.OrderDetail.OrderByDescending(t => t.OrderDetailID).First();
            Assert.AreEqual(detail.Quantity, createdItem.Quantity);
            Assert.AreEqual(detail.UnitPrice, createdItem.UnitPrice);
            Assert.Greater(createdItem.OrderDetailID, 0);
        }

        [Test]
        public void UpdateRelatedObject() {
            Container container = GetODataContainer();
            Order order = container.Order.Expand(t => t.OrderDetails).OrderBy(t => t.Date).First();
            OrderDetail detail = order.OrderDetails.OrderBy(t => t.OrderDetailID).First();

            short oldQuantity = detail.Quantity;
            detail.Quantity += 1;
            container.UpdateObject(detail);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            OrderDetail updatedItem = container.Order.Expand(t => t.OrderDetails)
                .Where(t => t.ID == order.ID).First()
                .OrderDetails.Where(d => d.OrderDetailID == detail.OrderDetailID).First();
            Assert.AreNotEqual(oldQuantity, updatedItem.Quantity);
            Assert.AreEqual(detail.Quantity, updatedItem.Quantity);
        }

        [Test]
        public void DeleteRelatedObject() {
            Container container = GetODataContainer();
            Order order = container.Order.Expand(t => t.OrderDetails).OrderBy(t => t.Date).First();
            OrderDetail detail = order.OrderDetails.OrderBy(t => t.OrderDetailID).First();

            container.DeleteLink(order, "OrderDetails", detail);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = container.Order.Expand(t => t.OrderDetails).Where(t => t.ID == order.ID).First();
            Assert.False(updatedItem.OrderDetails.Any(d => d.OrderDetailID == detail.OrderDetailID));
        }

        [Test]
        public void DeleteAggregatedCollection() {
            Container container = GetODataContainer();
            Order order = container.Order.Expand(t => t.OrderDetails).OrderBy(t => t.Date).First();

            container.DeleteObject(order);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            var details = container.OrderDetail.Where(t => t.Order.ID == order.ID).ToList();
            Assert.AreEqual(0, details.Count);
        }

        [Test]
        public void AddLink() {
            Container container = GetODataContainer();
            Order order = container.Order.Where(t => t.Customer == null).OrderByDescending(t => t.ID).First();
            Customer customer = container.Customer.Where(t => t.CustomerID == "BSBEV").First();

            container.AddLink(customer, "Orders", order);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = container.Order.Expand(t => t.Customer).Where(t => t.ID == order.ID).First();
            Assert.IsNull(order.Customer);
            Assert.NotNull(updatedItem.Customer);
            Assert.AreEqual(updatedItem.Customer.CustomerID, customer.CustomerID);
        }

        [Test]
        public void UpdateLink() {
            Container container = GetODataContainer();
            Order order = container.Order.Where(t => t.Customer.CustomerID == "BSBEV").OrderByDescending(t => t.ID).First();
            Customer customer = container.Customer.Where(t => t.CustomerID == "SANTG").First();

            container.AddLink(customer, "Orders", order);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = container.Order.Expand(t => t.Customer).Where(t => t.ID == order.ID).First();
            Assert.AreEqual(updatedItem.Customer.CustomerID, customer.CustomerID);
        }

        [Test]
        public void DeleteLink() {
            Container container = GetODataContainer();
            Order order = container.Order.Expand(t => t.Customer).Where(t => t.Customer.CustomerID == "BSBEV").OrderByDescending(t => t.ID).First();

            container.DeleteLink(order.Customer, "Orders", order);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = container.Order.Expand(t => t.Customer).Where(t => t.ID == order.ID).First();
            Assert.IsNotNull(order.Customer);
            Assert.IsNull(updatedItem.Customer);
        }

        [Test]
        public void DeleteInheritedObject() {
            Container container = GetODataContainer();
            int contractId = container.Contract.Where(t => t.Number == "2018-0003").First().ID;
            BaseDocument contract = container.BaseDocument.Where(t => t.ID == contractId).First();

            container.DeleteObject(contract);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            int count = container.BaseDocument.ToList().Count(t => t.ID == contractId);
            Assert.AreEqual(0, count);
        }

        [Test]
        public void AddLinkToInherited() {
            Container container = GetODataContainer();
            Order order = container.Order.Where(t => t.ParentDocument == null).OrderByDescending(t => t.ID).First();
            Contract contract = container.Contract.Where(t => t.Number == "2018-0003").First();

            container.AddLink(contract, "LinkedDocuments", order);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = container.Order.Expand(t => t.ParentDocument).Where(t => t.ID == order.ID).First();
            Assert.IsNull(order.ParentDocument);
            Assert.NotNull(updatedItem.ParentDocument);
            Assert.AreEqual(updatedItem.ParentDocument.GetType(), typeof(Contract));
            Assert.AreEqual(updatedItem.ParentDocument.ID, contract.ID);
        }

        [Test]
        public void UpdateLinkToInherited() {
            Container container = GetODataContainer();
            Order order = container.Order.Where(t => (t.ParentDocument as Contract).Number == "2018-0002").First();
            Contract contract = container.Contract.Where(t => t.Number == "2018-0003").First();

            container.AddLink(contract, "LinkedDocuments", order);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = container.Order.Expand(t => t.ParentDocument).Where(t => t.ID == order.ID).First();
            Assert.AreEqual(updatedItem.ParentDocument.ID, contract.ID);
        }

        [Test]
        public void DeleteLinkToInherited() {
            Container container = GetODataContainer();
            Order order = container.Order.Expand(t => t.ParentDocument).Where(t => (t.ParentDocument as Contract).Number == "2018-0002").First();

            container.DeleteLink(order.ParentDocument, "LinkedDocuments", order);
            var response = container.SaveChanges();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = container.Order.Expand(t => t.ParentDocument).Where(t => t.ID == order.ID).First();
            Assert.IsNotNull(order.ParentDocument);
            Assert.IsNull(updatedItem.ParentDocument);
        }

        [Test]
        public void SetLink() {
            Container container = GetODataContainer();
            Order order = new Order() {
                Date = DateTimeOffset.Now,
                OrderStatus = OrderStatus.New
            };
            container.AddToOrder(order);
            var response1 = container.SaveChanges();

            Assert.AreEqual(1, response1.Count());
            Assert.AreEqual((int)HttpStatusCode.Created, response1.First().StatusCode);
            Customer customer = new Customer() {
                CustomerID = "Duffy",
                CompanyName = "Acme"
            };
            container.AddToCustomer(customer);
            order.Customer = customer;
            container.SetLink(order, "Customer", customer);
            var response2 = container.SaveChanges();
            Assert.AreEqual(2, response2.Count());
            Assert.IsTrue(response2.Any(r => r.StatusCode == (int)HttpStatusCode.Created));
            Assert.IsTrue(response2.Any(r => r.StatusCode == (int)HttpStatusCode.NoContent));

            container = GetODataContainer();
            Order createdItem = container.Order.Expand(t => t.Customer).Where(t => t.ID == order.ID).First();
            Assert.IsNotNull(createdItem.Customer);
            Assert.AreEqual(order.Customer.CustomerID, createdItem.Customer.CustomerID);
        }


        [Test]
        public void RemoveLink() {
            Container container = GetODataContainer();
            Order order = new Order() {
                Date = DateTimeOffset.Now,
                OrderStatus = OrderStatus.New
            };
            container.AddToOrder(order);
            Customer customer = new Customer() {
                CustomerID = "Duffy",
                CompanyName = "Acme"
            };
            container.AddToCustomer(customer);
            container.SetLink(order, "Customer", customer);
            var response1 = container.SaveChanges();
            Assert.AreEqual(3, response1.Count());
            Assert.AreEqual(2, response1.Count(r => r.StatusCode == (int)HttpStatusCode.Created));
            Assert.IsTrue(response1.Any(r => r.StatusCode == (int)HttpStatusCode.NoContent));

            container = GetODataContainer();
            Order theOrder = container.Order.Expand(t => t.Customer).Where(t => t.ID == order.ID).First();
            Assert.IsNotNull(theOrder.Customer);
            Assert.AreEqual(customer.CustomerID, theOrder.Customer.CustomerID);

            container.SetLink(theOrder, "Customer", null);
            var response2 = container.SaveChanges();
            Assert.AreEqual(1, response2.Count());
            Assert.AreEqual(1, response2.Count(r => r.StatusCode == (int)HttpStatusCode.NoContent));

            container = GetODataContainer();
            Order theOrder2 = container.Order.Expand(t => t.Customer).Where(t => t.ID == order.ID).First();
            Assert.IsNull(theOrder2.Customer);
        }
    }
}
