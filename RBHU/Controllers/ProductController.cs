using RBHU_DbServices.Interface;
using RBHU_DbServices.Models;
using RBHU_DbServices.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace RBHU.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IEService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductService productService, IEService emailService, IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
        }

        // Product category pages
        public IActionResult Pneumatic(int? subCategory1Id = null, int? subCategory2Id = null)
        {
            var categoryName = "Pneumatic";
            var categories = _productService.GetAllCategories();
            var category = categories.FirstOrDefault(c => c.Name.Contains(categoryName, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                return View(new CategoryViewModel
                {
                    CategoryName = categoryName,
                    Products = new List<Product>(),
                    SubCategories1 = new List<SubCategory1>()
                });
            }

            var viewModel = new CategoryViewModel
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                SubCategories1 = _productService.GetSubCategories1ByCategory(category.Id),
                SubCategories2Map = GetSubCategoriesMapForCategory(category.Id),
                Products = subCategory2Id.HasValue ? _productService.GetProductsBySubCategory2(subCategory2Id.Value) :
                           subCategory1Id.HasValue ? _productService.GetProductsBySubCategory1(subCategory1Id.Value) :
                           _productService.GetProductsByCategory(category.Id),
                SelectedSubCategory1Id = subCategory1Id,
                SelectedSubCategory2Id = subCategory2Id
            };

            return View(viewModel);
        }

        //public IActionResult PowerTools()
        //{
        //    var products = GetProductsByCategoryName("Power Tools");
        //    return View(products);
        //}

        //public IActionResult CuttingTools()
        //{
        //    var products = GetProductsByCategoryName("Cutting Tools");
        //    return View(products);
        //}

        //public IActionResult Abrasives()
        //{
        //    var products = GetProductsByCategoryName("Abrasives");
        //    return View(products);
        //}

        // Helper method to get products by category name
        private List<Product> GetProductsByCategoryName(string categoryName)
        {
            var categories = _productService.GetAllCategories();
            var category = categories.FirstOrDefault(c => c.Name.Contains(categoryName, StringComparison.OrdinalIgnoreCase));

            if (category != null)
            {
                return _productService.GetProductsByCategory(category.Id);
            }

            return new List<Product>();
        }

        [Authorize(Roles = "Admin")]
        // GET: Product/Index - Display products with filtering
        public IActionResult Index(int categoryId = 0, int subCategory1Id = 0, int subCategory2Id = 0, string searchTerm = "")
        {
            var viewModel = new ProductListViewModel
            {
                Products = categoryId > 0 ? _productService.GetProductsByCategory(categoryId) :
                          subCategory1Id > 0 ? _productService.GetProductsBySubCategory1(subCategory1Id) :
                          subCategory2Id > 0 ? _productService.GetProductsBySubCategory2(subCategory2Id) :
                          !string.IsNullOrEmpty(searchTerm) ? _productService.SearchProducts(searchTerm) :
                          _productService.GetAllProducts(),
                Categories = _productService.GetAllCategories(),
                SubCategories1 = _productService.GetAllSubCategories1(),
                SubCategories2 = _productService.GetAllSubCategories2(),
                Emails = _emailService.GetEmailFromDb(),
                SelectedCategoryId = categoryId,
                SelectedSubCategory1Id = subCategory1Id,
                SelectedSubCategory2Id = subCategory2Id,
                SearchTerm = searchTerm
            };

            var productDtos = viewModel.Products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                description = p.Description,
                price = p.Price,
                offerPrice = p.OfferPrice,
                imageUrl = p.ImageUrl,
                createdAt = p.CreatedAt,
                category = new
                {
                    id = p.Category?.Id ?? 0,
                    name = p.Category?.Name ?? ""
                },
                subCategory1 = new
                {
                    id = p.SubCategory1?.Id ?? 0,
                    name = p.SubCategory1?.Name ?? ""
                },
                subCategory2 = new
                {
                    id = p.SubCategory2?.Id ?? 0,
                    name = p.SubCategory2?.Name ?? ""
                }
            }).ToList();

            ViewBag.ProductsJson = System.Text.Json.JsonSerializer.Serialize(productDtos);

            return View(viewModel);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            var viewModel = new ProductViewModel
            {
                Categories = _productService.GetAllCategories(),
                SubCategories1 = new List<SubCategory1>(),
                SubCategories2 = new List<SubCategory2>()
            };
            return View(viewModel);
        }

        // POST: Product/CreateProduct
        [HttpPost]
        public async Task<JsonResult> CreateProduct([FromBody] ProductViewModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name) || model.CategoryId <= 0 || model.Price <= 0)
                    return Json(new { success = false, message = "Please fill in all required fields." });

                if (model.OfferPrice.HasValue && model.OfferPrice.Value >= model.Price)
                    return Json(new { success = false, message = "Offer price must be less than regular price." });

                var product = new Product
                {
                    Name = model.Name.Trim(),
                    Description = model.Description?.Trim(),
                    Price = model.Price,
                    OfferPrice = model.OfferPrice ?? 0,
                    CategoryId = model.CategoryId,
                    SubCategory1_Id = model.SubCategory1Id > 0 ? model.SubCategory1Id : null,
                    SubCategory2_Id = model.SubCategory2Id > 0 ? model.SubCategory2Id : null,
                    CreatedAt = DateTime.Now
                };

                if (!string.IsNullOrEmpty(model.ImageData))
                {
                    if (model.ImageData.Length > 7 * 1024 * 1024)
                        return Json(new { success = false, message = "Image size should not exceed 5MB." });

                    if (IsValidBase64Image(model.ImageData))
                    {
                        product.ImageUrl = model.ImageData;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid image format." });
                    }
                }

                var result = _productService.SaveProductToDb(product);
                return Json(new { success = result, message = result ? "Product saved successfully!" : "Error saving product." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // POST: Product/UpdateProduct
        [HttpPost]
        public async Task<JsonResult> UpdateProduct([FromBody] ProductViewModel model)
        {
            try
            {
                if (model.Id <= 0 || string.IsNullOrWhiteSpace(model.Name) || model.CategoryId <= 0 || model.Price <= 0)
                    return Json(new { success = false, message = "Please fill in all required fields." });

                if (model.OfferPrice.HasValue && model.OfferPrice.Value >= model.Price)
                    return Json(new { success = false, message = "Offer price must be less than regular price." });

                var existingProduct = _productService.GetProductById(model.Id);
                if (existingProduct == null)
                    return Json(new { success = false, message = "Product not found." });

                var product = new Product
                {
                    Id = model.Id,
                    Name = model.Name.Trim(),
                    Description = model.Description?.Trim(),
                    Price = model.Price,
                    OfferPrice = model.OfferPrice ?? 0,
                    CategoryId = model.CategoryId,
                    SubCategory1_Id = model.SubCategory1Id > 0 ? model.SubCategory1Id : null,
                    SubCategory2_Id = model.SubCategory2Id > 0 ? model.SubCategory2Id : null,
                    CreatedAt = existingProduct.CreatedAt,
                    ImageUrl = existingProduct.ImageUrl
                };

                if (!string.IsNullOrEmpty(model.ImageData))
                {
                    if (model.ImageData.Length > 7 * 1024 * 1024)
                        return Json(new { success = false, message = "Image size should not exceed 5MB." });

                    if (IsValidBase64Image(model.ImageData))
                    {
                        product.ImageUrl = model.ImageData;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid image format." });
                    }
                }
                else if (string.IsNullOrEmpty(model.ImageUrl))
                {
                    product.ImageUrl = null;
                }

                var result = _productService.UpdateProduct(product);
                return Json(new { success = result, message = result ? "Product updated successfully!" : "Error updating product." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // GET: Product/Edit/5
        public IActionResult Edit(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                OfferPrice = product.OfferPrice > 0 ? product.OfferPrice : null,
                CategoryId = product.CategoryId,
                SubCategory1Id = product.SubCategory1_Id ?? 0,
                SubCategory2Id = product.SubCategory2_Id ?? 0,
                ImageUrl = product.ImageUrl,
                CreatedAt = product.CreatedAt ?? DateTime.Now,
                Categories = _productService.GetAllCategories(),
                SubCategories1 = product.CategoryId > 0 ? _productService.GetSubCategories1ByCategory(product.CategoryId) : new List<SubCategory1>(),
                SubCategories2 = product.SubCategory1_Id.HasValue && product.SubCategory1_Id > 0 ? _productService.GetSubCategories2BySubCategory1(product.SubCategory1_Id.Value) : new List<SubCategory2>()
            };

            return View(viewModel);
        }

        // GET: Product/Details/5
        public IActionResult Details(int id)
        {
            try
            {
                var product = _productService.GetProductById(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                return View(product);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading product details.";
                return RedirectToAction("Index");
            }
        }

        // POST: Product/DeleteProduct
        [HttpPost]
        public JsonResult DeleteProduct(int id)
        {
            try
            {
                if (id <= 0)
                    return Json(new { success = false, message = "Invalid product ID." });

                var product = _productService.GetProductById(id);
                if (product == null)
                    return Json(new { success = false, message = "Product not found." });

                var result = _productService.DeleteProduct(id);
                return Json(new { success = result, message = result ? "Product deleted successfully!" : "Error deleting product." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // AJAX endpoints
        [HttpGet]
        public JsonResult GetProduct(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
                return Json(new { success = false, message = "Product not found." });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = product.Id,
                    name = product.Name,
                    description = product.Description,
                    price = product.Price,
                    offerPrice = product.OfferPrice > 0 ? product.OfferPrice : (decimal?)null,
                    categoryId = product.CategoryId,
                    subCategory1Id = product.SubCategory1_Id,
                    subCategory2Id = product.SubCategory2_Id,
                    imageUrl = product.ImageUrl,
                    createdAt = product.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"
                }
            });
        }

        public JsonResult GetCategories()
        {
            var categories = _productService.GetAllCategories();
            return Json(new
            {
                success = true,
                data = categories.Select(c => new { id = c.Id, name = c.Name })
            });
        }

        
        public JsonResult GetSubCategories1(int categoryId)
        {
            var subCategories = categoryId > 0 ?
                _productService.GetSubCategories1ByCategory(categoryId) :
                new List<SubCategory1>();

            return Json(new
            {
                success = true,
                data = subCategories.Select(sc => new { id = sc.Id, name = sc.Name })
            });
        }

       
        public JsonResult GetSubCategories2(int subCategory1Id)
        {
            var subCategories = subCategory1Id > 0 ?
                _productService.GetSubCategories2BySubCategory1(subCategory1Id) :
                new List<SubCategory2>();

            return Json(new
            {
                success = true,
                data = subCategories.Select(sc => new { id = sc.Id, name = sc.Name })
            });
        }

      
        public JsonResult GetProductsByCategory(int categoryId)
        {
            var products = categoryId > 0 ?
                _productService.GetProductsByCategory(categoryId) :
                _productService.GetAllProducts();

            return Json(new
            {
                success = true,
                data = products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description,
                    price = p.Price,
                    offerPrice = p.OfferPrice > 0 ? p.OfferPrice : (decimal?)null,
                    imageUrl = p.ImageUrl,
                    createdAt = p.CreatedAt?.ToString("dd/MM/yyyy") ?? "N/A",
                    categoryName = p.Category?.Name,
                    subCategory1Name = p.SubCategory1?.Name,
                    subCategory2Name = p.SubCategory2?.Name
                })
            });
        }

        [HttpGet]
        public JsonResult SearchProducts(string searchTerm)
        {
            var products = string.IsNullOrWhiteSpace(searchTerm) ?
                _productService.GetAllProducts() :
                _productService.SearchProducts(searchTerm.Trim());

            return Json(new
            {
                success = true,
                data = products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description,
                    price = p.Price,
                    offerPrice = p.OfferPrice > 0 ? p.OfferPrice : (decimal?)null,
                    imageUrl = p.ImageUrl,
                    createdAt = p.CreatedAt?.ToString("dd/MM/yyyy") ?? "N/A",
                    categoryName = p.Category?.Name,
                    subCategory1Name = p.SubCategory1?.Name,
                    subCategory2Name = p.SubCategory2?.Name
                })
            });
        }

        private bool IsValidBase64Image(string base64String)
        {
            try
            {
                if (!base64String.StartsWith("data:image/"))
                    return false;

                var validMimeTypes = new[] { "data:image/jpeg", "data:image/jpg", "data:image/png", "data:image/gif", "data:image/webp" };

                return validMimeTypes.Any(mime => base64String.StartsWith(mime));
            }
            catch
            {
                return false;
            }
        }

        // Add AJAX endpoint for filtering products
        public JsonResult GetProductsByFilter(int categoryId, int? subCategory1Id = null, int? subCategory2Id = null)
        {
            List<Product> products;

            if (subCategory2Id.HasValue && subCategory2Id.Value > 0)
            {
                products = _productService.GetProductsBySubCategory2(subCategory2Id.Value);
            }
            else if (subCategory1Id.HasValue && subCategory1Id.Value > 0)
            {
                products = _productService.GetProductsBySubCategory1(subCategory1Id.Value);
            }
            else
            {
                products = _productService.GetProductsByCategory(categoryId);
            }

            return Json(new
            {
                success = true,
                data = products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description,
                    price = p.Price,
                    offerPrice = p.OfferPrice,
                    imageUrl = p.ImageUrl,
                    createdAt = p.CreatedAt?.ToString("MMM dd, yyyy") ?? "N/A",
                    categoryName = p.Category?.Name,
                    subCategory1Name = p.SubCategory1?.Name,
                    subCategory2Name = p.SubCategory2?.Name
                }).ToList()
            });
        }

        // Update other category methods similarly
        public IActionResult PowerTools(int? subCategory1Id = null, int? subCategory2Id = null)
        {
            var categoryName = "PowerTools";
            var categories = _productService.GetAllCategories();
            var category = categories.FirstOrDefault(c => c.Name.Contains(categoryName, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                return View(new CategoryViewModel
                {
                    CategoryName = categoryName,
                    Products = new List<Product>(),
                    SubCategories1 = new List<SubCategory1>()
                });
            }

            var viewModel = new CategoryViewModel
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                SubCategories1 = _productService.GetSubCategories1ByCategory(category.Id),
                Products = subCategory2Id.HasValue ? _productService.GetProductsBySubCategory2(subCategory2Id.Value) :
                           subCategory1Id.HasValue ? _productService.GetProductsBySubCategory1(subCategory1Id.Value) :
                           _productService.GetProductsByCategory(category.Id),
                SelectedSubCategory1Id = subCategory1Id,
                SelectedSubCategory2Id = subCategory2Id
            };

            return View(viewModel);
        }

        public IActionResult CuttingTools(int? subCategory1Id = null, int? subCategory2Id = null)
        {
            var categoryName = "CuttingTools";
            var categories = _productService.GetAllCategories();
            var category = categories.FirstOrDefault(c => c.Name.Contains(categoryName, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                return View(new CategoryViewModel
                {
                    CategoryName = categoryName,
                    Products = new List<Product>(),
                    SubCategories1 = new List<SubCategory1>()
                });
            }

            var viewModel = new CategoryViewModel
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                SubCategories1 = _productService.GetSubCategories1ByCategory(category.Id),
                Products = subCategory2Id.HasValue ? _productService.GetProductsBySubCategory2(subCategory2Id.Value) :
                           subCategory1Id.HasValue ? _productService.GetProductsBySubCategory1(subCategory1Id.Value) :
                           _productService.GetProductsByCategory(category.Id),
                SelectedSubCategory1Id = subCategory1Id,
                SelectedSubCategory2Id = subCategory2Id
            };

            return View(viewModel);
        }

        public IActionResult Abrasives(int? subCategory1Id = null, int? subCategory2Id = null)
        {
            var categoryName = "Abrasives";
            var categories = _productService.GetAllCategories();
            var category = categories.FirstOrDefault(c => c.Name.Contains(categoryName, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                return View(new CategoryViewModel
                {
                    CategoryName = categoryName,
                    Products = new List<Product>(),
                    SubCategories1 = new List<SubCategory1>()
                });
            }

            var viewModel = new CategoryViewModel
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                SubCategories1 = _productService.GetSubCategories1ByCategory(category.Id),
                SubCategories2Map = GetSubCategoriesMapForCategory(category.Id),
                Products = subCategory2Id.HasValue ? _productService.GetProductsBySubCategory2(subCategory2Id.Value) :
                           subCategory1Id.HasValue ? _productService.GetProductsBySubCategory1(subCategory1Id.Value) :
                           _productService.GetProductsByCategory(category.Id),
                SelectedSubCategory1Id = subCategory1Id,
                SelectedSubCategory2Id = subCategory2Id
            };

            return View(viewModel);
        }

        // Add this private helper method to ProductController
        private Dictionary<int, List<SubCategory2>> GetSubCategoriesMapForCategory(int categoryId)
        {
            var result = new Dictionary<int, List<SubCategory2>>();
            var subCat1List = _productService.GetSubCategories1ByCategory(categoryId);

            foreach (var subCat1 in subCat1List)
            {
                result[subCat1.Id] = _productService.GetSubCategories2BySubCategory1(subCat1.Id);
            }

            return result;
        }

        // GET: /Product/Pneumatic/CP
        // GET: /Product/CuttingTools/Bipico
        // GET: /Product/Abrasives/Norton
        // GET: /Product/PowerTools/Makita
        [HttpGet]
        [Route("Product/Category/{categoryName}/{brandName?}")]
        public IActionResult CategoryByBrand(string categoryName, string brandName = null)
        {
            
                try
                {
                    // Map category names to IDs
                    var categoryMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "Pneumatic", 3 },
            { "CuttingTools", 2 },
            { "Abrasives", 1 },
            { "PowerTools", 4 }
        };

                    if (!categoryMap.TryGetValue(categoryName, out int categoryId))
                    {
                        return NotFound("Category not found");
                    }

                    // Get the category
                    var category = _productService.GetCategoryById(categoryId);
                    if (category == null)
                    {
                        return NotFound("Category not found");
                    }

                    // Get all SubCategories1 for this category
                    var subCategories1 = _productService.GetSubCategories1ByCategory(categoryId);

                    // Find the selected brand (SubCategory1)
                    int? selectedSubCategory1Id = null;
                    if (!string.IsNullOrEmpty(brandName))
                    {
                        var selectedBrand = subCategories1.FirstOrDefault(s =>
                            s.Name.Equals(brandName, StringComparison.OrdinalIgnoreCase));

                        if (selectedBrand != null)
                        {
                            selectedSubCategory1Id = selectedBrand.Id;
                        }
                    }

                    // Get SubCategories2 Map
                    var subCategories2Map = new Dictionary<int, List<SubCategory2>>();
                    foreach (var subCat1 in subCategories1)
                    {
                        var subCat2List = _productService.GetSubCategories2BySubCategory1(subCat1.Id);
                        subCategories2Map[subCat1.Id] = subCat2List;
                    }

                    // Get products based on selection
                    List<Product> products;
                    if (selectedSubCategory1Id.HasValue)
                    {
                        // Get products for selected brand
                        products = _productService.GetProductsBySubCategory1(selectedSubCategory1Id.Value);
                    }
                    else
                    {
                        // Get all products for this category
                        products = _productService.GetProductsByCategory(categoryId);
                    }

                    // Create ViewModel
                    var viewModel = new CategoryViewModel
                    {
                        CategoryId = categoryId,
                        CategoryName = category.Name,
                        SubCategories1 = subCategories1,
                        SubCategories2Map = subCategories2Map,
                        Products = products,
                        SelectedSubCategory1Id = selectedSubCategory1Id,
                        SelectedSubCategory2Id = null
                    };

                    // Return appropriate view based on category
                    string viewName = categoryName switch
                    {
                        "Pneumatic" => "Pneumatic",
                        "CuttingTools" => "CuttingTools",
                        "Abrasives" => "Abrasives",
                        "PowerTools" => "PowerTools",
                        _ => "Category"
                    };

                    return View(viewName, viewModel);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading category by brand: {ex.Message}");
                    return StatusCode(500, "An error occurred while loading the category");
                }
           
        }

        //public IActionResult SaveCategory(string name)
        //{ 
        //bool isSaved = _productService.SaveCategoryToDb(new Category { Name = name });
        //    if (isSaved)
        //    {
        //        return Json(new { success = true, message = "Category saved successfully!" });
        //    }
        //    else
        //    {
        //        return Json(new { success = false, message = "Error saving category." });
        //    }
        //}

        [HttpGet]
        public JsonResult GetSampleProductsForSubCategories2(int subCategory1Id)
        {
           var subCategories2 = _productService.GetSampleProductsForSubCategories2(subCategory1Id);

            return Json(subCategories2);
        }

    }
}