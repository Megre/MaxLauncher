'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Class FavoritesBar
    Inherits ContentControl

    Private Shared ReadOnly _instance As New FavoritesBar

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
    End Sub

    Friend Shared Function GetInstance() As FavoritesBar
        Return _instance
    End Function

End Class
