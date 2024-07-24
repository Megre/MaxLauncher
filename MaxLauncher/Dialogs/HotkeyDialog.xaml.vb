'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports MaxLauncher.Utility

Class HotkeyDialog
    Public Property HKey As Hotkey

    Public Sub New(ByVal value As Hotkey)
        Me.HKey = value
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call.

    End Sub

#Region "Events"

    Private Sub HotkeyDialog_Initialized(sender As Object, e As EventArgs)
        Me.Icon = Imaging.GetAppIconImage

        'Set modifier
        altCheckBox.IsChecked = HKey.IsAlt()
        ctrlCheckBox.IsChecked = HKey.IsControl()
        shiftCheckBox.IsChecked = HKey.IsShift()
        winCheckBox.IsChecked = HKey.IsWindows()

        'Set key text
        keyTextBox.Text = Keyboard.GetTextFromVK(HKey.VKey)

        'Set activate tab
        Dim tabIndex = ConfigManager.AppConfig.ActivateTab
        If (tabIndex >= -1) AndAlso (tabIndex <= 9) Then
            If (tabIndex = -1) Then
                tabComboBox.SelectedIndex = 0
            ElseIf tabIndex = 0 Then
                tabComboBox.SelectedIndex = 10
            Else
                tabComboBox.SelectedIndex = tabIndex
            End If
        Else
            tabComboBox.SelectedIndex = 0
            ConfigManager.AppConfig.ActivateTab = -1
        End If

        'Update hotkey
        HKey.UnRegister()
        UpdateHotkey()
    End Sub

    Private Sub KeyTextBox_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        e.Handled = True

        Dim k As Key
        If (e.Key = Key.System) Then
            k = e.SystemKey
        Else
            k = e.Key
        End If

        'Use the more common description of keys instead of the text in Key class(D0, D1, OEM1, OEM2).
        Dim virtualKey = KeyInterop.VirtualKeyFromKey(k)
        Dim keyText = Keyboard.GetTextFromVK(virtualKey)

        keyTextBox.Text = keyText

        'Move keyboard focus for every keypress. Without moving the focus, default actions by keys like 
        'ESC, TAB, ENTER, ALT + F4, accelerators, etc.will Not work.
        tabComboBox.Focus()

        HKey.UnRegister() 'Unregister hotkey.
        HKey.VKey = KeyInterop.VirtualKeyFromKey(k)
        UpdateHotkey() 'Register and save hotkey.
    End Sub

    Private Sub ModifierCheckBox_Click(sender As Object, e As RoutedEventArgs)
        HKey.UnRegister()

        HKey.Modifiers = HKey.CombineModifiers(altCheckBox.IsChecked,
                                                   ctrlCheckBox.IsChecked,
                                                   shiftCheckBox.IsChecked,
                                                   winCheckBox.IsChecked)
        UpdateHotkey()
    End Sub

    Private Sub TabComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim cbi = DirectCast(tabComboBox.SelectedItem, ComboBoxItem)
        ConfigManager.AppConfig.ActivateTab = cbi.Tag
    End Sub

#End Region

    Private Sub UpdateHotkey()
        If (HKey.Register()) Then
            statusTextBlock.Text = Localization.GetString("String_Available")
            HKey.UnRegister()
        Else
            statusTextBlock.Text = Localization.GetString("String_Unavailable")
        End If
    End Sub

End Class
