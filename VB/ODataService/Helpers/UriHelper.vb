Imports Microsoft.OData
Imports Microsoft.OData.UriParser
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net.Http
Imports System.Web.Http.Routing
Imports System.Web.OData
Imports System.Web.OData.Extensions
Imports System.Web.OData.Routing

Namespace ODataService.Helpers
    Public NotInheritable Class UriHelper

        Private Sub New()
        End Sub

        Public Shared Function GetKeyFromUri(Of TKey)(ByVal request As HttpRequestMessage, ByVal uri As Uri) As TKey
            If uri Is Nothing Then
                Throw New ArgumentNullException("uri")
            End If
            Dim urlHelper = If(request.GetUrlHelper(), New UrlHelper(request))
            Dim pathHandler = CType(request.GetRequestContainer().GetService(GetType(IODataPathHandler)), IODataPathHandler)
            Dim serviceRoot As String = urlHelper.CreateODataLink(request.ODataProperties().RouteName, pathHandler, New List(Of ODataPathSegment)())
            Dim odataPath = pathHandler.Parse(serviceRoot, uri.LocalPath, request.GetRequestContainer())
            Dim keySegment = odataPath.Segments.OfType(Of KeySegment)().FirstOrDefault()
            If keySegment Is Nothing Then
                Throw New InvalidOperationException("The link does not contain a key.")
            End If
            Dim value = keySegment.Keys.FirstOrDefault().Value
            Return CType(value, TKey)
        End Function

        Public Shared Function GetServiceRootUri(ByVal controller As ODataController) As String
            Dim routeName = controller.Request.ODataProperties().RouteName
            Dim odataRoute As ODataRoute = TryCast(controller.Configuration.Routes(routeName), ODataRoute)
            Dim prefixName = odataRoute.RoutePrefix
            If Not String.IsNullOrEmpty(prefixName) Then
                Dim requestUri = controller.Request.RequestUri.ToString()
                Dim serviceRootUri = requestUri.Substring(0, requestUri.IndexOf(prefixName, StringComparison.InvariantCultureIgnoreCase) + prefixName.Length)
                Return serviceRootUri
            Else
                Return controller.Url.Content("~/")
            End If
        End Function
    End Class
End Namespace