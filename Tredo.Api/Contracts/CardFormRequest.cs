using System.ComponentModel.DataAnnotations;
using Tredo.Api.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace Tredo.Api.Contracts
{
    public class CardFormRequest
    {
        public string Title { get; set; } = "";
        public decimal Price { get; set; }
        public int CityId { get; set; }
   
        public string Phone { get; set; } = "";
        public string Description { get; set; } = "";
        public int CategoryId { get; set; }
        public IFormFile? Image { get; set; }
    }
}

