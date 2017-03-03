using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackLeadsBidder.Email
{
    public class EmailDeliveryResult
    {
        public string Email { get; set; }

        public bool Success { get; set; }

        public string ProviderId { get; set; }

        public string ProviderResult { get; set; }

        public bool ProviderBounced { get; set; }
    }
}