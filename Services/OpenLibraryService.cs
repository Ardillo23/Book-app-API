using System.Net.Http.Json;
using System.Text.Json.Serialization;
using VistaTiBooks.Api.DTOs;

namespace VistaTiBooks.Api.Services
{
    public class OpenLibraryService
    {
        private readonly HttpClient _http;

        public OpenLibraryService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BookResultDto>> SearchAsync(string query, int limit = 10)
        {
            query = query.Trim();

            // (Opcional pero recomendado) Pedimos solo los campos que necesitamos
            // La doc permite usar "fields" en /search.json :contentReference[oaicite:1]{index=1}
            var url =
                $"/search.json?q={Uri.EscapeDataString(query)}" +
                $"&limit={limit}" +
                $"&fields=key,title,author_name,first_publish_year,cover_i";

            var data = await _http.GetFromJsonAsync<OpenLibrarySearchResponse>(url);

            if (data?.Docs == null)
                return new List<BookResultDto>();

            return data.Docs
                .Where(d => !string.IsNullOrWhiteSpace(d.Key))
                .Select(d => new BookResultDto
                {
                    ExternalId = NormalizeWorkKey(d.Key!), // "/works/OL123W"
                    Title = d.Title ?? "(Sin título)",
                    Authors = (d.AuthorName != null && d.AuthorName.Count > 0)
                        ? string.Join(", ", d.AuthorName)
                        : "Desconocido",
                    FirstPublishYear = d.FirstPublishYear,
                    CoverUrl = d.CoverI.HasValue
                        ? $"https://covers.openlibrary.org/b/id/{d.CoverI.Value}-M.jpg"
                        : null
                })
                .ToList();
        }

        private static string NormalizeWorkKey(string key)
        {
            // En la doc a veces aparece "OL27448W" o "/works/OL166894W" :contentReference[oaicite:2]{index=2}
            // Normalizamos para que siempre te quede con "/works/..."
            if (key.StartsWith("/")) return key;
            if (key.StartsWith("OL") && key.EndsWith("W")) return "/works/" + key;
            return key;
        }

        private class OpenLibrarySearchResponse
        {
            [JsonPropertyName("docs")]
            public List<Doc>? Docs { get; set; }
        }

        private class Doc
        {
            [JsonPropertyName("key")]
            public string? Key { get; set; } // "OL27448W" o "/works/OLxxxW"

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("author_name")]
            public List<string>? AuthorName { get; set; }

            [JsonPropertyName("first_publish_year")]
            public int? FirstPublishYear { get; set; }

            [JsonPropertyName("cover_i")]
            public int? CoverI { get; set; }
        }
    }
}
