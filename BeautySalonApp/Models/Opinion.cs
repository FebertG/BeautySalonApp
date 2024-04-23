using BeautySalonApp.Areas.Identity.Data;

namespace BeautySalonApp.Models
{
    public class Opinion
    {
        public long Id { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; }
        public DateTime DateAdded { get; set; }
        public UserApp UserApp { get; set; }
        public string UserId { get; set; }
        public long SalonId { get; set; }
        public Salon Salon { get; set; }
    }
}
