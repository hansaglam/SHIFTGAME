using System.Collections.Generic;
namespace Shift.Game
{
    public static class ObjectiveText
    {
        public static string Format(IReadOnlyList<PieceColor> colors) => Format(colors, GameLanguageService.Shared.CurrentLanguage);
        public static string Format(IReadOnlyList<PieceColor> colors, string language) => Format(colors, language != null && language.StartsWith("tr", System.StringComparison.OrdinalIgnoreCase) ? GameLanguage.Turkish : GameLanguage.English);
        public static string Format(IReadOnlyList<PieceColor> colors, GameLanguage language) => LocalizationCatalog.Format(colors.Count > 1 ? colors.Count == 2 ? "objective.both" : "objective.all" : "objective." + colors[0], language);
    }
}
