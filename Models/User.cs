namespace VistaTiBooks.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;

        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
