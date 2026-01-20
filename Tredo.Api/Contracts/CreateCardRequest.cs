namespace Tredo.Api.Contracts
{
    public class CreateCardRequest
    {
        public string Title { get; set; } = "";
        public decimal Price { get; set; }
        public string City { get; set; } = "";
        public int CityId { get; set; }
        public string Description { get; set; } = "";
        public string Phone { get; set; } = "";
        public int CategoryId { get; set; }
        public IFormFile? Image { get; set; }
    }
}
