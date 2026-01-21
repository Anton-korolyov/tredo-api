namespace Tredo.Api.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string NameHe { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string NameRu { get; set; } = "";

        public ICollection<Card> Cards { get; set; } = new List<Card>();
    }
}
