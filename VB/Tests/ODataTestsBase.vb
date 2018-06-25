Imports System
Imports System.Diagnostics
Imports System.IO
Imports [Default]
Imports NUnit.Framework

Namespace Tests
    Public MustInherit Class ODataTestsBase

        Private Const ODataServiceUrl As String = "http://localhost:5000/"
        Private iisProcess As Process = Nothing

        Protected Function GetODataContainer() As Container
            Return New Container(New Uri(ODataServiceUrl))
        End Function

        <OneTimeSetUp> _
        Public Sub OneTimeSetup()
            Dim iisExpressPath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "IIS Express", "iisexpress.exe")
            Dim appPath As String = Path.GetDirectoryName(Me.GetType().Assembly.Location)
            appPath = Path.GetFullPath(Path.Combine(appPath, "..", "..", "..", "ODataService"))
            Dim args As String = String.Format("/path:""{0}"" /port:5000", appPath)
            iisProcess = Process.Start(iisExpressPath, args)
        End Sub

        <OneTimeTearDown> _
        Public Sub OneTimeTearDown()
            If iisProcess IsNot Nothing Then
                iisProcess.Kill()
            End If
        End Sub

        <SetUp> _
        Public Sub Setup()
            Dim container As Container = GetODataContainer()
            container.InitializeDatabase().Execute()
        End Sub
    End Class
End Namespace
