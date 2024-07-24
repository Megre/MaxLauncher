'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Text

Namespace Utility

    Friend Class Keyboard
        Friend Enum MAPVK As Integer
            VK_TO_VSC = 0
            VSC_TOVK = 1
            VK_TO_CHAR = 2
            VSC_TO_VK_EX = 3
        End Enum

        ''' <summary>
        ''' Returns the text associated with the scancode
        ''' </summary>
        ''' <param name="scancode">key's scancode</param>
        ''' <returns>String - string associated with the scancode</returns>
        ''' <remarks></remarks>
        Friend Shared Function GetText(ByVal scancode As Integer) As String

            Dim keyStates(256) As Byte
            Dim sb As New StringBuilder(10)

            Dim vk As Integer = NativeMethods.MapVirtualKey(CInt(scancode), Keyboard.MAPVK.VSC_TOVK)
            Dim rc As Integer = NativeMethods.ToUnicodeEx(CUInt(vk), CUInt(scancode), keyStates, sb, sb.Capacity, 0, NativeMethods.GetKeyboardLayout(0))

            Return sb.ToString().ToUpper.Trim
        End Function

        ''' <summary>
        ''' Returns the text associated with the virtualkey.
        ''' </summary>
        ''' <param name="vkey"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Shared Function GetTextFromVK(ByVal vkey As Integer) As String
            Dim key = KeyInterop.KeyFromVirtualKey(vkey)
            Dim keyText As String
            Dim retValue As String

            'ESCAPE = 27, BACKSPACE  = 8
            If (vkey = 27) OrElse (vkey = 8) Then
                keyText = key.ToString
            ElseIf KeyInterop.KeyFromVirtualKey(vkey) = Key.PageDown Then
                'PAGE DOWN = 22
                keyText = "PageDown"
            Else
                keyText = (GetText(NativeMethods.MapVirtualKey(vkey, Keyboard.MAPVK.VK_TO_VSC)))
            End If

            If (String.IsNullOrEmpty(keyText)) Then
                retValue = key.ToString
            Else
                retValue = keyText
            End If

            Return retValue
        End Function

    End Class

End Namespace
