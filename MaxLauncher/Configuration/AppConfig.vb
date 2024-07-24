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
Public Class AppConfig
    Implements INotifyPropertyChanged

#Region "FavoritesBar Properties"

    <Description("Gets/Sets FavoritesBar Height. This is a GridLength wrapper"),
    DefaultValueAttribute(100.0)>
    Public Property FavoritesBarHeight As System.Double = 100.0

    <NonSerialized()>
    Private _FavoritesBarGridLength As New GridLength(100.0)
    <Description("Gets/Sets FavoritesBar Grid Length"),
    XmlIgnore(),
    DefaultValueAttribute(100.0)>
    Public Property FavoritesBarGridLength() As GridLength
        Get
            Return New GridLength(FavoritesBarHeight)
        End Get
        Set(ByVal value As GridLength)
            If Not (value = _FavoritesBarGridLength) Then
                _FavoritesBarGridLength = value
                FavoritesBarHeight = value.Value
            End If
        End Set
    End Property


#End Region

#Region "Settings > Options"

    <Description("Gets/Sets single instance mode."),
    DefaultValueAttribute(True)>
    Public Property SingleInstance As Boolean = True

    <Description("Gets/Sets whether to open the last file used"),
    DefaultValueAttribute(True)>
    Public Property OpenLastFileUsed As Boolean = True

    <Description("Gets/Sets the last file used"),
    DefaultValueAttribute("")>
    Public Property LastFileUsed As String = ""

    <Description("Gets/Sets minimizes the window when closed by clicking the [X] or [Alt]+[F4]"),
    DefaultValueAttribute(True)>
    Public Property MinimizeOnClose As Boolean = True

    <Description("Gets/Sets Startup Window State"),
    DefaultValueAttribute(False)>
    Public Property StartMinimized As Boolean = False

    Private _ShowInTaskbar As Boolean = True
    <Description("Gets/Sets ShowInTaskbar"),
    DefaultValueAttribute(True)>
    Public Property ShowInTaskbar As Boolean
        Get
            Return _ShowInTaskbar
        End Get
        Set(ByVal value As Boolean)
            If Not (value = _ShowInTaskbar) Then
                _ShowInTaskbar = value
                OnPropertyChanged("ShowInTaskbar")
            End If
        End Set
    End Property

    <Description("Gets/Sets AutoSelectTab"),
    DefaultValueAttribute(False)>
    Public Property AutoSelectTab As Boolean = False

    <Description("Gets/Sets Hide Menu"),
    DefaultValueAttribute(False)>
    Public Property HideMenu As Boolean = False

    <Description("Gets/Sets if checking for an update on startup."),
    DefaultValueAttribute(True)>
    Public Property CheckUpdate As Boolean = True

    <Description("Gets/Sets if mouse double-click is required to run."),
    DefaultValueAttribute(False)>
    Public Property MouseDoubleClick As Boolean = False

    <Description("Gets/Sets Press key twice to launch."),
    DefaultValueAttribute(False)>
    Public Property PressKeyTwiceToLaunch As Boolean = False

    <Description("Gets/Sets Always use application theme."),
    DefaultValueAttribute(True)>
    Public Property AlwaysUseApplicationTheme As Boolean = True

    <Description("Gets/Sets Arrow keys selects tabs."),
    DefaultValueAttribute(True)>
    Public Property ArrowKeysSelectsTabs As Boolean = True

    Private _HideButtonIcons As Boolean = False
    <Description("Gets/Sets Hiding of button icons."),
    DefaultValueAttribute(False)>
    Public Property HideButtonIcons As Boolean
        Get
            Return _HideButtonIcons
        End Get
        Set(value As Boolean)
            If Not (value = _HideButtonIcons) Then
                _HideButtonIcons = value
                OnPropertyChanged("HideButtonIcons")
            End If
        End Set
    End Property

    <Description("Gets/Sets using folder paths as target."),
    DefaultValueAttribute(False)>
    Public Property FolderPathAsTarget As Boolean = False

    <Description("Gets/Sets whether to clear search box when restoring the window."),
    DefaultValueAttribute(False)>
    Public Property ClearSearchBox As Boolean = False

    Private _HideButtonText As Boolean = False
    <Description("Gets/Sets whether to show button text."),
    DefaultValueAttribute(False)>
    Public Property HideButtonText As Boolean
        Get
            Return _HideButtonText
        End Get
        Set(value As Boolean)
            If Not (value = _HideButtonText) Then
                _HideButtonText = value
                OnPropertyChanged("HideButtonText")
            End If
        End Set
    End Property

