using Shopping.Data.Entities;

namespace Shopping.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Product> Products { get; set; }

        public float Quantity { get; set; }
    }
}
