Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Threading.Tasks
Imports System.Web
Imports System.Web.Http
Imports System.Web.OData
Imports System.Web.OData.Extensions
Imports System.Web.OData.Routing
Imports DevExpress.Xpo
Imports WebApplication1.Models
Imports ODataService.Helpers

Namespace WebApplication1.Controllers
    Public Class OrdersController
        Inherits ODataController

        Private Session As UnitOfWork

        <EnableQuery> _
        Public Function [Get]() As IQueryable(Of Order)
            Session = ConnectionHelper.CreateSession()
            Return Session.Query(Of Order)().AsWrappedQuery()
        End Function

        <EnableQuery> _
        Public Function [Get](<FromODataUri> ByVal key As Integer) As SingleResult(Of Order)
            Session = ConnectionHelper.CreateSession()
            Dim result As IQueryable(Of Order) = Session.Query(Of Order)().AsWrappedQuery().Where(Function(t) t.OrderID = key)
            Return SingleResult.Create(result)
        End Function

        <EnableQuery> _
        Public Function GetCustomerID(<FromODataUri> ByVal key As Integer) As SingleResult(Of Customer)
            Session = ConnectionHelper.CreateSession()
            Dim result = Session.Query(Of Order)().AsWrappedQuery().Where(Function(m) m.OrderID = key).Select(Function(m) m.Customer)
            Return SingleResult.Create(result)
        End Function

        <EnableQuery> _
        Public Function GetOrderDetails(<FromODataUri> ByVal key As Integer) As IQueryable(Of OrderDetail)
            Session = ConnectionHelper.CreateSession()
            Return Session.Query(Of OrderDetail)().AsWrappedQuery().Where(Function(t) t.Order.OrderID = key)
        End Function

        <HttpPost> _
        Public Function Post(ByVal order As Order) As IHttpActionResult
            If Not ModelState.IsValid Then
                Return BadRequest()
            End If
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim entity As New Order(uow) With { _
                    .OrderID = order.OrderID, _
                    .OrderDate = order.OrderDate, _
                    .OrderStatus = order.OrderStatus _
                }
                uow.CommitChanges()
                Return Created(entity)
            End Using
        End Function

        <HttpPut> _
        Public Function Put(<FromODataUri> ByVal key As Integer, ByVal order As Order) As IHttpActionResult
            If Not ModelState.IsValid Then
                Return BadRequest()
            End If
            If key <> order.OrderID Then
                Return BadRequest()
            End If
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim existing As Order = uow.GetObjectByKey(Of Order)(key)
                If existing Is Nothing Then
                    Dim entity As New Order(uow) With { _
                        .OrderID = order.OrderID, _
                        .OrderDate = order.OrderDate, _
                        .OrderStatus = order.OrderStatus _
                    }
                    uow.CommitChanges()
                    Return Created(entity)
                Else
                    existing.OrderDate = order.OrderDate
                    uow.CommitChanges()
                    Return Updated(existing)
                End If
            End Using
        End Function

        <HttpPatch> _
        Public Function Patch(<FromODataUri> ByVal key As Integer, ByVal order As Delta(Of Order)) As IHttpActionResult
            If Not ModelState.IsValid Then
                Return BadRequest()
            End If
            Dim result = ApiHelper.Patch(Of Order, Integer)(key, order)
            If result IsNot Nothing Then
                Return Updated(result)
            End If
            Return NotFound()
        End Function

        <HttpDelete> _
        Public Function Delete(<FromODataUri> ByVal key As Integer) As IHttpActionResult
            Return StatusCode(ApiHelper.Delete(Of Order, Integer)(key))
        End Function

        <HttpPost, HttpPut, ODataRoute("Orders({key})/OrderDetails")> _
        Public Function AddToOrderDetails(<FromODataUri> ByVal key As Integer, ByVal orderDetail As OrderDetail) As IHttpActionResult
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim order As Order = uow.GetObjectByKey(Of Order)(key)
                If order Is Nothing Then
                    Return NotFound()
                End If
                Dim existing As OrderDetail = order.OrderDetails.FirstOrDefault(Function(d) d.OrderDetailID = orderDetail.OrderDetailID)
                If existing Is Nothing Then
                    Dim entity As New OrderDetail(uow) With { _
                        .Quantity = orderDetail.Quantity, _
                        .UnitPrice = orderDetail.UnitPrice _
                    }
                    order.OrderDetails.Add(entity)
                    uow.CommitChanges()
                    Return Created(entity)
                Else
                    existing.Quantity = orderDetail.Quantity
                    existing.UnitPrice = orderDetail.UnitPrice
                    uow.CommitChanges()
                    Return Updated(existing)
                End If
            End Using
        End Function

        <HttpDelete> _
        Public Function DeleteRef(<FromODataUri> ByVal key As Integer, <FromODataUri> ByVal relatedKey As Integer, ByVal navigationProperty As String) As IHttpActionResult
            Return StatusCode(ApiHelper.DeleteRef(Of Order, Integer, Integer)(key, relatedKey, navigationProperty))
        End Function

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            MyBase.Dispose(disposing)
            If disposing Then
                If Session IsNot Nothing Then
                    Session.Dispose()
                    Session = Nothing
                End If
            End If
        End Sub
    End Class
End Namespace