using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc.Html;
using Exceptionless;
using ServiceStack.Text;
using Slack.Webhooks;
using SlackLeadsBidder.Config;
using SlackLeadsBidder.Email.Providers;
using SlackLeadsBidder.Models;
using SlackLeadsBidder.Utils;

namespace SlackLeadsBidder.Services
{
    public class SlackLeadsBidderService
    {

        private static readonly object ServiceLock = new object();

        public static void CreateAuction()
        {
            lock (ServiceLock)
            {
                using (var db = new SlackLeadsBidderContext())
                {
                    if (!db.Leads.Any(e => e.AuctionStarted != null && e.AuctionEnded == null))
                    {
                        var lead = db.Leads.FirstOrDefault(e => e.AuctionStarted == null && e.AuctionEnded == null);
                        if (lead == null) return;

                        lead.AuctionStarted = DateTime.UtcNow;
                        db.SaveChanges();

                        var slackClient = new SlackClient(SlackLeadsBidderSettings.Settings.SlackOutgoingWebhook);

                        int auctionsEnds =
                            (int)
                                lead.AuctionStarted.Value.AddSeconds(
                                    SlackLeadsBidderSettings.Settings.AuctionTimeInSeconds)
                                    .ToUnixEpochSeconds();

                        var auctionEndsStr = $"<!date^{auctionsEnds}^"+ "{date} at {time}|?>";

                        var slackMessage = new SlackMessage()
                        {
                            Attachments = new List<SlackAttachment>()
                            {
                                new SlackAttachment()
                                {
                                    Pretext = "New Auction Started",
                                    Color = "good",
                                    Fields = new List<SlackField>()
                                    {
                                        new SlackField()
                                        {
                                            Title = "From",
                                            Value = lead.FromName+"\n"+lead.FromEmail
                                        },
                                        new SlackField()
                                        {
                                            Title = "Subject",
                                            Value = lead.Subject
                                        },
                                        new SlackField()
                                        {
                                            Title = "Ends",
                                            Value = auctionEndsStr
                                        }
                                    }
                                }
                            }
                        };

                        slackClient.Post(slackMessage);
                    }
                }
            }
        }

        public static void EndAuction()
        {
            lock (ServiceLock)
            {
                using (var db = new SlackLeadsBidderContext())
                {
                    var ago = DateTime.UtcNow.AddSeconds(-SlackLeadsBidderSettings.Settings.AuctionTimeInSeconds);

                    var lead = db.Leads.FirstOrDefault(e => e.AuctionStarted != null && e.AuctionEnded == null && e.AuctionStarted < ago);
                    if (lead == null) return;

                    int? winnerId = null;

                    //Calculate winner:

                    var agentCredits = db.Transactions
                        .GroupBy(e => e.AgentId)
                        .Select(e => new
                        {
                            AgentId = e.Key,
                            Credits = e.Sum(f => (int?) f.Amount)
                        })
                        .ToDictionary(e => e.AgentId, e => e.Credits);

                    var agentBids = db.Agents
                        .ToList()
                        .Select(e => new
                        {
                            e.Id,
                            e.NextBid,
                            Credits = agentCredits.ContainsKey(e.Id) ? agentCredits[e.Id] : 0
                        })
                        .Where(e => e.Credits >= e.NextBid)
                        .GroupBy(e => e.NextBid)
                        .OrderByDescending(e => e.Key)
                        .Take(1)
                        .FirstOrDefault();

                    if (agentBids != null && agentBids.Any())
                    {
                        winnerId = agentBids.PickRandom().Id;
                    }

                    int? bid = null;

                    if (winnerId == null)
                    {
                        winnerId = db.Agents.Select(e => e.Id).ToList().PickRandom();
                        bid = 0;
                    }

                    lead.AuctionEnded = DateTime.UtcNow;

                    var winner = db.Agents.First(e => e.Id == winnerId.Value);
          
                    bid = bid ?? winner.NextBid;

                    //Create transaction:

                    var transaction = new Transaction()
                    {
                        AgentId = winnerId.Value,
                        Amount = -bid.Value,
                        Created = DateTime.UtcNow,
                        TransactionTypeId = (int)TransactionTypeValues.BidDedit,
                        LeadId = lead.Id
                    };

                    if (agentCredits.ContainsKey(winnerId.Value))
                    {
                        agentCredits[winnerId.Value] -= bid.Value;
                    }

                    db.Transactions.Add(transaction);
               
                    //Reset all bids:

                    foreach (var agent in db.Agents.ToList())
                    {
                        agent.NextBid = agentCredits.ContainsKey(winnerId.Value) ? 
                            Math.Min((int)agentCredits[winnerId.Value], agent.AutoBid) : 0;
                    }

                    db.SaveChanges();

                    //Post to Slack:

                    var slackClient = new SlackClient(SlackLeadsBidderSettings.Settings.SlackOutgoingWebhook);
                    slackClient.Post(new SlackMessage()
                    {
                        Text = $"Lead from {lead.FromName}<{lead.FromEmail}> won by @{winner.SlackId} with {bid} credits."
                    });

                    //Send email to winner:

                    MailMessage message = new MailMessage
                    {
                        From = new MailAddress("info@tessin.se", "Slack Leads Bidder"),
                        IsBodyHtml = true,
                        Subject = "Lead:" + lead.Subject,
                        Body = lead.Body
                    };

                    var delivery = ProviderFactory.Provider.Value.Prepare("AuctionWinner", message);
                    delivery.AddRecipient(winner.Email);
                    delivery.Send();
                }
            }
        }

