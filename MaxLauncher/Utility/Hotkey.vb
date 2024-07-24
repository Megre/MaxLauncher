'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Windows.Interop

Namespace Utility

    Public Class Hotkey
        Friend Const HK_MSG_ID As Integer = &H312

        Property Window As Window
        Property Id As Integer
        Property Value As UShort

        ''' <summary>
        ''' Hi Byte of HKValue = Modifiers
        ''' </summary>
        ''' <remarks></remarks>
        Private _modifiers As Byte
        Property Modifiers() As Byte
            Get
                Dim bytes As Byte() = BitConverter.GetBytes(Me.Value)
                Return bytes(1)
            End Get
            Set(value As Byte)
                _modifiers = value
                Dim bytes As Byte() = BitConverter.GetBytes(Me.Value)
                bytes(1) = value
                Me.Value = BitConverter.ToUInt16(bytes, 0)
            End Set
        End Property

        ''' <summary>
        ''' Low Byte of HKValue = virtual key
        ''' </summary>
        ''' <remarks></remarks>
        Private _vkey As String
        Property VKey() As Byte
            Get
                Dim bytes As Byte() = BitConverter.GetBytes(Me.Value)
                Return bytes(0)
            End Get
            Set(value As Byte)
                _vkey = value
                Dim bytes As Byte() = BitConverter.GetBytes(Me.Value)
                bytes(0) = value
                Me.Value = BitConverter.ToUInt16(bytes, 0)
            End Set
        End Property

        Sub New()
        End Sub

        Sub New(ByVal wnd As Window, ByVal id As Integer, ByVal hotkeyValue As UShort)
            Me.Window = wnd
            Me.Id = id
            Me.Value = hotkeyValue
        End Sub

        Function IsNone() As Boolean
            Return If(Modifiers = ModifierKeys.None, True, False)
        End Function

        Function IsAlt() As Boolean
            Return ((Modifiers And ModifierKeys.Alt) = ModifierKeys.Alt)
        End Function

        Function IsControl() As Boolean
            Return ((Modifiers And ModifierKeys.Control) = ModifierKeys.Control)
        End Function

        Function IsShift() As Boolean
            Return ((Modifiers And ModifierKeys.Shift) = ModifierKeys.Shift)
        End Function

        Function IsWindows() As Boolean
            Return ((Modifiers And ModifierKeys.Windows) = ModifierKeys.Windows)
        End Function

        ''' <summary>
        ''' Combines the modifiers.
        ''' </summary>
        ''' <param name="alt"></param>
        ''' <param name="ctrl"></param>
        ''' <param name="shift"></param>
        ''' <param name="win"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function CombineModifiers(ByVal alt As Boolean,
                                                ByVal ctrl As Boolean,
                                                ByVal shift As Boolean,
                                                ByVal win As Boolean) As Byte
            Dim mods As Byte = 0

            If (alt) Then mods = mods Or ModifierKeys.Alt
            If (ctrl) Then mods = mods Or ModifierKeys.Control
            If (shift) Then mods = mods Or ModifierKeys.Shift
            If (win) Then mods = mods Or ModifierKeys.Windows

            Return mods
        End Function

        ''' <summary>
        ''' Returns a string representation of the modifier + key.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            Dim keyText = Keyboard.GetTextFromVK(VKey)

            Return String.Format("{0}{1}", GetModifierString(), keyText)
        End Function

        ''' <summary>
        ''' Returns a string representation of the selected modifiers.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetModifierString() As String
            Dim s As String = ""

            If (IsAlt()) Then s &= "ALT + "
            If (IsControl()) Then s &= "CTRL + "

            If (IsShift()) Then s &= "SHIFT + "

            If (IsWindows()) Then s &= "WIN + "

            Return s
        End Function

        Function Register() As Boolean
            If (Me.Window Is Nothing) Then Throw New Exception(Localization.GetString("String_WindowCannotBeNothing"))

            Dim wnd As New WindowInteropHelper(Me.Window)

            If (KeyInterop.KeyFromVirtualKey(VKey) = 0) Then Return False

            Return NativeMethods.RegisterHotKey(wnd.Handle, Me.Id, CInt(Modifiers), CInt(VKey))
        End Function

        Function UnRegister() As Boolean
            Dim wnd As New WindowInteropHelper(Me.Window)

            Return NativeMethods.UnregisterHotKey(wnd.Handle, Me.Id)
        End Function

    End Class

End Namespace
