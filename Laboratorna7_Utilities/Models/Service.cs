using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laboratorna7_Utilities.Models
{
    public class Service
    {
        public int ID { get; set; }
        public string ServiceName { get; set; }
        public decimal? RatePerSqMeter { get; set; }
        public decimal? RatePerPerson { get; set; }
    }
}
