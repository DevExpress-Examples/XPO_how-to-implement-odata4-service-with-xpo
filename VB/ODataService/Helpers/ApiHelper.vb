Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Web
Imports System.Web.Http
Imports System.Web.OData
Imports DevExpress.Xpo

Namespace ODataService.Helpers
    Public NotInheritable Class ApiHelper

        Private Sub New()
        End Sub

        Public Shared Function Patch(Of TEntity As Class, TKey)(ByVal key As TKey, ByVal delta As Delta(Of TEntity)) As TEntity
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim existing As TEntity = uow.GetObjectByKey(Of TEntity)(key)
                If existing IsNot Nothing Then
                    delta.CopyChangedValues(existing)
                    uow.CommitChanges()
                End If
                Return existing
            End Using
        End Function

        Public Shared Function Delete(Of TEntity, TKey)(ByVal key As TKey) As HttpStatusCode
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim existing As TEntity = uow.GetObjectByKey(Of TEntity)(key)
                If existing Is Nothing Then
                    Return HttpStatusCode.NotFound
                End If
                uow.Delete(existing)
                uow.CommitChanges()
                Return HttpStatusCode.NoContent
            End Using
        End Function

        Public Shared Function CreateRef(Of TEntity, TKey)(ByVal request As HttpRequestMessage, ByVal key As TKey, ByVal navigationProperty As String, ByVal link As Uri) As HttpStatusCode
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim entity As TEntity = uow.GetObjectByKey(Of TEntity)(key)
                If entity Is Nothing Then
                    Return HttpStatusCode.NotFound
                End If
                Dim classInfo = uow.GetClassInfo(Of TEntity)()
                Dim memberInfo = classInfo.FindMember(navigationProperty)
                If memberInfo Is Nothing OrElse Not memberInfo.IsAssociationList Then
                    Return HttpStatusCode.BadRequest
                End If
                Dim relatedKey As Integer = UriHelper.GetKeyFromUri(Of Integer)(request, link)
                Dim reference = uow.GetObjectByKey(memberInfo.CollectionElementType, relatedKey)
                Dim collection = DirectCast(memberInfo.GetValue(entity), IList)
                collection.Add(reference)
                uow.CommitChanges()
                Return HttpStatusCode.NoContent
            End Using
        End Function

        Public Shared Function DeleteRef(Of TEntity, TKey, TRelatedKey)(ByVal key As TKey, ByVal relatedKey As TRelatedKey, ByVal navigationProperty As String) As HttpStatusCode
            Using uow As UnitOfWork = ConnectionHelper.CreateSession()
                Dim entity As TEntity = uow.GetObjectByKey(Of TEntity)(key)
                If entity Is Nothing Then
                    Return HttpStatusCode.NotFound
                End If
                Dim classInfo = uow.GetClassInfo(Of TEntity)()
                Dim memberInfo = classInfo.FindMember(navigationProperty)
                If memberInfo Is Nothing OrElse Not memberInfo.IsAssociationList Then
                    Return HttpStatusCode.BadRequest
                End If
                Dim reference = uow.GetObjectByKey(memberInfo.CollectionElementType, relatedKey)
                Dim collection = DirectCast(memberInfo.GetValue(entity), IList)
                collection.Remove(reference)
                uow.CommitChanges()
                Return HttpStatusCode.NoContent
            End Using
        End Function
    End Class
End Namespace