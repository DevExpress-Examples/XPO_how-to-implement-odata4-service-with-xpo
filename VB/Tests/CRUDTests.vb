Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports [Default]
Imports NUnit.Framework
Imports WebApplication1.Models

Namespace Tests
    <TestFixture> _
    Public Class CRUDOperationTests
        Inherits ODataTestsBase

        <Test> _
        Public Sub CreateObject()
            Dim container As Container = GetODataContainer()

            Dim customer_Renamed As New Customer() With { _
                .CustomerID = "C0001", _
                .CompanyName = "Test Company" _
            }
            container.AddToCustomers(customer_Renamed)
            Dim response = container.SaveChanges()

            Assert.AreEqual(1, response.Count())
            Assert.AreEqual(CInt(HttpStatusCode.Created), response.First().StatusCode)

            container = GetODataContainer()
            Dim createdItem As Customer = container.Customers.Where(Function(t) t.CustomerID = customer_Renamed.CustomerID).First()
            Assert.AreEqual(customer_Renamed.CustomerID, createdItem.CustomerID)
            Assert.AreEqual(customer_Renamed.CompanyName, createdItem.CompanyName)
        End Sub

        <Test> _
        Public Sub UpdateObject()
            Dim container As Container = GetODataContainer()

            Dim customer_Renamed As Customer = container.Customers.Where(Function(t) t.CustomerID = "BSBEV").First()

            customer_Renamed.CompanyName = "Test Company Renamed"
            container.UpdateObject(customer_Renamed)
            Dim response = container.SaveChanges()

            Assert.AreEqual(1, response.Count())
            Assert.AreEqual(CInt(HttpStatusCode.NoContent), response.First().StatusCode)

            container = GetODataContainer()
            Dim updatedItem As Customer = container.Customers.Where(Function(t) t.CustomerID = "BSBEV").First()
            Assert.AreEqual(customer_Renamed.CustomerID, updatedItem.CustomerID)
            Assert.AreEqual(customer_Renamed.CompanyName, updatedItem.CompanyName)
        End Sub

        <Test> _
        Public Sub BatchUpdate()
            Dim container As Container = GetODataContainer()
            Dim customers = container.Customers.ToList()

            For Each customer_Renamed In customers
                customer_Renamed.CompanyName &= " -renamed"
                container.UpdateObject(customer_Renamed)
            Next customer_Renamed
            Dim response = container.SaveChanges(Microsoft.OData.Client.SaveChangesOptions.BatchWithSingleChangeset)

            Assert.AreEqual(4, response.Count())
            For Each res In response
                Assert.AreEqual(CInt(HttpStatusCode.NoContent), res.StatusCode)
            Next res

            container = GetODataContainer()
            customers = container.Customers.ToList()

            For Each customer_Renamed In customers
                Assert.IsTrue(customer_Renamed.CompanyName.EndsWith(" -renamed"))
            Next customer_Renamed
        End Sub

        <Test> _
        Public Sub DeleteObject()
            Dim container As Container = GetODataContainer()

            Dim customer_Renamed As Customer = container.Customers.Where(Function(t) t.CustomerID = "WARTH").First()

            container.DeleteObject(customer_Renamed)
            Dim response = container.SaveChanges()

            Assert.AreEqual(1, response.Count())
            Assert.AreEqual(CInt(HttpStatusCode.NoContent), response.First().StatusCode)
        End Sub

        <Test> _
        Public Sub CreateObjectWithBlobField()
            Dim pictureData((256 * 256 * 4) - 1) As Byte
            For i As Integer = 0 To pictureData.Length - 1
                pictureData(i) = CByte(i)
            Next i
            Dim container As Container = GetODataContainer()

            Dim product_Renamed As New Product() With { _
                .ProductName = "test product", _
                .Picture = pictureData _
            }
            container.AddToProducts(product_Renamed)
            Dim response = container.SaveChanges()

            Assert.AreEqual(1, response.Count())
            Assert.AreEqual(CInt(HttpStatusCode.Created), response.First().StatusCode)

            container = GetODataContainer()
            Dim createdItem As Product = container.Products.Where(Function(t) t.ProductName = product_Renamed.ProductName).First()

            Assert.IsNotNull(createdItem.Picture)
            Assert.AreEqual(product_Renamed.Picture.Length, createdItem.Picture.Length)
        End Sub

        <Test> _
        Public Sub UpdateObjectWithBlobField()
            Dim pictureData((256 * 256 * 4) - 1) As Byte
            For i As Integer = 0 To pictureData.Length - 1
                pictureData(i) = CByte(i)
            Next i
            Dim container As Container = GetODataContainer()

            Dim product_Renamed = container.Products.Where(Function(t) t.ProductName = "Queso Cabrales").First()
            Dim oldPicture() As Byte = product_Renamed.Picture
            product_Renamed.Picture = pictureData
            container.UpdateObject(product_Renamed)
            Dim response = container.SaveChanges()

            Assert.AreEqual(1, response.Count())
            Assert.AreEqual(CInt(HttpStatusCode.NoContent), response.First().StatusCode)

            container = GetODataContainer()
            Dim updatedItem As Product = container.Products.Where(Function(t) t.ProductID = product_Renamed.ProductID).First()
            Assert.IsNull(oldPicture)
            Assert.IsNotNull(updatedItem.Picture)
            Assert.AreEqual(pictureData.Length, updatedItem.Picture.Length)
        End Sub

        <Test> _
        Public Sub AddRelatedObject()
            Dim container As Container = GetODataContainer()

            Dim order_Renamed As Order = container.Orders.OrderByDescending(Function(t) t.OrderID).First()

            Dim detail As New OrderDetail() With { _
                .Quantity = 105, _
                .UnitPrice = 201.37D _
            }
            container.AddRelatedObject(order_Renamed, "OrderDetails", detail)
            Dim response = container.SaveChanges()

            Assert.AreEqual(1, response.Count())
            Assert.AreEqual(CInt(HttpStatusCode.Created), response.First().StatusCode)

            container = GetODataContainer()
            Dim createdItem As OrderDetail = container.OrderDetails.OrderByDescending(Function(t) t.OrderDetailID).First()
            Assert.AreEqual(detail.Quantity, createdItem.Quantity)
            Assert.AreEqual(detail.UnitPrice, createdItem.UnitPrice)
            Assert.Greater(createdItem.OrderDetailID, 0)
        End Sub

        <Test> _
        Public Sub UpdateRelatedObject()
            Dim container As Container = GetODataContainer()

            Dim order_Renamed As Order = container.Orders.Expand(Function(t) t.OrderDetails).OrderBy(Function(t) t.OrderDate).First()
            Dim detail As OrderDetail = order_Renamed.OrderDetails.OrderBy(Function(t) t.OrderDetailID).First()

            Dim oldQuantity As Short = detail.Quantity
            detail.Quantity += 1
            container.UpdateObject(detail)
            Dim response = container.SaveChanges()

            Assert.AreEqual(1, response.Count())
            Assert.AreEqual(CInt(HttpStatusCode.NoContent), response.First().StatusCode)

            container = GetODataContainer()
            Dim updatedItem As OrderDetail = container.Orders.Expand(Function(t) t.OrderDetails).Where(Function(t) t.OrderID = order_Renamed.OrderID).First().OrderDetails.Where(Function(d) d.OrderDetailID = detail.OrderDetailID).First()
            Assert.AreNotEqual(oldQuantity, updatedItem.Quantity)
            Assert.AreEqual(detail.Quantity, updatedItem.Quantity)
        End Sub

        <Test> _
        Public Sub DeleteRelatedObject()
            Dim container As Container = GetODataContainer()

            Dim order_Renamed As Order = container.Orders.Expand(Function(t) t.OrderDetails).OrderBy(Function(t) t.OrderDate).First()
            Dim detail As OrderDetail = order_Renamed.OrderDetails.OrderBy(Function(t) t.OrderDetailID).First()

            container.DeleteLink(order_Renamed, "OrderDetails", detail)
            Dim response = container.SaveChanges()

            Assert.AreEqual(1, response.Count())
            Assert.AreEqual(CInt(HttpStatusCode.NoContent), response.First().StatusCode)

            container = GetODataContainer()
            Dim updatedItem As Order = container.Orders.Expand(Function(t) t.OrderDetails).Where(Function(t) t.OrderID = order_Renamed.OrderID).First()
            Assert.False(updatedItem.OrderDetails.Any(Function(d) d.OrderDetailID = detail.OrderDetailID))
        End Sub

        <Test> _
        Public Sub DeleteAggregatedCollection()
            Dim container As Container = GetODataContainer()

            Dim order_Renamed As Order = container.Orders.Expand(Function(t) t.OrderDetails).OrderBy(Function(t) t.OrderDate).First()

            container.DeleteObject(order_Renamed)
            Dim response = container.SaveChanges()

            Assert.AreEqual(1, response.Count())
            Assert.AreEqual(CInt(HttpStatusCode.NoContent), response.First().StatusCode)

            container = GetODataContainer()
            Dim details = container.OrderDetails.Where(Function(t) t.Order.OrderID = order_Renamed.OrderID).ToList()
            Assert.AreEqual(0, details.Count)
        End Sub

        <Test> _
        Public Sub AddLink()
            Dim container As Container = GetODataContainer()

            Dim order_Renamed As Order = container.Orders.Where(Function(t) t.Customer Is Nothing).OrderByDescending(Function(t) t.OrderID).First()

            Dim customer_Renamed As Customer = container.Customers.Where(Function(t) t.CustomerID = "BSBEV").First()

            container.AddLink(customer_Renamed, "Orders", order_Renamed)
            Dim response = container.SaveChanges()

            Assert.AreEqual(1, response.Count())
            Assert.AreEqual(CInt(HttpStatusCode.NoContent), response.First().StatusCode)

            container = GetODataContainer()
            Dim updatedItem As Order = container.Orders.Expand(Function(t) t.Customer).Where(Function(t) t.OrderID = order_Renamed.OrderID).First()
            Assert.IsNull(order_Renamed.Customer)
            Assert.NotNull(updatedItem.Customer)
            Assert.AreEqual(updatedItem.Customer.CustomerID, customer_Renamed.CustomerID)
        End Sub

        <Test> _
        Public Sub UpdateLink()
            Dim container As Container = GetODataContainer()

            Dim order_Renamed As Order = container.Orders.Where(Function(t) t.Customer.CustomerID = "BSBEV").OrderByDescending(Function(t) t.OrderID).First()

            Dim customer_Renamed As Customer = container.Customers.Where(Function(t) t.CustomerID = "SANTG").First()

            container.AddLink(customer_Renamed, "Orders", order_Renamed)
            Dim response = container.SaveChanges()

            Assert.AreEqual(1, response.Count())
            Assert.AreEqual(CInt(HttpStatusCode.NoContent), response.First().StatusCode)

            container = GetODataContainer()
            Dim updatedItem As Order = container.Orders.Expand(Function(t) t.Customer).Where(Function(t) t.OrderID = order_Renamed.OrderID).First()
            Assert.AreEqual(updatedItem.Customer.CustomerID, customer_Renamed.CustomerID)
        End Sub

        <Test> _
        Public Sub DeleteLink()
            Dim container As Container = GetODataContainer()

            Dim order_Renamed As Order = container.Orders.Expand(Function(t) t.Customer).Where(Function(t) t.Customer.CustomerID = "BSBEV").OrderByDescending(Function(t) t.OrderID).First()

            container.DeleteLink(order_Renamed.Customer, "Orders", order_Renamed)
            Dim response = container.SaveChanges()

            Assert.AreEqual(1, response.Count())
            Assert.AreEqual(CInt(HttpStatusCode.NoContent), response.First().StatusCode)

            container = GetODataContainer()
            Dim updatedItem As Order = container.Orders.Expand(Function(t) t.Customer).Where(Function(t) t.OrderID = order_Renamed.OrderID).First()
            Assert.IsNotNull(order_Renamed.Customer)
            Assert.IsNull(updatedItem.Customer)
        End Sub
    End Class
End Namespace
