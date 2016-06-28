Public Class Editor

    Public Sub editloop(display As VBGame)

        Dim grid As New Grid(20, 40, 40, New Point(0, 0), New Point(39, 39))
        Dim currentCell As Cell

        grid.drawAllCells(display)

        While True

            For Each e As MouseEvent In display.getMouseEvents()
                If e.action = MouseEvent.actions.up Then

                    currentCell = grid.cells(Math.Floor(e.location.X / grid.side), Math.Floor(e.location.Y / grid.side))

                    If e.button = MouseEvent.buttons.left Then
                        currentCell.state = Cell.States.Wall
                        grid.dirtycells.Add(currentCell)

                    ElseIf e.button = MouseEvent.buttons.right Then
                        currentCell.state = Cell.States.Empty
                        grid.dirtycells.Add(currentCell)
                    End If
                End If
            Next

            grid.drawDirtyCells(display)
            grid.dirtycells.Clear()

            display.update()
            display.clockTick(30)

        End While

    End Sub

End Class
