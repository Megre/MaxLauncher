'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Drawing
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Text
Imports MaxLauncher.Utility

Class IconCacheDB
    Private Shared _instance As IconCacheDB
    Friend Property Filename As String = ""
    Private IconCacheDictionary As New Dictionary(Of String, System.Drawing.Bitmap)
    Friend Const DEFAULT_ICON_SIZE As Integer = 32

    Friend Shared Function GetInstance() As IconCacheDB
        If _instance Is Nothing Then
            _instance = New IconCacheDB

            'Add one entry to prevent "Error opening image cache" exception.
            'This fix has not been confirmed since the error is difficult to reproduce.
            _instance.IconCacheDictionary.Add("IconCacheDB_2", Nothing)

            _instance.IsDirty = True
        End If

        Return _instance
    End Function

    Friend Function Exists() As Boolean
        If (String.IsNullOrEmpty(Me.Filename)) Then Return False

        Return True
    End Function

    Property IsDirty As Boolean = False

    Sub Save()
        Save(Me.Filename)
    End Sub

    Friend Sub Save(filename As String)
        If IsDirty Then
            Try
                Dim oSerializer As BinaryFormatter = New BinaryFormatter

                Using oStream As Stream = New FileStream(filename, FileMode.Create)
                    oSerializer.Serialize(oStream, IconCacheDictionary)
                    Me.Filename = filename
                End Using
            Catch ex As Exception
                Throw New Exception(String.Format(Localization.GetString("String_ErrorSavingIconCache"), filename), ex)
            End Try
        End If
    End Sub

    Friend Sub Open(filename As String)
        'Dim iStream As Stream = Nothing
        Try
            Dim oDeSerializer As BinaryFormatter = New BinaryFormatter
            Me.Filename = filename

            Using iStream As Stream = New BufferedStream(New FileStream(filename, FileMode.Open))
                IconCacheDictionary = CType(oDeSerializer.Deserialize(iStream), Dictionary(Of String, System.Drawing.Bitmap))

                IsDirty = False

                'Delete old iconcachedb format.
                If IconCacheDictionary.ContainsKey("RobertoConcepcion") Then
                    iStream.Close()
                    File.Delete(filename)

                    _instance = Nothing
                    Dim newInstance = GetInstance()
                    newInstance.Filename = filename
                End If
            End Using
        Catch ex As Exception
            Try
                File.Delete(filename)
            Finally
            End Try
            Throw New Exception(String.Format(Localization.GetString("String_ErrorOpeningIconCache"), filename), ex)
        End Try
    End Sub

    Friend Function GetImage(path As String, Optional index As Integer = -2, Optional forceUpdate As Boolean = False) As System.Windows.Media.ImageSource
        Dim image As System.Windows.Media.ImageSource = Nothing
        Dim fullPath = FileUtils.Path.ExpandEnvironmentVariables(path)

        'Check if path is valid
        Try
            Dim testFullPath = System.IO.Path.GetFullPath(fullPath)
        Catch ex As Exception
            Return Nothing
        End Try

        Dim imageKey As String = path & "," & index
        Dim icon As System.Drawing.Icon = Nothing

        If (forceUpdate) Then
            'Remove image from DB to force an update.
            If (IconCacheDictionary.ContainsKey(imageKey)) Then IconCacheDictionary.Remove(imageKey)

            IsDirty = True
        End If

        If (IconCacheDictionary.ContainsKey(imageKey)) Then
            image = Imaging.BitmapToBitmapImage(IconCacheDictionary.Item(imageKey))
        Else
            If (File.Exists(fullPath)) Then
                Dim fileExtension As String = IO.Path.GetExtension(fullPath).ToLower

                'Image file
                If fileExtension.Equals(".bmp") OrElse
                   fileExtension.Equals(".dib") OrElse
                   fileExtension.Equals(".gif") OrElse
                   fileExtension.Equals(".jpg") OrElse
                   fileExtension.Equals(".jpe") OrElse
                   fileExtension.Equals(".jpeg") OrElse
                   fileExtension.Equals(".jfif") OrElse
                   fileExtension.Equals(".png") OrElse
                   fileExtension.Equals(".tif") OrElse
                   fileExtension.Equals(".tiff") Then

                    icon = Imaging.GetIconFromImageFile(fullPath, IconCacheDB.DEFAULT_ICON_SIZE)
                ElseIf (index = -2) Then
                    'DefaultIcon

                    'Check if path is UNC.
                    If fullPath.StartsWith("\\") Then
                        icon = Imaging.GetAssociatedIconFromUNC(fullPath)
                    Else
                        icon = Imaging.GetAssociatedIcon(fullPath)
                    End If
                Else
                    icon = Imaging.GetIconFromIconFile(fullPath, index)
                End If
            ElseIf (Directory.Exists(fullPath)) Then
                icon = Imaging.GetAssociatedIcon(System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%SYSTEMROOT%"), "explorer.exe"))
            End If

            'Add image to DB.
            If (icon IsNot Nothing) Then
                IconCacheDictionary.Item(imageKey) = icon.ToBitmap
                image = Imaging.IconToBitmapImage(icon)
                icon.Dispose()

                IsDirty = True
            End If
        End If

        Return image
    End Function


End Class
