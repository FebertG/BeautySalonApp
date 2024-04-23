using BeautySalonApp.Areas.Identity.Data;
using BeautySalonApp.Models;


namespace BeautySalonApp.Models
{
    public class Employee
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Description { get; set; }
        public List <EmployeesWorkingDate> WorkingDate { get; set; } = new List <EmployeesWorkingDate> ();
        public UserApp UserApp { get; set; }
        public string UserId { get; set; }
    }
}
