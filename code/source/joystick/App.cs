using System;
using System.Diagnostics;
using System.Windows;

namespace WpfEpuckLayout {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public class App : Application {
    /// <summary>
    /// Entry point of Wpf application.
    /// </summary>
    [System.STAThreadAttribute()]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public static void Main() {
      WpfEpuckLayout.App app = new WpfEpuckLayout.App();
      app.InitializeComponent();
      app.Run();
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
    }

    static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
      try {
        Exception ex = (Exception)e.ExceptionObject;
        MessageBox.Show(ex.Message + "stack trace \n\n" + ex.StackTrace);
      } finally {
        Application.Current.Shutdown(1);
      }
    }
    /// <summary>
    /// Set Window1 as startup window.
    /// </summary>
    public void InitializeComponent() {
      this.StartupUri = new System.Uri("Window1.xaml", System.UriKind.Relative);
     
    }
  }
}

