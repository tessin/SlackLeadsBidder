using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using Exceptionless;
using Newtonsoft.Json.Linq;
using SlackLeadsBidder.Models;
using SlackLeadsBidder.Services;

namespace SlackLeadsBidder.Controllers
{
    public class MandrillController : ApiController
    {

        private readonly object lockOnThis = new object();

        [HttpPost]
        public HttpResponseMessage Inbound(FormDataCollection formData)
        {
            lock (lockOnThis)
            {
                var jEvents = JArray.Parse(formData["mandrill_events"]);

                foreach (dynamic jEvent in jEvents)
                {
                    if (jEvent.@event == "inbound")
                    {
                        try
                        {
                            var lead = new Lead
                            {
                                FromName = jEvent.msg.from_name,
                                FromEmail = jEvent.msg.from_email,
                                Subject = jEvent.msg.subject,
                                Body = jEvent.msg.html
                            };

                            ExceptionlessClient.Default.CreateLog(nameof(Inbound), "Lead", "Info").AddObject(lead).Submit();

                            SlackLeadsBidderService.CreateLead(lead);
                        }
                        catch (Exception ex)
                        {
                            ex.ToExceptionless().MarkAsCritical().Submit();
                        }

                    }
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

    }
}