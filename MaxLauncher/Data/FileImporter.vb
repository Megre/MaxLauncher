'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Windows.Forms
Imports System.Xml.Linq
Imports Microsoft.VisualBasic.Logging

Class FileImporter
    Private _buttonIndex As ButtonIndex
    Private _fileImportLog As New FileImportLog

    Friend Sub ImportTabControlData(sourceFile As String, destinationFile As String)
        Try
            Dim srcXmlDoc = XDocument.Load(sourceFile)
            Dim destXmlDoc = XDocument.Load(destinationFile)

            Dim root = srcXmlDoc.Root
            Dim destRoot = destXmlDoc.Root

            _buttonIndex = New ButtonIndex(destRoot, _fileImportLog)
            Dim tabElements = root.Elements("Tab")
            For Each tabElement In tabElements
                If tabElement.Elements("Button").Count = 0 Then GoTo nextEle

                Dim destElement = FindDestTabElement(destRoot, tabElement)
                If destElement IsNot Nothing Then
                    CopyButtons(tabElement, destElement)
                End If
nextEle:
            Next

            CheckNotImportedButtons(root)
            destXmlDoc.Save(destinationFile)

            _fileImportLog.WriteLine("Import completed.")
            _fileImportLog.Close()

        Catch ex As Exception
            _fileImportLog.WriteLine(ex.ToString())
            Throw New Exception(String.Format(Localization.GetString("String_ErrorConvertingFile"), sourceFile, destinationFile))
        End Try
    End Sub

    Public Function GetLog() As FileImportLog
        Return _fileImportLog
    End Function

    Private Sub CheckNotImportedButtons(srcRoot As XElement)
        Dim tabElements = srcRoot.Elements("Tab")
        For Each tabElement In tabElements
            If tabElement.Elements("Button").Count = 0 Then GoTo nextEle

            For Each buttonElement In tabElement.Elements("Button")
                _fileImportLog.LogButton("button not imported because target Tab is full", buttonElement)
            Next
nextEle:
        Next
    End Sub

    Private Sub CopyButtons(srcTabEle As XElement, destTabEle As XElement)
        Do While True
            Dim button As XElement = srcTabEle.Element("Button")
            If button Is Nothing Then Return

            Dim existButton = _buttonIndex.GetButton(button)
            If existButton IsNot Nothing Then
                _fileImportLog.LogButton("button already exists", existButton)
                button.Remove()
            Else
                Dim placeId = FindEmptyButtonPlace(destTabEle, button.Element("ID").Value)
                If Not String.IsNullOrEmpty(placeId) Then
                    button.Remove()
                    button.Element("Tab_ID").Value = destTabEle.Element("ID").Value
                    button.Element("ID").Value = placeId
                    destTabEle.Add(button)
                    _buttonIndex.IndexButtonElement(button)
                End If
            End If
        Loop
    End Sub

    Private Function FindEmptyButtonPlace(tabEle As XElement, expectedPlace As String) As String
        Dim place = FindEmptyButtonPlace(tabEle, CInt(expectedPlace), CInt(expectedPlace))

        If String.IsNullOrEmpty(place) Then place = FindEmptyButtonPlace(tabEle, 16, 26)
        If String.IsNullOrEmpty(place) Then place = FindEmptyButtonPlace(tabEle, 30, 40)
        If String.IsNullOrEmpty(place) Then place = FindEmptyButtonPlace(tabEle, 44, 54)

        Return place
    End Function

    Private Function FindEmptyButtonPlace(tabEle As XElement, fromIndex As Integer, toIndex As Integer) As String
        Dim index As Integer
        For index = fromIndex To toIndex
            Dim buttons = tabEle.Elements("Button")
            Dim found As Boolean = False
            For Each button In buttons
                Dim ele = ElementByValue(button, "ID", CStr(index))
                If ele IsNot Nothing Then
                    found = True
                    GoTo nextIndex
                End If
            Next
nextIndex:
            If Not found Then
                Return CStr(index)
            End If
        Next

        Return ""
    End Function

    Private Function ElementByValue(ele As XElement, XName As String, value As String) As XElement
        Dim children = ele.Elements(XName)
        For Each child In children
            If String.Equals(child.Value, value) Then
                Return child
            End If
        Next

        Return Nothing
    End Function
    Private Function FindDestTabElement(destRoot As XElement, srcElement As XElement) As XElement
        Dim destElement As XElement = Nothing

        Dim id = srcElement.Element("ID").Value
        Dim text = srcElement.Element("Text").Value

        If Not String.IsNullOrEmpty(text) Then
            destElement = FindTabElementByName(destRoot, text)
            If destElement Is Nothing Then
                destElement = AddNewTab(destRoot, text)
            End If
        Else
            destElement = FindTabElementById(destRoot, id)
            If destElement Is Nothing Then
                destElement = AddNewTab(destRoot, "SrcTab" & id)
            End If
        End If

        Return destElement
    End Function

    Private Function FindTabElementById(root As XElement, tabId As String) As XElement
        Dim tabElements = root.Elements("Tab")
        For Each tabElement In tabElements
            Dim id = tabElement.Element("ID").Value

            If String.Equals(tabId, id) Then
                Return tabElement
            End If
        Next

        Return Nothing
    End Function

    Private Function FindTabElementByName(root As XElement, tabName As String) As XElement
        Dim tabElements = root.Elements("Tab")
        For Each tabElement In tabElements
            Dim text = tabElement.Element("Text").Value

            If String.Equals(tabName, text) Then
                Return tabElement
            End If
        Next

        Return Nothing
    End Function

    Private Function AddNewTab(root As XElement, name As String) As XElement
        Dim tabElements = root.Elements("Tab")
        If tabElements.Count >= 10 Then
            Return Nothing
        End If

        Dim newTab As XElement =
            <Tab>
                <ID></ID>
                <Text></Text>
            </Tab>

        newTab.Element("ID").Value = CStr((tabElements.Count + 1) Mod 10)
        If Not String.IsNullOrEmpty(name) Then
            newTab.Element("Text").Value = name
        End If
        root.Add(newTab)

        Return newTab
    End Function


End Class
