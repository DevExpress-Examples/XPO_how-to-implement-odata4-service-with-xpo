Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports [Default]
Imports NUnit.Framework
Imports WebApplication1.Models

Namespace Tests

    <TestFixture> _
    Public Class SelectOperationsTests
        Inherits ODataTestsBase

        <Test> _
        Public Sub SelectSimple()
            Dim container As Container = GetODataContainer()
            Dim customers = container.Customers.ToList()

            Assert.AreEqual(4, customers.Count)
            Assert.True(customers.Exists(Function(c) c.CustomerID = "BSBEV" AndAlso c.CompanyName = "B's Beverages"))
            Assert.True(customers.Exists(Function(c) c.CustomerID = "OCEAN" AndAlso c.CompanyName = "Océano Atlántico Ltda."))
            Assert.True(customers.Exists(Function(c) c.CustomerID = "SANTG" AndAlso c.CompanyName = "Santé Gourmet"))
            Assert.True(customers.Exists(Function(c) c.CustomerID = "WARTH" AndAlso c.CompanyName = "Wartian Herkku"))
        End Sub

        <Test> _
        Public Sub GetCount()
            Dim container As Container = GetODataContainer()
            Dim count As Integer = container.Customers.Count()
            Assert.AreEqual(4, count)
        End Sub

        <Test> _
        Public Sub FilterAndSort()
            Dim container As Container = GetODataContainer()
            Dim customers = container.Customers.Where(Function(c) String.Compare(c.CustomerID, "C") >= 0).OrderByDescending(Function(c) c.CompanyName).ToList()

            Assert.AreEqual(3, customers.Count)
            Assert.IsTrue(customers(0).CustomerID = "WARTH")
            Assert.IsTrue(customers(1).CustomerID = "SANTG")
            Assert.IsTrue(customers(2).CustomerID = "OCEAN")
        End Sub

        <Test> _
        Public Sub SkipAndTake()
            Dim container As Container = GetODataContainer()
            Dim products = container.Products.OrderBy(Function(t) t.UnitPrice).Skip(1).Take(2).ToList()

            Assert.AreEqual(2, products.Count)
            Assert.AreEqual(200.12D, products(0).UnitPrice)
            Assert.AreEqual(300.13D, products(1).UnitPrice)
        End Sub

        <Test> _
        Public Sub FilterByNull()
            Dim container As Container = GetODataContainer()
            Dim orders = container.Orders.Where(Function(o) o.Customer Is Nothing).ToList()
            Assert.AreEqual(1, orders.Count)
        End Sub

        <Test> _
        Public Sub FilterByNotNull()
            Dim container As Container = GetODataContainer()
            Dim orders = container.Orders.Where(Function(o) o.Customer IsNot Nothing).ToList()
            Assert.AreEqual(4, orders.Count)
        End Sub

        <Test> _
        Public Sub FilterByEnum()
            Dim container As Container = GetODataContainer()
            Dim orders = container.Orders.Where(Function(o) o.OrderStatus = OrderStatus.[New]).ToList()
            Assert.AreEqual(2, orders.Count)
        End Sub

        <Test> _
        Public Sub Expand()
            Dim container As Container = GetODataContainer()
            Dim orders = container.Orders.Expand(Function(o) o.Customer).Expand("OrderDetails($expand=Product)").ToList()

            orders = orders.OrderBy(Function(o) o.OrderDate).ToList()
            Assert.AreEqual(5, orders.Count)
            Assert.AreEqual("BSBEV", orders(0).Customer.CustomerID)
            Assert.AreEqual("OCEAN", orders(1).Customer.CustomerID)
            Assert.AreEqual("SANTG", orders(2).Customer.CustomerID)
            Assert.AreEqual("BSBEV", orders(3).Customer.CustomerID)
            Assert.IsNull(orders(4).Customer)
            Assert.AreEqual(0, orders(4).OrderDetails.Count)
            For i As Integer = 0 To 3
                Assert.AreEqual(3, orders(i).OrderDetails.Count)
                For j As Integer = 0 To 2
                    Assert.IsNotNull(orders(i).OrderDetails(j).Product)
                Next j
            Next i
        End Sub

        <Test> _
        Public Sub FilterAndSortWithExpand()
            Dim container As Container = GetODataContainer()
            Dim orders = container.Orders.Expand(Function(o) o.Customer).Expand("OrderDetails($expand=Product)").Where(Function(o) o.Customer.CustomerID <> "OCEAN").OrderBy(Function(o) o.Customer.CompanyName).ThenBy(Function(o) o.OrderDate).ToList()

            Assert.AreEqual(3, orders.Count)
            Assert.AreEqual("BSBEV", orders(0).Customer.CustomerID)
            Assert.AreEqual("BSBEV", orders(1).Customer.CustomerID)
            Assert.AreEqual("SANTG", orders(2).Customer.CustomerID)
            For i As Integer = 0 To 2
                Assert.AreEqual(3, orders(i).OrderDetails.Count)
                For j As Integer = 0 To 2
                    Assert.IsNotNull(orders(i).OrderDetails(j).Product)
                Next j
            Next i
        End Sub

        <Test> _
        Public Sub FilterByChildCollections()
            Dim container As Container = GetODataContainer()
            Dim orders = container.Orders.Expand("OrderDetails($expand=Product)").Where(Function(o) o.OrderDetails.Any(Function(t) t.Product.ProductName = "Queso Cabrales")).ToList()

            Assert.AreEqual(2, orders.Count)
            For i As Integer = 0 To 1
                Assert.IsTrue(orders(i).OrderDetails.Any(Function(d) d.Product.ProductName = "Queso Cabrales"))
            Next i
        End Sub

        <Test> _
        Public Sub FilterByDateTime()
            Dim startDate As New DateTimeOffset(New Date(2018, 03, 01))
            Dim endDate As New DateTimeOffset(New Date(2018, 06, 01))
            Dim container As Container = GetODataContainer()
            Dim orders = container.Orders.Where(Function(o) o.OrderDate > startDate AndAlso o.OrderDate <= endDate).ToList()

            Assert.AreEqual(2, orders.Count)
            For i As Integer = 0 To orders.Count - 1
                Assert.Greater(orders(i).OrderDate, startDate)
                Assert.LessOrEqual(orders(i).OrderDate, endDate)
            Next i
        End Sub

        <Test> _
        Public Sub FilterByDateTimePart()
            Dim container As Container = GetODataContainer()
            Dim orders = container.Orders.Where(Function(o) o.OrderDate.Value.Year = 2018 AndAlso (o.OrderDate.Value.Month = 3 OrElse o.OrderDate.Value.Month = 6)).ToList()
            Assert.AreEqual(3, orders.Count)
        End Sub

        <Test> _
        Public Sub SelectBlobValues()
            Dim container As Container = GetODataContainer()

            Dim product_Renamed = container.Products.Where(Function(t) t.ProductName = "Vegie-spread").First()

            Assert.IsNotNull(product_Renamed.Picture)
            Assert.AreEqual(10, product_Renamed.Picture.Length)
            For i As Integer = 0 To 9
                Assert.AreEqual(i + 1, product_Renamed.Picture(i))
            Next i
        End Sub

        <Test> _
        Public Sub SelectWithProjection()
            Dim container As Container = GetODataContainer()
            Dim orders = container.Orders.Expand(Function(t) t.Customer).OrderBy(Function(t) t.OrderDate).ToList()
            Dim projected = container.Orders.OrderBy(Function(t) t.OrderDate).Select(Function(t) New With { _
                Key .OrderID = t.OrderID, _
                Key .OrderDate = t.OrderDate, _
                Key .Customer = If(t.Customer IsNot Nothing, t.Customer.CompanyName, Nothing) _
            }).ToList()

            Assert.AreEqual(orders.Count, projected.Count)
            For i As Integer = 0 To orders.Count - 1
                Assert.AreEqual(orders(i).OrderID, projected(i).OrderID)
                Assert.AreEqual(orders(i).OrderDate, projected(i).OrderDate)
                Assert.AreEqual(orders(i).Customer?.CompanyName, projected(i).Customer)
            Next i
        End Sub

        <Test> _
        Public Sub SelectWithProjectionAndFunctions()
            Dim container As Container = GetODataContainer()
            Dim orders = container.Orders.Expand(Function(t) t.Customer).OrderBy(Function(t) t.OrderDate).ToList()
            Dim projected = container.Orders.OrderBy(Function(t) t.OrderDate).Select(Function(t) New With { _
                Key .Year = t.OrderDate.Value.Year, _
                Key .Customer = If(t.Customer IsNot Nothing, t.Customer.CompanyName.ToUpper(), Nothing) _
            }).ToList()

            Assert.AreEqual(orders.Count, projected.Count)
            For i As Integer = 0 To orders.Count - 1
                Assert.AreEqual(orders(i).OrderDate.Value.Year, projected(i).Year)
                Assert.AreEqual(orders(i).Customer?.CompanyName.ToUpper(), projected(i).Customer)
            Next i
        End Sub

        <Test> _
        Public Sub ResourceReferenceProperty()
            Dim container As Container = GetODataContainer()
            Dim orderId As Integer = container.Orders.Where(Function(t) t.OrderDate = New DateTimeOffset(New Date(2018, 06, 01))).First().OrderID

            Dim details = container.Orders.ByKey(orderId).OrderDetails.Expand(Function(t) t.Product).ToList()
            Assert.AreEqual(3, details.Count)
        End Sub
    End Class
End Namespace
