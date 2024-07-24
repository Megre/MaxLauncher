Imports System.Text
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()> Public Class CheckLanguages

    <TestMethod()> Public Sub CheckLanguages()
        'Set baseDir to MaxLauncher base directory.
        Dim baseDir As String = My.Application.Info.DirectoryPath
        baseDir = Path.GetDirectoryName(baseDir)
        baseDir = Path.GetDirectoryName(baseDir)
        baseDir = Path.GetDirectoryName(baseDir)

        Dim refLanguageFile = Path.Combine(baseDir, "MaxLauncher\bin\Debug\Language\English.xaml.sample")
        Dim origRefLanguageFile = Path.Combine(baseDir, "MaxLauncher\Localization\English.xaml")
        Dim languageDir = Path.Combine(baseDir, "MaxLauncher\bin\Debug\Language")

        'Copy English.xaml to English.xaml.sample
        Try
            My.Computer.FileSystem.CopyFile(origRefLanguageFile, refLanguageFile, True)
        Catch ex As Exception
            Assert.Fail(ex.ToString)
        End Try

        'Read English.xaml.sample.
        Dim refLanguageFileContent As List(Of String) = StripData(File.ReadAllLines(refLanguageFile, Encoding.UTF8))

        'Compare English.xaml.sample to each *.xaml in the Language directory.
        For Each foundFile In My.Computer.FileSystem.GetFiles(languageDir, FileIO.SearchOption.SearchTopLevelOnly, "*.xaml")
            Dim foundFileName = Path.GetFileName(foundFile)
            Dim foundFileContent As List(Of String) = StripData(File.ReadAllLines(foundFile, Encoding.UTF8))

            'difference only contains the string in refLanguageFileContent.
            Dim difference = refLanguageFileContent.Except(foundFileContent)

            If difference.Any Then
                Trace.WriteLine(String.Format("Comparing {0}: FAILED", foundFileName))
                For Each line In difference
                    Trace.Write(line)
                Next
                Assert.Fail(foundFileName)
            Else
                Trace.WriteLine(String.Format("Comparing {0}: OK", foundFileName))
            End If
        Next
    End Sub

    Function StripData(input As String()) As List(Of String)
        Dim content As New List(Of String)

        For Each line In input
            line = line.Trim()
            line = RegularExpressions.Regex.Replace(line, "^\s*(<sys:.*?>).*(<.*?>)$", "$1$2")
            line = RegularExpressions.Regex.Replace(line, "^\s*<!.*?>$", "")
            line = RegularExpressions.Regex.Replace(line, " xml:space=""preserve""", "")

            If Not String.IsNullOrEmpty(line) Then
                content.Add(line)
            End If
        Next

        Return content
    End Function

End Class