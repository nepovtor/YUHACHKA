using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ColorPickerApp.Commands;
using ColorPickerApp.Models;
using ColorPickerApp.Services;

namespace ColorPickerApp.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly ColorStateStorage _storage = new();
    private readonly PersistedState _state;
    private string _searchText = string.Empty;
    private ColorInfo? _selectedColor;
    private SolidColorBrush _previewBrush = new(Colors.White);
    private SolidColorBrush _appliedBrush = new(Colors.White);
    private SolidColorBrush _previewForeground = new(Colors.Black);
    private SolidColorBrush _appliedForeground = new(Colors.Black);
    private string _previewHex = "#FFFFFF";
    private string _appliedHex = "#FFFFFF";

    public MainViewModel()
    {
        Colors = new ObservableCollection<ColorInfo>(LoadColors());
        ColorsView = CollectionViewSource.GetDefaultView(Colors);
        ColorsView.Filter = FilterColor;
        RecentColors = new ObservableCollection<ColorInfo>();
        ApplyCommand = new RelayCommand(Apply, () => SelectedColor != null);
        CancelCommand = new RelayCommand(Cancel);
        CopyHexCommand = new RelayCommand(CopyHex, () => SelectedColor != null);
        ToggleThemeCommand = new RelayCommand(ToggleTheme);
        SelectRecentCommand = new RelayCommand(param =>
        {
            if (param is ColorInfo color)
            {
                SelectRecent(color);
            }
        });

        _state = _storage.Load();
        InitializeState();
    }

    public ObservableCollection<ColorInfo> Colors { get; }

    public ICollectionView ColorsView { get; }

    public ObservableCollection<ColorInfo> RecentColors { get; }

    public ICommand ApplyCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand CopyHexCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand SelectRecentCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ColorsView.Refresh();
            }
        }
    }

    public ColorInfo? SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (SetProperty(ref _selectedColor, value))
            {
                UpdatePreview();
                (ApplyCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CopyHexCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public SolidColorBrush PreviewBrush
    {
        get => _previewBrush;
        private set => SetProperty(ref _previewBrush, value);
    }

    public SolidColorBrush AppliedBrush
    {
        get => _appliedBrush;
        private set
        {
            if (SetProperty(ref _appliedBrush, value))
            {
                OnPropertyChanged(nameof(WindowForegroundBrush));
            }
        }
    }

    public SolidColorBrush PreviewForeground
    {
        get => _previewForeground;
        private set => SetProperty(ref _previewForeground, value);
    }

    public SolidColorBrush AppliedForeground
    {
        get => _appliedForeground;
        private set
        {
            if (SetProperty(ref _appliedForeground, value))
            {
                OnPropertyChanged(nameof(WindowForegroundBrush));
            }
        }
    }

    public Brush WindowForegroundBrush => AppliedForeground;

    public string PreviewHex
    {
        get => _previewHex;
        private set => SetProperty(ref _previewHex, value);
    }

    public string AppliedHex
    {
        get => _appliedHex;
        private set => SetProperty(ref _appliedHex, value);
    }

    public void SelectRecent(ColorInfo color)
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            SearchText = string.Empty;
        }
        SelectedColor = Colors.FirstOrDefault(c => c.Hex == color.Hex) ?? color;
    }

    public void PersistState()
    {
        SaveState();
    }

    private void InitializeState()
    {
        if (_state.LastAppliedColor.HasValue)
        {
            var color = _state.LastAppliedColor.Value;
            AppliedBrush = new SolidColorBrush(color);
            AppliedHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            AppliedForeground = new SolidColorBrush(GetContrastColor(color));
        }
        else
        {
            AppliedBrush = new SolidColorBrush(Colors.White);
            AppliedForeground = new SolidColorBrush(GetContrastColor(Colors.White));
            AppliedHex = "#FFFFFF";
        }

        RecentColors.Clear();
        foreach (var color in _state.RecentColors.Take(5))
        {
            RecentColors.Add(new ColorInfo($"#{color.R:X2}{color.G:X2}{color.B:X2}", color));
        }

        SelectedColor = Colors.FirstOrDefault(c => c.Hex == AppliedHex);
        UpdatePreview();
    }

    private IEnumerable<ColorInfo> LoadColors()
    {
        var props = typeof(Colors).GetProperties()
            .Where(p => p.PropertyType == typeof(Color))
            .Select(p => new ColorInfo(p.Name, (Color)p.GetValue(null)!))
            .OrderBy(c => c.Name)
            .ToList();
        return props;
    }

    private bool FilterColor(object obj)
    {
        if (obj is not ColorInfo color)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        return color.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
               color.Hex.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    private void UpdatePreview()
    {
        var color = SelectedColor?.Color ?? AppliedBrush.Color;
        PreviewBrush = new SolidColorBrush(color);
        PreviewForeground = new SolidColorBrush(GetContrastColor(color));
        PreviewHex = SelectedColor?.Hex ?? AppliedHex;
    }

    private void Apply()
    {
        if (SelectedColor is null)
        {
            return;
        }

        var color = SelectedColor.Color;
        AppliedBrush = new SolidColorBrush(color);
        AppliedForeground = new SolidColorBrush(GetContrastColor(color));
        AppliedHex = SelectedColor.Hex;
        PreviewHex = AppliedHex;

        AddToRecent(SelectedColor);
        SaveState();
    }

    private void Cancel()
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            SearchText = string.Empty;
        }
        SelectedColor = Colors.FirstOrDefault(c => c.Hex == AppliedHex);
        UpdatePreview();
    }

    private void CopyHex()
    {
        if (SelectedColor is null)
        {
            return;
        }

        System.Windows.Clipboard.SetText(SelectedColor.Hex);
    }

    private void ToggleTheme()
    {
        var nextTheme = ThemeManager.CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
        ThemeManager.ApplyTheme(nextTheme);
    }

    private void AddToRecent(ColorInfo color)
    {
        if (RecentColors.FirstOrDefault(c => c.Hex == color.Hex) is { } existing)
        {
            RecentColors.Remove(existing);
        }

        RecentColors.Insert(0, color);

        while (RecentColors.Count > 5)
        {
            RecentColors.RemoveAt(RecentColors.Count - 1);
        }
    }

    private void SaveState()
    {
        var state = new PersistedState
        {
            LastAppliedHex = AppliedHex,
            RecentHexColors = RecentColors.Select(c => c.Hex).ToList()
        };
        _storage.Save(state);
    }

    private static Color GetContrastColor(Color color)
    {
        var brightness = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
        return brightness > 0.6 ? Colors.Black : Colors.White;
    }
}
