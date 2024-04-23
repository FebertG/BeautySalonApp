namespace BeautySalonApp.Models
{
    public class EmployeesWorkingDate
    {
        public long Id { get; set; }
        public DayOfWeek WorkDay { get; set; }
        public TimeSpan StartOfWork {  get; set; }
        public TimeSpan EndOfWork { get; set; }
        public long EmployeeId { get; set; }
    }


}
