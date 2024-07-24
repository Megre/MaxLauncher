'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

<Serializable()>
Class MLTabData

    Public Shared ReadOnly DataFormat As String = "MLTabData"

    Friend Property MLButtonDataList As New Dictionary(Of Int16, MLButtonData)
    Friend Property Header As String = ""

    Public Sub New(ByVal tabHeader As String, listOfButtons As List(Of TabButton))
        Me.Header = tabHeader

        For Each button As TabButton In listOfButtons
            If (button.GetButtonData() IsNot Nothing) Then
                MLButtonDataList.Add(button.Scancode, New MLButtonData(button.GetButtonData))
            Else
                MLButtonDataList.Add(button.Scancode, Nothing)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Sorts the internal button data list.
    ''' </summary>
    ''' <exception cref="System.Exception">Thrown when sorting fails.</exception>
    Public Sub Sort(ByVal sortType As MLButtonData.SortType)
        Dim listOfScanCodes As List(Of Int16) = MLButtonDataList.Keys.ToList

        listOfScanCodes.Sort()

        Dim listOfButtonData As List(Of MLButtonData) = MLButtonDataList.Values.ToList()

        Select Case sortType
            Case MLButtonData.SortType.Name
                listOfButtonData.Sort(MLButtonData.SortByName())
            Case MLButtonData.SortType.Target
                listOfButtonData.Sort(MLButtonData.SortByTarget())
        End Select

        Dim dictOfButtons As New Dictionary(Of Int16, MLButtonData)

        Dim j As Integer = 0
        For Each scanCode As Int16 In listOfScanCodes
            dictOfButtons.Add(scanCode, listOfButtonData.Item(j))
            j += 1
        Next

        MLButtonDataList = dictOfButtons
    End Sub

End Class