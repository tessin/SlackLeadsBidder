using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using SlackLeadsBidder.Utils;

namespace SlackLeadsBidder.Models
{
    public enum TransactionTypeValues
    {
        BidDedit = 1,
        DailyAllowanceCredit = 2
    }

    public class TransactionType
    {

        [Required]
        public int Id { get; set; }

        [Required]
        public string DescriptionText { get; set; }

        public static void SeedDb(SlackLeadsBidderContext context)
        {
            foreach (var v in EnumMetadata<TransactionTypeValues>.Values)
            {
                var s = new TransactionType
                {
                    Id = v.Value,
                    DescriptionText = v.Name,
                };

                context.TransactionTypes.AddOrUpdate(s);
            }
        }

    }
}