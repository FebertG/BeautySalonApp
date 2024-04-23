using BeautySalonApp.Areas.Identity.Data;

namespace BeautySalonApp.Models
{
    public class GiftCard
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public double Value { get; set; }
        public DateTime ExpirationDate { get; set; }
        public UserApp UserApp { get; set; }
        public string UserId { get; set; }
    }
}
