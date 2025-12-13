using RBHU_DbServices.Models;
using RBHU_DbServices.ViewModel;


namespace RBHU_DbServices.Interface
{
    public interface IProductService
    {
        // Product operations
        bool SaveProductToDb(Product product);
        List<Product> GetAllProducts();
        Product GetProductById(int id);
        bool UpdateProduct(Product product);
        bool DeleteProduct(int id);
        List<Product> SearchProducts(string searchTerm);
        List<Product> GetProductsWithCategories();

        // Category operations
        List<Product> GetProductsByCategory(int categoryId);
        List<Category> GetAllCategories();
        Category GetCategoryById(int id);
        bool SaveCategoryToDb(Category category);

        // SubCategory1 operations
        List<Product> GetProductsBySubCategory1(int subCategory1Id);
        List<SubCategory1> GetAllSubCategories1();
        List<SubCategory1> GetSubCategories1ByCategory(int categoryId);
        SubCategory1 GetSubCategory1ById(int id);
        SubCategory1 GetSubCategory1ByName(string name); // NEW

        // SubCategory2 operations
        List<Product> GetProductsBySubCategory2(int subCategory2Id);
        List<SubCategory2> GetAllSubCategories2();
        List<SubCategory2> GetSubCategories2BySubCategory1(int subCategory1Id);
        SubCategory2 GetSubCategory2ById(int id);
        List<Product> GetSpecialOffers();

        // Email operations
        bool MarkASRead(int id);

        List<dynamic> GetSampleProductsForSubCategories2(int subCategory1Id);
        public List<CategoryViewModel> GetAllCategoriesForHome();
    }
}