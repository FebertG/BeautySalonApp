namespace BeautySalonApp.Models
{
    public class Product
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Brand { get; set; }
        public long Amount { get; set; }
    }
}
