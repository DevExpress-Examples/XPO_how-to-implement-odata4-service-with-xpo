Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports [Default]
Imports NUnit.Framework

Namespace Tests
    Public Class ActionTests
        Inherits ODataTestsBase

        <Test> _
        Public Sub FunctionTest()
            Dim container As Container = GetODataContainer()
            Dim sales2017 As Decimal = container.TotalSalesByYear(2017).GetValue()
            Dim sales2018 As Decimal = container.TotalSalesByYear(2018).GetValue()

            Assert.AreEqual(0, sales2017)
            Assert.AreEqual(3501.55D, sales2018)
        End Sub
    End Class
End Namespace
