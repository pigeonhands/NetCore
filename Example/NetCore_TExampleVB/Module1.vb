Imports NetCore

Module Module1

    Sub Main()
        Console.WriteLine(My.Computer.GetType())

        Console.WriteLine(returnFun())
        Console.ReadLine()
    End Sub

    <RemoteCall>
    Function returnFun() As String
        'My.Computer.FileSystem.ReadAllBytes("")

        Return "Fuck VB"
    End Function

End Module
