Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web.Http
Imports System.Web.OData
Imports DevExpress.Xpo
Imports WebApplication1.Models
Imports ODataService.Helpers

Namespace WebApplication1.Controllers
    Public Class OrderDetailsController
        Inherits ODataController

        Private Session As UnitOfWork

        <EnableQuery> _
        Public Function [Get]() As IQueryable(Of OrderDetail)
            Session = ConnectionHelper.CreateSession()
            Return Session.Query(Of OrderDetail)().AsWrappedQuery()
        End Function

        <EnableQuery> _
        Public Function [Get](<FromODataUri> ByVal key As Integer) As SingleResult(Of OrderDetail)
            Session = ConnectionHelper.CreateSession()
            Dim result As IQueryable(Of OrderDetail) = Session.Query(Of OrderDetail)().AsWrappedQuery().Where(Function(t) t.OrderDetailID = key)
            Return SingleResult.Create(result)
        End Function

        <HttpPut> _
        Public Function Put(<FromODataUri> ByVal key As Integer, ByVal orderDetail As OrderDetail) As IHttpActionResult
            If Not ModelState.IsValid Then
                Return BadRequest()
            End If
            If key <> orderDetail.OrderDetailID Then
                Return BadRequest()
            End If
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim existing As OrderDetail = uow.GetObjectByKey(Of OrderDetail)(key)
                If existing Is Nothing Then
                    Dim entity As New OrderDetail(uow) With { _
                        .Order = orderDetail.Order, _
                        .Quantity = orderDetail.Quantity, _
                        .UnitPrice = orderDetail.UnitPrice _
                    }
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

        <HttpPatch> _
        Public Function Patch(<FromODataUri> ByVal key As Integer, ByVal orderDetail As Delta(Of OrderDetail)) As IHttpActionResult
            If Not ModelState.IsValid Then
                Return BadRequest()
            End If
            Dim result = ApiHelper.Patch(Of OrderDetail, Integer)(key, orderDetail)
            If result IsNot Nothing Then
                Return Updated(result)
            End If
            Return NotFound()
        End Function

        <HttpDelete> _
        Public Function Delete(<FromODataUri> ByVal key As Integer) As IHttpActionResult
            Return StatusCode(ApiHelper.Delete(Of OrderDetail, Integer)(key))
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