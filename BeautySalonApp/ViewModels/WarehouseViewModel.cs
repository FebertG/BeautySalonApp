using BeautySalonApp.Models;

namespace BeautySalonApp.ViewModels
{
    public class WarehouseViewModel
    {
        public Warehouse Warehouse { get; set; }
        public Product NewProduct { get; set; } = new Product();
        public long ProductIdToModify { get; set; }
        public int ModifyAmount { get; set; }
    }
}
