Imports System.Threading

Public Class Form1

    Public thread As New Thread(AddressOf mainloop)
    Public display As New VBGame

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        display.setDisplay(Me, New Size(800, 800), "Pathfinding", True)
        thread.Start()
    End Sub

    Public Sub mainloop()

        Dim editor As New Editor

        editor.editloop(display)

    End Sub

End Class
