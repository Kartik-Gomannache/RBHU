using RBHU_DbServices.Models;

namespace RBHU_DbServices.ViewModel
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<SubCategory1> SubCategories1 { get; set; } = new List<SubCategory1>();
        public Dictionary<int, List<SubCategory2>> SubCategories2Map { get; set; } = new Dictionary<int, List<SubCategory2>>();
        public List<Product> Products { get; set; } = new List<Product>();
        public int? SelectedSubCategory1Id { get; set; }
        public int? SelectedSubCategory2Id { get; set; }
    }
}