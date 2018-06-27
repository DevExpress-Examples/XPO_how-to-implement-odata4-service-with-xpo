using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Default;
using NUnit.Framework;
using WebApplication1.Models;

namespace Tests {

    [TestFixture]
    public class SelectOperationsTests : ODataTestsBase {

        [Test]
        public void SelectSimple() {
            Container container = GetODataContainer();
            var customers = container.Customers.ToList();

            Assert.AreEqual(4, customers.Count);
            Assert.True(customers.Exists(c => c.CustomerID == "BSBEV" && c.CompanyName == "B's Beverages"));
            Assert.True(customers.Exists(c => c.CustomerID == "OCEAN" && c.CompanyName == "Océano Atlántico Ltda."));
            Assert.True(customers.Exists(c => c.CustomerID == "SANTG" && c.CompanyName == "Santé Gourmet"));
            Assert.True(customers.Exists(c => c.CustomerID == "WARTH" && c.CompanyName == "Wartian Herkku"));
        }

        [Test]
        public void GetCount() {
            Container container = GetODataContainer();
            int count = container.Customers.Count();
            Assert.AreEqual(4, count);
        }

        [Test]
        public void FilterAndSort() {
            Container container = GetODataContainer();
            var customers = container.Customers
                .Where(c => string.Compare(c.CustomerID, "C") >= 0)
                .OrderByDescending(c => c.CompanyName)
                .ToList();

            Assert.AreEqual(3, customers.Count);
            Assert.IsTrue(customers[0].CustomerID == "WARTH");
            Assert.IsTrue(customers[1].CustomerID == "SANTG");
            Assert.IsTrue(customers[2].CustomerID == "OCEAN");
        }

        [Test]
        public void SkipAndTake() {
            Container container = GetODataContainer();
            var products = container.Products.OrderBy(t => t.UnitPrice)
                .Skip(1).Take(2)
                .ToList();

            Assert.AreEqual(2, products.Count);
            Assert.AreEqual(200.12m, products[0].UnitPrice);
            Assert.AreEqual(300.13m, products[1].UnitPrice);
        }

        [Test]
        public void FilterByNull() {
            Container container = GetODataContainer();
            var orders = container.Orders
                .Where(o => o.Customer == null)
                .ToList();
            Assert.AreEqual(1, orders.Count);
        }

        [Test]
        public void FilterByNotNull() {
            Container container = GetODataContainer();
            var orders = container.Orders
                .Where(o => o.Customer != null)
                .ToList();
            Assert.AreEqual(4, orders.Count);
        }

        [Test]
        public void FilterByEnum() {
            Container container = GetODataContainer();
            var orders = container.Orders
                .Where(o => o.OrderStatus == OrderStatus.New)
                .ToList();
            Assert.AreEqual(2, orders.Count);
        }

        [Test]
        public void Expand() {
            Container container = GetODataContainer();
            var orders = container.Orders
                .Expand(o => o.Customer)
                .Expand("OrderDetails($expand=Product)")
                .ToList();

            orders = orders.OrderBy(o => o.Date).ToList();
            Assert.AreEqual(5, orders.Count);
            Assert.AreEqual("BSBEV", orders[0].Customer.CustomerID);
            Assert.AreEqual("OCEAN", orders[1].Customer.CustomerID);
            Assert.AreEqual("SANTG", orders[2].Customer.CustomerID);
            Assert.AreEqual("BSBEV", orders[3].Customer.CustomerID);
            Assert.IsNull(orders[4].Customer);
            Assert.AreEqual(0, orders[4].OrderDetails.Count);
            for(int i = 0; i < 4; i++) {
                Assert.AreEqual(3, orders[i].OrderDetails.Count);
                for(int j = 0; j < 3; j++) {
                    Assert.IsNotNull(orders[i].OrderDetails[j].Product);
                }
            }
        }

        [Test]
        public void FilterAndSortWithExpand() {
            Container container = GetODataContainer();
            var orders = container.Orders
                .Expand(o => o.Customer)
                .Expand("OrderDetails($expand=Product)")
                .Where(o => o.Customer.CustomerID != "OCEAN")
                .OrderBy(o => o.Customer.CompanyName)
                .ThenBy(o => o.Date)
                .ToList();

            Assert.AreEqual(3, orders.Count);
            Assert.AreEqual("BSBEV", orders[0].Customer.CustomerID);
            Assert.AreEqual("BSBEV", orders[1].Customer.CustomerID);
            Assert.AreEqual("SANTG", orders[2].Customer.CustomerID);
            for(int i = 0; i < 3; i++) {
                Assert.AreEqual(3, orders[i].OrderDetails.Count);
                for(int j = 0; j < 3; j++) {
                    Assert.IsNotNull(orders[i].OrderDetails[j].Product);
                }
            }
        }

        [Test]
        public void FilterByChildCollections() {
            Container container = GetODataContainer();
            var orders = container.Orders
                .Expand("OrderDetails($expand=Product)")
                .Where(o => o.OrderDetails.Any(t => t.Product.ProductName == "Queso Cabrales"))
                .ToList();

            Assert.AreEqual(2, orders.Count);
            for(int i = 0; i < 2; i++) {
                Assert.IsTrue(orders[i].OrderDetails.Any(d => d.Product.ProductName == "Queso Cabrales"));
            }
        }

        [Test]
        public void FilterByDateTime() {
            DateTimeOffset startDate = new DateTimeOffset(new DateTime(2018, 03, 01));
            DateTimeOffset endDate = new DateTimeOffset(new DateTime(2018, 06, 01));
            Container container = GetODataContainer();
            var orders = container.Orders
                .Where(o => o.Date > startDate && o.Date <= endDate)
                .ToList();

            Assert.AreEqual(2, orders.Count);
            for(int i = 0; i < orders.Count; i++) {
                Assert.Greater(orders[i].Date, startDate);
                Assert.LessOrEqual(orders[i].Date, endDate);
            }
        }

