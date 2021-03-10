using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Models
{
    public class Entrepreneur : IdentityUser
    {
        public int Score { get; set; }

        public virtual int BusinessId { get; set; }
        public virtual Business Business { get; set; }
    }
}
