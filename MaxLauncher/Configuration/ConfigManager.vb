'Copyright 2014-2021 Roberto Concepcion.  All Rights Reserved.
'
'This file is part of MaxLauncher.
'
'MaxLauncher is released under Microsoft Reciprocal License (MS-RL).
'The license and further copyright text can be found in the file
'LICENSE.TXT at the root directory of the distribution.

Imports System.Xml.Serialization
Imports System.IO
Imports MaxLauncher.Utility

Class ConfigManager
    Private Shared ReadOnly PortableFile = Path.Combine(My.Application.Info.DirectoryPath,
                                                        MaxLauncher.PortableConfig.PORTABLE_CONFIG_FILE)
    'Private Shared ReadOnly PortableFile = MaxLauncher.PortableConfig.PORTABLE_CONFIG_FILE
    Private Shared ReadOnly AppDataDirectory = System.IO.Path.Combine(Environment.GetFolderPath(
                                                            Environment.SpecialFolder.ApplicationData),
                                                            My.Application.Info.AssemblyName)

    Private Shared _WindowConfig As New WindowConfig
    Friend Shared Function WindowConfig() As WindowConfig
        Return _WindowConfig
    End Function

    Private Shared _AppConfig As New AppConfig
    Friend Shared Function AppConfig() As AppConfig
        Return _AppConfig
    End Function

    Private Shared _PortableConfig As New PortableConfig
    Friend Shared Function PortableConfig() As PortableConfig
        Return _PortableConfig
    End Function

    ''' <summary>
    ''' Checks if portable.cfg file exists in the location of the working directory.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Shared Function IsPortable()
        Return File.Exists(PortableFile)
    End Function

    ''' <summary>
    ''' Loads the configuration paths defined in portable.cfg.
    ''' </summary>
    ''' <remarks>If a configuration item is missing, the default for it will be used(ie. Appdata directory).
    ''' It also creates the directory paths if they do not exist.</remarks>
    ''' <exception cref="System.Exception">when loading fails.</exception>
    Friend Shared Sub LoadConfigPaths()
        Try
            If (IsPortable()) Then
                Load(PortableFile, _PortableConfig)

                'Convert to full paths.
                PortableConfig.WindowConfigFile = System.IO.Path.GetFullPath(System.IO.Path.Combine(FileUtils.Path.ExpandEnvironmentVariables(
                                                       PortableConfig.WindowConfigFile), MaxLauncher.PortableConfig.WINDOW_CONFIG_FILE))
                PortableConfig.ShortcutsDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(FileUtils.Path.ExpandEnvironmentVariables(
                                                           PortableConfig.ShortcutsDirectory), MaxLauncher.PortableConfig.SHORTCUTS_DIRECTORY))
                PortableConfig.LanguageDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(FileUtils.Path.ExpandEnvironmentVariables(
                                                           PortableConfig.LanguageDirectory), MaxLauncher.PortableConfig.LANGUAGE_DIRECTORY))
                PortableConfig.ThemesDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(FileUtils.Path.ExpandEnvironmentVariables(
                                                           PortableConfig.ThemesDirectory), MaxLauncher.PortableConfig.THEMES_DIRECTORY))
                PortableConfig.ApplicationConfigFile = System.IO.Path.GetFullPath(System.IO.Path.Combine(FileUtils.Path.ExpandEnvironmentVariables(
                                                       PortableConfig.ApplicationConfigFile), MaxLauncher.PortableConfig.APPLICATION_CONFIG_FILE))
                PortableConfig.FavoritesConfigFile = System.IO.Path.GetFullPath(System.IO.Path.Combine(FileUtils.Path.ExpandEnvironmentVariables(
                                                       PortableConfig.FavoritesConfigFile), MaxLauncher.PortableConfig.FAVORITES_CONFIG_FILE))
                PortableConfig.IconCacheFile = System.IO.Path.GetFullPath(System.IO.Path.Combine(FileUtils.Path.ExpandEnvironmentVariables(
                                                       PortableConfig.IconCacheFile), MaxLauncher.PortableConfig.ICON_CACHE_FILE))
            Else
                'Load defaults
                PortableConfig.WindowConfigFile = System.IO.Path.Combine(AppDataDirectory, MaxLauncher.PortableConfig.WINDOW_CONFIG_FILE)
                PortableConfig.ShortcutsDirectory = System.IO.Path.Combine(AppDataDirectory, MaxLauncher.PortableConfig.SHORTCUTS_DIRECTORY)
                PortableConfig.LanguageDirectory = System.IO.Path.Combine(AppDataDirectory, MaxLauncher.PortableConfig.LANGUAGE_DIRECTORY)
                PortableConfig.ThemesDirectory = System.IO.Path.Combine(AppDataDirectory, MaxLauncher.PortableConfig.THEMES_DIRECTORY)
                PortableConfig.ApplicationConfigFile = System.IO.Path.Combine(AppDataDirectory, MaxLauncher.PortableConfig.APPLICATION_CONFIG_FILE)
                PortableConfig.FavoritesConfigFile = System.IO.Path.Combine(AppDataDirectory, MaxLauncher.PortableConfig.FAVORITES_CONFIG_FILE)
                PortableConfig.IconCacheFile = System.IO.Path.Combine(AppDataDirectory, MaxLauncher.PortableConfig.ICON_CACHE_FILE)

                'Create MaxLauncher directory in Documents only if not Portable.
                My.Computer.FileSystem.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), My.Application.Info.AssemblyName))
            End If

            'Create directories
            My.Computer.FileSystem.CreateDirectory(System.IO.Path.GetDirectoryName(PortableConfig.WindowConfigFile))
            My.Computer.FileSystem.CreateDirectory(PortableConfig.ShortcutsDirectory)
            My.Computer.FileSystem.CreateDirectory(PortableConfig.LanguageDirectory)
            My.Computer.FileSystem.CreateDirectory(PortableConfig.ThemesDirectory)
            My.Computer.FileSystem.CreateDirectory(System.IO.Path.GetDirectoryName(PortableConfig.ApplicationConfigFile))
            My.Computer.FileSystem.CreateDirectory(System.IO.Path.GetDirectoryName(PortableConfig.FavoritesConfigFile))
            My.Computer.FileSystem.CreateDirectory(System.IO.Path.GetDirectoryName(PortableConfig.IconCacheFile))

        Catch ex As Exception
            'Exceptions thrown when creating directories.
            Throw New Exception(String.Format(Localization.GetString("String_ErrorLoadingFile"), PortableFile), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Loads all configurations. Loads portable.cfg then app.cfg.
    ''' </summary>
    ''' <exception cref="System.Exception">Thrown when loading fails.</exception>
    Friend Shared Sub LoadAll()
        Try
            LoadConfigPaths()
        Finally
        End Try

        'window.cfg loading error. Show information error and continue loading.
        Try
            If (File.Exists(PortableConfig.WindowConfigFile)) Then _
                                Load(PortableConfig.WindowConfigFile, _WindowConfig)
        Catch ex As Exception
            'Ignore error(s).
        End Try

        'app.cfg loading error. Show information error and exit program.
        Try
            If (File.Exists(PortableConfig.ApplicationConfigFile)) Then _
                                Load(PortableConfig.ApplicationConfigFile, _AppConfig)
        Catch ex As Exception
            Throw New Exception(String.Format(Localization.GetString("String_ErrorLoadingFile"),
                                              PortableConfig.ApplicationConfigFile), ex)
        End Try


    End Sub

    ''' <summary>
    ''' Saves the favorites configuration file.
    ''' </summary>
    ''' <remarks>File is not saved if the favorites configuration file is in read-only mode.</remarks>
    Friend Shared Sub SaveFavorites()
        If (ConfigManager.PortableConfig.FavoritesConfigFileRO) Then Return

        'Save favorite buttons' data.
        Try
            If (FavoritesBarData.GetInstance.Exists()) Then
                FavoritesBarData.GetInstance.Save()
            End If
        Catch ex As Exception
            MessageBoxML.Show(ex.Message,
                              Localization.GetString("String_SavingFavoritesBarData"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK,
                              ex.Message & vbCrLf & ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Saves the currently loaded .mld file.
    ''' </summary>
    ''' <remarks>File is not saved if the tab control data is in read-only mode.</remarks>
    Friend Shared Sub SaveTabControlData()
        If (ConfigManager.PortableConfig.TabControlDataRO) Then Return

        'Save tabcontrol data.
        Try
            If (TabControlData.GetInstance.Exists()) Then
                TabControlData.GetInstance.Save()
            End If
        Catch ex As Exception
            MessageBoxML.Show(ex.Message,
                              Localization.GetString("String_SavingData"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK,
                              ex.Message & vbCrLf & ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Saves the icon cache database.
    ''' </summary>
    ''' <remarks>File is not saved if the icon cache database is in read-only mode.</remarks>
    Friend Shared Sub SaveIconCacheDB()
        If (ConfigManager.PortableConfig.IconCacheFileRO) Then Return

        'Save IconCacheDB
        Try
            If (IconCacheDB.GetInstance.Exists()) Then
                IconCacheDB.GetInstance.Save()
            End If
        Catch ex As Exception
            MessageBoxML.Show(ex.Message,
                              Localization.GetString("String_SavingIconCacheDatabase"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK,
                              ex.Message & vbCrLf & ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Saves the application configuration file.
    ''' </summary>
    ''' <remarks>File is not saved if application configuration is in read-only mode.</remarks>
    Friend Shared Sub SaveAppCfg()
        If (ConfigManager.PortableConfig.ApplicationConfigFileRO) Then Return

        'Save app.cfg
        Try
            If (TabControlData.GetInstance.Exists()) Then
                'Save open file to as last used file.
                ConfigManager.AppConfig.LastFileUsed = TabControlData.GetInstance.Filename
            End If

            Save(PortableConfig.ApplicationConfigFile, AppConfig)
        Catch ex As Exception
            MessageBoxML.Show(ex.Message,
                              Localization.GetString("String_SavingApplicationConfiguration"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK,
                              ex.Message & vbCrLf & ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Saves the window configuration file.
    ''' </summary>
    ''' <remarks>Saves the location and windows size.</remarks>
    Friend Shared Sub SaveWindowCfg()
        If (ConfigManager.PortableConfig.WindowConfigFileRO) Then Return

        'Save window.cfg
        Try
            Save(PortableConfig.WindowConfigFile, WindowConfig)
        Catch ex As Exception
            MessageBoxML.Show(ex.Message,
                              Localization.GetString("String_SavingWindowConfiguration"), MessageBoxButton.OK,
                              MessageBoxImage.Error, MessageBoxResult.OK,
                              ex.Message & vbCrLf & ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Serializes an object and saves it to a file.
    ''' </summary>
    ''' <param name="filePath">Path and filename of the destination.</param>
    ''' <param name="obj">The object to be serialized to the <i>filePath</i>.</param>
    ''' <remarks></remarks>
    ''' <exception cref="System.Exception">when save fails.</exception>
    Friend Shared Sub Save(ByVal filePath As String, ByVal obj As Object)
        'Dim fs As FileStream = Nothing
        Try
            'Bypass disk cache.
            'fs = New FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 1024, FileOptions.WriteThrough)
            'Using writer = New StreamWriter(filePath)
            Using writer As TextWriter = New StreamWriter(filePath)
                Dim serializer As New XmlSerializer(obj.GetType)

                serializer.Serialize(writer, obj)
            End Using
        Catch ex As Exception
            Throw New Exception(String.Format(Localization.GetString("String_ErrorSavingFile"), filePath), ex)
            'Finally
            'If fs IsNot Nothing Then fs.Close()
        End Try
    End Sub

    ''' <summary>
    ''' Loads a serialized object from a file.
    ''' </summary>
    ''' <param name="filePath">Path and filename of the destination.</param>
    ''' <param name="obj">The object to be serialized to the <i>filePath</i>.</param>
    ''' <remarks></remarks>
    ''' <exception cref="System.Exception">when load fails.</exception>
    Friend Shared Sub Load(ByVal filePath As String, ByRef obj As Object)
        Try
            Using reader As New StreamReader(filePath)
                Dim serializer As XmlSerializer = New XmlSerializer(obj.GetType)

                obj = serializer.Deserialize(reader)
            End Using
        Catch ex As Exception
            Throw New Exception(String.Format(Localization.GetString("String_ErrorLoadingFile"), filePath), ex)
        End Try
    End Sub
End Class
