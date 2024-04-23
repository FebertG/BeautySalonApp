using BeautySalonApp.Models;

namespace BeautySalonApp.ViewModels
{
    public class ServiceViewModel
    {
        public Service Service { get; set; }
        public Employee Employee { get; set; }
        public Reservation Reservation { get; set; }
        public Salon Salon { get; set; }
    }
}
