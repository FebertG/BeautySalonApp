using BeautySalonApp.Areas.Identity.Data;
using BeautySalonApp.Models;

namespace BeautySalonApp.ViewModels
{
    public class SalonViewModel
    {
        public Salon Salon { get; set; }
        public Address Address { get; set; }   
        public UserApp UserApp { get; set; }
        public string UserId { get; set; }
        public List<Opinion> Opinions { get; set; } = new List<Opinion>();
        public Opinion Opinion { get; set; }
        public List<Service> Services { get; set; }
        public double AverageRating { get; set; }
    }
}
