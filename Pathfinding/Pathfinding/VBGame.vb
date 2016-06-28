Option Explicit On
Option Strict On

Imports System.Windows.Forms
Imports System.IO
Imports System.Threading

Public Class MouseEvent

    Enum buttons
        none
        left
        right
        middle
        scrollUp
        scrollDown
    End Enum

    Enum actions
        move
        down
        up
        scroll
    End Enum

    Public action As Byte
    Public location As Point
    Public button As Byte

    Public Sub New(locationt As Point, actiont As Byte, buttont As Byte)
        action = actiont
        location = locationt
        button = buttont
    End Sub

    Public Shared Function InterpretFormEvent(e As MouseEventArgs, action As Byte) As MouseEvent
        Dim button As Byte
        If action = actions.down Or action = actions.up Then
            If e.Button = MouseButtons.Left Then
                button = CByte(buttons.left)
            ElseIf e.Button = MouseButtons.Right Then
                button = CByte(buttons.right)
            ElseIf e.Button = MouseButtons.Middle Then
                button = CByte(buttons.middle)
            End If
        ElseIf action = actions.scroll Then
            If e.Delta > 0 Then
                button = CByte(buttons.scrollUp)
            ElseIf e.Delta < 0 Then
                button = CByte(buttons.scrollDown)
            End If
        End If
        Return New MouseEvent(e.Location, action, button)
    End Function

End Class

Public Class DrawBase

    Public displaybuffer As BufferedGraphics
    Public displaycontext As System.Drawing.BufferedGraphicsContext

    Public parentgraphics As Graphics

    Public x_shift As Integer = 0
    Public Property x As Integer
        Set(value As Integer)
            x_shift = value
            allocate()
        End Set
        Get
            Return x_shift
        End Get
    End Property

    Public y_shift As Integer = 0
    Public Property y As Integer
        Set(value As Integer)
            y_shift = value
            allocate()
        End Set
        Get
            Return y_shift
        End Get
    End Property

    Public width As Integer
    Public height As Integer

    Sub allocate()
        displaybuffer = displaycontext.Allocate(parentgraphics, getRect(True))
    End Sub

    ''' <summary>
    ''' Renders the display buffer to the form.
    ''' </summary>
    ''' <remarks></remarks>
    Sub update()
        Try
            displaybuffer.Render()
        Catch ex As System.ArgumentException
            End
        End Try
    End Sub

    ''' <summary>
    ''' Gets the display area as a rectangle.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function getRect(Optional shifted As Boolean = False) As Rectangle
        If shifted Then
            Return New Rectangle(CInt(x_shift), CInt(y_shift), width, height)
        Else
            Return New Rectangle(0, 0, width, height)
        End If
    End Function

    Function shiftRect(rect As Rectangle) As Rectangle
        Return New Rectangle(rect.X + x, rect.Y + y, rect.Width, rect.Height)
    End Function

    Function shiftPoint(point As Point) As Point
        Return New Point(point.X + x, point.Y + y)
    End Function

    Function getCenter() As Point
        Return New Point(CInt(width / 2), CInt(height / 2))
    End Function

    Sub fill(color As System.Drawing.Color)
        drawRect(getRect(), color)
    End Sub

    Sub setPixel(point As Point, color As System.Drawing.Color)
        drawRect(New Rectangle(point.X + x, point.Y + y, 1, 1), color)
    End Sub

    ''' <summary>
    ''' Draws an image to the screen (scaled).
    ''' </summary>
    ''' <param name="image"></param>
    ''' <param name="rect"></param>
    ''' <remarks></remarks>
    Sub blit(image As Image, rect As Rectangle)
        displaybuffer.Graphics.DrawImage(image, rect, -0.5, -0.5, image.Width, image.Height, GraphicsUnit.Pixel)
    End Sub

    ''' <summary>
    ''' Draws an image to the screen (unscaled).
    ''' </summary>
    ''' <param name="image"></param>
    ''' <param name="point"></param>
    ''' <remarks></remarks>
    Sub blit(image As Image, point As Point)
        If Not IsNothing(image) Then
            displaybuffer.Graphics.DrawImageUnscaled(image, shiftPoint(point))
            image.Dispose()
        End If
    End Sub

    Sub drawText(point As Point, s As String, color As System.Drawing.Color, Optional font As Font = Nothing)
        Dim brush As New System.Drawing.SolidBrush(color)
        If IsNothing(font) Then
            font = New Font("Arial", 16)
        End If
        Dim format As New System.Drawing.StringFormat
        displaybuffer.Graphics.DrawString(s, font, brush, point.X + x, point.Y + y, format)
        brush.Dispose()
    End Sub

    Sub drawText(rect As Rectangle, s As String, color As System.Drawing.Color, Optional font As Font = Nothing)
        If IsNothing(font) Then
            font = New Font("Arial", 16)
        End If
        TextRenderer.DrawText(displaybuffer.Graphics, s, font, shiftRect(rect), color, color.Empty, TextFormatFlags.VerticalCenter Or TextFormatFlags.HorizontalCenter)
    End Sub

    'line drawing ------------------------------------------------------------------
    Sub drawLines(ByVal points() As Point, color As System.Drawing.Color, Optional width As Integer = 1)

        If x <> 0 AndAlso y <> 0 Then
            For Each Point As Point In points
                Point = shiftPoint(Point)
            Next
        End If

        If points.Length >= 2 Then
            Dim pen As New Pen(color, width)
            pen.Alignment = Drawing2D.PenAlignment.Center
            displaybuffer.Graphics.DrawLines(pen, points)
            pen.Dispose()
        End If
    End Sub

    Sub drawLine(point1 As Point, point2 As Point, color As System.Drawing.Color, Optional width As Integer = 1)
        Dim pen As New Pen(color, width)
        pen.Alignment = Drawing2D.PenAlignment.Center
        displaybuffer.Graphics.DrawLine(pen, shiftPoint(point1), shiftPoint(point2))
        pen.Dispose()
    End Sub

    'shape drawing ------------------------------------------------------------------
    Sub drawRect(ByVal rect As Rectangle, color As System.Drawing.Color, Optional filled As Boolean = True)
        rect = shiftRect(rect)
        If filled Then
            Dim brush As New System.Drawing.SolidBrush(color)
            displaybuffer.Graphics.FillRectangle(brush, rect)
            brush.Dispose()
        Else
            Dim pen As New Pen(color)
            displaybuffer.Graphics.DrawRectangle(pen, rect)
            pen.Dispose()
        End If
    End Sub

    Sub drawCircle(center As Point, radius As Integer, color As System.Drawing.Color, Optional filled As Boolean = True)
        Dim rect As New Rectangle(center.X - radius, center.Y - radius, radius * 2, radius * 2)
        drawEllipse(rect, color, filled) 'Rect shift not needed, ellipse takes care of that.
    End Sub

    Sub drawEllipse(rect As Rectangle, color As System.Drawing.Color, Optional filled As Boolean = True)
        rect = shiftRect(rect)
        If filled Then
            Dim brush As New System.Drawing.SolidBrush(color)
            displaybuffer.Graphics.FillEllipse(brush, rect)
            brush.Dispose()
        Else
            Dim pen As New Pen(color)
            displaybuffer.Graphics.DrawEllipse(pen, rect)
            pen.Dispose()
        End If
    End Sub
