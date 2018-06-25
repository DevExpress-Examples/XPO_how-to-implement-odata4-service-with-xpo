Imports System
Imports System.Linq
Imports System.Web.Http
Imports System.Web.OData
Imports DevExpress.Xpo
Imports WebApplication1.Models
Imports ODataService.Helpers

Namespace WebApplication1.Controllers
    Public Class CustomersController
        Inherits ODataController

        Private Session As UnitOfWork

        <EnableQuery> _
        Public Function [Get]() As IQueryable(Of Customer)
            Session = ConnectionHelper.CreateSession()
            Return Session.Query(Of Customer)().AsWrappedQuery()
        End Function

        <EnableQuery> _
        Public Function [Get](<FromODataUri> ByVal key As String) As SingleResult(Of Customer)
            Session = ConnectionHelper.CreateSession()
            Dim result As IQueryable(Of Customer) = Session.Query(Of Customer)().AsWrappedQuery().Where(Function(t) t.CustomerID = key)
            Return SingleResult.Create(result)
        End Function

        <EnableQuery> _
        Public Function GetOrders(<FromODataUri> ByVal key As String) As IQueryable(Of Order)
            Session = ConnectionHelper.CreateSession()
            Return Session.Query(Of Customer)().AsWrappedQuery().Where(Function(m) m.CustomerID = key).SelectMany(Function(m) m.Orders)
        End Function


        <HttpPost> _
        Public Function Post(ByVal customer As Customer) As IHttpActionResult
            If Not ModelState.IsValid Then
                Return BadRequest()
            End If
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim entity As New Customer(uow) With { _
                    .CustomerID = customer.CustomerID, _
                    .CompanyName = customer.CompanyName _
                }
                uow.CommitChanges()
                Return Created(entity)
            End Using
        End Function

        <HttpPut> _
        Public Function Put(<FromODataUri> ByVal key As String, ByVal customer As Customer) As IHttpActionResult
            If Not ModelState.IsValid Then
                Return BadRequest()
            End If
            If key <> customer.CustomerID Then
                Return BadRequest()
            End If
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim existing As Customer = uow.GetObjectByKey(Of Customer)(key)
                If existing Is Nothing Then
                    Dim entity As New Customer(uow) With { _
                        .CustomerID = customer.CustomerID, _
                        .CompanyName = customer.CompanyName _
                    }
                    uow.CommitChanges()
                    Return Created(entity)
                Else
                    existing.CustomerID = customer.CustomerID
                    existing.CompanyName = customer.CompanyName
                    uow.CommitChanges()
                    Return Updated(customer)
                End If
            End Using
        End Function

        <HttpPatch> _
        Public Function Patch(<FromODataUri> ByVal key As String, ByVal customer As Delta(Of Customer)) As IHttpActionResult
            If Not ModelState.IsValid Then
                Return BadRequest()
            End If
            Dim result = ApiHelper.Patch(Of Customer, String)(key, customer)
            If result IsNot Nothing Then
                Return Updated(result)
            End If
            Return NotFound()
        End Function

        <HttpDelete> _
        Public Function Delete(<FromODataUri> ByVal key As String) As IHttpActionResult
            Return StatusCode(ApiHelper.Delete(Of Customer, String)(key))
        End Function

        <HttpPost, HttpPut> _
        Public Function CreateRef(<FromODataUri> ByVal key As String, ByVal navigationProperty As String, <FromBody> ByVal link As Uri) As IHttpActionResult
            Return StatusCode(ApiHelper.CreateRef(Of Customer, String)(Request, key, navigationProperty, link))
        End Function

        <HttpDelete> _
        Public Function DeleteRef(<FromODataUri> ByVal key As String, <FromODataUri> ByVal relatedKey As Integer, ByVal navigationProperty As String) As IHttpActionResult
            Return StatusCode(ApiHelper.DeleteRef(Of Customer, String, Integer)(key, relatedKey, navigationProperty))
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