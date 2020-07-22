using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdleBusiness.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string MessageBody { get; set; }
        public bool ReadByBusiness { get; set; }
        public DateTime DateReceived { get; set; }

        public int ReceivingBusinessId { get; set; }
        public Business ReceivingBusiness { get; set; }

        public int SendingBusinessId { get; set; }
        public Business SendingBusiness { get; set; }
    }
}