        [Test]
        public void FilterByDateTimePart() {
            Container container = GetODataContainer();
            var orders = container.Orders
                .Where(o => o.Date.Value.Year == 2018 && (o.Date.Value.Month == 3 || o.Date.Value.Month == 6))
                .ToList();
            Assert.AreEqual(3, orders.Count);
        }

        [Test]
        public void SelectBlobValues() {
            Container container = GetODataContainer();
            var product = container.Products.Where(t => t.ProductName == "Vegie-spread").First();

            Assert.IsNotNull(product.Picture);
            Assert.AreEqual(10, product.Picture.Length);
            for(int i = 0; i < 10; i++) {
                Assert.AreEqual(i + 1, product.Picture[i]);
            }
        }

        [Test]
        public void SelectWithProjection() {
            Container container = GetODataContainer();
            var orders = container.Orders.Expand(t => t.Customer).OrderBy(t => t.Date).ToList();
            var projected = container.Orders
                .OrderBy(t => t.Date)
                .Select(t => new {
                    OrderID = t.ID,
                    OrderDate = t.Date,
                    Customer = (t.Customer != null) ? t.Customer.CompanyName : null
                })
                .ToList();

            Assert.AreEqual(orders.Count, projected.Count);
            for(int i = 0; i < orders.Count; i++) {
                Assert.AreEqual(orders[i].ID, projected[i].OrderID);
                Assert.AreEqual(orders[i].Date, projected[i].OrderDate);
                Assert.AreEqual(orders[i].Customer?.CompanyName, projected[i].Customer);
            }
        }

        [Test]
        public void SelectWithProjectionAndFunctions() {
            Container container = GetODataContainer();
            var orders = container.Orders.Expand(t => t.Customer).OrderBy(t => t.Date).ToList();
            var projected = container.Orders
                .OrderBy(t => t.Date)
                .Select(t => new {
                    Year = t.Date.Value.Year,
                    Customer = (t.Customer != null) ? t.Customer.CompanyName.ToUpper() : null
                })
                .ToList();

            Assert.AreEqual(orders.Count, projected.Count);
            for(int i = 0; i < orders.Count; i++) {
                Assert.AreEqual(orders[i].Date.Value.Year, projected[i].Year);
                Assert.AreEqual(orders[i].Customer?.CompanyName.ToUpper(), projected[i].Customer);
            }
        }

        [Test]
        public void ResourceReferenceProperty() {
            Container container = GetODataContainer();
            int orderId = container.Orders
                .Where(t => t.Date == new DateTimeOffset(new DateTime(2018, 06, 01)))
                .First().ID;

            var details = container.Orders.ByKey(orderId).OrderDetails.Expand(t => t.Product).ToList();
            Assert.AreEqual(3, details.Count);
        }

        [Test]
        public void InheritanceFilterByType() {
            Container container = GetODataContainer();
            var documents = container.Documents.ToList();
            var orders = container.Documents.Where(t => t is Order).ToList();
            var contracts = container.Documents.Where(t => t is Contract).ToList();
            var notOrders = container.Documents.Where(t => !(t is Order)).ToList();
            Assert.AreEqual(5, orders.Count);
            Assert.AreEqual(3, contracts.Count);
            Assert.AreEqual(8, documents.Count);
            Assert.AreEqual(contracts.Count, notOrders.Count);
        }

        [Test]
        public void InheritanceCast() {
            Container container = GetODataContainer();
            var contracts = container.Documents.Where(t => (t as Contract).Number != "2018-0003").ToList();
            Assert.AreEqual(2, contracts.Count);
            contracts = container.Documents.Where(t => (t as Contract).Number == "2018-0003").ToList();
            Assert.AreEqual(1, contracts.Count);
        }

        [Test]
        public void SelectInheritedReference() {
            Container container = GetODataContainer();
            int orderId = container.Orders
                .Where(t => t.Date == new DateTimeOffset(new DateTime(2018, 01, 22, 10, 00, 01)))
                .First().ID;
            BaseDocument parentDoc = container.Orders.ByKey(orderId).ParentDocument.GetValue();
            Assert.IsNotNull(parentDoc);
            Assert.AreEqual(typeof(Contract), parentDoc.GetType());
            Assert.AreEqual("2018-0001", ((Contract)parentDoc).Number);
        }

        [Test]
        public void ExpandInheritedCollection() {
            Container container = GetODataContainer();
            Contract contract = container.Contracts.Expand(c => c.LinkedDocuments)
                .Where(t => t.Number == "2018-0001")
                .First();
            Assert.AreEqual(3, contract.LinkedDocuments.Count);
            var linkedDocuments = contract.LinkedDocuments.OrderBy(t => t.GetType().Name).ToList();
            Assert.AreEqual(typeof(Contract), linkedDocuments[0].GetType());
            Assert.AreEqual(typeof(Order), linkedDocuments[1].GetType());
            Assert.AreEqual(typeof(Order), linkedDocuments[2].GetType());
        }

        [Test]
        public void CrossFilterByChildCollection() {
            Container container = GetODataContainer();
            var contracts = container.Contracts.Where(c => c.LinkedDocuments.Any(d => d.Date <= c.Date)).ToList();
            Assert.AreEqual(1, contracts.Count);
            Assert.AreEqual("2018-0001", contracts[0].Number);
        }
    }
}
