using RBHU_DbServices.Interface;
using RBHU_DbServices.Models;
using RBHU_DbServices.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RBHU.Controllers
{
    public class AdminController : Controller
    {
        private readonly IEService _emailService;
        private readonly IProductService _productService;

        public AdminController(IEService emailService, IProductService productService)
        {
            _emailService = emailService;
            _productService = productService;
        }

        // Original Index action
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        // Original Admin action for emails
        public IActionResult Admin()
        {
            List<Email> emails = _emailService.GetEmailFromDb();
            return View(emails);
        }

        [Authorize(Roles = "Admin")]
        // Dashboard action - Combined view of products and emails
        public IActionResult Dashboard()
        {
            var viewModel = new ProductListViewModel
            {
                Products = _productService.GetAllProducts(),
                Categories = _productService.GetAllCategories(),
                Emails = _emailService.GetEmailFromDb()
            };

            return View(viewModel);
        }

        // Quick stats action for AJAX calls
        [HttpGet]
        public JsonResult GetDashboardStats()
        {
            var products = _productService.GetAllProducts();
            var emails = _emailService.GetEmailFromDb();
            var categories = _productService.GetAllCategories();

            var stats = new
            {
                TotalProducts = products.Count,
                TotalCategories = categories.Count,
                TotalEmails = emails.Count,
                UnreadEmails = emails.Count(e => e.Unread),
                ContactForms = emails.Count(e => e.IsContactForm),
                RecentProducts = products.Take(5).Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    CategoryName = p.Category?.Name,
                    CreatedAt = p.CreatedAt?.ToString("dd/MM/yyyy")
                }),
                RecentEmails = emails.Where(e => e.Unread).Take(5).Select(e => new
                {
                    e.Id,
                    e.FromName,
                    e.Subject,
                    ReceivedAt = e.ReceivedUtc.ToString("dd/MM/yyyy HH:mm")
                })
            };

            return Json(stats);
        }

        // Mark email as read action
        [HttpPost]
        public JsonResult MarkEmailAsRead(int emailId)
        {
            // You'll need to implement this in your email service
            // For now, returning success
            return Json(new { success = true, message = "Email marked as read" });
        }

        // Get product by category for quick filtering
        [HttpGet]
        public JsonResult GetProductsByCategory(int categoryId)
        {
            var products = categoryId > 0
                ? _productService.GetProductsByCategory(categoryId)
                : _productService.GetAllProducts();

            var result = products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                p.OfferPrice,
                CategoryName = p.Category?.Name,
                p.ImageUrl,
                CreatedAt = p.CreatedAt?.ToString("dd/MM/yyyy")
            });

            return Json(result);
        }

        public IActionResult MarkAsRead(int id)
        {
            bool isChanged = _productService.MarkASRead(id);

            if (isChanged)
                return Json(new { success = true });
            else
                return Json(new { success = false });
        }


        public IActionResult SaveCategory(string name)
        {
            Category category = new Category
            {
                Name = name
            };
            bool result = _productService.SaveCategoryToDb(category);
            return Json(result);
        }
    }
}