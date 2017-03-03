using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SlackLeadsBidder.Models
{
    public class Agent
    {

        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set;  }

        [Required]
        public int AutoBid { get; set;  }

        [Required]
        public int NextBid { get; set; }

        [Required]
        public string SlackId { get; set; }

        [Required]
        public int DailyAllowance { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }

    }
}