using BeautySalonApp.Areas.Identity.Data;

namespace BeautySalonApp.Models
{
    public class Warehouse
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public UserApp UserApp { get; set; } 
        public List<Product> Products { get; set; } = new List<Product>();

    }
}
