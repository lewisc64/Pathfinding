Imports System.Threading

Public Class Form1

    Public thread As New Thread(AddressOf mainloop)
    Public display As New VBGame

    Public gwidth As Integer = 51
    Public gheight As Integer = 51

    Public side As Integer = 16

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        adjustSize()
        thread.Start()
        display.mouse = New MouseEventArgs(Windows.Forms.MouseButtons.None, 0, 0, 0, 0)
    End Sub

    Public Sub adjustSize()
        display.setDisplay(Me, New Size(gwidth * side, gheight * side), "Pathfinding", True)
    End Sub

    Public Sub mainloop()

        Dim editor As New Editor

        editor.editloop(display)

    End Sub

End Class
