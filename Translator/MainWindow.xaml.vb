'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data
Imports System.Xml

Class MainWindow

#Region "Data, Fields, Properties, Enums, etc."

    '''' <summary>
    '''' ItemSource for TranslationView
    '''' </summary> 
    'Private Property OutCollection As ObservableCollection(Of TranslationItem) = New ObservableCollection(Of TranslationItem)

    ''' <summary>
    ''' Column name of Key in ListView, outList and TranslationItem.
    ''' </summary> 
    Private ReadOnly KEY_COLUMN As String = "Key"

    ''' <summary>
    ''' Column name of Original in ListView, outList and TranslationItem.
    ''' </summary> 
    Private ReadOnly ORIGINAL_COLUMN As String = "Original"

    ''' <summary>
    ''' Column name of Translation in ListView, outList and TranslationItem.
    ''' </summary> 
    Private ReadOnly TRANSLATION_COLUMN As String = "Translation"

    ''' <summary>
    ''' Table name in DataSet.
    ''' </summary> 
    Private ReadOnly DS_TABLE_NAME As String = "String"

    ''' <summary>
    ''' Column name of Value in DataSet.
    ''' </summary> 
    Private ReadOnly DS_VALUE_COLUMN As String = "String_Text"

    ''' <summary>
    ''' 'DataSet used by the Original file.
    ''' </summary> 
    Property OriginalDataSet As DataSet = Nothing

    ''' <summary>
    ''' 'DataSet used by the Translation file.
    ''' </summary> 
    Property TranslationDataSet As DataSet = Nothing

    ''' <summary>
    ''' Dependency Property flag for data changed state.
    ''' </summary> 
    ''' <remarks>True if the data has been modified, false otherwise.</remarks>
    Public Property IsDirty As Boolean
        Get
            Return CBool(GetValue(IsDirtyProperty))
        End Get

        Set(ByVal value As Boolean)
            SetValue(IsDirtyProperty, value)
        End Set
    End Property

    Public Shared ReadOnly IsDirtyProperty As DependencyProperty =
                           DependencyProperty.Register("IsDirty",
                           GetType(Boolean), GetType(Window),
                           New PropertyMetadata(False))

    ''' <summary>
    ''' Dependency Property flag if the Original file is loaded.
    ''' </summary> 
    ''' <remarks>True if the Original file is loaded, false otherwise.</remarks>
    Public Property IsOriginalFileLoaded As Boolean
        Get
            Return CBool(GetValue(IsOriginalFileLoadedProperty))
        End Get

        Set(ByVal value As Boolean)
            SetValue(IsOriginalFileLoadedProperty, value)
        End Set
    End Property

    Public Shared ReadOnly IsOriginalFileLoadedProperty As DependencyProperty =
                           DependencyProperty.Register("IsOriginalFileLoaded",
                           GetType(Boolean), GetType(Window),
                           New PropertyMetadata(False))
#End Region

