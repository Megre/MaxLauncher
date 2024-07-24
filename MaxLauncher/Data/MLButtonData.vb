'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

<Serializable()> _
Public Class MLButtonData
    Public Shared ReadOnly DataFormat As String = "MLButtonData"

    Public Property Scancode As Short
    Public Property Text As String = ""
    Public Property Filename As String = ""
    Public Property Arguments As String = ""
    Public Property WorkingDirectory As String = ""
    Public Property Description As String = ""
    Public Property IconFile As String = ""
    Public Property IconIndex As Short = 0
    Public Property WindowStyle As Integer = ProcessWindowStyle.Normal
    Public Property WindowStyleEx As Short = 0
    Public Property RunAsAdmin As Boolean = False
    Public Property WindowTitle As String = ""
    Public Property WindowProcessName As String = ""
    Public Property WindowRect As System.Drawing.Rectangle = Nothing
    Public Property GroupID As String = ""

    Enum SortType
        Name
        Target
    End Enum

    Public Sub New()
    End Sub

    Public Sub New(ByVal sourceMLButtonData As MLButtonData)
        Me.Scancode = sourceMLButtonData.Scancode
        Me.Text = sourceMLButtonData.Text
        Me.Filename = sourceMLButtonData.Filename
        Me.Arguments = sourceMLButtonData.Arguments
        Me.WorkingDirectory = sourceMLButtonData.WorkingDirectory
        Me.Description = sourceMLButtonData.Description
        Me.IconFile = sourceMLButtonData.IconFile
        Me.IconIndex = sourceMLButtonData.IconIndex
        Me.WindowStyle = sourceMLButtonData.WindowStyle
        Me.WindowStyleEx = sourceMLButtonData.WindowStyleEx
        Me.RunAsAdmin = sourceMLButtonData.RunAsAdmin
        Me.WindowTitle = sourceMLButtonData.WindowTitle
        Me.WindowProcessName = sourceMLButtonData.WindowProcessName
        Me.WindowRect = sourceMLButtonData.WindowRect
        Me.GroupID = sourceMLButtonData.GroupID
    End Sub

    Public Sub New(ByVal scancode As Short,
                   ByVal text As String,
                   ByVal filename As String,
                   ByVal arguments As String,
                   ByVal workingDirectory As String,
                   ByVal description As String,
                   ByVal iconFile As String,
                   ByVal iconIndex As Short,
                   ByVal windowStyle As Integer,
                   ByVal windowStyleEx As Short,
                   ByVal runAsAdmin As Boolean,
                   ByVal windowTitle As String,
                   ByVal windowProcessName As String,
                   ByVal windowRect As String,
                   ByVal groupID As String)

        Me.Scancode = scancode
        Me.Text = text
        Me.Filename = filename
        Me.Arguments = arguments
        Me.WorkingDirectory = workingDirectory
        Me.Description = description
        Me.IconFile = iconFile
        Me.IconIndex = iconIndex
        Me.WindowStyle = windowStyle
        Me.WindowStyleEx = windowStyleEx
        Me.RunAsAdmin = runAsAdmin
        Me.WindowTitle = windowTitle
        Me.WindowProcessName = windowProcessName
        Me.WindowRect = GetRectangle(windowRect)
        Me.GroupID = groupID
    End Sub

    Friend Function GetRectangle(ByVal windowRect As String) As System.Drawing.Rectangle
        Dim rect As New System.Drawing.Rectangle

        'If windowRect is nothing, a System.NullReferenceException is thrown when debugging.
        If (windowRect Is Nothing) Then Return Nothing

        Try
            Dim rectArray() = windowRect.Split(New Char() {","c})

            'rect = System.Drawing.Rectangle.FromLTRB(rectArray(0), rectArray(1), rectArray(2), rectArray(3))
            rect.X = Integer.Parse(rectArray(0))
            rect.Y = Integer.Parse(rectArray(1))
            rect.Width = Integer.Parse(rectArray(2))
            rect.Height = Integer.Parse(rectArray(3))
        Catch ex As Exception
            rect = Nothing
        End Try

        Return rect
    End Function

    Friend Function GetRectangleString() As String
        Return Me.WindowRect.Left & "," & Me.WindowRect.Top & "," & Me.WindowRect.Width & "," & Me.WindowRect.Height
    End Function

    Private Class SortByNameHelper
        Implements IComparer(Of MLButtonData)

        Public Function Compare(ByVal x As MLButtonData, ByVal y As MLButtonData) As Integer Implements IComparer(Of MLButtonData).Compare
            If IsNothing(x) Then
                If IsNothing(y) Then
                    Return 0
                Else
                    Return 1
                End If
            Else
                If IsNothing(y) Then
                    Return -1
                Else
                    Return New CaseInsensitiveComparer().Compare(x.Text, y.Text)
                End If
            End If
        End Function
    End Class

    Public Shared Function SortByName() As IComparer(Of MLButtonData)
        Return CType(New SortByNameHelper(), IComparer(Of MLButtonData))
    End Function

    Private Class SortByTargetHelper
        Implements IComparer(Of MLButtonData)

        Public Function Compare(ByVal x As MLButtonData, ByVal y As MLButtonData) As Integer Implements IComparer(Of MLButtonData).Compare
            If IsNothing(x) Then
                If IsNothing(y) Then
                    Return 0
                Else
                    Return 1
                End If
            Else
                If IsNothing(y) Then
                    Return -1
                Else
                    Return New CaseInsensitiveComparer().Compare(x.Filename, y.Filename)
                End If
            End If
        End Function
    End Class

    Public Shared Function SortByTarget() As IComparer(Of MLButtonData)
        Return CType(New SortByTargetHelper(), IComparer(Of MLButtonData))
    End Function

    Public Function IsDirectory() As Boolean
        Try
            Dim fullPath = System.IO.Path.GetFullPath(Me.Filename)

            If (String.Compare(IO.Path.GetFileName(fullPath), "explorer.exe", True) = 0) OrElse
                (System.IO.Directory.Exists(fullPath)) Then Return True
        Catch ex As Exception
            'Ignore
        End Try

        Return False
    End Function
End Class
