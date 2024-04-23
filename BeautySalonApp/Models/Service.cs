using BeautySalonApp.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeautySalonApp.Models
{
    public class Service
    {
        public long Id { get; set; }
        public string ServiceName { get; set; }
        public int DurationMin { get; set; }
        public double Price {  get; set; }
        public string Description { get; set;}
        public Employee Employee { get; set; }
        public long EmployeeId { get; set; }
        public UserApp UserApp { get; set; }
        public string UserId { get; set; }
        public Salon? Salon { get; set; }
        public long? SalonId { get; set; }

    }
}
