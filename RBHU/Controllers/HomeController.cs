using RBHU.Models;
using RBHU_DbServices.Interface;
using RBHU_DbServices.Models;
using RBHU_DbServices.ViewModel;
using RBHU_DbServices.ViewModel.ContactModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RBHU.Models;
using RBHU_DbServices.Interface;
using RBHU_DbServices.Models;
using RBHU_DbServices.ViewModel.ContactModel;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace RBHU.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RBHUContext _context; // Add your DbContext here
        private readonly IProductService _productService;
        private readonly IEService _emailService; // Assuming you have an email service for sending emails
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;              // STARTTLS
        private const bool UseSsl = true;
        private const string SmtpUsername = "eternalvision2025@gmail.com"; // your Gmail address
        private const string SmtpAppPassword = "gvvy enkz fjjo iccp"; // 16-char App Password
        private const string AdminEmail = "office@aniketsales.com";     // where you receive messages
        private const string FromDisplayName = "Contact from Website";      // display name shown to admin
        public HomeController(ILogger<HomeController> logger, IEService emailService, IProductService productService)
        {
            _logger = logger;
            _emailService = emailService;
            _productService = productService;
        }

        public IActionResult Home()
        {
            ViewData["Title"] = "Home";
            var Categories = _productService.GetAllCategoriesForHome();
            return View(Categories);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "About Us";
            return View();
        }

        public IActionResult Products()
        {
            ViewData["Title"] = "Products";
            return View();
        }

        public IActionResult Offers()
        {
            ViewData["Title"] = "Special Offers";

            // Get products with offer prices
            var productsWithOffers = _productService.GetSpecialOffers();

            return View(productsWithOffers);
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact Us";
            return View(); // No model passed
        }

        [HttpPost]
        public async Task<IActionResult> SendMail(ContactModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var firstError = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = $"Validation failed: {firstError}" });
                }


                // Create Email entity from ContactModel
                var email = new Email
                {
                    UniqueEmailId = Guid.NewGuid().ToString(),
                    GmailUid = 0, // Set to 0 for contact form submissions
                    Folder = "ContactForm",
                    FromName = model.FullName,
                    FromEmail = model.Email,
                    Subject = model.Subject,
                    Snippet = model.Message?.Length > 100 ? model.Message.Substring(0, 100) + "..." : model.Message,
                    ReceivedUtc = DateTime.UtcNow,
                    ReceivedLocal = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Unread = true,
                    HasAttachments = false,
                    HtmlBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2 style='color: #1a365d; border-bottom: 2px solid #ff6b35; padding-bottom: 10px;'>
                                New Contact Form Submission
                            </h2>
                            <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                                <p><strong>Name:</strong> {model.FullName}</p>
                                <p><strong>Email:</strong> {model.Email}</p>
                                <p><strong>Phone:</strong> {model.PhoneNumber}</p>
                                <p><strong>Company:</strong> {model.Company}</p>
                                <p><strong>GST Number:</strong> {model.GSTNumber}</p>
                                <p><strong>Subject:</strong> {model.Subject}</p>
                            </div>
                            <div style='margin: 20px 0;'>
                                <h3 style='color: #1a365d;'>Message:</h3>
                                <p style='background-color: #ffffff; padding: 15px; border: 1px solid #dee2e6; border-radius: 5px;'>
                                    {model.Message?.Replace("\n", "<br>")}
                                </p>
                            </div>
                            <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6; font-size: 12px; color: #6c757d;'>
                                <p>This email was sent from the Aniket Sales Agencies contact form on {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                            </div>
                        </div>",
                    TextBody = $@"New Contact Form Submission
                                Name: {model.FullName}
                                Email: {model.Email}
                                Phone: {model.PhoneNumber}
                                Company: {model.Company}
                                GST Number: {model.GSTNumber}
                                Subject: {model.Subject}
                                Message:
                                {model.Message}
                                Sent on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    LabelsJson = "[\"ContactForm\", \"Unread\"]",
                    Company = model.Company,
                    Phone = model.PhoneNumber,
                    GstNumber = model.GSTNumber,
                    Message = model.Message,
                    IsContactForm = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                //1 Save to database
                var isSaved = _emailService.SaveEmailToDb(email);

                _logger.LogInformation($"Contact form submission saved: {model.Email} - {model.Subject}");

                // 2. Send email notification
                await SendEmailNotification(model, email.UniqueEmailId);




                return Json(new { success = isSaved, message = "Thank you for your message! We'll get back to you soon." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing contact form submission");
                return Json(new { success = false, message = "An error occurred while sending your message. Please try again later." });
            }
        }


        private async Task SendEmailNotification(ContactModel model, string referenceId)
        {
            // Build HTML body safely
            string E(string s) => WebUtility.HtmlEncode(s ?? string.Empty);
            var bodyHtml = new StringBuilder()
                .AppendLine("<div style='font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#222'>")
                .AppendLine($"<h2>New Contact Form Submission For {E(model.Subject)}</h2>")
                //  .AppendLine($"<p><strong>Reference ID:</strong> {E(referenceId)}</p>")
                .AppendLine($"<p><strong>Subject : </strong>{E(model.Subject)}</p>")
                .AppendLine("<table style='border-collapse:collapse'>")
                .AppendLine($"<tr><td style='padding-right:10px'><b>Name</b></td><td>{E(model.FullName)}</td></tr>")
                .AppendLine($"<tr><td><b>Email</b></td><td>{E(model.Email)}</td></tr>")
                .AppendLine($"<tr><td><b>Phone</b></td><td>{E(model.PhoneNumber)}</td></tr>")
                .AppendLine($"<tr><td><b>Company</b></td><td>{E(model.Company ?? "-")}</td></tr>")
                .AppendLine($"<tr><td><b>GST</b></td><td>{E(model.GSTNumber ?? "-")}</td></tr>")
                //.AppendLine($"<tr><td><b>Subject</b></td><td>{E(model.Subject)}</td></tr>")
                .AppendLine("</table>")
                .AppendLine("<hr/>")
                .AppendLine("<div><b>Message:</b></div>")
                .AppendLine($"<div style='white-space:pre-wrap'>{E(model.Message)}</div>")
                .AppendLine("<hr/>")
                .AppendLine($"<p><small>Submitted at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</small></p>")
                .AppendLine("</div>")
                .ToString();

            using var smtp = new SmtpClient
            {
                Host = SmtpHost,
                Port = SmtpPort,
                EnableSsl = UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(SmtpUsername, SmtpAppPassword)
            };

            using var msg = new MailMessage
            {
                From = new MailAddress(SmtpUsername, $"Website Contact: {model.FullName}"),
                Sender = new MailAddress(SmtpUsername, FromDisplayName),
                Subject = $"Contact Form: {model.FullName} <{model.Email}>",
                Body = bodyHtml,
                IsBodyHtml = true
            };

            msg.To.Add(new MailAddress(AdminEmail));

            // Replies will go to the visitor
            if (!string.IsNullOrWhiteSpace(model.Email))
                msg.ReplyToList.Add(new MailAddress(model.Email, model.FullName));

            await smtp.SendMailAsync(msg);

            _logger.LogInformation("Email notification sent for contact form submission {ReferenceId}", referenceId);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}