namespace VistaTiBooks.Api.DTOs
{
    public class CreateFavoriteDto
    {
        public string ExternalId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Authors { get; set; } = null!;
        public int? FirstPublishYear { get; set; }
        public string? CoverUrl { get; set; }
    }
}
