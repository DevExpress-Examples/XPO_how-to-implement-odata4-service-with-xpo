using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using WebApplication1.Models;

namespace ODataService.Helpers {
    public static class DemoDataHelper {

        public static void CleanupDatabase() {
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                XPCollection<BaseDocument> docs = new XPCollection<BaseDocument>(uow);
                XPCollection<Customer> customers = new XPCollection<Customer>(uow);
                XPCollection<Product> products = new XPCollection<Product>(uow);
                XPCollection<OrderDetail> orderDetails = new XPCollection<OrderDetail>(uow);
                uow.Delete(orderDetails);
                uow.Delete(docs);
                uow.Delete(products);
                uow.Delete(customers);
                uow.CommitChanges();
            }
        }

        public static void CreateDemoData() {
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                Product[] products = new Product[]{
                    new Product(uow) {
                        ProductName = "Queso Cabrales",
                        UnitPrice = 100.11m
                    },
                    new Product(uow) {
                        ProductName = "Gumbär Gummibärchen",
                        UnitPrice = 200.12m
                    },
                    new Product(uow) {
                        ProductName = "Perth Pasties",
                        UnitPrice = 300.13m
                    },
                    new Product(uow) {
                        ProductName = "Guaraná Fantástica",
                        UnitPrice = 400.14m
                    },
                    new Product(uow) {
                        ProductName = "Vegie-spread",
                        UnitPrice = 500.15m,
                        Picture = new byte[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
                    }
                };
                Customer[] customers = new Customer[] {
                    new Customer(uow) {
                        CustomerID="BSBEV",
                        CompanyName="B's Beverages"
                    },
                    new Customer(uow) {
                        CustomerID="OCEAN",
                        CompanyName="Océano Atlántico Ltda."
                    },
                    new Customer(uow) {
                        CustomerID="SANTG",
                        CompanyName = "Santé Gourmet"
                    },
                    new Customer(uow) {
                        CustomerID="WARTH",
                        CompanyName = "Wartian Herkku"
                    }
                };
                Order[] orders = new Order[] {
                    new Order(uow) {
                        Customer = customers[0],
                        Date = new DateTime(2018, 01, 22, 10, 00, 01),
                        OrderStatus = OrderStatus.New
                    },
                    new Order(uow) {
                        Customer = customers[1],
                        Date = new DateTime(2018, 02, 22, 23, 14, 49),
                        OrderStatus = OrderStatus.InProgress
                    },
                    new Order(uow) {
                        Customer = customers[2],
                        Date = new DateTime(2018, 03, 15, 17, 00, 01),
                        OrderStatus = OrderStatus.Completed
                    },
                    new Order(uow) {
                        Customer = customers[0],
                        Date = new DateTime(2018, 06, 01, 00, 00, 00),
                        OrderStatus = OrderStatus.Cancelled
                    },
                    new Order(uow) {
                        Date = new DateTime(2018, 06, 14, 10, 26, 00),
                        OrderStatus = OrderStatus.New
                    }
                };
                for(int i = 0; i < orders.Length - 1; i++) {
                    Order order = orders[i];
                    for(int j = 0; j < 3; j++) {
                        var product = products[(i * 17 + j * 31) % products.Length];
                        order.OrderDetails.Add(new OrderDetail(uow) {
                            Product = product,
                            Quantity = 1,
                            UnitPrice = product.UnitPrice.Value
                        });
                    }
                }
                Contract[] contracts = new Contract[] {
                    new Contract(uow) {
                        Customer = customers[0],
                        Date = new DateTime(2018, 01, 22, 10, 00, 01),
                        Number = "2018-0001"
                    },
                    new Contract(uow) {
                        Customer = customers[1],
                        Date = new DateTime(2018, 02, 22, 23, 14, 49),
                        Number = "2018-0002"
                    },
                    new Contract(uow) {
                        Customer = customers[2],
                        Date = new DateTime(2018, 03, 15, 17, 00, 01),
                        Number = "2018-0003"
                    }
                };
                contracts[0].LinkedDocuments.Add(orders[0]);
                contracts[0].LinkedDocuments.Add(orders[1]);
                contracts[0].LinkedDocuments.Add(contracts[2]);
                contracts[1].LinkedDocuments.Add(orders[2]);
                uow.CommitChanges();
            }
        }
    }
}