#Region "Inner Data Class"

    ''' <summary>
    ''' Translation item composed of a Key, the Original value and the Translation value. Used in building lists.
    ''' </summary>
    Public Class TranslationItem

        ''' <summary>
        ''' XAML Key value.
        ''' </summary>
        ''' <returns>Key value as String.</returns>
        Public Property Key As String

        ''' <summary>
        ''' XAML Original value.
        ''' </summary>
        ''' <returns>Original value as String.</returns>
        Public Property Original As String

        ''' <summary>
        ''' XAML Translation value.
        ''' </summary>
        ''' <returns>Translation value as String.</returns>
        Public Property Translation As String

        Sub New(keyStr As String, originalStr As String, translationStr As String)
            Key = keyStr
            Original = originalStr
            Translation = translationStr
        End Sub

        Public Overrides Function ToString() As String
            Return $"Key = {Me.Key}, Original = {Me.Original}, Translation = {Me.Translation}"
        End Function
    End Class

#End Region

#Region "Events"

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        IsDirty = False
        hideTranslatedCheckBox.IsChecked = My.Settings.HIDE_TRANSLATED_TEXT

        DataContext = Me

        'Load Original file.
        If Not String.IsNullOrEmpty(My.Settings.ORIGINAL_FILE) Then
            Try
                OriginalDataSet = LoadFile(My.Settings.ORIGINAL_FILE)
                originalFile.Text = My.Settings.ORIGINAL_FILE

                IsDirty = False
                UpdateListView()
            Catch ex As Exception
                MessageBoxML.Show(Me, ex.Message, "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error, MessageBoxResult.Cancel, ex.InnerException.ToString)
            End Try
        End If

        'Load Translation file.
        If Not String.IsNullOrEmpty(My.Settings.TRANSLATION_FILE) Then
            Try
                TranslationDataSet = LoadFile(My.Settings.TRANSLATION_FILE)
                translationFile.Text = My.Settings.TRANSLATION_FILE

                IsDirty = False
                UpdateListView()
            Catch ex As Exception
                MessageBoxML.Show(Me, ex.Message, "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error, MessageBoxResult.Cancel, ex.InnerException.ToString)
            End Try
        End If
    End Sub

    Private Sub Window_Closed(sender As Object, e As EventArgs)
        My.Settings.ORIGINAL_FILE = originalFile.Text
        If Not String.IsNullOrEmpty(translationFile.Text) Then My.Settings.TRANSLATION_FILE = translationFile.Text
        My.Settings.HIDE_TRANSLATED_TEXT = CBool(hideTranslatedCheckBox.IsChecked)

        My.Settings.Save()
    End Sub

    Private Sub Window_Closing(sender As Object, e As CancelEventArgs)
        If Not PromptToSaveChanges() Then e.Cancel = True
    End Sub

    Private Sub GridViewColumnHeader_Click(sender As Object, e As RoutedEventArgs)
        ' Sort the TranslationListView based on the column clicked.
        Dim sortColName As String = String.Empty

        If translationListView.Items.CanSort Then
            Dim ch As GridViewColumnHeader = TryCast(sender, GridViewColumnHeader)
            If ch IsNot Nothing Then sortColName = ch.Content.ToString

            Dim sd As SortDescriptionCollection = translationListView.Items.SortDescriptions()

            If sd.Count > 0 Then
                If String.Compare(sd.Item(0).PropertyName, sortColName) = 0 Then
                    'Previously sorted column, so toggle sorting
                    If sd.Item(0).Direction = ListSortDirection.Ascending Then
                        sd.Clear()
                        sd.Add(New SortDescription(sortColName, ListSortDirection.Descending))
                    Else
                        sd.Clear()
                        sd.Add(New SortDescription(sortColName, ListSortDirection.Ascending))
                    End If
                Else
                    'A new column is selected to be sorted.
                    sd.Clear()
                    sd.Add(New SortDescription(sortColName, ListSortDirection.Ascending))
                End If
            Else
                'Unsorted.
                sd.Add(New SortDescription(sortColName, ListSortDirection.Ascending))
            End If
        End If
    End Sub

    Private Sub GridViewColumnHeader_SizeChanged(sender As Object, e As SizeChangedEventArgs)
        ' Limit the minimum width of columns in TranslationListView.
        If (e.NewSize.Width <= 200) Then
            e.Handled = True
            DirectCast(sender, GridViewColumnHeader).Column.Width = 200
        End If
    End Sub

    Private Sub FilterCheckBox_Changed(sender As Object, e As RoutedEventArgs)
        If Not hideTranslatedCheckBox.IsChecked AndAlso Not hideEqualTextCheckBox.IsChecked Then
            'Reset the ListView filter.
            translationListView.Items.Filter = Nothing
            refreshButton.IsEnabled = False
        Else
            'Filter TranslationListView.
            translationListView.Items.Filter = New Predicate(Of Object)(AddressOf Me.FilterListView)
            refreshButton.IsEnabled = True
        End If
    End Sub

    Private Sub OriginalFile_TextChanged(sender As Object, e As TextChangedEventArgs)
        If Not String.IsNullOrEmpty(originalFile.Text) Then
            IsOriginalFileLoaded = True
            hideTranslatedCheckBox.Visibility = Visibility.Visible
            hideEqualTextCheckBox.Visibility = Visibility.Visible
            refreshButton.Visibility = Visibility.Visible
        End If
    End Sub

    Private Sub TranslationTextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
        If Not IsDirty Then IsDirty = True
    End Sub

#Region "Helper functions"

    ''' <summary>
    ''' Prompts the user for a file to be opened.
    ''' </summary>
    ''' <param name="title">Title of the dialog box.</param>
    ''' <param name="filter">Filter of the dialog box.</param>
    ''' <param name="defaultFilename">Default filename of the dialog box.</param>
    ''' <returns><strong>String</strong> - returns a the selected filename, empty otherwise.</returns>
    Private Function GetOpenFilename(title As String, filter As String, defaultFilename As String) As String
        Dim filename As String = String.Empty

        Dim dlg As New Microsoft.Win32.OpenFileDialog()

        With dlg
            .Title = title
            .Filter = filter
            .FileName = defaultFilename

            Dim result? As Boolean = .ShowDialog(Me)

            If (result) Then filename = .FileName
        End With

        Return filename
    End Function

    ''' <summary>
    ''' Prompts the user for a filename to be used to save the data.
    ''' </summary>
    ''' <returns>Returns a <strong>String</strong> containing the selected filename, empty otherwise.</returns>.
    Private Function FileSaveDialog() As String
        Dim filename As String = String.Empty

        Dim dlg As New Microsoft.Win32.SaveFileDialog()

        With dlg
            .Title = "Save Translation File"
            .Filter = "Language files (*.xaml) | *.xaml"
            .FileName = String.Empty

            Dim result? As Boolean = .ShowDialog(Me)

            If (result) Then filename = .FileName
        End With

        Return filename
    End Function

    ''' <summary>
    ''' Saves the data to a file.
    ''' </summary>
    ''' <param name="filename">A <strong>String</strong> containing the filename to which data will be saved.</param>
    ''' <exception cref="Exception">Throws an exception if saving fails.</exception>
    Private Sub Save(filename As String)
        Try
            Dim doc As XmlDocument = New XmlDocument
            Dim root As XmlElement = doc.CreateElement(String.Empty, "ResourceDictionary", String.Empty)
            root.SetAttribute("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation")
            root.SetAttribute("xmlns:x", "http://schemas.microsoft.com/winfx/2006/xaml")
            root.SetAttribute("xmlns:sys", "clr-namespace:System;assembly=mscorlib")
            doc.AppendChild(root)

            Dim lvCollection As ObservableCollection(Of TranslationItem) = TryCast(translationListView.ItemsSource, ObservableCollection(Of TranslationItem))

            'Create nodes.
            For Each item As TranslationItem In lvCollection
                Dim element As XmlElement = doc.CreateElement("sys", DS_TABLE_NAME, "clr-namespace:System;assembly=mscorlib")
                Dim attribute As XmlAttribute = doc.CreateAttribute("x", KEY_COLUMN, "http://schemas.microsoft.com/winfx/2006/xaml")
                attribute.Value = item.Key
                element.Attributes.Append(attribute)

                If String.IsNullOrEmpty(item.Translation) Then
                    'Use Original if Translation is empty.
                    Dim text As XmlText = doc.CreateTextNode(item.Original)
                    element.AppendChild(text)
                    root.AppendChild(element)
                Else
                    Dim text As XmlText = doc.CreateTextNode(item.Translation)
                    element.AppendChild(text)
                    root.AppendChild(element)
                End If
            Next

            doc.Save(filename)
            IsDirty = False
        Catch ex As Exception
            Throw New Exception($"Error saving {filename}.", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Helper function to filter TranslationView. Hides translated items.
    ''' </summary>
    ''' <param name="o">TranslationItem object.</param>
    ''' <returns><stong>Boolean</stong> - False if the TranslationItem is filtered(hidden), True otherwise.</returns>
    Public Function FilterListView(ByVal o As Object) As Boolean
        Dim show As Boolean = True
        Dim tItem As TranslationItem = TryCast(o, TranslationItem)

        If tItem IsNot Nothing Then
            If hideTranslatedCheckBox.IsChecked Then
                If String.Compare(tItem.Original, tItem.Translation, StringComparison.CurrentCulture) <> 0 Then show = False
            End If

            If hideEqualTextCheckBox.IsChecked Then
                If String.Compare(tItem.Original, tItem.Translation, StringComparison.CurrentCulture) = 0 Then show = False
            End If
        End If

        Return show
    End Function

    ''' <summary>
    ''' Helper function to filter TranslationView. Hides equal text.
    ''' </summary>
    ''' <param name="o">TranslationItem object.</param>
    ''' <returns><stong>Boolean</stong> - False if text is equal, True otherwise.</returns>
    Public Function HideEqualText(ByVal o As Object) As Boolean
        Dim tItem As TranslationItem = TryCast(o, TranslationItem)

        If tItem IsNot Nothing Then
            'Return True to show item.
            If String.Compare(tItem.Original, tItem.Translation, StringComparison.CurrentCulture) = 0 Then Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' Loads the specified file into a DataSet.
    ''' </summary>
    ''' <param name="filename"><strong>String</strong> - filename to load.</param>
    ''' <returns><strong>DataSet</strong> - of the file if successful, Nothing otherwise.</returns>
    Private Function LoadFile(filename As String) As DataSet
        Dim tempDataSet As DataSet = New DataSet()

        Try
            tempDataSet.ReadXml(filename)
        Catch ex As Exception
            Throw New Exception($"Error opening file: {filename}.", ex)
        End Try

        Return tempDataSet
    End Function

    ''' <summary>
    ''' Updates the list view.
    ''' </summary>
    Private Sub UpdateListView()
        If OriginalDataSet Is Nothing Then Return

        Dim tempObservableCollection As ObservableCollection(Of TranslationItem) = New ObservableCollection(Of TranslationItem)

        Try
            Dim originalDataTable As DataTable = OriginalDataSet.Tables.Item(DS_TABLE_NAME)
            Dim translationDataTable As DataTable = Nothing

            If TranslationDataSet IsNot Nothing Then
                translationDataTable = TranslationDataSet.Tables.Item(DS_TABLE_NAME)

                'Set primary key on translation table for Find to work.
                Dim keys(1) As DataColumn
                keys(0) = translationDataTable.Columns.Item(KEY_COLUMN)
                translationDataTable.PrimaryKey = keys
            End If

            'Add strings to tempObservableCollection.
            Dim dr As DataRow = Nothing
            For Each row As DataRow In originalDataTable.Rows
                If TranslationDataSet IsNot Nothing Then dr = translationDataTable.Rows.Find(row.Field(Of String)(KEY_COLUMN))

                If dr IsNot Nothing Then
                    tempObservableCollection.Add(New TranslationItem(row.Item(KEY_COLUMN).ToString, row.Item(DS_VALUE_COLUMN).ToString, dr.Item(DS_VALUE_COLUMN).ToString))
                Else
                    'Add missing row.
                    tempObservableCollection.Add(New TranslationItem(row.Item(KEY_COLUMN).ToString, row.Item(DS_VALUE_COLUMN).ToString, row.Item(DS_VALUE_COLUMN).ToString))
                    If TranslationDataSet IsNot Nothing Then IsDirty = True
                End If
            Next

            translationListView.ItemsSource = tempObservableCollection
        Catch ex As Exception
            Throw New Exception($"Error loading data.", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Ask the user to save the file if it has been modified.
    ''' </summary>
    ''' <returns><strong>Boolean</strong> - True if the file was saved successfully, False otherwise. Also returns Flase if the user cancels the process.</returns>
    Private Function PromptToSaveChanges() As Boolean
        If IsDirty Then
            Dim response As MessageBoxResult
            Dim response2 As MessageBoxResult

            response = MessageBoxML.Show(Me, "Do you want to save changes?", "Save File", MessageBoxButton.YesNoCancel,
                                    MessageBoxImage.Error, MessageBoxResult.Yes, "")
            Select Case response
                Case MessageBoxResult.Yes
                    If String.IsNullOrEmpty(translationFile.Text) Then
                        'If no filename set, get filename and save.
                        Dim filename = FileSaveDialog()
                        If Not String.IsNullOrEmpty(filename) Then
                            Try
                                Save(filename)
                                My.Settings.TRANSLATION_FILE = filename
                                Return True
                            Catch ex As Exception
                                response2 = MessageBoxML.Show(Me, $"Error saving file: {filename}. Exit and discard changes?", "Error", MessageBoxButton.YesNo,
                                    MessageBoxImage.Error, MessageBoxResult.Cancel, ex.InnerException.ToString)
                            End Try
                        End If
                    Else
                        Try
                            Save(translationFile.Text)
                            Return True
                        Catch ex As Exception
                            response2 = MessageBoxML.Show(Me, $"Error saving file: {translationFile.Text}. Exit and discard changes?", "Error", MessageBoxButton.YesNo,
                                MessageBoxImage.Error, MessageBoxResult.Cancel, ex.InnerException.ToString)
                        End Try
                    End If

                    Select Case response2
                        Case MessageBoxResult.Yes
                            Return True
                        Case MessageBoxResult.No
                            Return False
                    End Select
                Case MessageBoxResult.No
                    Return True
                Case MessageBoxResult.Cancel
                    Return False
            End Select
        Else
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Prompts the user for a filename and saves Translation data to it.
    ''' </summary>
    Private Sub SaveAs()
        Dim filename As String = FileSaveDialog()

        If Not String.IsNullOrEmpty(filename) Then
            Try
                Save(filename)
                translationFile.Text = filename
            Catch ex As Exception
                Throw
            End Try
        End If
    End Sub

