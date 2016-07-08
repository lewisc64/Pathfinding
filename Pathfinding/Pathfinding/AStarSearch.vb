Public Class AStarSearch

    Public moveCost As Integer = 1

    Public tieBreaker As Boolean = False 'Something funky is going on...

    Public Sub calculateHCost(ByRef cell As Cell, start As Point, finish As Point)
        Dim current As Point = cell.getIndexXY()

        Dim dx, dy, dx2, dy2 As Double

        dx = Math.Abs(current.X - finish.X)
        dy = Math.Abs(current.Y - finish.Y)

        cell.hCost = dx + dy 'MANHATTAN

        If tieBreaker Then
            dx2 = 1 - finish.X
            dy2 = 1 - finish.Y
            cell.hCost += Math.Abs(dx * dy2 - dx2 * dy) * 0.01
        End If
    End Sub

    Public Sub calculateGCost(ByRef cell As Cell)
        Try
            cell.gCost = cell.parent.gCost + moveCost
        Catch
            cell.gCost = moveCost
        End Try
    End Sub

    Public Sub calculateFCost(ByRef cell As Cell)
        cell.fCost = cell.gCost + cell.hCost
    End Sub

    Public Sub drawCells(cells As List(Of Cell), color As System.Drawing.Color, display As VBGame)
        For Each Cell As Cell In cells
            display.drawRect(Cell.getRect(), color)
            display.drawText(Cell.getRect(), Cell.fCost, VBGame.black, New Font("Arial", 8))
        Next
    End Sub

    Public Function getNeighbors(grid As Grid, current As Cell) As List(Of Cell)
        Dim neighbors As New List(Of Cell)
        Dim currentPoint = current.getIndexXY()

        For x As Integer = -1 To 1
            For y As Integer = -1 To 1
                Try
                    If (x = 0 OrElse y = 0) AndAlso (Not (x = 0 AndAlso y = 0)) Then
                        neighbors.Add(grid.cells(currentPoint.X + x, currentPoint.Y + y))
                    End If
                Catch
                End Try
            Next
        Next
        Return neighbors
    End Function

    Public Sub wait(display As VBGame)
        While True
            For Each e As KeyEventArgs In display.getKeyUpEvents()
                Exit While
            Next
        End While
    End Sub

    Public Function getLine(current As Cell, Optional ByRef line As List(Of Point) = Nothing) As List(Of Point)
        If IsNothing(line) Then
            line = New List(Of Point)
            line.Add(New Point(current.x + (current.side / 2), current.y + (current.side / 2)))
        End If
        Try
            'display.drawLine(New Point(current.x + (current.side / 2), current.y + (current.side / 2)), New Point(current.parent.x + (current.parent.side / 2), current.parent.y + (current.parent.side / 2)), VBGame.blue, current.side / 5)
            'display.update()
            'display.clockTick(15)
            line.Add(New Point(current.parent.x + (current.parent.side / 2), current.parent.y + (current.parent.side / 2)))
            Return getLine(current.parent, line)
        Catch
            Return line
        End Try
    End Function

    Public Function searchLoop(grid As Grid, display As VBGame, Optional stepThrough As Boolean = True) As List(Of Point)

        Dim open As New List(Of Cell)
        Dim closed As New List(Of Cell)

        Dim current As Cell

        Dim lowest As Integer

        open.Add(grid.cells(grid.startpoint.X, grid.startpoint.Y))
        open(0).fCost = 9999

        current = open(0)

        grid.drawAllCells(display)

        While True

            lowest = 9999
            For Each Cell As Cell In open

                If Cell.fCost < lowest Then
                    lowest = Cell.fCost
                    current = Cell
                End If

            Next

            closed.Add(current)

            If stepThrough Then
                display.drawRect(current.getRect(), VBGame.red)
            End If

            open.Remove(current)

            If current.getIndexXY() = grid.finishpoint Then
                Dim line As List(Of Point)
                line = getLine(current)
                If stepThrough Then
                    display.drawLines(line.ToArray(), VBGame.blue, grid.side / 5)
                    display.update()
                    wait(display)
                End If
                Return line
            End If

            For Each Cell As Cell In getNeighbors(grid, current)
                If Not IsNothing(Cell) AndAlso Not closed.Contains(Cell) AndAlso Cell.state <> Cell.States.Wall Then

                    If Not open.Contains(Cell) Or current.gCost + moveCost < Cell.gCost Then

                        Cell.gCost = current.gCost + moveCost

                        calculateHCost(Cell, grid.startpoint, grid.finishpoint)
                        calculateFCost(Cell)

                        Cell.parent = current

                        If Not open.Contains(Cell) Then
                            open.Add(Cell)
                            If stepThrough Then
                                display.drawRect(Cell.getRect(), VBGame.green)
                            End If
                        End If

                    End If

                End If
            Next

            If open.Count = 0 Then
                Return New List(Of Point)
            End If

            If stepThrough Then
                display.update()
                display.clockTick(60)
            End If

        End While

        Return New List(Of Point)

    End Function

End Class