End Class

''' <summary>
''' Game loop must be in a thread.
''' </summary>
''' <remarks></remarks>
Public Class VBGame
    Inherits DrawBase

    Private WithEvents form As Form

    Public Shared white As Color = Color.FromArgb(255, 255, 255)
    Public Shared black As Color = Color.FromArgb(0, 0, 0)
    Public Shared grey As Color = Color.FromArgb(128, 128, 128)
    Public Shared red As Color = Color.FromArgb(255, 0, 0)
    Public Shared green As Color = Color.FromArgb(0, 255, 0)
    Public Shared blue As Color = Color.FromArgb(0, 0, 255)
    Public Shared cyan As Color = Color.FromArgb(0, 255, 255)
    Public Shared yellow As Color = Color.FromArgb(255, 255, 0)
    Public Shared magenta As Color = Color.FromArgb(255, 0, 255)

    Private fps As Integer = 0

    Private fpstimer As Stopwatch = Stopwatch.StartNew()

    Private keyupevents As New List(Of KeyEventArgs)
    Private keydownevents As New List(Of KeyEventArgs)

    Private mouseevents As New List(Of MouseEvent)
    Public mouse As MouseEventArgs
    Public Shared mouse_left As MouseButtons = MouseButtons.Left
    Public Shared mouse_right As MouseButtons = MouseButtons.Right
    Public Shared mouse_middle As MouseButtons = MouseButtons.Middle

    ''' <summary>
    ''' Saves image to a file
    ''' </summary>
    ''' <param name="image"></param>
    ''' <param name="path"></param>
    ''' <param name="format">Default is png format.</param>
    ''' <remarks></remarks>
    Public Shared Sub saveImage(image As Bitmap, path As String, Optional format As System.Drawing.Imaging.ImageFormat = Nothing)
        If IsNothing(format) Then
            format = System.Drawing.Imaging.ImageFormat.Png
        End If
        image.Save(path, format)
    End Sub

    Public Shared Function loadImage(path As String) As Image
        Return Image.FromFile(path)
    End Function

    Public Shared Function collideRect(r1 As Rectangle, r2 As Rectangle) As Boolean
        Return (r1.Left < r2.Right AndAlso r2.Left < r1.Right AndAlso r1.Top < r2.Bottom AndAlso r2.Top < r1.Bottom)
    End Function

    ''' <summary>
    ''' Seperates images from a larger image. Operates from left to right, then moving down.
    ''' </summary>
    ''' <param name="sheet">Image of spritesheet.</param>
    ''' <param name="rowcolumn">Amount of images in the width and height.</param>
    ''' <param name="nimages">How many images from the sheet should be sliced.</param>
    ''' <param name="reverse">To reverse the individual images after slicing.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function sliceSpriteSheet(sheet As Image, rowcolumn As Size, Optional nimages As Integer = 0, Optional reverse As Boolean = False) As List(Of Image)
        Dim list As New List(Of Image)
        Dim n As Integer = 0
        Dim image As Image = New Bitmap(CInt(sheet.Width / rowcolumn.Width), CInt(sheet.Height / rowcolumn.Height))
        Dim g As Graphics = Graphics.FromImage(image)
        g.SmoothingMode = Drawing2D.SmoothingMode.None
        g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
        For y As Integer = 0 To sheet.Height - image.Height Step image.Height
            For x As Integer = 0 To sheet.Width - image.Width Step image.Width
                n += 1
                g.DrawImage(sheet, New Rectangle(0, 0, image.Width, image.Height), New Rectangle(x, y, image.Width, image.Height), GraphicsUnit.Pixel)
                If reverse Then
                    image.RotateFlip(RotateFlipType.RotateNoneFlipX)
                End If
                list.Add(CType(image.Clone(), Drawing.Image))
                g.Clear(Color.Empty)
                If n >= nimages And nimages <> 0 Then
                    Exit For
                End If
            Next
            If n >= nimages And nimages <> 0 Then
                Exit For
            End If
        Next
        Return list
    End Function

    ''' <summary>
    ''' Configures VBGame for operation. Must be called before starting game loop.
    ''' </summary>
    ''' <param name="f">Form that will be drawn on.</param>
    ''' <param name="resolution">Width and height of display area, in pixels.</param>
    ''' <param name="title">String that will be displayed on title bar.</param>
    ''' <param name="sharppixels">Enabling this will turn off pixel smoothing. Good for pixel art.</param>
    ''' <param name="fullscreen"></param>
    ''' <remarks></remarks>
    Sub setDisplay(ByRef f As Form, resolution As Size, Optional title As String = "", Optional sharppixels As Boolean = False, Optional fullscreen As Boolean = False)
        form = f

        setSize(resolution)

        form.Invoke(Sub() form.Text = title)

        form.Invoke(Sub() form.KeyPreview = True)

        If fullscreen Then
            form.Invoke(Sub() form.FormBorderStyle = Windows.Forms.FormBorderStyle.None)
            form.Invoke(Sub() form.WindowState = FormWindowState.Maximized)
        Else
            form.Invoke(Sub() form.FormBorderStyle = Windows.Forms.FormBorderStyle.FixedSingle)
            form.Invoke(Sub() form.WindowState = FormWindowState.Normal)
        End If

        displaycontext = BufferedGraphicsManager.Current
        parentgraphics = form.CreateGraphics
        allocate()

        If sharppixels Then
            displaybuffer.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighSpeed
            displaybuffer.Graphics.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
        End If

    End Sub

    Sub setSize(size As Size)
        width = size.Width
        height = size.Height
        form.Invoke(Sub() form.Width = width)
        form.Invoke(Sub() form.Height = height)
        form.Invoke(Sub() form.Width += form.Width - form.DisplayRectangle().Width)
        form.Invoke(Sub() form.Height += form.Height - form.DisplayRectangle().Height)
    End Sub

    Sub pushKeyUpEvent(key As KeyEventArgs)
        keyupevents.Add(key)
    End Sub

    Sub pushKeyDownEvent(key As KeyEventArgs)
        keydownevents.Add(key)
    End Sub

    Sub pushMouseEvent(e As MouseEvent)
        mouseevents.Add(e)
    End Sub

    Function getKeyUpEvents() As List(Of KeyEventArgs)
        Dim tlist As List(Of KeyEventArgs)
        Try
            tlist = keyupevents.ToList()
        Catch ex As ArgumentException
            tlist = New List(Of KeyEventArgs)
        End Try
        keyupevents.Clear()
        Return tlist
    End Function

    Function getKeyDownEvents() As List(Of KeyEventArgs)
        Dim tlist As List(Of KeyEventArgs)
        Try
            tlist = keydownevents.ToList()
        Catch ex As ArgumentException
            tlist = New List(Of KeyEventArgs)
        End Try
        keydownevents.Clear()
        Return tlist
    End Function

    Function getMouseEvents() As List(Of MouseEvent)
        Dim tlist As List(Of MouseEvent)
        Try
            tlist = mouseevents.ToList()
        Catch ex As ArgumentException
            tlist = New List(Of MouseEvent)
        End Try
        mouseevents.Clear()
        Return tlist
    End Function

    'Form event hooks.

    Private Sub form_MouseWheel(ByVal sender As Object, ByVal e As MouseEventArgs) Handles form.MouseWheel
        mouseevents.Add(MouseEvent.InterpretFormEvent(e, CByte(MouseEvent.actions.scroll)))
        mouse = e
    End Sub

    Private Sub form_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs) Handles form.MouseMove
        mouseevents.Add(MouseEvent.InterpretFormEvent(e, CByte(MouseEvent.actions.move)))
        mouse = e
    End Sub

    Private Sub form_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles form.MouseDown
        mouseevents.Add(MouseEvent.InterpretFormEvent(e, CByte(MouseEvent.actions.down)))
        mouse = e
    End Sub

    Private Sub form_MouseClick(ByVal sender As Object, ByVal e As MouseEventArgs) Handles form.MouseClick
        mouseevents.Add(MouseEvent.InterpretFormEvent(e, CByte(MouseEvent.actions.up)))
        mouse = e
    End Sub

    Private Sub form_MouseDoubleClick(ByVal sender As Object, ByVal e As MouseEventArgs) Handles form.MouseDoubleClick
        mouseevents.Add(MouseEvent.InterpretFormEvent(e, CByte(MouseEvent.actions.up)))
        mouse = e
    End Sub

    Private Sub form_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles form.KeyDown
        keydownevents.Add(e)
    End Sub

    Private Sub form_KeyUp(ByVal sender As Object, ByVal e As KeyEventArgs) Handles form.KeyUp
        keyupevents.Add(e)
    End Sub

    ''' <summary>
    ''' Waits so that the specified fps can be achieved.
    ''' </summary>
    ''' <param name="fps"></param>
    ''' <remarks></remarks>
    Sub clockTick(fps As Double)
        Dim tfps As Double
        tfps = 1000 / fps
        While fpstimer.ElapsedMilliseconds < tfps
        End While
        fpstimer.Reset()
        fpstimer.Start()
    End Sub

    ''' <summary>
    ''' Gets the time in milliseconds since the last clockTick()
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function getTime() As Long
        Return fpstimer.ElapsedMilliseconds
    End Function

    Function getImageFromDisplay() As Image
        Dim bitmap As Bitmap = New Bitmap(width, height, displaybuffer.Graphics)
        Dim g As Graphics = Graphics.FromImage(bitmap)
        g.CopyFromScreen(New Point(CInt(form.Location.X + (form.Width - form.DisplayRectangle().Width) / 2), CInt(form.Location.Y + (form.Height - form.DisplayRectangle().Height) * (15 / 19))), New Point(0, 0), New Size(width, height))
        Return bitmap
    End Function
