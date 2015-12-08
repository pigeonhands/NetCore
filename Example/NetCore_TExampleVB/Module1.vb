Imports System.Data.SqlClient
Imports System.Windows.Forms
Imports NetCore

Module Module1

    Sub Main()
        If Not NetCoreClient.Connect("127.0.0.1", 3345) Then
            Console.WriteLine("Failed to connect")
            Console.ReadLine()
            Return
        End If

        Dim dt = returnFun()
        Console.WriteLine(dt.TableName)
        Console.ReadLine()
    End Sub

    <RemoteCall>
    Function returnFun() As DataTable
        Return New DataTable("Test1")
    End Function

End Module
