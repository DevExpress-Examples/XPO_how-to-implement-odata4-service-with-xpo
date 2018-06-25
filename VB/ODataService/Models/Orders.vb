Imports System
Imports DevExpress.Xpo
Imports DevExpress.Data.Filtering
Imports System.Collections.Generic

Namespace WebApplication1.Models

    <Persistent("Orders")> _
    Public Class Order
        Inherits XPLiteObject

        Public Sub New(ByVal session As Session)
            MyBase.New(session)
        End Sub
        Public Sub New()
        End Sub
        Public Overrides Sub AfterConstruction()
            MyBase.AfterConstruction()
        End Sub

        Private fOrderID As Integer
        <Key(True)> _
        Public Property OrderID() As Integer
            Get
                Return fOrderID
            End Get
            Set(ByVal value As Integer)
                SetPropertyValue(Of Integer)(NameOf(OrderID), fOrderID, value)
            End Set
        End Property

        Private fOrderStatus As OrderStatus
        Public Property OrderStatus() As OrderStatus
            Get
                Return fOrderStatus
            End Get
            Set(ByVal value As OrderStatus)
                SetPropertyValue(Of OrderStatus)(NameOf(OrderStatus), fOrderStatus, value)
            End Set
        End Property

        Private fCustomerID As Customer
        <Size(5), Association("OrdersReferencesCustomers"), Persistent("CustomerID")> _
        Public Property Customer() As Customer
            Get
                Return fCustomerID
            End Get
            Set(ByVal value As Customer)
                SetPropertyValue(Of Customer)(NameOf(Customer), fCustomerID, value)
            End Set
        End Property

        Private fOrderDate? As Date
        <Indexed(Name := "OrderDate")> _
        Public Property OrderDate() As Date?
            Get
                Return fOrderDate
            End Get
            Set(ByVal value? As Date)
                SetPropertyValue(Of Date?)(NameOf(OrderDate), fOrderDate, value)
            End Set
        End Property

        <Association("OrdersReferencesOrderDetails"), Aggregated> _
        Public ReadOnly Property OrderDetails() As XPCollection(Of OrderDetail)
            Get
                Return GetCollection(Of OrderDetail)(NameOf(OrderDetails))
            End Get
        End Property
    End Class

End Namespace
