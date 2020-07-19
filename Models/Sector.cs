using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Models
{
    public class Sector
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Business> Businesses { get; set; }
    }

    public enum SectorType
    {
        Tech = 1,
        RealEstate = 2,
        Marketing = 3,
    }
}
