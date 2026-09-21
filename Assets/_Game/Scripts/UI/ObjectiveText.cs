using System.Collections.Generic;

namespace Shift.Game
{
    // Locale supplied by a future language selector; no gameplay or level-number coupling.
    public static class ObjectiveText
    {
        public static string Format(IReadOnlyList<PieceColor> colors, string language = "en")
        {
            bool tr = language != null && language.StartsWith("tr", System.StringComparison.OrdinalIgnoreCase);
            if (colors.Count > 1) return tr ? (colors.Count == 2 ? "İkisini temizle" : "Hepsini temizle")
                : (colors.Count == 2 ? "Clear Both" : "Clear All");
            string color = colors[0].ToString();
            if (tr) color = colors[0] switch { PieceColor.Red => "Kırmızıyı", PieceColor.Blue => "Maviyi",
                PieceColor.Yellow => "Sarıyı", PieceColor.Green => "Yeşili", _ => color };
            return tr ? color + " temizle" : "Clear " + color;
        }
    }
}