#End Region

#Region "Menu Events"
    Private Sub File_OpenOrignalFile_MenuItem_Click(sender As Object, e As RoutedEventArgs)
        If PromptToSaveChanges() Then
            Dim filename As String = GetOpenFilename("Open Original File", "Language files (*.sample, *.xaml) | *.sample;*.xaml; | Original file (*.sample) | *.sample | XAML files (*.xaml) | *.xaml", "English.xaml.sample")

            If Not String.IsNullOrEmpty(filename) Then
                Try
                    OriginalDataSet = LoadFile(filename)
                    originalFile.Text = filename

                    TranslationDataSet = Nothing
                    translationFile.Text = String.Empty

                    IsDirty = False
                    UpdateListView()
                Catch ex As Exception
                    MessageBoxML.Show(Me, ex.Message, "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error, MessageBoxResult.Cancel, ex.InnerException.ToString)
                End Try
            End If
        End If
    End Sub

    Private Sub File_NewTranslationFile_MenuItem_Click(sender As Object, e As RoutedEventArgs)
        If PromptToSaveChanges() Then
            TranslationDataSet = Nothing
            translationFile.Text = String.Empty

            Try
                IsDirty = False
                UpdateListView()
            Catch ex As Exception
                MessageBoxML.Show(Me, ex.Message, "Error", MessageBoxButton.OK,
                                    MessageBoxImage.Error, MessageBoxResult.Cancel, ex.InnerException.ToString)
            End Try
        End If
    End Sub

    Private Sub File_OpenTranslationFile_MenuItem_Click(sender As Object, e As RoutedEventArgs)
        If PromptToSaveChanges() Then
            Dim filename As String = GetOpenFilename("Open Translation File", "Language files (*.xaml) | *.xaml", String.Empty)

            If Not String.IsNullOrEmpty(filename) Then
                Try
                    TranslationDataSet = LoadFile(filename)
                    translationFile.Text = filename

                    IsDirty = False
                    UpdateListView()
                Catch ex As Exception
                    MessageBoxML.Show(Me, ex.Message, "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error, MessageBoxResult.Cancel, ex.InnerException.ToString)
                End Try
            End If
        End If
    End Sub

    Private Sub File_SaveTranslationFile_MenuItem_Click(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrEmpty(translationFile.Text) Then
            Try
                SaveAs()
            Catch ex As Exception
                MessageBoxML.Show(Me, ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK, ex.InnerException.ToString)

            End Try
        Else
            Try
                Save(translationFile.Text)
            Catch ex As Exception
                MessageBoxML.Show(Me, ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK, ex.InnerException.ToString)
            End Try
        End If
    End Sub

    Private Sub File_SaveTranslationFileAs_MenuItem_Click(sender As Object, e As RoutedEventArgs)
        Try
            SaveAs()
        Catch ex As Exception
            MessageBoxML.Show(Me, ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK, ex.InnerException.ToString)
        End Try
    End Sub

    Private Sub File_Exit_MenuItem_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub

    Private Sub Help_VisitWebSite_Click(sender As Object, e As RoutedEventArgs)
        Try
            Process.Start(My.Settings.AppWebsite)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Help_About_Click(sender As Object, e As RoutedEventArgs)
        Dim aboutDlg As New AboutDialog With {
            .Owner = Me
        }
        aboutDlg.ShowDialog()
    End Sub


#End Region

#End Region
End Class

