using RBHU_DbServices.Interface;
using RBHU_DbServices.Models;
using RBHU_DbServices.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace RBHU_DbServices.Implementation
{
    public class ProductService : IProductService
    {
        private readonly DbContextOptions<RBHUContext> _dbConnection;

        public ProductService(string connectionString)
        {
            _dbConnection = new DbContextOptionsBuilder<RBHUContext>()
                .UseSqlServer(connectionString)
                .Options;
        }

        // Product operations
        public bool SaveProductToDb(Product product)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    product.CreatedAt = DateTime.Now;
                    db.Products.Add(product);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving product: {ex.Message}");
                return false;
            }
        }

        public List<Product> GetAllProducts()
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.Products
                        .Include(p => p.Category)
                        .Include(p => p.SubCategory1)
                        .Include(p => p.SubCategory2)
                        .OrderByDescending(p => p.CreatedAt)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving products: {ex.Message}");
                return new List<Product>();
            }
        }

        public Product GetProductById(int id)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.Products
                        .Include(p => p.Category)
                        .Include(p => p.SubCategory1)
                        .Include(p => p.SubCategory2)
                        .FirstOrDefault(p => p.Id == id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving product: {ex.Message}");
                return null;
            }
        }

        public bool UpdateProduct(Product product)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    var existingProduct = db.Products.Find(product.Id);
                    if (existingProduct != null)
                    {
                        existingProduct.Name = product.Name;
                        existingProduct.Description = product.Description;
                        existingProduct.Price = product.Price;
                        existingProduct.OfferPrice = product.OfferPrice;
                        existingProduct.CategoryId = product.CategoryId;
                        existingProduct.SubCategory1_Id = product.SubCategory1_Id;
                        existingProduct.SubCategory2_Id = product.SubCategory2_Id;

                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            existingProduct.ImageUrl = product.ImageUrl;
                        }

                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex.Message}");
                return false;
            }
        }

        public bool DeleteProduct(int id)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    var product = db.Products.Find(id);
                    if (product != null)
                    {
                        db.Products.Remove(product);
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product: {ex.Message}");
                return false;
            }
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    if (string.IsNullOrWhiteSpace(searchTerm))
                        return GetAllProducts();

                    return db.Products
                        .Include(p => p.Category)
                        .Include(p => p.SubCategory1)
                        .Include(p => p.SubCategory2)
                        .Where(p => p.Name.Contains(searchTerm) ||
                               p.Description.Contains(searchTerm) ||
                               p.Category.Name.Contains(searchTerm) ||
                               (p.SubCategory1 != null && p.SubCategory1.Name.Contains(searchTerm)) ||
                               (p.SubCategory2 != null && p.SubCategory2.Name.Contains(searchTerm)))
                        .OrderByDescending(p => p.CreatedAt)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching products: {ex.Message}");
                return new List<Product>();
            }
        }

        public List<Product> GetProductsWithCategories()
        {
            return GetAllProducts();
        }

        // Category operations
        public List<Product> GetProductsByCategory(int categoryId)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.Products
                        .Include(p => p.Category)
                        .Include(p => p.SubCategory1)
                        .Include(p => p.SubCategory2)
                        .Where(p => p.CategoryId == categoryId)
                        .OrderByDescending(p => p.CreatedAt)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving products by category: {ex.Message}");
                return new List<Product>();
            }
        }

        public List<Category> GetAllCategories()
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.Categories
                        .OrderBy(c => c.Name)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving categories: {ex.Message}");
                return new List<Category>();
            }
        }

        public Category GetCategoryById(int id)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.Categories.Find(id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving category: {ex.Message}");
                return null;
            }
        }

        public bool SaveCategoryToDb(Category category)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    db.Categories.Add(category);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving category: {ex.Message}");
                return false;
            }
        }

        // SubCategory1 operations
        public List<Product> GetProductsBySubCategory1(int subCategory1Id)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.Products
                        .Include(p => p.Category)
                        .Include(p => p.SubCategory1)
                        .Include(p => p.SubCategory2)
                        .Where(p => p.SubCategory1_Id == subCategory1Id)
                        .OrderByDescending(p => p.CreatedAt)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving products by subcategory1: {ex.Message}");
                return new List<Product>();
            }
        }

        public List<SubCategory1> GetAllSubCategories1()
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.SubCategory1s
                        .Include(sc => sc.Category)
                        .OrderBy(sc => sc.Name)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving subcategories1: {ex.Message}");
                return new List<SubCategory1>();
            }
        }

        public List<SubCategory1> GetSubCategories1ByCategory(int categoryId)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.SubCategory1s
                        .Include(sc => sc.Category)
                        .Where(sc => sc.CategoryId == categoryId)
                        .OrderBy(sc => sc.Name)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving subcategories1 by category: {ex.Message}");
                return new List<SubCategory1>();
            }
        }

        public SubCategory1 GetSubCategory1ById(int id)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.SubCategory1s
                        .Include(sc => sc.Category)
                        .FirstOrDefault(sc => sc.Id == id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving subcategory1: {ex.Message}");
                return null;
            }
        }


        public SubCategory1 GetSubCategory1ByName(string name)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.SubCategory1s
                        .Include(sc => sc.Category)
                        .FirstOrDefault(sc => sc.Name.ToLower() == name.ToLower());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving subcategory1 by name: {ex.Message}");
                return null;
            }
        }

        // SubCategory2 operations
        public List<Product> GetProductsBySubCategory2(int subCategory2Id)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.Products
                        .Include(p => p.Category)
                        .Include(p => p.SubCategory1)
                        .Include(p => p.SubCategory2)
                        .Where(p => p.SubCategory2_Id == subCategory2Id)
                        .OrderByDescending(p => p.CreatedAt)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving products by subcategory2: {ex.Message}");
                return new List<Product>();
            }
        }

        public List<SubCategory2> GetAllSubCategories2()
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.SubCategory2s
                        .Include(sc => sc.SubCategory1)
                        .ThenInclude(sc1 => sc1.Category)
                        .OrderBy(sc => sc.Name)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving subcategories2: {ex.Message}");
                return new List<SubCategory2>();
            }
        }

        public List<SubCategory2> GetSubCategories2BySubCategory1(int subCategory1Id)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.SubCategory2s
                        .Include(sc => sc.SubCategory1)
                        .Where(sc => sc.SubCategory1Id == subCategory1Id)
                        .OrderBy(sc => sc.Name)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving subcategories2 by subcategory1: {ex.Message}");
                return new List<SubCategory2>();
            }
        }

        public List<dynamic> GetSampleProductsForSubCategories2(int subCategory1Id)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    var result = db.SubCategory2s
                        .Where(s => s.SubCategory1Id == subCategory1Id)
                        .Select(s => new
                        {
                            subCategory2Id = s.Id,
                            subCategory2Name = s.Name,
                            product = db.Products
                                .Where(p => p.SubCategory2_Id == s.Id)  // FIX: Use SubCategory2_Id, not SubCategory1_Id
                                .Select(p => new
                                {
                                    id = p.Id,
                                    name = p.Name,
                                    description = p.Description,
                                    imageUrl = p.ImageUrl
                                })
                                .FirstOrDefault()
                        })
                        .ToList()
                        .Cast<dynamic>()
                        .ToList();

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving sample products for subcategories2: {ex.Message}");
                return new List<dynamic>();
            }
        }


        public SubCategory2 GetSubCategory2ById(int id)
        {
            try
            {
                using (var db = new RBHUContext(_dbConnection))
                {
                    return db.SubCategory2s
                        .Include(sc => sc.SubCategory1)
                        .ThenInclude(sc1 => sc1.Category)
                        .FirstOrDefault(sc => sc.Id == id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving subcategory2: {ex.Message}");
                return null;
            }
        }

        // Email operations
        public bool MarkASRead(int id)
        {
            using (var db = new RBHUContext(_dbConnection))
            {
                var email = db.Emails.FirstOrDefault(e => e.Id == id);
                if (email == null) return false;

                email.Unread = false;
                email.UpdatedAt = DateTime.UtcNow;

                db.SaveChanges();
                return true;
            }
        }

        public List<Product> GetSpecialOffers()
        {
            var offers = new List<Product>();
            using (var db = new RBHUContext(_dbConnection))
            {
                offers = db.Products
                .Include(p => p.Category)
                .Include(p => p.SubCategory1)
                .Include(p => p.SubCategory2)
                .Where(p => p.OfferPrice > 0 && p.OfferPrice < p.Price)
                .OrderByDescending(p => ((p.Price - p.OfferPrice) / p.Price) * 100) // Sort by discount percentage
                .ToList();
                return offers;
            }
        }

        public List<CategoryViewModel> GetAllCategoriesForHome()
        {
            using (var db = new RBHUContext(_dbConnection))
            {
                var categories = db.Categories
           .Include(c => c.SubCategory1s)
               .ThenInclude(sc1 => sc1.SubCategory2s)
           .ToList();

                // Transform to CategoryViewModel
                var categoryViewModels = categories.Select(cat => new CategoryViewModel
                {
                    CategoryId = cat.Id,
                    CategoryName = cat.Name,
                    SubCategories1 = cat.SubCategory1s.ToList(),
                    SubCategories2Map = cat.SubCategory1s
                        .Where(sc1 => sc1.SubCategory2s != null && sc1.SubCategory2s.Any())
                        .ToDictionary(
                            sc1 => sc1.Id,
                            sc1 => sc1.SubCategory2s.ToList()
                        )
                }).ToList();
                return categoryViewModels;
            }
        }
    }
}