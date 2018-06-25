Imports System
Imports DevExpress.Xpo
Imports WebApplication1.Models
Imports DevExpress.Xpo.DB
Imports DevExpress.Xpo.Metadata

Namespace ODataService.Helpers

    Public NotInheritable Class ConnectionHelper

        Private Sub New()
        End Sub

        Private Shared persistentTypes() As Type = { GetType(Customer), GetType(OrderDetail), GetType(Order), GetType(Product) }
        Public Shared Function GetPersistentTypes() As Type()
            Dim copy(persistentTypes.Length - 1) As Type
            Array.Copy(persistentTypes, copy, persistentTypes.Length)
            Return copy
        End Function
        Public Shared ReadOnly Property ConnectionString() As String
            Get
                Return System.Configuration.ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString
            End Get
        End Property
        Public Shared Function CreateSession() As UnitOfWork
            Return New UnitOfWork() With {.IdentityMapBehavior = IdentityMapBehavior.Strong}
        End Function
        Public Shared Function CreateDataLayer(ByVal autoCreationOption As AutoCreateOption, ByVal threadSafe As Boolean) As IDataLayer
            Dim dictionary = New ReflectionDictionary()
            dictionary.NullableBehavior = NullableBehavior.ByUnderlyingType
            dictionary.GetDataStoreSchema(persistentTypes)
            Dim provider = XpoDefault.GetConnectionProvider(ConnectionString, autoCreationOption)
            If threadSafe Then
                Return New ThreadSafeDataLayer(dictionary, provider)
            Else
                Return New SimpleDataLayer(dictionary, provider)
            End If
        End Function
        Public Shared Sub EnsureDatabaseCreated()
            Using dataLayer As IDataLayer = CreateDataLayer(AutoCreateOption.DatabaseAndSchema, False)
                Using uow As New UnitOfWork(dataLayer)
                    uow.UpdateSchema(persistentTypes)
                End Using
            End Using
        End Sub
    End Class

End Namespace
