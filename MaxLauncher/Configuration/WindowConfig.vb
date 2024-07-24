'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.ComponentModel
Imports System.Xml.Serialization
Imports MaxLauncher

<Serializable()>
Public Class WindowConfig
    Implements INotifyPropertyChanged

#Region "Window Properties"

    Private _WinTop As System.Double = 100.0
    <Description("Gets/Sets MainWindow Top"),
    DefaultValueAttribute(100.0)>
    Public Property WinTop() As System.Double
        Get
            Return _WinTop
        End Get
        Set(ByVal value As System.Double)
            If Not (value = _WinTop) Then
                _WinTop = value
                OnPropertyChanged("WinTop")
            End If
        End Set
    End Property

    Private _WinLeft As System.Double = 1.0
    <Description("Gets/Sets MainWindow Left"),
    DefaultValueAttribute(1.0)>
    Public Property WinLeft() As System.Double
        Get
            Return _WinLeft
        End Get
        Set(ByVal value As System.Double)
            If Not (value = _WinLeft) Then
                _WinLeft = value
                OnPropertyChanged("WinLeft")
            End If
        End Set
    End Property

    Private _WinWidth As System.Double = 1000.0
    <Description("Gets/Sets MainWindow Width"),
    DefaultValueAttribute(1000.0)>
    Public Property WinWidth() As System.Double
        Get
            Return _WinWidth
        End Get
        Set(ByVal value As System.Double)
            If Not (value = _WinWidth) Then
                _WinWidth = value
                OnPropertyChanged("WinWidth")
            End If
        End Set
    End Property

    Private _WinHeight As System.Double = 500.0
    <Description("Gets/Sets MainWindow Height"),
    DefaultValueAttribute(500.0)>
    Public Property WinHeight() As System.Double
        Get
            Return _WinHeight
        End Get
        Set(ByVal value As System.Double)
            If Not (value = _WinHeight) Then
                _WinHeight = value
                OnPropertyChanged("WinHeight")
            End If
        End Set
    End Property

#End Region

    ''' <summary>
    ''' ProperyChanged Event
    ''' </summary>
    ''' <remarks></remarks>
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    ''' <summary>
    ''' OnPropertyChanged
    ''' </summary>
    ''' <param name="name">Name of the property that changed.</param>
    ''' <remarks>Raises the property changed event with the specified property name.</remarks>
    Friend Sub OnPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub

End Class


