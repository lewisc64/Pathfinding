Public Class Editor

    Public Sub editloop(display As VBGame)

        Dim drawLines As Boolean = True

        Dim grid As New Grid(Form1.side, Form1.gwidth, Form1.gheight, New Point(1, 1), New Point(Form1.gwidth - 2, Form1.gheight - 2))
        Dim currentCell As Cell
        Dim mode As Integer = 1

        grid.drawAllCells(display)

        While True

            For Each e As MouseEvent In display.getMouseEvents()
                If e.action = MouseEvent.actions.up Then

                    Try

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
                    Catch ex As Exception
                    End Try
                End If

            Next

            For Each e As KeyEventArgs In display.getKeyDownEvents()
                If e.KeyCode = Keys.Enter Then
                    Dim search As New AStarSearch
                    Dim startpresent, finishpresent As Boolean

                    startpresent = False
                    finishpresent = False

                    For Each Cell As Cell In grid.cells
                        Try
                            If Cell.state = Cell.States.Start Then
                                startpresent = True
                            ElseIf Cell.state = Cell.States.Finish Then
                                finishpresent = True
                            End If
                        Catch
                        End Try
                    Next

                    If startpresent AndAlso finishpresent Then
                        search.searchLoop(grid, display)
                        grid.drawAllCells(display)
                    Else
                        If Not startpresent Then
                            MsgBox("Add start by left clicking in mode 2.")
                        End If
                        If Not finishpresent Then
                            MsgBox("Add finish by right clicking in mode 2.")
                        End If
                    End If

                ElseIf e.KeyCode = Keys.M Then
                    Dim maze As New Generator(grid)
                    maze.mazeLoop(grid, display)
                    grid.drawAllCells(display)

                ElseIf e.KeyCode = Keys.D1 Then
                    mode = 1
                ElseIf e.KeyCode = Keys.D2 Then
                    mode = 2
                End If
            Next

            grid.drawDirtyCells(display)
            grid.dirtycells.Clear()

            If drawLines Then
                For x As Integer = 0 To display.width Step grid.side
                    display.drawLine(New Point(x, 0), New Point(x, display.height), VBGame.black)
                Next
                For y As Integer = 0 To display.width Step grid.side
                    display.drawLine(New Point(0, y), New Point(display.width, y), VBGame.black)
                Next
            End If

            If display.mouse.Button = VBGame.mouse_left Then
                display.pushMouseEvent(New MouseEvent(display.mouse.Location, MouseEvent.actions.up, MouseEvent.buttons.left))

            ElseIf display.mouse.Button = VBGame.mouse_right Then
                display.pushMouseEvent(New MouseEvent(display.mouse.Location, MouseEvent.actions.up, MouseEvent.buttons.right))
            End If

                display.update()
                display.clockTick(30)

        End While

    End Sub

End Class
