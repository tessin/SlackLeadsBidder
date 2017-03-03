using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using SlackLeadsBidder.Models;
using SlackLeadsBidder.Services;

namespace SlackLeadsBidder.Controllers
{
    public class MandrillController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Inbound(FormDataCollection formData)
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

                        SlackLeadsBidderService.CreateLead(lead);
                    }
                    catch (Exception)
                    {
                        //todo: Report to Exceptionless.
                    }

                }
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

    }
}