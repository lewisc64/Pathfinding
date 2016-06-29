Public Class Generator 'From Maze-Generator

    Enum directions
        up
        down
        left
        right
    End Enum

    Public grid As Grid
    Public open As New List(Of Cell)

    Public currentcell As Cell
    Public previousdirection As Byte

    Public random As Random

    Public draw As Boolean = True

    Public Sub New(ByRef usegrid As Grid)

        grid = usegrid

        For Each Cell As Cell In grid.cells
            Try
                Cell.state = Cell.States.Wall
            Catch
            End Try
        Next

        usegrid.startpoint = New Point(1, 1)
        usegrid.finishpoint = New Point(usegrid.width - 2, usegrid.height - 2)

        grid.cells(1, 1).state = Cell.States.Start
        grid.cells(grid.width - 2, grid.height - 2).state = Cell.States.Finish


        open.Add(grid.cells(grid.startpoint.X, grid.startpoint.Y))
        currentcell = open(0)

        grid.cells(grid.startpoint.X, grid.startpoint.Y).state = Cell.States.Empty
        grid.cells(grid.finishpoint.X, grid.finishpoint.Y).state = Cell.States.Wall

        random = New Random()

    End Sub

    Public Sub drawAllOpen(ByRef vbgame As VBGame)

        For Each Cell As Cell In open
            vbgame.drawRect(Cell.getRect(), Color.FromArgb(0, 128, 0))
        Next

    End Sub

    Public Sub drawDirtyOpen(ByRef vbgame As VBGame)

        For Each Cell As Cell In open
            If grid.dirtycells.Contains(Cell) Then
                vbgame.drawRect(Cell.getRect(), Color.FromArgb(0, 128, 0))
                grid.dirtycells.Remove(Cell)
            End If
        Next

    End Sub

    Public Function doDirection(direction As Byte, opoint As Point, amount As Integer) As Point
        Dim point As Point
        If direction = directions.up Then
            point = New Point(opoint.X, opoint.Y - amount)
        ElseIf direction = directions.down Then
            point = New Point(opoint.X, opoint.Y + amount)
        ElseIf direction = directions.left Then
            point = New Point(opoint.X - amount, opoint.Y)
        ElseIf direction = directions.right Then
            point = New Point(opoint.X + amount, opoint.Y)
        End If
        Return point
    End Function

    Public Sub backtrack()
        open.Remove(currentcell)
        If draw Then
            grid.dirtycells.Add(currentcell)
        End If
        Try
            currentcell = open(random.Next(0, open.ToArray().Length))
        Catch
        End Try
    End Sub

    Public Function handle() As Boolean
        Dim direction As Byte
        Dim checkdonedirection As Point
        Dim checkdirection As Byte
        Dim donedirection As Point
        Dim donedirectionone As Point
        Dim donedirections As New List(Of Byte)
        Dim done As Boolean
        Dim backs As Integer

        done = False
        backs = 0
        While Not done

            If donedirections.ToArray().Length = 4 Then
                backtrack()
                backs += 1
                If backs >= open.ToArray().Length Then
                    Return False
                End If
                donedirections.Clear()
            End If

            direction = random.Next(0, 4)

            If donedirections.Contains(direction) Then
                Continue While
            Else
                donedirections.Add(direction)
            End If

            donedirection = doDirection(direction, currentcell.getIndexXY(), 2)

            If donedirection.X > 0 And donedirection.X < grid.width And donedirection.Y > 0 And donedirection.Y < grid.height Then
                If grid.cells(donedirection.X, donedirection.Y).state = Cell.States.Wall Then
                    done = True
                End If
            End If

        End While

        donedirectionone = doDirection(direction, currentcell.getIndexXY(), 1)

        grid.cells(donedirectionone.X, donedirectionone.Y).state = Cell.States.Empty
        grid.cells(donedirection.X, donedirection.Y).state = Cell.States.Empty

        If draw Then
            grid.dirtycells.Add(grid.cells(donedirectionone.X, donedirectionone.Y))
            grid.dirtycells.Add(grid.cells(donedirection.X, donedirection.Y))
        End If

        If direction <> previousdirection Then
            done = False
            For checkdirection = 0 To 3
                If checkdirection <> direction Then
                    checkdonedirection = doDirection(checkdirection, currentcell.getIndexXY, 2)
                    Try
                        If grid.cells(checkdonedirection.X, checkdonedirection.Y).state = Cell.States.Wall Then
                            done = True
                            Exit For
                        End If
                    Catch
                    End Try
                End If
            Next

            If done Then
                open.Add(grid.cells(currentcell.ix, currentcell.iy))
            End If

            If draw Then
                grid.dirtycells.Add(grid.cells(currentcell.ix, currentcell.iy))
            End If
        End If

        currentcell = grid.cells(donedirection.X, donedirection.Y)

        previousdirection = direction

        Return True

    End Function

    Public Function mazeLoop(ogrid As Grid, display As VBGame)

        grid.drawAllCells(display)

        While open.Count <> 0

            handle()

            grid.drawDirtyCells(display)
            grid.dirtycells.Clear()

            display.update()
            display.clockTick(60)

        End While

        grid.cells(1, 1).state = Cell.States.Start
        grid.cells(grid.width - 2, grid.height - 2).state = Cell.States.Finish

        Return True

    End Function

End Class
