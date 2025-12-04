using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SharedKernel.Utilities
{
    public static class SlugGenerator
    {
        public static string Generate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            var cleanString = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            
            // Replace spaces with hyphens
            cleanString = Regex.Replace(cleanString, @"\s+", "-");
            
            // Remove invalid chars (keep only a-z, 0-9, -)
            cleanString = Regex.Replace(cleanString, @"[^a-z0-9\-]", "");
            
            // Remove multiple hyphens
            cleanString = Regex.Replace(cleanString, @"-+", "-");
            
            // Trim hyphens from start and end
            return cleanString.Trim('-');
        }
    }
}
