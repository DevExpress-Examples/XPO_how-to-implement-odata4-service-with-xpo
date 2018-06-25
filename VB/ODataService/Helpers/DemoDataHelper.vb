Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports DevExpress.Xpo
Imports DevExpress.Xpo.DB
Imports WebApplication1.Models

Namespace ODataService.Helpers
    Public NotInheritable Class DemoDataHelper

        Private Sub New()
        End Sub


        Public Shared Sub CleanupDatabase()
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim orders As New XPCollection(Of Order)(uow)
                Dim customers As New XPCollection(Of Customer)(uow)
                Dim products As New XPCollection(Of Product)(uow)
                Dim orderDetails As New XPCollection(Of OrderDetail)(uow)
                uow.Delete(orderDetails)
                uow.Delete(orders)
                uow.Delete(products)
                uow.Delete(customers)
                uow.CommitChanges()
            End Using
        End Sub

        Public Shared Sub CreateDemoData()
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim products() As Product = { _
                    New Product(uow) With { _
                        .ProductName = "Queso Cabrales", _
                        .UnitPrice = 100.11D _
                    }, _
                    New Product(uow) With { _
                        .ProductName = "Gumbär Gummibärchen", _
                        .UnitPrice = 200.12D _
                    }, _
                    New Product(uow) With { _
                        .ProductName = "Perth Pasties", _
                        .UnitPrice = 300.13D _
                    }, _
                    New Product(uow) With { _
                        .ProductName = "Guaraná Fantástica", _
                        .UnitPrice = 400.14D _
                    }, _
                    New Product(uow) With { _
                        .ProductName = "Vegie-spread", _
                        .UnitPrice = 500.15D, _
                        .Picture = New Byte(){ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } _
                    } _
                }
                Dim customers() As Customer = { _
                    New Customer(uow) With { _
                        .CustomerID="BSBEV", _
                        .CompanyName="B's Beverages" _
                    }, _
                    New Customer(uow) With { _
                        .CustomerID="OCEAN", _
                        .CompanyName="Océano Atlántico Ltda." _
                    }, _
                    New Customer(uow) With { _
                        .CustomerID="SANTG", _
                        .CompanyName = "Santé Gourmet" _
                    }, _
                    New Customer(uow) With { _
                        .CustomerID="WARTH", _
                        .CompanyName = "Wartian Herkku" _
                    } _
                }
                Dim orders() As Order = { _
                    New Order(uow) With { _
                        .Customer = customers(0), _
                        .OrderDate = New Date(2018, 01, 22, 10, 00, 01), _
                        .OrderStatus = OrderStatus.[New] _
                    }, _
                    New Order(uow) With { _
                        .Customer = customers(1), _
                        .OrderDate = New Date(2018, 02, 22, 23, 14, 49), _
                        .OrderStatus = OrderStatus.InProgress _
                    }, _
                    New Order(uow) With { _
                        .Customer = customers(2), _
                        .OrderDate = New Date(2018, 03, 15, 17, 00, 01), _
                        .OrderStatus = OrderStatus.Completed _
                    }, _
                    New Order(uow) With { _
                        .Customer = customers(0), _
                        .OrderDate = New Date(2018, 06, 01, 00, 00, 00), _
                        .OrderStatus = OrderStatus.Cancelled _
                    }, _
                    New Order(uow) With { _
                        .OrderDate = New Date(2018, 06, 14, 10, 26, 00), _
                        .OrderStatus = OrderStatus.[New] _
                    } _
                }
                For i As Integer = 0 To orders.Length - 2
                    Dim order As Order = orders(i)
                    For j As Integer = 0 To 2
                        Dim product = products((i * 17 + j * 31) Mod products.Length)
                        order.OrderDetails.Add(New OrderDetail(uow) With { _
                            .Product = product, _
                            .Quantity = 1, _
                            .UnitPrice = product.UnitPrice.Value _
                        })
                    Next j
                Next i
                uow.CommitChanges()
            End Using
        End Sub
    End Class
End Namespace