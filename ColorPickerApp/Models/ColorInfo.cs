using System.Windows.Media;

namespace ColorPickerApp.Models;

public class ColorInfo
{
    public ColorInfo(string name, Color color)
    {
        Name = name;
        Color = color;
        Brush = new SolidColorBrush(color);
        if (Brush.CanFreeze)
        {
            Brush.Freeze();
        }
    }

    public string Name { get; }
    public Color Color { get; }
    public SolidColorBrush Brush { get; }
    public string Hex => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";
}
