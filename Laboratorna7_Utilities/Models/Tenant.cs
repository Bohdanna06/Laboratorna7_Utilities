using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laboratorna7_Utilities.Models
{
    public class Tenant
    {
        public int ID { get; set; }
        public string AccountNumber { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public int OccupantsCount { get; set; }
        public decimal Area { get; set; }
    }
}
