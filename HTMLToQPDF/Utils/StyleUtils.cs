using HtmlAgilityPack;

namespace HTMLQuestPDF.Utils
{
    internal static class StyleUtils
    {
        /// <summary>
        /// Parses the inline style attribute and returns a dictionary of CSS properties
        /// </summary>
        public static Dictionary<string, string> ParseInlineStyles(HtmlNode node)
        {
            var styles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var styleAttr = node.GetAttributeValue("style", string.Empty);

            if (string.IsNullOrWhiteSpace(styleAttr))
                return styles;

            // Split by semicolon to get individual style declarations
            var declarations = styleAttr.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var declaration in declarations)
            {
                var parts = declaration.Split(':', 2, StringSplitOptions.TrimEntries);
                if (parts.Length == 2)
                {
                    var property = parts[0].Trim();
                    var value = parts[1].Trim();
                    styles[property] = value;
                }
            }

            return styles;
        }

        /// <summary>
        /// Gets the text-align value from inline styles, or null if not specified
        /// </summary>
        public static string? GetTextAlign(HtmlNode node)
        {
            var styles = ParseInlineStyles(node);
            return styles.TryGetValue("text-align", out var value) ? value : null;
        }
    }
}