End Class

''' <summary>
''' Gets a portion of the display given to do drawing operations on.
''' Anything drawn outside of the bounds of the surface will not be drawn on the parent display.
''' Surfaces are static.
''' </summary>
''' <remarks></remarks>
Public Class Surface
    Inherits DrawBase

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="rect"></param>
    ''' <param name="parentdisplay">Display to draw on.</param>
    ''' <remarks></remarks>
    Public Sub New(rect As Rectangle, parentdisplay As VBGame)

        x_shift = rect.X
        y_shift = rect.Y
        width = rect.Width
        height = rect.Height

        displaycontext = BufferedGraphicsManager.Current

        parentgraphics = parentdisplay.displaybuffer.Graphics

        allocate()
    End Sub

End Class

Public Class BitmapSurface
    Inherits DrawBase

    Public bitmap As Bitmap

    Public Sub New(size As Size, Optional format As Imaging.PixelFormat = Nothing)

        If format = Imaging.PixelFormat.Undefined Then
            format = Imaging.PixelFormat.Format24bppRgb
        End If

        width = size.Width
        height = size.Height

        bitmap = New Bitmap(width, height, format)
        bitmap.MakeTransparent()

        parentgraphics = Graphics.FromImage(bitmap)

        displaycontext = BufferedGraphicsManager.Current
        allocate()
    End Sub

    Public Function getImage(Optional autoupdate As Boolean = True) As Image
        If autoupdate Then
            update()
        End If
        Return bitmap
    End Function