#End Region

#Region "View options"

    Private _AutoHide As Boolean = True
    <Description("Gets/Sets Auto Hide"),
    DefaultValueAttribute(True)>
    Public Property AutoHide() As Boolean
        Get
            Return _AutoHide
        End Get
        Set(ByVal value As Boolean)
            If Not (value = _AutoHide) Then
                _AutoHide = value
                OnPropertyChanged("AutoHide")
            End If
        End Set
    End Property

    Private _AlwaysOnTop As Boolean = False
    <Description("Gets/Sets whether the window is always on top of other windows"),
    DefaultValueAttribute(False)>
    Public Property AlwaysOnTop() As Boolean
        Get
            Return _AlwaysOnTop
        End Get
        Set(ByVal value As Boolean)
            If Not (value = _AlwaysOnTop) Then
                _AlwaysOnTop = value
                OnPropertyChanged("AlwaysOnTop")
            End If
        End Set
    End Property

    Private _CenterOnScreen As Boolean = True
    <Description("Gets/Sets CenterOnScreen"),
    DefaultValueAttribute(True)>
    Public Property CenterOnScreen As Boolean
        Get
            Return _CenterOnScreen
        End Get
        Set(ByVal value As Boolean)
            If Not (value = _CenterOnScreen) Then
                _CenterOnScreen = value
                OnPropertyChanged("CenterOnScreen")
            End If
        End Set
    End Property

    Private _LockGUI As Boolean = False
    <Description("Gets/Sets LockGUI"),
    DefaultValueAttribute(False)>
    Public Property LockGUI As Boolean
        Get
            Return _LockGUI
        End Get
        Set(ByVal value As Boolean)
            If Not (value = _LockGUI) Then
                _LockGUI = value
                OnPropertyChanged("LockGUI")
            End If
        End Set
    End Property

    Private _HideEmptyTabButtons As Boolean = False
    <Description("Shows/Hides empty tab buttons."),
    DefaultValueAttribute(False)>
    Public Property HideEmptyTabButtons As Boolean
        Get
            Return _HideEmptyTabButtons
        End Get
        Set(ByVal value As Boolean)
            If Not (value = _HideEmptyTabButtons) Then
                _HideEmptyTabButtons = value
                OnPropertyChanged("HideEmptyTabButtons")
            End If
        End Set
    End Property

    Private _HideTabButtonsRow1 As Boolean = False
    <Description("Shows/Hides tab buttons row 1."),
    DefaultValueAttribute(False)>
    Public Property HideTabButtonsRow1 As Boolean
        Get
            Return _HideTabButtonsRow1
        End Get
        Set(ByVal value As Boolean)
            If Not (value = _HideTabButtonsRow1) Then
                _HideTabButtonsRow1 = value
                OnPropertyChanged("HideTabButtonsRow1")
            End If
        End Set
    End Property

    Private _HideTabButtonsRow2 As Boolean = False
    <Description("Shows/Hides tab buttons row 2."),
    DefaultValueAttribute(False)>
    Public Property HideTabButtonsRow2 As Boolean
        Get
            Return _HideTabButtonsRow2
        End Get
        Set(ByVal value As Boolean)
            If Not (value = _HideTabButtonsRow2) Then
                _HideTabButtonsRow2 = value
                OnPropertyChanged("HideTabButtonsRow2")
            End If
        End Set
    End Property

    Private _HideTabButtonsRow3 As Boolean = False
    <Description("Shows/Hides tab buttons row 3."),
    DefaultValueAttribute(False)>
    Public Property HideTabButtonsRow3 As Boolean
        Get
            Return _HideTabButtonsRow3
        End Get
        Set(ByVal value As Boolean)
            If Not (value = _HideTabButtonsRow3) Then
                _HideTabButtonsRow3 = value
                OnPropertyChanged("HideTabButtonsRow3")
            End If
        End Set
    End Property

