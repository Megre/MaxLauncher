'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.ComponentModel

Class XItemCollectionPropertyDescriptor
    Inherits PropertyDescriptor

    Private collection As XItemCollection = Nothing
    Private index As Integer = -1

    Public Sub New(coll As XItemCollection, index As Integer)
        MyBase.New("#" + index.ToString, Nothing)
        Me.collection = coll
        Me.index = index
    End Sub

    Public Overrides Function CanResetValue(component As Object) As Boolean
        Return True
    End Function

    Public Overrides ReadOnly Property ComponentType As Type
        Get
            Return Me.collection.Item(index).Value.GetType()
        End Get
    End Property

    Public Overrides Function GetValue(component As Object) As Object
        Return Me.collection.Item(index).Value
    End Function

    Public Overrides ReadOnly Property IsReadOnly As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property PropertyType As Type
        Get
            Return Me.collection.Item(index).Value.GetType
        End Get
    End Property

    Public Overrides Sub ResetValue(component As Object)

    End Sub

    Public Overrides Sub SetValue(component As Object, value As Object)
        Me.collection.Item(index).Value = value
    End Sub

    Public Overrides Function ShouldSerializeValue(component As Object) As Boolean
        Return True
    End Function

    Public Overrides ReadOnly Property DisplayName As String
        Get
            Return Me.collection.Item(index).Name
        End Get
    End Property

    Public Overrides ReadOnly Property Attributes() As AttributeCollection
        Get
            Return New AttributeCollection(Nothing)
        End Get
    End Property

    Public Overrides ReadOnly Property Category As String
        Get
            Return Me.collection.Item(index).Category
        End Get
    End Property
End Class
