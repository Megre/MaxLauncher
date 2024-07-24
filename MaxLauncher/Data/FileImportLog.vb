Imports System.IO
Imports System.Text
Imports System.Xml.Linq

Public Class FileImportLog
    Private _filename As String = "FileImportLog.md"
    Dim _fs As FileStream

    Public Sub New()
        If File.Exists(_filename) Then File.Delete(_filename)
        _fs = File.Create(_filename)

        WriteLine(DateTime.Now.ToString())
    End Sub

    Public Sub LogButton(msg As String, buttonElement As XElement)
        WriteLine("`" & msg & "`: `[" &
           If(buttonElement.Element("Text").Value, "") & "]` " &
           If(buttonElement.Element("Filename").Value, "") & " in `Tab [" &
           If(buttonElement.Parent.Element("ID").Value, "") & "]` " &
           If(buttonElement.Parent.Element("Text").Value, "")
           )
    End Sub

    Public Sub WriteLine(line As String)
        Dim bytes As Byte() = Encoding.UTF8.GetBytes(line & "<br/>" & Environment.NewLine)
        _fs.Write(bytes, 0, bytes.Length)
    End Sub

    Public Function FileName() As String
        Return _filename
    End Function

    Public Sub Close()
        _fs.Close()
    End Sub

End Class
