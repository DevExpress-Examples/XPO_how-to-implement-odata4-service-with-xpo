Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace DevExpress.Xpo.Helpers

    Public NotInheritable Class TypeSystem

        Private Sub New()
        End Sub

        Private ReadOnly Shared convertableType As New Dictionary(Of KeyValuePair(Of Type, Type), Boolean)()
        Friend Shared Function GetElementType(ByVal seqType As Type) As Type
            Dim ienum As Type = FindIEnumerable(seqType)
            If ienum Is Nothing Then
                Return seqType
            End If
            Return ienum.GetGenericArguments()(0)
        End Function
        Private Shared Function FindIEnumerable(ByVal seqType As Type) As Type
            If seqType Is Nothing OrElse seqType Is GetType(String) Then
                Return Nothing
            End If
            If seqType.IsArray Then
                Return GetType(IEnumerable(Of )).MakeGenericType(seqType.GetElementType())
            End If
            If seqType.IsGenericType Then
                For Each arg As Type In seqType.GetGenericArguments()
                    Dim ienum As Type = GetType(IEnumerable(Of )).MakeGenericType(arg)
                    If ienum.IsAssignableFrom(seqType) Then
                        Return ienum
                    End If
                Next arg
            End If
            Dim ifaces() As Type = seqType.GetInterfaces()
            If ifaces IsNot Nothing AndAlso ifaces.Length > 0 Then
                For Each iface As Type In ifaces
                    Dim ienum As Type = FindIEnumerable(iface)
                    If ienum IsNot Nothing Then
                        Return ienum
                    End If
                Next iface
            End If
            If seqType.BaseType IsNot Nothing AndAlso seqType.BaseType IsNot GetType(Object) Then
                Return FindIEnumerable(seqType.BaseType)
            End If
            Return Nothing
        End Function

        Public Shared Function ProcessTypeName(ByVal namespaceName As String, ByVal type As Type) As String
            Return ProcessTypeName(namespaceName, type.FullName)
        End Function
        Public Shared Function ProcessTypeName(ByVal namespaceName As String, ByVal typeName As String) As String
            Dim name As String
            If typeName.Length > namespaceName.Length AndAlso typeName.Substring(0, namespaceName.Length) = namespaceName Then
                name = typeName.Substring(namespaceName.Length + 1).Replace(".", "_").Replace("+", "_")
            Else
                name = typeName.Replace(".", "_").Replace("+", "_")
            End If
            Return name
        End Function
        Public Shared Function AreConvertable(ByVal type1 As Type, ByVal type2 As Type) As Boolean
            If type1 Is Nothing OrElse type2 Is Nothing Then
                Return False
            End If
            Dim isExists As Boolean = Nothing
            SyncLock convertableType
                If convertableType.TryGetValue(New KeyValuePair(Of Type, Type)(type1, type2), isExists) Then
                    Return isExists
                End If
                isExists = CheckConvertable(type1, type2)
                convertableType.Add(New KeyValuePair(Of Type, Type)(type1, type2), isExists)
            End SyncLock
            Return isExists
        End Function

        Private Shared Function CheckConvertable(ByVal type1 As Type, ByVal type2 As Type) As Boolean
            If type1 Is type2 Then
                Return True
            End If
            Dim tempType As Type = type2
            Do
                If tempType.IsAssignableFrom(type1) Then
                    Return True
                End If
                tempType = tempType.BaseType
            Loop While tempType IsNot Nothing AndAlso tempType IsNot GetType(Object)

            tempType = type1
            Do
                If tempType.IsAssignableFrom(type2) Then
                    Return True
                End If
                tempType = tempType.BaseType
            Loop While tempType IsNot Nothing AndAlso tempType IsNot GetType(Object)

            Return False

        End Function

        Friend Shared Function IsQueryableType(ByVal collectionType As Type) As Boolean
            If Not collectionType.IsGenericTypeDefinition Then
                If Not collectionType.IsGenericType Then
                    Return False
                End If
                collectionType = collectionType.GetGenericTypeDefinition()
            End If
            Return GetType(IQueryable(Of )).IsAssignableFrom(collectionType) OrElse collectionType.GetInterfaces().Any(Function(inf) inf.IsGenericType AndAlso GetType(IQueryable(Of )) Is inf.GetGenericTypeDefinition())
        End Function

        Friend Shared Function IsEnumerableType(ByVal collectionType As Type) As Boolean
            If Not collectionType.IsGenericTypeDefinition Then
                If Not collectionType.IsGenericType Then
                    Return False
                End If
                collectionType = collectionType.GetGenericTypeDefinition()
            End If
            Return GetType(IEnumerable(Of )).IsAssignableFrom(collectionType) OrElse collectionType.GetInterfaces().Any(Function(inf) inf.IsGenericType AndAlso GetType(IEnumerable(Of )) Is inf.GetGenericTypeDefinition())
        End Function
    End Class
End Namespace
