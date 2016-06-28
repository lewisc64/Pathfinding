Public Class Editor

    Public Sub editloop(display As VBGame)

        Dim grid As New Grid(20, 40, 40, New Point(0, 0), New Point(39, 39))
        Dim currentCell As Cell
        Dim mode As Integer = 1

        grid.drawAllCells(display)

        While True

            For Each e As MouseEvent In display.getMouseEvents()
                If e.action = MouseEvent.actions.up Then

                    currentCell = grid.cells(Math.Floor(e.location.X / grid.side), Math.Floor(e.location.Y / grid.side))

                    If e.button = MouseEvent.buttons.left Then
                        grid.dirtycells.Add(currentCell)

                        If mode = 1 Then
                            currentCell.state = Cell.States.Wall

                        Else
                            grid.dirtycells.Add(grid.cells(grid.startpoint.X, grid.startpoint.Y))
                            grid.cells(grid.startpoint.X, grid.startpoint.Y).state = Cell.States.Empty
                            grid.startpoint = currentCell.getIndexXY()
                            currentCell.state = Cell.States.Start
                        End If

                    ElseIf e.button = MouseEvent.buttons.right Then
                        grid.dirtycells.Add(currentCell)

                        If mode = 1 Then
                            currentCell.state = Cell.States.Empty

                        Else
                            grid.dirtycells.Add(grid.cells(grid.finishpoint.X, grid.finishpoint.Y))
                            grid.cells(grid.finishpoint.X, grid.finishpoint.Y).state = Cell.States.Empty
                            grid.finishpoint = currentCell.getIndexXY()
                            currentCell.state = Cell.States.Finish
                        End If
                    End If
                End If

            Next

            For Each e As KeyEventArgs In display.getKeyDownEvents()
                If e.KeyCode = Keys.Enter Then
                    Dim search As New AStarSearch
                    search.searchLoop(grid, display)

                ElseIf e.KeyCode = Keys.D1 Then
                    mode = 1
                ElseIf e.KeyCode = Keys.D2 Then
                    mode = 2
                End If
            Next

            grid.drawDirtyCells(display)
            grid.dirtycells.Clear()

            display.update()
            display.clockTick(30)

        End While

    End Sub

End Class
