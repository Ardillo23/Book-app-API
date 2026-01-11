using System.Net; // ✅ NUEVO
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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

            var url =
                $"/search.json?q={Uri.EscapeDataString(query)}" +
                $"&limit={limit}" +
                $"&fields=key,title,author_name,first_publish_year,cover_i";

            var data = await _http.GetFromJsonAsync<OpenLibrarySearchResponse>(url);

            if (data?.Docs == null)
                return new List<BookResultDto>();

            // ✅ NUEVO: limitar cuántas llamadas simultáneas haces a GetDescriptionAsync
            using var sem = new SemaphoreSlim(3); // prueba 2-5 según rendimiento

            var results = await Task.WhenAll(
                data.Docs
                    .Where(d => !string.IsNullOrWhiteSpace(d.Key))
                    .Select(async d =>
                    {
                        var workKey = NormalizeWorkKey(d.Key!); // "/works/OL123W"

                        // ✅ NUEVO: GetDescriptionAsync protegido por semaphore
                        string? description;
                        await sem.WaitAsync();
                        try
                        {
                            description = await GetDescriptionAsync(workKey);
                        }
                        finally
                        {
                            sem.Release();
                        }

                        return new BookResultDto
                        {
                            ExternalId = workKey,
                            Title = d.Title ?? "(Sin título)",
                            Authors = (d.AuthorName != null && d.AuthorName.Count > 0)
                                ? string.Join(", ", d.AuthorName)
                                : "Desconocido",
                            FirstPublishYear = d.FirstPublishYear,
                            CoverUrl = d.CoverI.HasValue
                                ? $"https://covers.openlibrary.org/b/id/{d.CoverI.Value}-M.jpg"
                                : null,
                            Description = description
                        };
                    })
            );

            return results.ToList();
        }

        private static string NormalizeWorkKey(string key)
        {
            if (key.StartsWith("/")) return key;
            if (key.StartsWith("OL") && key.EndsWith("W")) return "/works/" + key;
            return key;
        }

        // =========================================================
        //  WORK RESPONSE
        // =========================================================
        private class OpenLibraryWorkResponse
        {
            [JsonPropertyName("description")]
            public JsonElement? Description { get; set; }
        }

        // =========================================================
        //  GET DESCRIPTION
        // =========================================================
        private async Task<string?> GetDescriptionAsync(string workKey)
        {
            var url = $"{workKey}.json";

            // ✅ NUEVO: evitar que reviente por 503/429 (GetFromJsonAsync lanza excepción)
            HttpResponseMessage resp;
            try
            {
                resp = await _http.GetAsync(url);
            }
            catch
            {
                return null;
            }

            // ✅ NUEVO: si OpenLibrary responde 503/429/etc, no reventar
            if (!resp.IsSuccessStatusCode)
            {
                // ✅ NUEVO: retry suave para 503 o 429 (opcional pero útil)
                if (resp.StatusCode == HttpStatusCode.ServiceUnavailable || (int)resp.StatusCode == 429)
                {
                    await Task.Delay(300);
                    try
                    {
                        resp = await _http.GetAsync(url);
                        if (!resp.IsSuccessStatusCode) return null;
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            OpenLibraryWorkResponse? data;
            try
            {
                data = await resp.Content.ReadFromJsonAsync<OpenLibraryWorkResponse>();
            }
            catch
            {
                return null;
            }

            if (data?.Description is null) return null;

            var desc = data.Description.Value;
            string? result = null;

            // Caso 1: "description": "texto..."
            if (desc.ValueKind == JsonValueKind.String)
                result = desc.GetString();

            // Caso 2: "description": { "value": "texto..." }
            if (desc.ValueKind == JsonValueKind.Object &&
                desc.TryGetProperty("value", out var valueProp) &&
                valueProp.ValueKind == JsonValueKind.String)
                result = valueProp.GetString();

            return CleanDescription(result);
        }

        // =========================================================
        //  CLEAN DESCRIPTION  ✅ AQUÍ ESTÁ LA PARTE NUEVA
        // =========================================================
        private static string? CleanDescription(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            // Normalizar saltos de línea
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            // Quitar espacios repetidos
            text = Regex.Replace(text, @"[ \t]+", " ");

            // Reducir saltos múltiples (máx 2)
            text = Regex.Replace(text, @"\n{3,}", "\n\n");

            // Quitar basura típica
            text = text.Replace("[source]", "");

            return text.Trim();
        }

        // =========================================================
        //  SEARCH RESPONSE
        // =========================================================
        private class OpenLibrarySearchResponse
        {
            [JsonPropertyName("docs")]
            public List<Doc>? Docs { get; set; }
        }

        private class Doc
        {
            [JsonPropertyName("key")]
            public string? Key { get; set; }

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
