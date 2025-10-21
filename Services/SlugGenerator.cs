using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ASM_1.Services
{
    public class SlugGenerator
    {
        private readonly HashSet<string> _stopWords;
        private readonly int _maxWords;

        public SlugGenerator(IConfiguration config)
        {
            var stopWords = config.GetSection("SlugSettings:StopWords").Get<string[]>() ?? Array.Empty<string>();
            _stopWords = new HashSet<string>(stopWords, StringComparer.OrdinalIgnoreCase);

            _maxWords = config.GetValue<int>("SlugSettings:MaxWords", 8);
        }

        //public string GenerateSlug(string title, string description = "")
        //{
        //    if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(description))
        //        return string.Empty;

        //    // 1. Xử lý tiêu đề
        //    string baseText = title?.Trim() ?? "";

        //    // 2. Nếu tiêu đề mơ hồ → bổ sung keyword từ mô tả
        //    if (IsTitleAmbiguous(baseText) && !string.IsNullOrWhiteSpace(description))
        //    {
        //        var keywordsFromDesc = ExtractKeywords(description);
        //        baseText += " " + string.Join(" ", keywordsFromDesc);
        //    }

        //    // 3. Chuyển sang slug
        //    return ConvertToSlug(baseText);
        //}

        //private string ConvertToSlug(string text)
        //{
        //    // 1. Bỏ dấu
        //    string str = RemoveDiacritics(text.ToLowerInvariant());

        //    // 2. Xóa ký tự đặc biệt
        //    str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

        //    // 3. Chia từ
        //    var words = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        //    // 4. Loại bỏ stop words (nếu bỏ không mất nghĩa)
        //    var filteredWords = new List<string>();
        //    foreach (var word in words)
        //    {
        //        if (!_stopWords.Contains(word) || IsMeaningfulStopWord(word, words))
        //            filteredWords.Add(word);
        //    }

        //    // 5. Giới hạn số từ
        //    filteredWords = filteredWords.Take(_maxWords).ToList();

        //    // 6. Ghép thành slug
        //    str = string.Join("-", filteredWords);

        //    // 7. Xóa dấu "-" thừa
        //    str = Regex.Replace(str, "-{2,}", "-").Trim('-');

        //    return str;
        //}

        public string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // 1. Bỏ dấu
            string str = RemoveDiacritics(input.ToLowerInvariant());

            // 2. Xóa ký tự đặc biệt
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            // 3. Chia từ
            var words = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // 4. Loại bỏ stop words (nếu bỏ không mất nghĩa)
            var filteredWords = new List<string>();
            foreach (var word in words)
            {
                //if (!_stopWords.Contains(word) || IsMeaningfulStopWord(word, words))
                    filteredWords.Add(word);
            }

            // 5. Giới hạn số từ
            filteredWords = filteredWords.Take(_maxWords).ToList();

            // 6. Ghép thành slug
            str = string.Join("-", filteredWords);

            // 7. Xóa dấu "-" thừa
            str = Regex.Replace(str, "-{2,}", "-").Trim('-');

            return str;
        }

        private bool IsMeaningfulStopWord(string word, string[] allWords)
        {
            // Nếu stop word đứng giữa hai danh từ, giữ lại
            // Ở đây mình đơn giản hóa, thực tế có thể dùng NLP để phân loại từ
            return true; // Giữ lại nếu cần
        }

        //private bool IsTitleAmbiguous(string title)
        //{
        //    if (string.IsNullOrWhiteSpace(title)) return true;

        //    // Mơ hồ nếu tiêu đề < 3 từ hoặc toàn stop words
        //    var words = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        //    return words.Length < 3 || words.All(w => _stopWords.Contains(w.ToLowerInvariant()));
        //}

        //private IEnumerable<string> ExtractKeywords(string text)
        //{
        //    string cleaned = RemoveDiacritics(text.ToLowerInvariant());
        //    cleaned = Regex.Replace(cleaned, @"[^a-z0-9\s-]", "");
        //    var words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        //                       .Where(w => !_stopWords.Contains(w))
        //                       .Distinct();
        //    return words.Take(_maxWords);
        //}

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
