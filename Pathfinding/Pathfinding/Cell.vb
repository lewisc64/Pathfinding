Public Class Cell

    Public Enum States
        Empty
        Wall
        Start
        Finish
    End Enum

    Public state As Byte
    Public side As Integer
    Public x As Integer
    Public y As Integer
    Public ix As Integer
    Public iy As Integer

    Public Sub New(rect As Rectangle, xindex As Integer, yindex As Integer, Optional type As Byte = States.Empty)
        state = type
        x = rect.X
        y = rect.Y
        side = rect.Width
        ix = xindex
        iy = yindex
    End Sub

    Public Function getRect() As Rectangle
        Return New Rectangle(x, y, side, side)
    End Function

    Public Function getIndexXY() As Point
        Return New Point(ix, iy)
    End Function

    Public Sub draw(ByRef vbgame As VBGame)
        If state = States.Empty Then
            vbgame.drawRect(getRect(), vbgame.white)
        ElseIf state = States.Wall Then
            vbgame.drawRect(getRect(), vbgame.black)
        ElseIf state = States.Start Then
            vbgame.drawRect(getRect(), vbgame.green)
        ElseIf state = States.Finish Then
            vbgame.drawRect(getRect(), vbgame.red)
        End If
    End Sub

End Class
