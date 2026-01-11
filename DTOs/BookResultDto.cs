namespace VistaTiBooks.Api.DTOs
{
    public class BookResultDto
    {
        public string ExternalId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Authors { get; set; } = default!;
        public int? FirstPublishYear { get; set; }
        public string? CoverUrl { get; set; }
        public string? Description { get; set; }

    }
}
