' The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

'
'* Copyright (c) 2014 Microsoft Mobile
'* 
'* Permission is hereby granted, free of charge, to any person obtaining a copy
'* of this software and associated documentation files (the "Software"), to deal
'* in the Software without restriction, including without limitation the rights
'* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'* copies of the Software, and to permit persons to whom the Software is
'* furnished to do so, subject to the following conditions:
'* The above copyright notice and this permission notice shall be included in
'* all copies or substantial portions of the Software.
'* 
'* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
'* THE SOFTWARE.
'

Imports Lumia.Imaging
Imports Lumia.Imaging.Adjustments
Imports Windows.Storage
Imports Windows.Storage.Pickers
Imports Windows.Storage.Streams
Imports Windows.UI.Popups
''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page

    Private _grayscaleEffect As GrayscaleEffect
    Private _brightnessEffect As BrightnessEffect
    Private m_renderer As SwapChainPanelRenderer

    ' The following  WriteableBitmap contains 
    ' The filtered and thumbnail image.
    Private _writeableBitmap As WriteableBitmap
    Private _thumbnailImageBitmap As WriteableBitmap

    Public Sub New()

        InitializeComponent()

        Dim scaleFactor As Double = 1.0
        scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel

        _writeableBitmap = New WriteableBitmap(CInt(Window.Current.Bounds.Width * scaleFactor), CInt(Window.Current.Bounds.Height * scaleFactor))
        _thumbnailImageBitmap = New WriteableBitmap(CInt(OriginalImage.Width), CInt(OriginalImage.Height))
        _grayscaleEffect = New GrayscaleEffect()
        _brightnessEffect = New BrightnessEffect(_grayscaleEffect)

    End Sub

    Private Async Sub OnSwapChainPanelLoaded(sender As Object, e As RoutedEventArgs) Handles SwapChainPanelTarget.Loaded
        If SwapChainPanelTarget.ActualHeight > 0 AndAlso SwapChainPanelTarget.ActualWidth > 0 Then
            If m_renderer Is Nothing Then
                m_renderer = New SwapChainPanelRenderer(_brightnessEffect, SwapChainPanelTarget)
                Await LoadDefaultImageAsync()
            End If
        End If


    End Sub

    Private Async Sub OnSwapChaninPanelSizeChaged(sender As Object, e As SizeChangedEventArgs) Handles SwapChainPanelTarget.SizeChanged
        If m_renderer Is Nothing Then
            Return
        End If

        Await m_renderer.RenderAsync()


    End Sub



    Private Async Function LoadDefaultImageAsync() As Task
        Dim file = Await StorageFile.GetFileFromApplicationUriAsync(New System.Uri("ms-appx:///Assets/defaultImage.jpg"))
        Await ApplyEffectAsync(file)
    End Function

    Private Async Function ApplyEffectAsync(file As StorageFile) As Task(Of Boolean)
        ' Open a stream for the selected file.
        Dim fileStream As IRandomAccessStream = Await file.OpenAsync(FileAccessMode.Read)

        Dim errorMessage As String = Nothing

        Try
            ' Show thumbnail of original image.
            _thumbnailImageBitmap.SetSource(fileStream)
            OriginalImage.Source = _thumbnailImageBitmap

            ' Rewind stream to start.                     
            fileStream.Seek(0)

            ' Set the imageSource on the effect and render            
            DirectCast(_grayscaleEffect, IImageConsumer).Source = New Lumia.Imaging.RandomAccessStreamImageSource(fileStream)

            Await m_renderer.RenderAsync()

        Catch exception As Exception
            errorMessage = exception.Message
        End Try

        If Not String.IsNullOrEmpty(errorMessage) Then
            Dim dialog = New MessageDialog(errorMessage)
            Await dialog.ShowAsync()
            Return False
        End If

        Return True
    End Function

    Private Async Function SaveImageAsync(file As StorageFile) As Task(Of Boolean)

        If _grayscaleEffect Is Nothing Then
            Return False
        End If

        Dim errorMessage As String = Nothing

        Try
            Using jpegRenderer = New JpegRenderer(_grayscaleEffect)
                Using stream = Await file.OpenAsync(FileAccessMode.ReadWrite)
                    ' Jpeg renderer gives the raw buffer containing the filtered image.
                    Dim jpegBuffer As IBuffer = Await jpegRenderer.RenderAsync()
                    Await stream.WriteAsync(jpegBuffer)
                    Await stream.FlushAsync()
                End Using
            End Using
        Catch exception As Exception
            errorMessage = exception.Message
        End Try

        If Not String.IsNullOrEmpty(errorMessage) Then
            Dim dialog = New MessageDialog(errorMessage)
            Await dialog.ShowAsync()
            Return False
        End If
        Return True
    End Function

    Private Sub PickImage_Click(sender As Object, e As RoutedEventArgs)

        Dim openPicker = New FileOpenPicker()
        openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary
        openPicker.ViewMode = PickerViewMode.Thumbnail


        ' Filter to include a sample subset of file types.
        openPicker.FileTypeFilter.Clear()
        openPicker.FileTypeFilter.Add(".bmp")
        openPicker.FileTypeFilter.Add(".png")
        openPicker.FileTypeFilter.Add(".jpeg")
        openPicker.FileTypeFilter.Add(".jpg")

        PickImage(openPicker)
    End Sub

    Private Sub SaveImage_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click

        SaveButton.IsEnabled = False

        Dim savePicker = New FileSavePicker()

        savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary
        savePicker.SuggestedFileName = String.Format("QuickstartImage_{0}", DateTime.Now.ToString("yyyyMMddHHmmss"))

        savePicker.FileTypeChoices.Add("JPG File", New List(Of String)() From {".jpg"})

        SaveImage(savePicker)
        SaveButton.IsEnabled = False
    End Sub


    Private Async Sub PickImage(openPicker As FileOpenPicker)
        ' Open the file picker.
        Dim file As StorageFile = Await openPicker.PickSingleFileAsync()

        ' file is null if user cancels the file picker.
        If file IsNot Nothing Then
            If Not (Await ApplyEffectAsync(file)) Then
                Return
            End If

            SaveButton.IsEnabled = True
        End If
    End Sub

    Private Async Sub SaveImage(savePicker As FileSavePicker)

        Dim file = Await savePicker.PickSaveFileAsync()
        If file IsNot Nothing Then
            Await SaveImageAsync(file)
        End If

        SaveButton.IsEnabled = True
    End Sub

    Private Async Sub OnBrightnessChanged(sender As Object, e As RangeBaseValueChangedEventArgs)

        _brightnessEffect.Level = e.NewValue
        If DirectCast(_grayscaleEffect, IImageConsumer).Source IsNot Nothing Then
            Await m_renderer.RenderAsync()
        End If

    End Sub


End Class
