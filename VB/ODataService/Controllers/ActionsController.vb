Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Threading.Tasks
Imports System.Web
Imports System.Web.Http
Imports DevExpress.Xpo
Imports WebApplication1.Models
Imports ODataService.Helpers
Imports System.Web.OData
Imports System.Web.OData.Routing

Namespace WebApplication1.Controllers
    Public Class ActionsController
        Inherits ODataController

        <ODataRoute("InitializeDatabase")> _
        Public Function InitializeDatabase() As IHttpActionResult
            DemoDataHelper.CleanupDatabase()
            DemoDataHelper.CreateDemoData()
            Return Ok()
        End Function

        <HttpGet, ODataRoute("TotalSalesByYear(year={year})")> _
        Public Function TotalSalesByYear(ByVal year As Integer) As IHttpActionResult
            Using uow As New UnitOfWork()
                Dim result As Decimal = uow.Query(Of Order)().Where(Function(o) o.OrderDate.Value.Year = year).Sum(Function(o) o.OrderDetails.Sum(Function(d) d.Quantity * d.UnitPrice))
                Return Ok(result)
            End Using
        End Function
    End Class
End Namespace