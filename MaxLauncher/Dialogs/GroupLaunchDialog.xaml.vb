'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.ComponentModel
Imports MaxLauncher.Utility

Public Class GroupLaunchDialog

    Property ID As String = ""

    Sub New()
        ' This call is required by the designer.
        InitializeComponent()
    End Sub

    Sub New(id As String)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.ID = id
    End Sub

    Private Sub GroupLaunchDialog_Loaded(sender As Object, e As RoutedEventArgs)
        Me.MinWidth = Me.ActualWidth
        Me.MinHeight = Me.ActualHeight
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub

    Private Sub GroupLaunchListView_Loaded(sender As Object, e As RoutedEventArgs)

        Dim groupLaunchList As List(Of GroupMember) = New List(Of GroupMember)

        If FavoritesBarData.GetInstance IsNot Nothing Then

            Dim groupIDQuery As EnumerableRowCollection = Nothing

            If String.IsNullOrEmpty(ID) Then
                groupIDQuery = From s In FavoritesBarData.GetInstance.Tables("Button").AsEnumerable()
                               Where s.RowState <> DataRowState.Deleted AndAlso (s.Field(Of String)("GroupID") Is Nothing Or s.Field(Of String)("GroupID") <> "")
                               Select New With {.ID = s.Field(Of System.Int16)("ID"),
                                                .Text = s.Field(Of System.String)("Text"),
                                                .Filename = s.Field(Of System.String)("Filename"),
                                                .Arguments = s.Field(Of System.String)("Arguments")}
            Else
                groupIDQuery = From s In FavoritesBarData.GetInstance.Tables("Button").AsEnumerable
                               Where s.RowState <> DataRowState.Deleted AndAlso s.Field(Of String)("GroupID") = ID
                               Select New With {.ID = s.Field(Of System.Int16)("ID"),
                                                .Text = s.Field(Of System.String)("Text"),
                                                .Filename = s.Field(Of System.String)("Filename"),
                                                .Arguments = s.Field(Of System.String)("Arguments")}
            End If

            If groupIDQuery IsNot Nothing Then
                For Each row In groupIDQuery
                    Dim key = Keyboard.GetText(row.Id)
                    groupLaunchList.Add(New GroupMember(ID, "", "", key, row.Text, row.Filename, row.Arguments))
                Next
            End If
        End If

        'Return if no tab or tab control exists.
        If TabControlData.GetInstance IsNot Nothing Then

            Dim groupIDQuery2 As EnumerableRowCollection = Nothing

            If String.IsNullOrEmpty(ID) Then
                groupIDQuery2 = From s In TabControlData.GetInstance.Tables("Button").AsEnumerable
                                Where s.RowState <> DataRowState.Deleted AndAlso (s.Field(Of String)("GroupID") Is Nothing Or s.Field(Of String)("GroupID") <> "")
                                Select New With {
                                    .Group_ID = s.Field(Of System.String)("GroupID"),
                                    .Tab_ID = s.Field(Of System.Int16)("Tab_ID"),
                                    .ID = s.Field(Of System.Int16)("ID"),
                                    .Text = s.Field(Of System.String)("Text"),
                                    .Filename = s.Field(Of System.String)("Filename"),
                                    .Arguments = s.Field(Of System.String)("Arguments")}

                If groupIDQuery2 IsNot Nothing Then
                    For Each row In groupIDQuery2
                        groupLaunchList.Add(New GroupMember(row.Group_ID, row.Tab_ID, TabControlData.GetInstance.Tab.FindById(row.Tab_ID).Text, Keyboard.GetText(row.Id), row.Text, row.Filename, row.Arguments))
                    Next
                End If
            Else
                groupIDQuery2 = From s In TabControlData.GetInstance.Tables("Button").AsEnumerable
                                Where s.RowState <> DataRowState.Deleted AndAlso s.Field(Of String)("GroupID") = ID
                                Select New With {
                                    .Group_ID = s.Field(Of System.String)("GroupID"),
                                    .Tab_ID = s.Field(Of System.Int16)("Tab_ID"),
                                    .ID = s.Field(Of System.Int16)("ID"),
                                    .Text = s.Field(Of System.String)("Text"),
                                    .Filename = s.Field(Of System.String)("Filename"),
                                    .Arguments = s.Field(Of System.String)("Arguments")}

                If groupIDQuery2 IsNot Nothing Then
                    For Each row In groupIDQuery2
                        groupLaunchList.Add(New GroupMember(ID, row.Tab_ID, TabControlData.GetInstance.Tab.FindById(row.Tab_ID).Text, Keyboard.GetText(row.Id), row.Text, row.Filename, row.Arguments))
                    Next
                End If
            End If

        End If

        groupLaunchListView.ItemsSource = groupLaunchList

        Dim view As CollectionView = DirectCast(CollectionViewSource.GetDefaultView(groupLaunchListView.ItemsSource), CollectionView)
        view.SortDescriptions.Add(New SortDescription("GroupID", ListSortDirection.Ascending))
        view.SortDescriptions.Add(New SortDescription("TabNumber", ListSortDirection.Ascending))
        view.SortDescriptions.Add(New SortDescription("TabName", ListSortDirection.Ascending))
        view.SortDescriptions.Add(New SortDescription("ButtonKey", ListSortDirection.Ascending))
        view.SortDescriptions.Add(New SortDescription("ButtonName", ListSortDirection.Ascending))
        view.SortDescriptions.Add(New SortDescription("Target", ListSortDirection.Ascending))
        view.SortDescriptions.Add(New SortDescription("Arguments", ListSortDirection.Ascending))
    End Sub

    Private Class GroupMember
        Public Property GroupID As String
        Public Property TabNumber As String
        Public Property TabName As String
        Public Property ButtonKey As String
        Public Property ButtonName As String
        Public Property Target As String
        Public Property Arguments As String

        Public Sub New()
        End Sub

        Public Sub New(groupMembers As GroupMember)
            Me.GroupID = groupMembers.GroupID
            Me.TabNumber = groupMembers.TabNumber
            Me.TabName = groupMembers.TabName
            Me.ButtonKey = groupMembers.ButtonKey
            Me.ButtonName = groupMembers.ButtonName
            Me.Target = groupMembers.Target
            Me.Arguments = groupMembers.Arguments
        End Sub

        Public Sub New(groupID As String, tabNumber As String, tabName As String, buttonKey As String, buttonName As String,
                       target As String, arguments As String)
            Me.GroupID = groupID
            Me.TabNumber = tabNumber
            Me.TabName = tabName
            Me.ButtonKey = buttonKey
            Me.ButtonName = buttonName
            Me.Target = target
            Me.Arguments = arguments
        End Sub
    End Class
End Class
