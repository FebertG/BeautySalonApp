using BeautySalonApp.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace BeautySalonApp.Models
{
    public class Reservation
    {
        public long Id { get; set; }
        public UserApp? UserApp { get; set; }
        public string? UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Service Service { get; set; }
        public long ServiceId { get; set; }
        public double Price {  get; set; }


    }
}
