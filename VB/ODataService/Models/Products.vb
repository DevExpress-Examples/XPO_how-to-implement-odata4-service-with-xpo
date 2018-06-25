Imports System
Imports DevExpress.Xpo
Imports DevExpress.Data.Filtering
Imports System.Collections.Generic

Namespace WebApplication1.Models

    <Persistent("Products")> _
    Partial Public Class Product
        Inherits XPLiteObject

        Public Sub New()
        End Sub
        Public Sub New(ByVal session As Session)
            MyBase.New(session)
        End Sub
        Public Overrides Sub AfterConstruction()
            MyBase.AfterConstruction()
        End Sub

        Private fProductID As Integer
        <Key(True)> _
        Public Property ProductID() As Integer
            Get
                Return fProductID
            End Get
            Set(ByVal value As Integer)
                SetPropertyValue(Of Integer)(NameOf(ProductID), fProductID, value)
            End Set
        End Property

        Private fProductName As String
        <Indexed(Name := "ProductName"), Size(40), Nullable(False)> _
        Public Property ProductName() As String
            Get
                Return fProductName
            End Get
            Set(ByVal value As String)
                SetPropertyValue(Of String)(NameOf(ProductName), fProductName, value)
            End Set
        End Property

        Private fUnitPrice? As Decimal
        <ColumnDbDefaultValue("(0)")> _
        Public Property UnitPrice() As Decimal?
            Get
                Return fUnitPrice
            End Get
            Set(ByVal value? As Decimal)
                SetPropertyValue(Of Decimal?)(NameOf(UnitPrice), fUnitPrice, value)
            End Set
        End Property

        Private fPicture() As Byte
        Public Property Picture() As Byte()
            Get
                Return fPicture
            End Get
            Set(ByVal value As Byte())
                SetPropertyValue(Of Byte())(NameOf(Picture), fPicture, value)
            End Set
        End Property

        <Association("ProductsReferencesOrderDetails")> _
        Public ReadOnly Property OrderDetails() As XPCollection(Of OrderDetail)
            Get
                Return GetCollection(Of OrderDetail)(NameOf(OrderDetails))
            End Get
        End Property
    End Class

End Namespace
