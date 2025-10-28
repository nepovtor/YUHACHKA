using System.Windows;

namespace ColorPickerApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ThemeManager.Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        ThemeManager.Dispose();
    }
}
