Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection
Imports DevExpress.Xpo
Imports DevExpress.Xpo.Helpers
Imports System.Collections.Concurrent

Namespace ODataService.Helpers

    Friend Module IQueryableExtensions
        <System.Runtime.CompilerServices.Extension> _
        Public Function AsWrappedQuery(Of T)(ByVal source As XPQuery(Of T)) As XpoLinqQuery(Of T)
            Return New XpoLinqQuery(Of T)(source)
        End Function
    End Module

    Public Class XpoLinqQuery(Of T)
        Implements IOrderedQueryable(Of T)

        Private ReadOnly queryExpression As Expression
        Private ReadOnly queryProvider As IQueryProvider

        Public Sub New(ByVal query As XPQuery(Of T))
            Me.New(New XpoLinqQueryProvider(query, query.Session), DirectCast(query, IQueryable).Expression)
        End Sub

        Public Sub New(ByVal queryProvider As XpoLinqQueryProvider)
            If queryProvider Is Nothing Then
                Throw New ArgumentNullException("provider")
            End If
            Me.queryProvider = queryProvider
            Me.queryExpression = Expression.Constant(Me)
        End Sub

        Public Sub New(ByVal queryProvider As IQueryProvider, ByVal queryExpression As Expression)
            If queryProvider Is Nothing Then
                Throw New ArgumentNullException("provider")
            End If
            If queryExpression Is Nothing Then
                Throw New ArgumentNullException("expression")
            End If
            If Not queryExpression.Type.IsGenericType Then
                Throw New ArgumentOutOfRangeException("expression")
            End If
            Me.queryProvider = queryProvider
            Me.queryExpression = queryExpression
        End Sub

        #Region "IEnumerable<T> Members"

        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Dim result As Object = Me.queryProvider.Execute(Me.queryExpression)
            Return DirectCast(result, IEnumerable(Of T)).GetEnumerator()
        End Function

        #End Region

        #Region "IEnumerable Members"

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return DirectCast(Me.queryProvider.Execute(Me.queryExpression), IEnumerable).GetEnumerator()
        End Function

        #End Region

        #Region "IQueryable Members"

        Public ReadOnly Property ElementType() As Type Implements System.Linq.IQueryable(Of T).ElementType
            Get
                Return GetType(T)
            End Get
        End Property

        Public ReadOnly Property Expression() As Expression Implements System.Linq.IQueryable(Of T).Expression
            Get
                Return Me.queryExpression
            End Get
        End Property

        Public ReadOnly Property Provider() As IQueryProvider Implements System.Linq.IQueryable(Of T).Provider
            Get
                Return Me.queryProvider
            End Get
        End Property
        #End Region
    End Class

    Public Class XpoLinqQueryProvider
        Implements IQueryProvider

        Private ReadOnly session As Session
        Private ReadOnly queryProviderInternal As IQueryProvider

        Public Sub New(ByVal baseQueryProvider As IQueryProvider, ByVal session As Session)
            queryProviderInternal = baseQueryProvider
            Me.session = session
        End Sub

        Public Function CreateQuery(ByVal expression As Expression) As IQueryable Implements IQueryProvider.CreateQuery
            Try
                Dim elementType As Type = GetExpressionType(expression)
                Return CreateQuery(elementType, expression)
            Catch tie As TargetInvocationException
                Throw tie.InnerException
            End Try
        End Function

        Public Function CreateQuery(Of TElement)(ByVal expression As Expression) As IQueryable(Of TElement) Implements IQueryProvider.CreateQuery
            Return New XpoLinqQuery(Of TElement)(Me, expression)
        End Function

        Private Function CreateQuery(ByVal elementType As Type, ByVal expression As Expression) As IQueryable
            Dim result As Type = GetType(XpoLinqQuery(Of )).MakeGenericType(elementType)
            Dim ci As ConstructorInfo = result.GetConstructor(New Type() { Me.GetType(), GetType(Expression) })
            Return DirectCast(ci.Invoke(New Object() { Me, expression }), IQueryable)
        End Function

        Public Function Execute(Of TResult)(ByVal expression As Expression) As TResult
            Dim result As Object = Me.Execute(expression)
            Return DirectCast(result, TResult)
        End Function

        Public Function Execute(ByVal expression As Expression) As Object Implements IQueryProvider.Execute
            expression = PreprocessExpression(expression)
            If expression.NodeType = ExpressionType.Constant Then
                Dim c As ConstantExpression = CType(expression, ConstantExpression)
                Return DirectCast(c.Value, IQueryable)
            End If
            Dim executeSingle As Boolean = False
            If expression.Type.IsGenericType Then
                Dim expressionType As Type = TypeSystem.GetElementType(expression.Type)
                If IsPersistentType(expressionType) Then
                    Dim queryCreator = GetXPQueryCreator(expressionType)
                    Dim xpQuery As IQueryable = DirectCast(queryCreator.CreateXPQuery(session), IQueryProvider).CreateQuery(expression)
                    Return xpQuery
                End If
            Else
                executeSingle = True
            End If
            Dim elementType As Type = GetExpressionType(expression)
            If executeSingle Then
                Dim queryCreator = GetXPQueryCreator(elementType)
                Dim result As Object = DirectCast(queryCreator.CreateXPQuery(session), IQueryProvider).Execute(expression)
                Return result
            End If

            Dim xpCreator = GetXPQueryCreator(elementType)
            Return xpCreator.Enumerate(DirectCast(xpCreator.CreateXPQuery(session), IQueryProvider).CreateQuery(expression))
        End Function

        Private Function GetExpressionType(ByVal expression As Expression) As Type
            Dim currentExpression As Expression = expression
            Dim elementType As Type = Nothing
            Do While elementType Is Nothing
                If currentExpression.Type.IsGenericType Then
                    Dim expressionType As Type = TypeSystem.GetElementType(currentExpression.Type)
                    If IsPersistentType(expressionType) Then
                        elementType = expressionType
                        Exit Do
                    End If
                End If
                Dim [call] As MethodCallExpression = TryCast(currentExpression, MethodCallExpression)
                If [call] Is Nothing Then
                    Throw New InvalidOperationException()
                End If
                currentExpression = [call].Arguments(0)
            Loop
            Return elementType
        End Function


        Private Function PreprocessExpression(ByVal expression As Expression) As Expression
            Dim preprocessor = New LinqExpressionPreprocessor()
            Return preprocessor.Process(expression)
        End Function

        Private Shared ReadOnly xpQueryCreatorDict As New ConcurrentDictionary(Of Type, XPQueryCreatorBase)()

        Private Function IsPersistentType(ByVal type As Type) As Boolean
            Return type.IsSubclassOf(GetType(PersistentBase))
        End Function

        Private Function GetXPQueryCreator(ByVal type As Type) As XPQueryCreatorBase
            Return xpQueryCreatorDict.GetOrAdd(type, Function(t)
                Dim creatorType As Type = GetType(XPQueryCreator(Of )).MakeGenericType(type)
                Dim queryCreator As XPQueryCreatorBase = DirectCast(Activator.CreateInstance(creatorType), XPQueryCreatorBase)
                Return queryCreator
            End Function)
        End Function
    End Class

    Friend MustInherit Class XPQueryCreatorBase
        Public MustOverride Function CreateXPQuery(ByVal session As Session) As IQueryable
        Public MustOverride Function Enumerate(ByVal queryable As IQueryable) As IEnumerable
    End Class

    Friend Class XPQueryCreator(Of T)
        Inherits XPQueryCreatorBase

        Public Sub New()
        End Sub
        Public Overrides Function CreateXPQuery(ByVal session As Session) As IQueryable
            Return New XPQuery(Of T)(session)
        End Function
        Public Overrides Function Enumerate(ByVal queryable As IQueryable) As IEnumerable
            Return queryable.Cast(Of Object)()
        End Function
    End Class

    Friend Class LinqExpressionPreprocessor
        Inherits ExpressionVisitor

        Public Function Process(ByVal expression As Expression) As Expression
            Return Visit(expression)
        End Function

        Protected Overrides Function VisitBinary(ByVal node As BinaryExpression) As Expression
            Select Case node.NodeType
                Case ExpressionType.Equal
                    If node.Left.NodeType <> ExpressionType.Conditional OrElse node.Right.NodeType <> ExpressionType.Constant OrElse node.Right.Type IsNot GetType(Boolean?) OrElse Not DirectCast((TryCast(node.Right, ConstantExpression)).Value, Boolean?).Equals(True) Then
                        Exit Select
                    End If
                    Dim ifNode As ConditionalExpression = (TryCast(node.Left, ConditionalExpression))
                    If ifNode.Test.NodeType <> ExpressionType.Equal Then
                        Exit Select
                    End If
                    Dim ifTest As BinaryExpression = (TryCast(ifNode.Test, BinaryExpression))
                    If ifTest.Right.NodeType <> ExpressionType.Constant OrElse (TryCast(ifTest.Right, ConstantExpression)).Value IsNot Nothing Then
                        Exit Select
                    End If
                    If ifNode.IfTrue.NodeType <> ExpressionType.Constant OrElse (TryCast(ifNode.IfTrue, ConstantExpression)).Value IsNot Nothing Then
                        Exit Select
                    End If
                    If ifNode.IfFalse.NodeType = ExpressionType.Convert AndAlso ifNode.IfFalse.Type Is GetType(Boolean?) Then
                        Return MyBase.Visit((TryCast(ifNode.IfFalse, UnaryExpression)).Operand)
                    End If
                    Return MyBase.Visit(ifNode.IfFalse)
            End Select
            Return MyBase.VisitBinary(node)
        End Function

        Protected Overrides Function VisitMethodCall(ByVal node As MethodCallExpression) As Expression
            Select Case node.Method.Name
                Case "Compare"
                    If node.Method.DeclaringType Is GetType(String) AndAlso node.Arguments.Count = 3 Then
                        Dim method As MethodInfo = GetType(String).GetMethod("Compare", New Type() { GetType(String), GetType(String) })
                        Dim newExpr As Expression = Expression.Call(method, node.Arguments(0), node.Arguments(1))
                        Return MyBase.Visit(newExpr)
                    End If
            End Select
            Return MyBase.VisitMethodCall(node)
        End Function
    End Class
End Namespace