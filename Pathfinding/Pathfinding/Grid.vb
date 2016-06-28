Public Class Grid

    Public width As Integer
    Public height As Integer
    Public side As Integer

    Public startpoint As Point
    Public finishpoint As Point

    Public cells As Array

    Public dirtycells As New List(Of Cell)

    Public Sub New(cellside As Integer, gridwidth As Integer, gridheight As Integer, start As Point, finish As Point)

        startpoint = start
        finishpoint = finish

        side = cellside

        width = gridwidth
        height = gridheight

        Dim cellst(width, height) As Cell
        cells = cellst

        For x As Integer = 0 To width - 1
            For y As Integer = 0 To height - 1

                cells(x, y) = New Cell(New Rectangle(x * side, y * side, side, side), x, y, Cell.States.Empty)

            Next
        Next

        cells(start.X, start.Y).state = Cell.States.Start
        cells(finish.X, finish.Y).state = Cell.States.Finish

    End Sub

    Public Sub drawDirtyCells(ByRef display As VBGame)
        For Each Cell As Cell In dirtycells
            Cell.draw(display)
        Next
    End Sub

    Public Sub drawAllCells(ByRef display As VBGame)
        For x As Integer = 0 To width - 1
            For y As Integer = 0 To height - 1

                cells(x, y).draw(display)

            Next
        Next
    End Sub

End Class