End Class

Public Class Sound

    Public Declare Function mciSendString Lib "winmm.dll" Alias "mciSendStringA" (ByVal lpstrCommand As String, ByVal lpstrReturnString As String, ByVal uReturnLength As Integer, ByVal hwndCallback As Integer) As Integer

    Public name As String
    Private vol As Integer = 1000

    Public Sub New(filename As String)
        name = filename
        load()
    End Sub

    ''' <summary>
    ''' Changing this will update the volume.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Property volume As Integer
        Set(value As Integer)
            vol = CInt(value)
            If vol < 0 Then
                vol = 0
            End If
            If vol > 1000 Then
                vol = 1000
            End If

            'Dim thread As New Thread(AddressOf Me.setVolume)
            'thread.IsBackground = True
            'thread.Start(vol)
            setVolume(vol)
        End Set
        Get
            Return vol
        End Get
    End Property

    Sub load()
        mciSendString("Open """ & getPath() & """ alias """ & name & """", CStr(0), 0, 0)
    End Sub

    ''' <summary>
    ''' </summary>
    ''' <param name="repeat">If enabled, the sound will loop. Note: this does not work with .wav files.</param>
    ''' <remarks></remarks>
    Sub play(Optional repeat As Boolean = False)
        Dim thread As New Thread(AddressOf Me.playSync)
        thread.IsBackground = True
        thread.Start(repeat)
    End Sub

    Private Sub playSync(repeat As Object)
        If CBool(repeat) Then
            mciSendString("play """ & name & """ repeat", CStr(0), 0, 0)
        Else
            mciSendString("play """ & name & """", CStr(0), 0, 0)
        End If
    End Sub

    Sub halt()
        mciSendString("close """ & name & """", CStr(0), 0, 0)
    End Sub

    Sub pause()
        mciSendString("pause """ & name & """", CStr(0), 0, 0)
    End Sub

    Sub resumePaused()
        mciSendString("resume """ & name & """", CStr(0), 0, 0)
    End Sub

    Private Sub setVolume(volume As Integer)
        Console.WriteLine(mciSendString("setaudio """ & name & """ volume to " & CStr(volume), CStr(0), 0, 0))
    End Sub

    Private Function getPath() As String
        Return Directory.GetCurrentDirectory() & "\" & name
    End Function

End Class

Public Class Animation

    Public frames As New List(Of Image)
    Public interval As Integer 'time between frames (ms)
    Public index As Integer 'current frame

    Public playing As Boolean

    Public loopanim As Boolean = True

    Public timer As New Stopwatch

    Public Function clone() As Animation
        Return DirectCast(Me.MemberwiseClone(), Animation)
    End Function

    Sub playAnim()
        playing = True
        timer.Start()
    End Sub

    Sub stopAnim()
        playing = False
        timer.Reset()
        index = 0
    End Sub

    Sub pauseAnim()
        playing = False
        timer.Stop()
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="strip">Image of spritesheet.</param>
    ''' <param name="rowcolumn">Amount of images in the width and height.</param>
    ''' <param name="nframes">How many images from the sheet should be sliced.</param>
    ''' <param name="reverse">To reverse the individual images after slicing.</param>
    ''' <param name="animloop">If enabled, the animation will loop.</param>
    ''' <remarks></remarks>
    Sub New(strip As Image, rowcolumn As Size, timing As Integer, Optional nframes As Integer = 0, Optional reverse As Boolean = False, Optional animloop As Boolean = True)
        loopanim = animloop
        index = 0
        interval = timing
        getFramesFromStrip(strip, rowcolumn, nframes, reverse)
        playing = False
    End Sub

    ''' <summary>
    ''' See VBGame.sliceSpriteSheet()
    ''' </summary>
    ''' <remarks></remarks>
    Sub getFramesFromStrip(strip As Image, rowcolumn As Size, Optional nframes As Integer = 0, Optional reverse As Boolean = False)
        frames = VBGame.sliceSpriteSheet(strip, rowcolumn, nframes, reverse)
    End Sub

    ''' <summary>
    ''' Used in conjection with VBGame.blit(), this will pick the image to return based on a timer.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function handle() As Image
        While timer.ElapsedMilliseconds >= interval
            timer.Restart()
            Return getFrame(loopanim)
        End While
        Return frames(index)
    End Function

    ''' <summary>
    ''' Gets the next frame.
    ''' </summary>
    ''' <param name="loopanim"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function getFrame(Optional loopanim As Boolean = True) As Image
        Dim frame As Image
        frame = frames(index)
        index += 1
        If index >= frames.ToArray.Length Then
            If loopanim Then
                index = 0
            Else
                index -= 1
                playing = False
            End If
        End If
        Return frame
    End Function

End Class

Public Class Animations

    Private items As New Dictionary(Of String, Animation)

    Public active As String

    Public Function clone() As Animations
        Return DirectCast(Me.MemberwiseClone(), Animations)
    End Function

    Sub addAnim(key As String, animation As Animation)
        items.Add(key, animation)
        If IsNothing(active) Then
            active = key
        End If
    End Sub

    Sub setActive(key As String, Optional autoplay As Boolean = True)
        If active <> key Then
            getAnim(active).stopAnim()
            active = key
            If autoplay Then
                getAnim(active).playAnim()
            End If
        End If
    End Sub

    ''' <summary>
    ''' Returns a frame from the active animation.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function handle() As Image
        Return getAnim(active).handle()
    End Function

    Function getAnim(key As String) As Animation
        Return items(key)
    End Function

    Sub playActive()
        getActive().playAnim()
    End Sub

    Sub stopActive()
        getActive().stopAnim()
    End Sub

    Sub pauseActive()
        getActive().pauseAnim()
    End Sub

    Function getActive() As Animation
        Return items(active)
    End Function

End Class

Public Class Sprite
    Public image As Image
    Public width As Double = 0
    Public height As Double = 0

    Public x As Double = 0
    Public y As Double = 0
    Public pxc As Double = 0
    Public nxc As Double = 0
    Public pyc As Double = 0
    Public nyc As Double = 0

    ''' <summary>
    ''' How much the x value of the sprite should move when move() is called.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property xc As Double
        Set(value As Double)
            If value > 0 Then
                nxc = 0
                pxc = value
            ElseIf value < 0 Then
                pxc = 0
                nxc = Math.Abs(value)
            Else
                pxc = 0
                nxc = 0
            End If
        End Set
        Get
            Return CDbl(pxc - nxc)
        End Get
    End Property

    ''' <summary>
    ''' How much the y value of the sprite should move when move() is called.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property yc As Double
        Set(value As Double)
            If value > 0 Then
                nyc = 0
                pyc = value
            ElseIf value < 0 Then
                pyc = 0
                nyc = Math.Abs(value)
            Else
                pyc = 0
                nyc = 0
            End If
        End Set
        Get
            Return CDbl(pyc - nyc)
        End Get
    End Property

    Public angle As Double = 0
    Public speed As Double = 0
    Public frames As Integer = 0
    Public color As System.Drawing.Color = color.White

    Public animations As New Animations

    Public Function clone() As Sprite
        Return DirectCast(Me.MemberwiseClone(), Sprite)
    End Function

    Public Sub New(Optional rect As Rectangle = Nothing)
        If Not IsNothing(rect) Then
            setRect(rect)
        End If
    End Sub

    Sub move(Optional trig As Boolean = False)
        Dim mp As PointF
        mp = calcMove(trig)
        x = mp.X
        y = mp.Y
    End Sub

    Function calcMove(Optional trig As Boolean = False) As PointF
        Dim xt, yt As Double
        If trig Then
            xt = x + Math.Cos(angle * (Math.PI / 180)) * speed
            yt = y + Math.Sin(angle * (Math.PI / 180)) * speed
        Else
            xt = x + pxc - nxc
            yt = y + pyc - nyc
        End If
        Return New PointF(CSng(xt), CSng(yt))
    End Function

    ''' <summary>
    ''' Ensures the sprite's angle is between 0 and 360 degrees.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub normalizeAngle()
        While angle > 360
            angle -= 360
        End While
        While angle < 0
            angle += 360
        End While
    End Sub

    Public Enum Sides
        none
        top
        bottom
        left
        right
    End Enum

    ''' <summary>
    ''' Keeps the sprite in a rectangle.
    ''' </summary>
    ''' <param name="bounds">Rectangle container.</param>
    ''' <param name="trig">Whether or not the sprite is using angled movement.</param>
    ''' <param name="bounce">If enabled, the sprite will change it's movement to give the appearence of bouncing.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function keepInBounds(bounds As Rectangle, Optional trig As Boolean = False, Optional bounce As Boolean = False) As Boolean
        Dim wd As Boolean = False

        If x + width > bounds.X + bounds.Width Then
            wd = True
            x = bounds.X + bounds.Width - width
            If bounce Then
                bounceX(trig)
            End If

        ElseIf x < bounds.X Then
            wd = True
            x = bounds.X
            If bounce Then
                bounceX(trig)
            End If
        End If

        If y + height > bounds.Y + bounds.Height Then
            wd = True
            y = bounds.Y + bounds.Height - height
            If bounce Then
                bounceY(trig)
            End If

        ElseIf y < bounds.Y Then
            wd = True
            y = bounds.Y
            If bounce Then
                bounceY(trig)
            End If
        End If

        Return wd
    End Function

    Private Sub bounceY(trig As Boolean)
        If trig Then
            angle = -angle
        Else
            Dim tmp As Double
            tmp = pyc
            pyc = nyc
            nyc = tmp
        End If
    End Sub

    Private Sub bounceX(trig As Boolean)
        If trig Then
            angle = -angle + 180
        Else
            Dim tmp As Double
            tmp = pxc
            pxc = nxc
            nxc = tmp
        End If
    End Sub

    Private Function verticalCollisions(intersectRect As Rectangle, hitRect As Rectangle, trig As Boolean, bounce As Boolean) As Byte

        If intersectRect.Y = hitRect.Y Then
            y = hitRect.Y - height
            If bounce Then
                bounceY(trig)
            End If
            Return CByte(Sides.top)

        ElseIf intersectRect.Y + intersectRect.Height = hitRect.Y + hitRect.Height Then
            y = hitRect.Bottom
            If bounce Then
                bounceY(trig)
            End If
            Return CByte(Sides.bottom)
        End If

        Return CByte(Sides.none)
    End Function

    Private Function horizontalCollisions(intersectRect As Rectangle, hitRect As Rectangle, trig As Boolean, bounce As Boolean) As Byte

        If intersectRect.X = hitRect.X Then
            x = hitRect.X - width
            If bounce Then
                bounceX(trig)
            End If
            Return CByte(Sides.left)

        ElseIf intersectRect.X + intersectRect.Width = hitRect.X + hitRect.Width Then
            x = hitRect.Right
            If bounce Then
                bounceX(trig)
            End If
            Return CByte(Sides.right)
        End If

        Return CByte(Sides.none)
    End Function

    Public Function keepOutsideBounds(bounds As Rectangle, Optional trig As Boolean = False, Optional bounce As Boolean = False) As Byte
        If VBGame.collideRect(getRect(), bounds) Then
            Dim side As Byte
            Dim intersectRect As Rectangle = getRect()

            intersectRect.Intersect(bounds)

            If intersectRect.Width > intersectRect.Height Then
                side = verticalCollisions(intersectRect, bounds, trig, bounce)
                If side = Sides.none Then
                    Return horizontalCollisions(intersectRect, bounds, trig, bounce)
                Else
                    Return side
                End If

            Else
                side = horizontalCollisions(intersectRect, bounds, trig, bounce)
                If side = Sides.none Then
                    Return verticalCollisions(intersectRect, bounds, trig, bounce)
                Else
                    Return side
                End If
            End If
        Else
            Return CByte(Sides.none)
        End If

    End Function

    Sub setRect(rect As Rectangle)
        x = rect.X
        y = rect.Y
        width = rect.Width
        height = rect.Height
    End Sub

    Sub setXY(point As Point)
        x = point.X
        y = point.Y
    End Sub

    Function getRect() As Rectangle
        Return New Rectangle(CInt(x), CInt(y), CInt(width), CInt(height))
    End Function

    Function getXY() As Point
        Return New Point(CInt(x), CInt(y))
    End Function

    Function getCenter() As Point
        Return New Point(CInt(x + width / 2), CInt(y + height / 2))
    End Function

    Function getRadius() As Double
        Return (getRect().Width / 2 + getRect().Width / 2) / 2
    End Function
End Class

Class Button

    Inherits Sprite

    ''' <summary>
    ''' Put in vbgame.getMouseEvents() loop.
    ''' </summary>
    ''' <remarks></remarks>

    Public display As VBGame
    Public hover As Boolean = False
    Public hovercolor As Color
    Public hoverimage As Image

    Public text As String
    Public hovertext As String
    Public fontsize As Integer
    Public fontname As String
    Public textcolor As System.Drawing.Color
    Public hovertextcolor As System.Drawing.Color

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="vbgame">Display to draw onto.</param>
    ''' <param name="textt">Text to display on the button.</param>
    ''' <param name="rect">Rectangle of the button</param>
    ''' <param name="fontnamet"></param>
    ''' <param name="fontsizet"></param>
    ''' <remarks></remarks>
    Public Sub New(ByRef vbgame As VBGame, textt As String, Optional rect As Rectangle = Nothing, Optional fontnamet As String = "Arial", Optional fontsizet As Integer = 0)
        display = vbgame
        If Not IsNothing(rect) Then
            setRect(rect)
        End If
        text = textt
        fontname = fontnamet
        If fontsizet = 0 Then
            calculateFontSize()
        Else
            fontsize = fontsizet
        End If
    End Sub

    ''' <summary>
    ''' Calculates the font size based on the current rectangle.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub calculateFontSize()
        For f As Integer = 1 To 75
            If display.displaybuffer.Graphics.MeasureString(text, New Font(fontname, f)).Width < width Then
                fontsize = f
            End If
        Next
    End Sub

    Public Sub setColor(mouseoff As System.Drawing.Color, mouseon As System.Drawing.Color)
        color = mouseoff
        hovercolor = mouseon
    End Sub

    Public Sub setTextColor(mouseoff As System.Drawing.Color, mouseon As System.Drawing.Color)
        textcolor = mouseoff
        hovertextcolor = mouseon
    End Sub

    ''' <summary>
    ''' Draws the button. Keep out of event loops.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub draw()
        If IsNothing(image) Then
            If hover Then
                display.drawRect(getRect(), hovercolor)
            Else
                display.drawRect(getRect(), color)
            End If
        Else
            If hover Then
                display.blit(hoverimage, getRect())
            Else
                display.blit(image, getRect())
            End If
        End If

        If hover Then
            If IsNothing(hovertext) Then
                display.drawText(getRect(), text, hovertextcolor, New Font(fontname, fontsize))
            Else
                display.drawText(getRect(), hovertext, hovertextcolor, New Font(fontname, fontsize))
            End If
        Else
            display.drawText(getRect(), text, textcolor, New Font(fontname, fontsize))
        End If

    End Sub

    ''' <summary>
    ''' Put inside the VBGame.getMouseEvents() loop. Will return the MouseEvent of a successful click.
    ''' </summary>
    ''' <param name="e">MouseEvent from loop.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function handle(e As MouseEvent) As Byte
        If VBGame.collideRect(New Rectangle(e.location.X, e.location.Y, 1, 1), getRect()) Then
            hover = True
            If e.action = MouseEvent.actions.up Then
                Return e.button
            End If
        Else
            hover = False
        End If
        Return CByte(MouseEvent.buttons.none)
    End Function

End Class

Class TextInput

    Public display As BitmapSurface

    Public width As Double = 0
    Public height As Double = 0
    Public x As Double = 0
    Public y As Double = 0

    Public text As String
    Public focus As Boolean = True

    Public fontName As String
    Public fontSize As Integer

    Public fontColor As Color
    Public color As Color

    Public allowNewLine As Boolean

    Private blinkTimer As New Stopwatch

    Sub setRect(rect As Rectangle)
        x = rect.X
        y = rect.Y
        width = rect.Width
        height = rect.Height
    End Sub

    Sub setXY(point As Point)
        x = point.X
        y = point.Y
    End Sub

    Function getRect() As Rectangle
        Return New Rectangle(CInt(x), CInt(y), CInt(width), CInt(height))
    End Function

    Function getXY() As Point
        Return New Point(CInt(x), CInt(y))
    End Function

    Function getCenter() As Point
        Return New Point(CInt(x + width / 2), CInt(y + height / 2))
    End Function

    Public Sub New(rect As Rectangle, Optional fontnamet As String = "Arial", Optional fontsizet As Integer = 0)

        setRect(rect)


        display = New BitmapSurface(New Size(rect.Width, rect.Height))

        fontName = fontnamet
        If fontsizet = 0 Then
            calculateFontSize()
        Else
            fontSize = fontsizet
        End If

        fontColor = VBGame.black
        color = color.FromArgb(0, 0, 0, 0)

        allowNewLine = False

        text = ""

        blinkTimer.Start()

    End Sub

    Private Sub calculateFontSize()
        For f As Integer = 1 To 75
            If display.displaybuffer.Graphics.MeasureString(text, New Font(fontName, f)).Width < width Then
                fontSize = f
            End If
        Next
    End Sub

    Public Function handle(e As KeyEventArgs) As Boolean

        If e.KeyCode = Keys.Back AndAlso text.Length > 0 Then
            text = text.Substring(0, text.Length - 1)

        ElseIf e.KeyCode >= Keys.A AndAlso e.KeyCode <= Keys.Z Then
            If e.Shift Then
                addText(e.KeyCode.ToString())
            Else
                addText(e.KeyCode.ToString().ToLower())
            End If

        ElseIf e.KeyCode >= Keys.D0 AndAlso e.KeyCode <= Keys.D9 Then
            addText(e.KeyCode.ToString().Substring(1, 1))

        ElseIf e.KeyCode = Keys.Space Then
            addText(" ")
        ElseIf e.KeyCode = Keys.OemPeriod Then
            addText(".")
        ElseIf e.KeyCode = Keys.Oemcomma Then
            addText(",")
        ElseIf e.KeyCode = Keys.Enter AndAlso allowNewLine Then
            addText(vbCrLf)
        ElseIf e.KeyCode = Keys.Enter Then
            Return True
        End If

        Return False
    End Function

    Private Sub addText(s As String)
        If checkLength(s) Then
            text = text & s
        End If
    End Sub

    Private Function checkLength(s As String) As Boolean
        'Dim size As SizeF = display.displaybuffer.Graphics.MeasureString(text & s, New Font(fontName, fontSize))
        'If size.Width > width OrElse size.Height > height Then
        '    Return False
        'End If
        Return True
    End Function

    Public Sub draw(mdisplay As VBGame)
        display.drawRect(New Rectangle(0, 0, CInt(width), CInt(height)), color)

        If blinkTimer.ElapsedMilliseconds < 500 Then
            display.drawText(New Point(0, 0), text, fontColor)
        ElseIf blinkTimer.ElapsedMilliseconds >= 500 Then
            display.drawText(New Point(0, 0), text & "|", fontColor)
            If blinkTimer.ElapsedMilliseconds >= 1000 Then
                blinkTimer.Restart()
            End If
        End If

        mdisplay.blit(display.getImage(), getRect())

    End Sub

End Class