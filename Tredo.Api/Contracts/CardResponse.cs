namespace Tredo.Api.Contracts
{
    public class CardResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public decimal Price { get; set; }
        public int CityId { get; set; }
        public string Image { get; set; } = "";
        public string Phone { get; set; } = "";
        public int CategoryId { get; set; }
        public string Description { get; set; } = "";
        public Guid OwnerId { get; set; }
    }
}
