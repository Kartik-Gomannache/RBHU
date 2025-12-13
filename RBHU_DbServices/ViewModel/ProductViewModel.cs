using RBHU_DbServices.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RBHU_DbServices.ViewModel
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OfferPrice { get; set; }
        public int CategoryId { get; set; }
        public int SubCategory1Id { get; set; }
        public int SubCategory2Id { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public IFormFile ImageFile { get; set; }
        public string ImageData { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<SubCategory1> SubCategories1 { get; set; } = new List<SubCategory1>();
        public List<SubCategory2> SubCategories2 { get; set; } = new List<SubCategory2>();
    }

    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<SubCategory1> SubCategories1 { get; set; } = new List<SubCategory1>();
        public List<SubCategory2> SubCategories2 { get; set; } = new List<SubCategory2>();
        public List<Email> Emails { get; set; } = new List<Email>();
        public int SelectedCategoryId { get; set; }
        public int SelectedSubCategory1Id { get; set; }
        public int SelectedSubCategory2Id { get; set; }
        public string SearchTerm { get; set; }
    }
}