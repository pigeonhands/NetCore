Imports NetCore

Module Module1

    Sub Main()
        If Not NetCoreClient.Connect("127.0.0.1", 3345) Then
            Console.WriteLine("Failed to connect")
            Console.ReadLine()
            Return

        End If
        Console.WriteLine(returnFun())
        Console.ReadLine()
    End Sub

    <RemoteCall>
    Function returnFun() As String
        Return "Derp"
    End Function

End Module
