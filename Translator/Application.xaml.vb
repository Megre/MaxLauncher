Class Application
    Private Sub Application_DispatcherUnhandledException(sender As Object, e As Windows.Threading.DispatcherUnhandledExceptionEventArgs)
        MessageBoxML.Show(e.Exception.Message, "Unhandled Exception",
                  MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
                  e.Exception.ToString)

        e.Handled = True
    End Sub

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

End Class
