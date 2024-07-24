'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Xml.linq

Class FileConverter
    Friend Sub TabControlData(sourceFile As String, destinationFile As String)
        Try
            Dim tabArray(9) As Boolean
            For Each t In tabArray
                t = False
            Next

            Dim xmlDoc = XDocument.Load(sourceFile)

            Dim root = xmlDoc.Root

            root.RemoveAttributes()

            For Each el As XElement In xmlDoc.Descendants()
                el.Name = el.Name.LocalName
            Next

            Dim tabElements = root.Elements("MATab")
            For Each tabElement In tabElements
                tabElement.Name = "Tab"
                tabElement.RemoveAttributes()

                Dim tabIDElements = tabElement.Elements("tabID")
                For Each tabIDElement In tabIDElements
                    With tabIDElement
                        .Name = "ID"
                        Select Case .Value.ToLower
                            Case "tbp0"
                                .Value = "1"
                                tabArray(0) = True
                            Case "tbp1"
                                .Value = "2"
                                tabArray(1) = True
                            Case "tbp2"
                                .Value = "3"
                                tabArray(2) = True
                            Case "tbp3"
                                .Value = "4"
                                tabArray(3) = True
                            Case "tbp4"
                                .Value = "5"
                                tabArray(4) = True
                            Case "tbp5"
                                .Value = "6"
                                tabArray(5) = True
                            Case "tbp6"
                                .Value = "7"
                                tabArray(6) = True
                            Case "tbp7"
                                .Value = "8"
                                tabArray(7) = True
                            Case "tbp8"
                                .Value = "9"
                                tabArray(8) = True
                            Case "tbp9"
                                .Value = "0"
                                tabArray(9) = True
                        End Select
                    End With
                Next

                Dim tabNameElements = tabElement.Elements("tabName")
                For Each tabNameElement In tabNameElements
                    tabNameElement.Name = "Text"
                Next

                Dim maButtonElements = tabElement.Elements("MAButton")
                For Each maButtonElement In maButtonElements
                    maButtonElement.Name = "Button"

                    Dim buttonTabIDElements = maButtonElement.Elements("tabID")
                    For Each buttonTabIDElement In buttonTabIDElements
                        With buttonTabIDElement
                            .Name = "Tab_ID"
                            Select Case .Value.ToLower
                                Case "tbp0"
                                    .Value = 1
                                Case "tbp1"
                                    .Value = 2
                                Case "tbp2"
                                    .Value = 3
                                Case "tbp3"
                                    .Value = 4
                                Case "tbp4"
                                    .Value = 5
                                Case "tbp5"
                                    .Value = 6
                                Case "tbp6"
                                    .Value = 7
                                Case "tbp7"
                                    .Value = 8
                                Case "tbp8"
                                    .Value = 9
                                Case "tbp9"
                                    .Value = 0
                            End Select
                        End With
                    Next

                    Dim buttonButtonIDElements = maButtonElement.Elements("buttonID")
                    For Each buttonButtonIDElement In buttonButtonIDElements
                        With buttonButtonIDElement
                            .Name = "ID"
                            Select Case .Value.ToLower
                                'Row 1
                                Case "btnmaq"
                                    .Value = 16
                                Case "btnmaw"
                                    .Value = 17
                                Case "btnmae"
                                    .Value = 18
                                Case "btnmar"
                                    .Value = 19
                                Case "btnmat"
                                    .Value = 20

                                Case "btnmay"
                                    .Value = 21
                                Case "btnmau"
                                    .Value = 22
                                Case "btnmai"
                                    .Value = 23
                                Case "btnmao"
                                    .Value = 24
                                Case "btnmap"
                                    .Value = 25
                                    'row 2
                                Case "btnmaa"
                                    .Value = 30
                                Case "btnmas"
                                    .Value = 31
                                Case "btnmad"
                                    .Value = 32
                                Case "btnmaf"
                                    .Value = 33
                                Case "btnmag"
                                    .Value = 34

                                Case "btnmah"
                                    .Value = 35
                                Case "btnmaj"
                                    .Value = 36
                                Case "btnmak"
                                    .Value = 37
                                Case "btnmal"
                                    .Value = 38
                                Case "btnmasemicolon"
                                    'older version of madapplauncher has a different case => btnmasemicolon
                                    .Value = 39
                                    'row 3
                                Case "btnmaz"
                                    .Value = 44
                                Case "btnmax"
                                    .Value = 45
                                Case "btnmac"
                                    .Value = 46
                                Case "btnmav"
                                    .Value = 47
                                Case "btnmab"
                                    .Value = 48

                                Case "btnman"
                                    .Value = 49
                                Case "btnmam"
                                    .Value = 50
                                Case "btnmacomma"
                                    .Value = 51
                                Case "btnmaperiod"
                                    .Value = 52
                                Case "btnmaslash"
                                    .Value = 53
                            End Select
                        End With
                    Next

                    RenameElement(maButtonElement.Element("buttonText"), "Text")
                    RenameElement(maButtonElement.Element("fileName"), "Filename")
                    RenameElement(maButtonElement.Element("arguments"), "Arguments")
                    RenameElement(maButtonElement.Element("workingDirectory"), "WorkingDirectory")
                    RenameElement(maButtonElement.Element("iconFile"), "IconFile")
                    RenameElement(maButtonElement.Element("iconIndex"), "IconIndex")
                    RenameElement(maButtonElement.Element("windowStyle"), "WindowStyle")
                    RenameElement(maButtonElement.Element("windowStyleEx"), "WindowStyleEx")
                    RenameElement(maButtonElement.Element("runAsAdmin"), "RunAsAdmin")

                    RemoveElement(maButtonElement.Element("windowMoveDelay"))
                Next
            Next

            For i As Integer = 0 To tabArray.Length - 1
                If Not (tabArray(i)) Then
                    If (i = 9) Then
                        root.Add(New XElement("Tab", {New XElement("ID", 0),
                                                      New XElement("Text")}))
                    Else
                        root.Add(New XElement("Tab", {New XElement("ID", i + 1),
                                                      New XElement("Text")}))
                    End If
                End If
            Next

            'Dim ns As XNamespace = MaxLauncher.TabControlData.GetInstance.Namespace

            'root.Name = ns + "TabControlData"
            root.Name = "TabControlData"

            'For Each el As XElement In xmlDoc.Descendants()
            '    el.Name = ns + el.Name.LocalName
            'Next

            xmlDoc.Save(destinationFile)

        Catch ex As Exception
            Throw New Exception(String.Format(Localization.GetString("String_ErrorConvertingFile"), sourceFile, destinationFile))
        End Try
    End Sub

    Private Sub RenameElement(element As XElement, newTag As String)
        If (element IsNot Nothing) Then element.Name = newTag
    End Sub

    Private Sub RemoveElement(element As XElement)
        If (element IsNot Nothing) Then element.Remove()
    End Sub
End Class
