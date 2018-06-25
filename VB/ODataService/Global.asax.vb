Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.Http
Imports System.Web.Http.Validation
Imports System.Web.ModelBinding
Imports System.Web.Routing
Imports DevExpress.Xpo
Imports DevExpress.Xpo.DB
Imports ODataService.Helpers
Imports WebApplication1.Models

Namespace WebApplication1
    Public Class WebApiApplication
        Inherits System.Web.HttpApplication

        Protected Sub Application_Start()
            GlobalConfiguration.Configuration.Services.Replace(GetType(IBodyModelValidator), New CustomBodyModelValidator())
            GlobalConfiguration.Configure(AddressOf WebApiConfig.Register)
            ConnectionHelper.EnsureDatabaseCreated()
            XpoDefault.DataLayer = ConnectionHelper.CreateDataLayer(AutoCreateOption.SchemaAlreadyExists, True)
        End Sub

        Public Class CustomBodyModelValidator
            Inherits DefaultBodyModelValidator

            Private ReadOnly persistentTypes As HashSet(Of Type)
            Public Sub New()
                persistentTypes = New HashSet(Of Type)(ConnectionHelper.GetPersistentTypes())
            End Sub
            Public Overrides Function ShouldValidateType(ByVal type As Type) As Boolean
                Return Not persistentTypes.Contains(type)
            End Function
        End Class
    End Class
End Namespace
