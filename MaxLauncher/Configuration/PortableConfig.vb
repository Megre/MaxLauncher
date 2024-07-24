'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.ComponentModel
Imports System.Xml.Serialization
Imports System.IO
Imports System.Reflection

Public Class PortableConfig
    Public Const THEMES_DIRECTORY = "Themes"
    Public Const LANGUAGE_DIRECTORY = "Language"
    Public Const SHORTCUTS_DIRECTORY = "Shortcuts"
    Public Const ICON_CACHE_FILE = "iconcache.mldb"
    Public Const FAVORITES_CONFIG_FILE = "favorites.cfg"
    Public Const APPLICATION_CONFIG_FILE = "app.cfg"
    Public Const PORTABLE_CONFIG_FILE = "portable.cfg"
    Public Const WINDOW_CONFIG_FILE = "window.cfg"

    <Description("Gets/Sets window.cfg location")>
    Public Property WindowConfigFile As String = My.Application.Info.DirectoryPath

    <Description("Gets/Sets iconcache.mldb location")>
    Public Property IconCacheFile As String = My.Application.Info.DirectoryPath

    <Description("Gets/Sets favorites config file location")>
    Public Property FavoritesConfigFile As String = My.Application.Info.DirectoryPath

    <Description("Gets/Sets themes directory location")> _
    Public Property ThemesDirectory As String = My.Application.Info.DirectoryPath

    <Description("Gets/Sets language directory location")>
    Public Property LanguageDirectory As String = My.Application.Info.DirectoryPath

    <Description("Gets/Sets shortcuts directory location")>
    Public Property ShortcutsDirectory As String = My.Application.Info.DirectoryPath

    <Description("Gets/Sets application config location")>
    Public Property ApplicationConfigFile As String = My.Application.Info.DirectoryPath

    <Description("Gets/Sets WindowConfigFile Read Only mode")>
    Public Property WindowConfigFileRO As Boolean = False

    <Description("Gets/Sets ApplicationConfigFile Read Only mode")>
    Public Property ApplicationConfigFileRO As Boolean = False

    <Description("Gets/Sets FavoritesConfigFile Read Only mode")>
    Public Property FavoritesConfigFileRO As Boolean = False

    <Description("Gets/Sets TabControlData Read Only mode")> _
    Public Property TabControlDataRO As Boolean = False

    <Description("Gets/Sets IconCacheDB Read Only mode")> _
    Public Property IconCacheFileRO As Boolean = False

End Class
