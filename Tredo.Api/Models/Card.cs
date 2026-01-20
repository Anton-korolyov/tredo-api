namespace Tredo.Api.Models
{
    public class Card
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = "";
        public decimal Price { get; set; }
        public int CityId { get; set; }
        public City City { get; set; }
        public string Image { get; set; } = "";
        public string Description { get; set; } = "";
        public string Phone { get; set; } = "";

        // 🔐 ВАЖНО
        public Guid OwnerId { get; set; }

        // ✅ категория
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
