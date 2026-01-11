namespace VistaTiBooks.Api.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string ExternalId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Authors { get; set; } = null!;
        public int? FirstPublishYear { get; set; }
        public string? CoverUrl { get; set; }
    }
}