        public static void CreditDailyAllowances()
        {
            lock (ServiceLock)
            {
                using (var db = new SlackLeadsBidderContext())
                {
                    var hoursAgo = DateTime.UtcNow.AddHours(-23);
                    var agents =
                        db.Agents.Where(
                            e =>
                                e.DailyAllowance > 0 &&
                                !e.Transactions.Any(
                                    f =>
                                        f.Created > hoursAgo &&
                                        f.TransactionTypeId == (int) TransactionTypeValues.DailyAllowanceCredit))
                            .ToList();

                    foreach (var agent in agents)
                    {
                        var transaction = new Transaction()
                        {
                            TransactionTypeId = (int) TransactionTypeValues.DailyAllowanceCredit,
                            Amount = agent.DailyAllowance,
                            AgentId = agent.Id,
                            Created = DateTime.UtcNow
                        };
                        db.Transactions.Add(transaction);
                        db.SaveChanges();
                        var slackClient = new SlackClient(SlackLeadsBidderSettings.Settings.SlackOutgoingWebhook);
                        slackClient.Post(new SlackMessage()
                        {
                            Text = $"Added {agent.DailyAllowance} credits to {agent.Name}'s account."
                        });
                    }
                }
            }
        }

        public static void SetAgentNextBid(string slackUser, int amount, SlackClient agentClient)
        {
            lock (ServiceLock)
            {
                using (var db = new SlackLeadsBidderContext())
                {
                    var agent = db.Agents.FirstOrDefault(e => e.SlackId == slackUser);

                    if (agent == null)
                    {
                        agentClient.PostError($"No agent configured for '{slackUser}'");
                        return;
                    }

                    var availableCredits = db.Transactions
                        .Where(e => e.Agent.SlackId == slackUser)
                        .Sum(e => (int?) e.Amount) ?? 0;

                    if (availableCredits < amount)
                    {
                        agentClient.PostError($"You don't have enough credits. You're balance is {availableCredits}.");
                        return;
                    }

                    agent.NextBid = amount;
                    db.SaveChanges();

                    var slackClient = new SlackClient(SlackLeadsBidderSettings.Settings.SlackOutgoingWebhook);
                    slackClient.Post(new SlackMessage()
                    {
                        Text = $"Agent @{slackUser} changed bid to {amount}."
                    });
                }
            }
        }

        public static void SetAgentAutoBid(string slackUser, int amount, SlackClient agentClient)
        {
            lock (ServiceLock)
            {
                using (var db = new SlackLeadsBidderContext())
                {
                    var agent = db.Agents.FirstOrDefault(e => e.SlackId == slackUser);

                    if (agent == null)
                    {
                        agentClient.PostError($"No agent configured for '{slackUser}'");
                        return;
                    }

                    agent.AutoBid = amount;
                    db.SaveChanges();

                    agentClient.PostOk($"You autobid is set to {amount} for next auction.");
                }
            }
        }

