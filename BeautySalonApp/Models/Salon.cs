using BeautySalonApp.Areas.Identity.Data;

namespace BeautySalonApp.Models
{
    public class Salon
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Opinion> Opinions { get; set; } = new List<Opinion>();
        public UserApp UserApp { get; set; }
        public string UserId { get; set; }
        public Address Address { get; set; }
        public long AddressId { get; set; }
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public string PhoneNumber { get; set; }
        public List<Service> Services { get; set; } = new List<Service>();
    }
}
