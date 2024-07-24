'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.ComponentModel

Class TabItemControl
    Inherits TabItem

#Region "Dependency Properties"

    ''' <summary>
    ''' Tab key label
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Key As String
        Get
            Return GetValue(KeyProperty)
        End Get

        Set(ByVal value As String)
            SetValue(KeyProperty, value)
        End Set
    End Property

    Public Shared ReadOnly KeyProperty As DependencyProperty = _
                           DependencyProperty.Register("Key", _
                           GetType(String), GetType(TabItemControl), _
                           New PropertyMetadata(Nothing))

#End Region

#Region "Events"

    Private Sub TabItem_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        Me.IsSelected = True
    End Sub

    Private Sub TabItem_DragOver(sender As Object, e As DragEventArgs)
        Application.Current.MainWindow.Activate()

        Me.IsSelected = True
        e.Effects = DragDropEffects.None
        e.Handled = True
    End Sub

    Private Sub TabItem_MouseMove(sender As Object, e As MouseEventArgs)
        If (ConfigManager.AppConfig.AutoSelectTab) Then
            If Not Me.IsSelected Then
                Me.IsSelected = True
            End If
        End If
    End Sub

#End Region

End Class
