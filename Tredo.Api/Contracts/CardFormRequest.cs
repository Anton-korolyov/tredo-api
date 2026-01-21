using System.ComponentModel.DataAnnotations;
using Tredo.Api.Models;

namespace Tredo.Api.Contracts
{
    public class CardFormRequest
    {
        public string Title { get; set; } = "";
        public decimal Price { get; set; }
        public int CityId { get; set; }
   
        public string Phone { get; set; } = "";
        public string Description { get; set; } = "";
        [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        public IFormFile? Image { get; set; }
    }
}
