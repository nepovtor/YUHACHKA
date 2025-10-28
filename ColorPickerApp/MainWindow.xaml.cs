using System;
using System.Windows;
using System.Windows.Input;
using ColorPickerApp.ViewModels;

namespace ColorPickerApp;

public partial class MainWindow : Window
{
    private readonly EventHandler _themeChangedHandler;

    public static readonly RoutedUICommand FocusSearchCommand = new("FocusSearch", nameof(FocusSearchCommand), typeof(MainWindow));
    public static readonly RoutedUICommand CancelOrClearCommand = new("CancelOrClear", nameof(CancelOrClearCommand), typeof(MainWindow));

    public MainWindow()
    {
        InitializeComponent();
        CommandBindings.Add(new CommandBinding(FocusSearchCommand, OnFocusSearch, OnAlwaysCanExecute));
        CommandBindings.Add(new CommandBinding(CancelOrClearCommand, OnCancelOrClear, OnAlwaysCanExecute));
        _themeChangedHandler = (_, _) => UpdateThemeButton();
        ThemeManager.ThemeChanged += _themeChangedHandler;
        UpdateThemeButton();
    }

    protected override void OnClosed(System.EventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.PersistState();
        }
        ThemeManager.ThemeChanged -= _themeChangedHandler;
        base.OnClosed(e);
    }

    private void OnAlwaysCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
        e.Handled = true;
    }

    private void OnFocusSearch(object sender, ExecutedRoutedEventArgs e)
    {
        SearchTextBox.Focus();
        SearchTextBox.SelectAll();
    }

    private void OnCancelOrClear(object sender, ExecutedRoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(vm.SearchText))
        {
            vm.SearchText = string.Empty;
        }
        else
        {
            vm.CancelCommand.Execute(null);
        }
    }

    private void UpdateThemeButton()
    {
        if (ThemeButton is null)
        {
            return;
        }

        ThemeButton.Content = ThemeManager.CurrentTheme == AppTheme.Light ? "Тёмная тема" : "Светлая тема";
    }
}
