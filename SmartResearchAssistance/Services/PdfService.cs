using UglyToad.PdfPig;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace SmartResearchAssistance.Services
{
    public class PdfService
    {
        public (string Text, Dictionary<string, string> Metadata) ExtractContent(string filePath)
        {
            using var document = PdfDocument.Open(filePath);
            var text = string.Join("\n", document.GetPages().Select(p => p.Text));

            var metadata = new Dictionary<string, string>
            {
                ["Title"] = document.Information.Title ?? "Untitled",
                ["Author"] = document.Information.Author ?? "Unknown Author",
                ["Date"] = document.Information.GetCreatedDateTimeOffset()?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "Unknown"
            };

            return (text, metadata);
        }

        public Dictionary<string, int> AnalyzeWordFrequency(string text)
        {
            // Define a basic list of stopwords
            var stopwords = new HashSet<string> {"the", "and", "of", "to", "in", "a", "is","year", "arxiv", "ieee", "transactions", "journal", "volume", "issue", "page"};

            var wordFrequencies = Regex.Split(text.ToLowerInvariant(), @"\W+")
                .Where(w => !string.IsNullOrEmpty(w) && w.Length > 2 && !stopwords.Contains(w) && !IsYearOrNumber(w))
                .GroupBy(w => w)
                .ToDictionary(g => g.Key, g => g.Count());

            // Limit to top 100 words based on frequency
            return wordFrequencies.OrderByDescending(kv => kv.Value)
                                  .Take(100)
                                  .ToDictionary(kv => kv.Key, kv => kv.Value);


        }
        private bool IsYearOrNumber(string word)
        {
            return word.All(char.IsDigit);  // This will check if the word consists only of digits (i.e., numbers)
        }
        public List<string> ExtractKeywords(Dictionary<string, int> frequencies, int topN = 10)
        {
            return frequencies.OrderByDescending(kv => kv.Value)
                              .Take(topN)
                              .Select(kv => kv.Key)
                              .ToList();
        }

        // New: Returns a JSON string representing word cloud data
        public string GetWordCloudJson(Dictionary<string, int> frequencies)
        {
            // Convert frequency data into a list of objects with 'text' and 'size' properties.
            var wordCloudData = frequencies.Select(kv => new { text = kv.Key, size = kv.Value }).ToList();
            return JsonSerializer.Serialize(wordCloudData);
        }
    }
}
