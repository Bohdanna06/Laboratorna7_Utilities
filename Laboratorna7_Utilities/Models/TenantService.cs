using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laboratorna7_Utilities.Models
{
    public class TenantService
    {
        public int TenantID { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public int ServiceID { get; set; }
        public Service Service { get; set; } = null!;
    }
}