#End Region

#Region "Application Theme"
    <Description("Gets/Sets theme."),
    DefaultValueAttribute("Default")>
    Public Property CurrentTheme As String = ThemeDialog.DefaultThemeName
#End Region

#Region "Application Hotkey"

    <Description("Gets/Sets the hotkey"),
    DefaultValueAttribute(704)>
    Public Property Hotkey1 As UShort = 704

    <Description("Gets/Sets the tab to activate when app is activated"),
    DefaultValueAttribute(-1)>
    Public Property ActivateTab As Integer = -1

#End Region

#Region "PropertyGrid"
    <Description("Gets/Sets the Name column width of the PropertyGrid"),
    DefaultValueAttribute(200)>
    Public Property PGridNameColumnWidth As Double = 200

    Private _PGridWinWidth As System.Double = 500.0
    <Description("Gets/Sets the property grid width"),
    DefaultValueAttribute(500.0)>
    Public Property PGridWinWidth() As System.Double
        Get
            Return _PGridWinWidth
        End Get
        Set(ByVal value As System.Double)
            If Not (value = _PGridWinWidth) Then
                _PGridWinWidth = value
                OnPropertyChanged("PGridWinWidth")
            End If
        End Set
    End Property

    Private _PGridWinHeight As System.Double = 500.0
    <Description("Gets/Sets the property grid height"),
    DefaultValueAttribute(500.0)>
    Public Property PGridWinHeight() As System.Double
        Get
            Return _PGridWinHeight
        End Get
        Set(ByVal value As System.Double)
            If Not (value = _PGridWinHeight) Then
                _PGridWinHeight = value
                OnPropertyChanged("PGridWinHeight")
            End If
        End Set
    End Property
#End Region

#Region "DataFiles"

    ''' <summary>
    ''' Set/Return a List of data files
    ''' </summary>
    ''' <remarks></remarks>
    <XmlElement("DataFiles")>
    Private _DataFiles As List(Of DataFile)
    Public Property DataFiles() As List(Of DataFile)
        Get
            If _DataFiles Is Nothing Then _DataFiles = New List(Of DataFile)
            Return _DataFiles
        End Get
        Set(ByVal value As List(Of DataFile))
            _DataFiles = value
        End Set
    End Property

    ''' <summary>
    ''' DataFile Class
    ''' </summary>
    ''' <remarks>DataFile object</remarks>
    <Serializable()>
    Public Class DataFile
        Implements IComparable(Of DataFile)

        Public Property menuname As String
        Public Property filename As String

        Public Sub New()
        End Sub

        Public Sub New(ByVal menuname As String, ByVal filename As String)
            Me.menuname = menuname
            Me.filename = filename
        End Sub

        Public Function CompareTo(df As DataFile) As Integer Implements IComparable(Of DataFile).CompareTo
            If df Is Nothing Then Return 1

            If df IsNot Nothing Then
                'Remove leading underscore of this object's menuname and use the result in comparison.
                Dim thisMenuname = Me.menuname.TrimStart("_")
                Dim dfMenuname = df.menuname.TrimStart("_")

                Return thisMenuname.CompareTo(dfMenuname)
            Else
                Throw New ArgumentException("Object is not a DataFile")
            End If
        End Function
    End Class
#End Region

#Region "Localization"
    <Description("Gets/Sets the language."),
    DefaultValueAttribute("English")>
    Public Property Language As String = Localization.DefaultLanguage
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


