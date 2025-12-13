using RBHU_DbServices.Interface;
using RBHU_DbServices.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBHU_DbServices.Implementation
{
    public class EService : IEService
    {
        private readonly DbContextOptions<RBHUContext> _dbconnection;

        public EService(string conn)
        {
            // Initialize the DbContextOptions with the provided connection string
            _dbconnection = new DbContextOptionsBuilder<RBHUContext>()
                .UseSqlServer(conn)
                .Options;
        }
            // Constructor logic can be added here if needed
        
        public bool SaveEmailToDb(Email email)
        {
            try
            {
                using (var db = new RBHUContext(_dbconnection))
                {
                    db.Emails.Add(email);
                    db.SaveChanges();
                }
                return true;
            }
             catch (Exception ex)
            {
                // Log the exception (not implemented here)
                Console.WriteLine($"Error saving email: {ex.Message}");
                return false;
            }
        }

        public List<Email> GetEmailFromDb()
        {
            try
            {
                using (var db = new RBHUContext(_dbconnection))
                {
                    return db.Emails.ToList();
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                Console.WriteLine($"Error retrieving emails: {ex.Message}");
                return new List<Email>();
            }
        }
    }
}
