using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackLeadsBidder.Email.Providers
{
    public class ProviderFactory
    {

        public static Lazy<IEmailProvider> Provider = new Lazy<IEmailProvider>(() => new MandrillProvider());

    }
}