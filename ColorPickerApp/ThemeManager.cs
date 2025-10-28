using System;
using System.Windows;
using System.Windows.Media;

namespace ColorPickerApp;

public static class ThemeManager
{
    public static event EventHandler? ThemeChanged;

    public static void Initialize()
    {
        ApplyTheme(AppTheme.Light);
    }

    public static void Dispose()
    {
        ThemeChanged = null;
    }

    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public static void ApplyTheme(AppTheme theme)
    {
        CurrentTheme = theme;
        Application.Current.Resources["ThemeBackgroundBrush"] = new SolidColorBrush(theme == AppTheme.Light
            ? ((SolidColorBrush)Application.Current.Resources["LightThemeBackground"]).Color
            : ((SolidColorBrush)Application.Current.Resources["DarkThemeBackground"]).Color);
        Application.Current.Resources["ThemeForegroundBrush"] = new SolidColorBrush(theme == AppTheme.Light
            ? ((SolidColorBrush)Application.Current.Resources["LightThemeForeground"]).Color
            : ((SolidColorBrush)Application.Current.Resources["DarkThemeForeground"]).Color);
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }
}

public enum AppTheme
{
    Light,
    Dark
}
