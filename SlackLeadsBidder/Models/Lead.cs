using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Web;

namespace SlackLeadsBidder.Models
{
    public class Lead
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public string FromEmail { get; set; }

        [Required]
        public string FromName { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public DateTime? AuctionStarted { get; set; }

        public DateTime? AuctionEnded { get; set; }

    }
}