'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Reflection
Imports System.Collections
Imports System.ComponentModel

Class XItemCollection
    Inherits CollectionBase
    Implements ICustomTypeDescriptor

    Public Sub Add(xitem As XItem)
        Me.List.Add(xitem)
    End Sub

    Public Sub Remove(xitem As XItem)
        Me.List.Remove(xitem)
    End Sub

    Default Public ReadOnly Property Item(index As Integer) As XItem
        Get
            Return Me.List(index)
        End Get
    End Property

#Region "ICustomTypeDescriptor"

    Public Function GetAttributes() As AttributeCollection Implements ICustomTypeDescriptor.GetAttributes
        Return TypeDescriptor.GetAttributes(Me, True)
    End Function

    Public Function GetComponentName() As String Implements ICustomTypeDescriptor.GetComponentName
        Return TypeDescriptor.GetComponentName(Me, True)
    End Function

    Public Function GetConverter() As TypeConverter Implements ICustomTypeDescriptor.GetConverter
        Return TypeDescriptor.GetConverter(Me, True)
    End Function

    Public Function GetDefaultEvent() As EventDescriptor Implements ICustomTypeDescriptor.GetDefaultEvent
        Return TypeDescriptor.GetDefaultEvent(Me, True)
    End Function

    Public Function GetEvents() As EventDescriptorCollection Implements ICustomTypeDescriptor.GetEvents
        Return TypeDescriptor.GetEvents(Me, True)
    End Function

    Public Function GetEvents(attributes As Attribute()) As EventDescriptorCollection Implements ICustomTypeDescriptor.GetEvents
        Return TypeDescriptor.GetEvents(Me, attributes, True)
    End Function

    Public Function GetClassName() As [String] Implements ICustomTypeDescriptor.GetClassName
        Return TypeDescriptor.GetClassName(Me, True)
    End Function

    Public Function GetDefaultProperty() As PropertyDescriptor Implements ICustomTypeDescriptor.GetDefaultProperty
        Return TypeDescriptor.GetDefaultProperty(Me, True)
    End Function

    Public Function GetEditor(editorBaseType As Type) As Object Implements ICustomTypeDescriptor.GetEditor
        Return TypeDescriptor.GetEditor(Me, editorBaseType, True)
    End Function

    Public Function GetPropertyOwner(pd As PropertyDescriptor) As Object Implements ICustomTypeDescriptor.GetPropertyOwner
        Return Me
    End Function

    Public Function GetProperties(attributes As Attribute()) As PropertyDescriptorCollection Implements ICustomTypeDescriptor.GetProperties
        Return GetProperties()
    End Function

    Public Function GetProperties() As PropertyDescriptorCollection Implements ICustomTypeDescriptor.GetProperties
        Dim pds As New PropertyDescriptorCollection(Nothing)

        For i As Integer = 0 To Me.List.Count - 1
            'Create a property descriptor for each item.
            Dim pd As New XItemCollectionPropertyDescriptor(Me, i)

            pds.Add(pd)
        Next

        Return pds
    End Function

#End Region
End Class
