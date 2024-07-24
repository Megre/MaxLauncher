'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Drawing
Imports System.Runtime.InteropServices

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
            Return My.Resources.translator
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

End Class