Imports System
Imports DevExpress.Xpo
Imports DevExpress.Data.Filtering
Imports System.Collections.Generic

Namespace WebApplication1.Models

    <Persistent("Customers")> _
    Public Class Customer
        Inherits XPLiteObject

        Public Sub New(ByVal session As Session)
            MyBase.New(session)
        End Sub
        Public Sub New()
            MyBase.New(XpoDefault.Session)
        End Sub
        Public Overrides Sub AfterConstruction()
            MyBase.AfterConstruction()
        End Sub

        Private fCustomerID As String
        <Key, Size(5), Nullable(False)> _
        Public Property CustomerID() As String
            Get
                Return fCustomerID
            End Get
            Set(ByVal value As String)
                SetPropertyValue(Of String)(NameOf(CustomerID), fCustomerID, value)
            End Set
        End Property

        Private fCompanyName As String
        <Indexed(Name := "CompanyName"), Size(40), Nullable(False)> _
        Public Property CompanyName() As String
            Get
                Return fCompanyName
            End Get
            Set(ByVal value As String)
                SetPropertyValue(Of String)(NameOf(CompanyName), fCompanyName, value)
            End Set
        End Property

        <Association("OrdersReferencesCustomers")> _
        Public ReadOnly Property Orders() As XPCollection(Of Order)
            Get
                Return GetCollection(Of Order)(NameOf(Orders))
            End Get
        End Property
    End Class

End Namespace
