using RBHU_DbServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBHU_DbServices.Interface
{
    public interface IEService
    {
        public bool SaveEmailToDb(Email email);
        public List<Email> GetEmailFromDb();
    }
}
