Imports System
Imports System.Linq
Imports System.Net
Imports System.Web.Http
Imports System.Web.OData
Imports DevExpress.Xpo
Imports WebApplication1.Models
Imports ODataService.Helpers

Namespace WebApplication1.Controllers
    Public Class ProductsController
        Inherits ODataController

        Private Session As UnitOfWork

        <EnableQuery> _
        Public Function [Get]() As IQueryable(Of Product)
            Session = ConnectionHelper.CreateSession()
            Return Session.Query(Of Product)().AsWrappedQuery()
        End Function

        <EnableQuery> _
        Public Function [Get](<FromODataUri> ByVal key As Integer) As SingleResult(Of Product)
            Session = ConnectionHelper.CreateSession()
            Dim result As IQueryable(Of Product) = Session.Query(Of Product)().AsWrappedQuery().Where(Function(t) t.ProductID = key)
            Return SingleResult.Create(result)
        End Function

        <HttpPost> _
        Public Function Post(ByVal product As Product) As IHttpActionResult
            If Not ModelState.IsValid Then
                Return BadRequest()
            End If
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim entity As New Product(uow) With { _
                    .ProductName = product.ProductName, _
                    .Picture = product.Picture _
                }
                uow.CommitChanges()
                Return Created(entity)
            End Using
        End Function

        <HttpPut> _
        Public Function Put(<FromODataUri> ByVal key As Integer, ByVal product As Product) As IHttpActionResult
            If Not ModelState.IsValid Then
                Return BadRequest()
            End If
            If key <> product.ProductID Then
                Return BadRequest()
            End If
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim existing As Product = uow.GetObjectByKey(Of Product)(key)
                If existing Is Nothing Then
                    Dim entity As New Product(uow) With { _
                        .ProductName = product.ProductName, _
                        .Picture = product.Picture _
                    }
                    uow.CommitChanges()
                    Return Created(entity)
                Else
                    existing.ProductName = product.ProductName
                    existing.Picture = product.Picture
                    uow.CommitChanges()
                    Return Updated(product)
                End If
            End Using
        End Function

        <HttpPatch> _
        Public Function Patch(<FromODataUri> ByVal key As Integer, ByVal product As Delta(Of Product)) As IHttpActionResult
            If Not ModelState.IsValid Then
                Return BadRequest()
            End If
            Dim result = ApiHelper.Patch(Of Product, Integer)(key, product)
            If result IsNot Nothing Then
                Return Updated(result)
            End If
            Return NotFound()
        End Function

        <HttpDelete> _
        Public Function Delete(<FromODataUri> ByVal key As Integer) As IHttpActionResult
            Return StatusCode(ApiHelper.Delete(Of Product, Integer)(key))
        End Function

        <HttpPost, HttpPut> _
        Public Function CreateRef(<FromODataUri> ByVal key As Integer, ByVal navigationProperty As String, <FromBody> ByVal link As Uri) As IHttpActionResult
            Return StatusCode(ApiHelper.CreateRef(Of Product, Integer)(Request, key, navigationProperty, link))
        End Function

        <HttpDelete> _
        Public Function DeleteRef(<FromODataUri> ByVal key As Integer, <FromODataUri> ByVal relatedKey As Integer, ByVal navigationProperty As String) As IHttpActionResult
            Return StatusCode(ApiHelper.DeleteRef(Of Product, Integer, Integer)(key, relatedKey, navigationProperty))
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