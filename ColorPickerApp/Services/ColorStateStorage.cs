using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Media;

namespace ColorPickerApp.Services;

public class ColorStateStorage
{
    private const string FileName = "color-state.json";
    private readonly string _filePath;

    public ColorStateStorage()
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
    }

    public PersistedState Load()
    {
        if (!File.Exists(_filePath))
        {
            return new PersistedState();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<PersistedState>(json);
            return data ?? new PersistedState();
        }
        catch
        {
            return new PersistedState();
        }
    }

    public void Save(PersistedState state)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(state, options);
        File.WriteAllText(_filePath, json);
    }
}

public class PersistedState
{
    public string? LastAppliedHex { get; set; }
    public List<string> RecentHexColors { get; set; } = new();

    public Color? LastAppliedColor => TryParse(LastAppliedHex);

    public IEnumerable<Color> RecentColors
    {
        get
        {
            foreach (var hex in RecentHexColors)
            {
                var color = TryParse(hex);
                if (color.HasValue)
                {
                    yield return color.Value;
                }
            }
        }
    }

    private static Color? TryParse(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex) || hex.Length != 7 || hex[0] != '#')
        {
            return null;
        }

        try
        {
            byte r = Convert.ToByte(hex.Substring(1, 2), 16);
            byte g = Convert.ToByte(hex.Substring(3, 2), 16);
            byte b = Convert.ToByte(hex.Substring(5, 2), 16);
            return Color.FromRgb(r, g, b);
        }
        catch
        {
            return null;
        }
    }
}
