Imports System
Imports DevExpress.Xpo
Imports DevExpress.Data.Filtering
Imports System.Collections.Generic

Namespace WebApplication1.Models

    <Persistent("OrderDetails")> _
    Public Class OrderDetail
        Inherits XPLiteObject

        Public Sub New(ByVal session As Session)
            MyBase.New(session)
        End Sub
        Public Sub New()
        End Sub
        Public Overrides Sub AfterConstruction()
            MyBase.AfterConstruction()
        End Sub

        Private fOrderDetailID As Integer
        <Key(True)> _
        Public Property OrderDetailID() As Integer
            Get
                Return fOrderDetailID
            End Get
            Set(ByVal value As Integer)
                SetPropertyValue(Of Integer)(NameOf(OrderDetailID), fOrderDetailID, value)
            End Set
        End Property

        Private fOrder As Order
        <Association("OrdersReferencesOrderDetails"), Persistent("OrderID")> _
        Public Property Order() As Order
            Get
                Return fOrder
            End Get
            Set(ByVal value As Order)
                SetPropertyValue(Of Order)(NameOf(Order), fOrder, value)
            End Set
        End Property

        Private fProduct As Product
        <Association("ProductsReferencesOrderDetails"), Persistent("ProductID")> _
        Public Property Product() As Product
            Get
                Return fProduct
            End Get
            Set(ByVal value As Product)
                SetPropertyValue(Of Product)(NameOf(Product), fProduct, value)
            End Set
        End Property

        Private fUnitPrice As Decimal
        <ColumnDbDefaultValue("(0)")> _
        Public Property UnitPrice() As Decimal
            Get
                Return fUnitPrice
            End Get
            Set(ByVal value As Decimal)
                SetPropertyValue(Of Decimal)(NameOf(UnitPrice), fUnitPrice, value)
            End Set
        End Property

        Private fQuantity As Short
        <ColumnDbDefaultValue("(1)")> _
        Public Property Quantity() As Short
            Get
                Return fQuantity
            End Get
            Set(ByVal value As Short)
                SetPropertyValue(Of Short)(NameOf(Quantity), fQuantity, value)
            End Set
        End Property
    End Class

End Namespace
