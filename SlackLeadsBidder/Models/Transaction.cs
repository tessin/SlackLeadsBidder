using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SlackLeadsBidder.Models
{
    public class Transaction
    {

        [Required]
        public int Id { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public int? LeadId { get; set; }

        [ForeignKey("LeadId")]
        public virtual Lead Lead { get; set; }

        [Required]
        public int AgentId { get; set; }

        [ForeignKey("AgentId")]
        public virtual Agent Agent { get; set; }

        [Required]
        public int TransactionTypeId { get; set; }

        [ForeignKey("TransactionTypeId")]
        public virtual TransactionType TransactionType { get; set; }

    }
}