        public static void ShowBids(SlackClient agentClient)
        {
            using (var db = new SlackLeadsBidderContext())
            {
                var agentBids = db.Agents.Select(e => new
                    {
                        e.Name,
                        e.NextBid
                    })
                    .ToList()
                    .GroupBy(e => e.NextBid)
                    .OrderByDescending(e => e.Key)
                    .Select((e,i) => $"[{e.Key}] {string.Join(", ", e.Select(f => f.Name).ToArray())}" + (i == 0 && e.Count()>1 ? " (winner will selected by random)" : ""))
                    .ToArray();

                var message = new SlackMessage()
                {
                    Attachments = new List<SlackAttachment>()
                    {
                        new SlackAttachment()
                        {
                            Pretext = "Current Bids:",
                            Text = string.Join("\n", agentBids),
                            Color = "good"
                        }
                    }
                };

                agentClient.Post(message);
            }
        }

        public static void ShowBalance(SlackClient agentClient)
        {
            using (var db = new SlackLeadsBidderContext())
            {
                var agentCredits = db.Transactions
                    .GroupBy(e => e.Agent.Name)
                    .Select(e => new
                    {
                        Name = e.Key,
                        Credits = e.Sum(f => (int?) f.Amount)
                    })
                    .OrderBy(e => e.Name)
                    .ToArray()
                    .Select(e => $"[{e.Credits}] {e.Name}")
                    .ToArray();

                var message = new SlackMessage()
                {
                    Attachments = new List<SlackAttachment>()
                    {
                        new SlackAttachment()
                        {
                            Pretext = "Current Balance:",
                            Text = string.Join("\n", agentCredits),
                            Color = "good"
                        }
                    }
                };

                agentClient.Post(message);
            }
        }

        public static void CreateLead(Lead lead)
        {
            using (var db = new SlackLeadsBidderContext())
            {
                var oldTexts = db.Leads.OrderByDescending(e => e.Created).Select(e => e.Body).Take(10).ToList();
                var sameText = oldTexts.FirstOrDefault(e => _SeenBefore(lead.Body, e));
                if (sameText != null)
                {
                    /*
                    ExceptionlessClient.Default.CreateLog(nameof(CreateLead), "Duplicate lead", "Info").AddObject(new
                    {
                        NewBody = lead.Body,
                        FoundBody = sameText
                    })
                    .Submit();
                    */
                    ExceptionlessClient.Default.CreateLog(nameof(CreateLead), "Duplicate lead", "Info").AddObject(lead).Submit();
                    return;
                }

                lead.Created = DateTime.UtcNow;
                db.Leads.Add(lead);
                db.SaveChanges();
                CreateAuction();
            }
        }

        public static void GetAgentNextBid(string slackUser, SlackClient agentClient)
        {
            using (var db = new SlackLeadsBidderContext())
            {
                var agent = db.Agents.FirstOrDefault(e => e.SlackId == slackUser);

                if (agent == null)
                {
                    agentClient.PostError($"No agent configured for '{slackUser}'");
                    return;
                }

                agentClient.Post(new SlackMessage()
                {
                    Text = $"Your current bid is {agent.NextBid}."
                });
            }
        }

        public static void GetAgentAutoBid(string slackUser, SlackClient agentClient)
        {
            using (var db = new SlackLeadsBidderContext())
            {
                var agent = db.Agents.FirstOrDefault(e => e.SlackId == slackUser);

                if (agent == null)
                {
                    agentClient.PostError($"No agent configured for '{slackUser}'");
                    return;
                }

                agentClient.Post(new SlackMessage()
                {
                    Text = $"Your auto bid is {agent.AutoBid}."
                });
            }
        }

        private static bool _SeenBefore(string newText, string oldText)
        {
            if (!newText.StartsWith("<table") || !oldText.StartsWith("<table")) return false;

            int idx0 = newText.IndexOf("</table>", StringComparison.Ordinal);
            int idx1 = oldText.IndexOf("</table>", StringComparison.Ordinal);

            if (idx0 == -1 || idx1 == -1) return false;

            return newText.Substring(0,idx0).Equals(oldText.Substring(0,idx1));
        }

    }
}