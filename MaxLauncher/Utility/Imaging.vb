'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Text

Namespace Utility

    Public NotInheritable Class Imaging

        ''' <summary>
        ''' Application Icon.
        ''' </summary>
        ''' <remarks>Using My.Resources.maxlauncher fails in .Net 3.5</remarks>
        Private Shared appIcon As System.Drawing.Icon = Nothing
        'Private Shared appIcon As System.Drawing.Icon = System.Drawing.Icon.ExtractAssociatedIcon(
        '   System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name)

        ''' <summary>
        ''' Application icon property.
        ''' </summary>
        ''' <value>default: The System.Drawing.Icon extracted from the assembly exe file.</value>
        ''' <returns>The System.Drawing.Icon representation of the application icon.</returns>
        Friend Shared Property ApplicationIcon() As System.Drawing.Icon
            Get
                Return appIcon
            End Get
            Set(ByVal value As System.Drawing.Icon)
                appIcon = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the ImageSource representation of the application icon.
        ''' </summary>
        ''' <returns>The System.Windows.Media.ImageSource representation of the application icon.
        ''' default: The System.Windows.Media.ImageSource representation of the application icon.</returns>
        Friend Shared Function GetAppIconImage() As System.Windows.Media.ImageSource
            Dim iconStream As New System.IO.MemoryStream
            Dim retValue As BitmapFrame = Nothing

            Try
                ApplicationIcon.Save(iconStream)
                iconStream.Seek(0, System.IO.SeekOrigin.Begin)
                retValue = BitmapFrame.Create(iconStream)
            Catch ex As Exception
                'Ignore
            Finally
                If (iconStream IsNot Nothing) Then iconStream.Dispose()
            End Try

            Return retValue
        End Function

        ''' <summary>
        ''' Gets the ImageSource representation of the application icon.
        ''' </summary>
        ''' <param name="icon">The System.Drawing.Icon source.</param>
        ''' <returns>The System.Windows.Media.ImageSource representation of the application icon.</returns>
        ''' <remarks>Using a MemoryStream to get an ImageSource representation from an icon
        ''' such as the one in GetAppIconImage results in a blurry image.</remarks>
        Friend Shared Function GetImageFromIcon(ByVal icon As System.Drawing.Icon) As System.Windows.Media.ImageSource
            Dim retValue As ImageSource = Nothing

            Try
                retValue = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            System.Windows.Int32Rect.Empty,
                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions)
            Catch ex As Exception
                'Ignore
            End Try

            Return retValue
        End Function

        Friend Shared Function GetImageFromIconFile(ByVal fileName As String, ByVal index As Integer) As System.Windows.Media.ImageSource
            Dim retValue As ImageSource = Nothing
            Dim hIcon As IntPtr

            Try
                'Make sure iconIndex is >= 0
                'Use try/catch. In some rare cases where index = -1 throws an exception.
                'A -1 returns the number of icons from the file.
                'For example, when dragging the iSCSI initiation shortcut to a button.
                If (index >= 0) Then
                    hIcon = NativeMethods.ExtractIcon(IntPtr.Zero, fileName, index)

                    retValue = GetImageFromIcon(System.Drawing.Icon.FromHandle(hIcon))
                End If
            Catch ex As Exception
                'Ignore.
            Finally
                NativeMethods.DestroyIcon(hIcon)
                hIcon = IntPtr.Zero
            End Try

            Return retValue
        End Function

        Friend Shared Function GetImageFromImageFile(ByVal filePath As String, ByVal decodedWidth As Integer) As System.Windows.Media.ImageSource
            Dim bitmapImage As New BitmapImage
            Dim errorFlag As Boolean = True

            Try
                ' BitmapImage.UriSource must be in a BeginInit/EndInit block.
                bitmapImage.BeginInit()
                bitmapImage.UriSource = New Uri(filePath, UriKind.RelativeOrAbsolute)
                bitmapImage.DecodePixelWidth = decodedWidth
                bitmapImage.EndInit()
                errorFlag = False
            Catch ex As Exception
            Finally
                If (errorFlag) Then bitmapImage = Nothing
            End Try

            Return bitmapImage
        End Function

        Friend Shared Function GetImageFromFile(ByVal filePath As String) As System.Windows.Media.ImageSource
            Dim retValue As ImageSource = Nothing

            Try
                Using sysicon As System.Drawing.Icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath)
                    retValue = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                    sysicon.Handle,
                                    System.Windows.Int32Rect.Empty,
                                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions())
                End Using
            Catch ex As Exception
                'Ignore.
            End Try

            Return retValue
        End Function

        Friend Shared Function BitmapToBitmapImage(ByVal bitmap As System.Drawing.Bitmap) As BitmapImage
            Dim bitmapImage As BitmapImage = New BitmapImage()

            Using memory As System.IO.MemoryStream = New System.IO.MemoryStream()
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png)
                memory.Position = 0
                bitmapImage.BeginInit()
                bitmapImage.StreamSource = memory
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad
                bitmapImage.EndInit()
            End Using

            Return bitmapImage
        End Function

        'Friend Shared Function ImageFileToBitmapImage(ByVal filePath As String, ByVal decodedWidth As Integer) As BitmapImage
        '    Dim bitmapImage As Bitmap = Nothing
        '    Dim bitmapImageResized As Bitmap = Nothing
        '    Dim retValue As BitmapImage = Nothing

        '    Try
        '        bitmapImage = New Bitmap(filePath)

        '        bitmapImageResized = New Bitmap(bitmapImage, decodedWidth, decodedWidth)

        '        retValue = BitmapToBitmapImage(bitmapImageResized)
        '    Catch ex As Exception
        '        'Ignore.
        '    Finally
        '        If (bitmapImage IsNot Nothing) Then bitmapImage.Dispose()
        '        If (bitmapImageResized IsNot Nothing) Then bitmapImageResized.Dispose()
        '    End Try

        '    Return retValue
        'End Function

        Friend Shared Function GetIconFromImageFile(ByVal filePath As String, ByVal decodedWidth As Integer) As System.Drawing.Icon
            Dim bitmapImage As Bitmap = Nothing
            Dim bitmapImageResized As Bitmap = Nothing
            Dim retValue As System.Drawing.Icon = Nothing

            Try
                bitmapImage = New Bitmap(filePath)

                bitmapImageResized = New Bitmap(bitmapImage, decodedWidth, decodedWidth)

                Dim iconHandle As System.IntPtr = bitmapImageResized.GetHicon
                retValue = System.Drawing.Icon.FromHandle(iconHandle)
            Catch ex As Exception
                'Ignore.
            Finally
                If (bitmapImage IsNot Nothing) Then bitmapImage.Dispose()
                If (bitmapImageResized IsNot Nothing) Then bitmapImageResized.Dispose()
            End Try

            Return retValue
        End Function

        Friend Shared Function IconToBitmapImage(ByVal icon As System.Drawing.Icon) As BitmapImage
            Dim retValue As BitmapImage = Nothing
            Dim bitmap As System.Drawing.Bitmap = icon.ToBitmap

            retValue = Imaging.BitmapToBitmapImage(bitmap)
            bitmap.Dispose()

            Return retValue
        End Function

        Friend Shared Function GetAssociatedIcon(ByVal filePath As String) As System.Drawing.Icon
            Dim retValue As System.Drawing.Icon = Nothing

            Try
                retValue = System.Drawing.Icon.ExtractAssociatedIcon(filePath)
            Catch ex As Exception
                'Ignore.
            End Try

            Return retValue
        End Function

        Friend Shared Function GetIconFromIconFile(ByVal fileName As String, ByVal index As Integer) As System.Drawing.Icon
            Dim retValue As System.Drawing.Icon = Nothing
            Dim hIcon As System.IntPtr

            Try
                'Make sure iconIndex is >= 0
                'Use try/catch. In some rare cases where index = -1 throws an exception.
                'A -1 returns the number of icons from the file.
                'For example, when dragging the iSCSI initiation shortcut to a button.
                If (index >= 0) Then
                    hIcon = NativeMethods.ExtractIcon(IntPtr.Zero, fileName, index)

                    retValue = System.Drawing.Icon.FromHandle(hIcon)
                End If
            Catch ex As Exception
                'Ignore.
            End Try

            Return retValue
        End Function

        Friend Shared Function GetAssociatedIconFromUNC(uncPath As String) As System.Drawing.Icon
            Dim uicon As UShort
            Dim retValue As System.Drawing.Icon = Nothing
            Dim strB As StringBuilder

            Try
                strB = New StringBuilder(260)
                strB.Append(uncPath)
                Dim handle As IntPtr = NativeMethods.ExtractAssociatedIcon(IntPtr.Zero, strB, uicon)
                retValue = Icon.FromHandle(handle)
            Catch ex As Exception
                'Ignore.

            End Try

            Return retValue
        End Function
    End Class

End Namespace
