using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http.Headers;
using BotOnWebApp.Services;
using BotOnWebApp.Models;
using System.IdentityModel.Tokens;
using System.Configuration;

namespace BotOnWebApp.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private string appKey = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private string graphResourceID = "https://graph.microsoft.com/";
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private string authority = aadInstance + "common";
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            var responseStr = "";
            var endPoint = "https://graph.microsoft.com/v1.0/me/events";
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            if (activity.Type == ActivityTypes.Message)
            {
                var token = "";
                switch (activity.Text.ToLower())
                {
                    case "hi":                   
                        var reply = activity.CreateReply($"Hello dude");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                        break;
                    case "forget me":
                        var requestStr = "";
                        if(BotRegistryHelper.ForgetUserToken(activity.From.Id))
                        {
                            requestStr = "You are forgotten.";
                        }
                        else
                        {
                            requestStr = "Cannot forget you.";
                        }
                        var forgetTokenreply = activity.CreateReply(requestStr);
                        await connector.Conversations.ReplyToActivityAsync(forgetTokenreply);

                        break;
                    case "who am i":
                        token = BotRegistryHelper.GetUserToken(activity.From.Id);
                        if (token != null)
                        {
                            var jwt = new JwtSecurityToken(token);
                            if (jwt != null)
                            {
                                if (jwt.ValidTo < DateTime.Now)
                                {
                                    //Expired Token
                                    var exiredTokenreply = activity.CreateReply($"Token is Expired");
                                    await connector.Conversations.ReplyToActivityAsync(exiredTokenreply);
                                    var retryTokenreply = activity.CreateReply($"goto: https://{Request.RequestUri.Host}:{Request.RequestUri.Port}/home/about?key={activity.From.Id}");
                                    await connector.Conversations.ReplyToActivityAsync(retryTokenreply);
                                    break;
                                }
                            }
                            try
                            {
                                var client = new HttpClient();
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                var graphResponse = await client.GetAsync("https://graph.microsoft.com/v1.0/me");

                                if (graphResponse.IsSuccessStatusCode)
                                {
                                    var json = await graphResponse.Content.ReadAsStringAsync();
                                    var me = JsonConvert.DeserializeObject<Me>(json);
                                    if (me != null)
                                        responseStr = $"Hi {me.displayName}";
                                    else
                                        responseStr = "I do not know who you are";
                                }
                                var meetingReply = activity.CreateReply($"{responseStr}");
                                await connector.Conversations.ReplyToActivityAsync(meetingReply);
                            }
                            catch (Exception ex)
                            {
                                var exceptionReply = activity.CreateReply($"Bummer: {ex.Message}");
                                await connector.Conversations.ReplyToActivityAsync(exceptionReply);
                            }
                        }
                        else
                        {
                            var noTokenreply = activity.CreateReply($"goto: https://{Request.RequestUri.Host}:{Request.RequestUri.Port}/home/about?key={activity.From.Id}");
                            await connector.Conversations.ReplyToActivityAsync(noTokenreply);
                        }
                        break;
                        break;
                    case "next meeting":
                        token = BotRegistryHelper.GetUserToken(activity.From.Id);
                        if (token != null)
                        {
                            var jwt = new JwtSecurityToken(token);
                            if (jwt != null)
                            {
                                if(jwt.ValidTo<DateTime.Now)
                                {
                                    //Expired Token
                                    var exiredTokenreply = activity.CreateReply($"Token is Expired");
                                    await connector.Conversations.ReplyToActivityAsync(exiredTokenreply);
                                    var retryTokenreply = activity.CreateReply($"goto: https://{Request.RequestUri.Host}:{Request.RequestUri.Port}/home/about?key={activity.From.Id}");
                                    await connector.Conversations.ReplyToActivityAsync(retryTokenreply);
                                    break;
                                }
                            }
                            try
                            {
                                var client = new HttpClient();
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                var graphResponse = await client.GetAsync(endPoint);

                                if (graphResponse.IsSuccessStatusCode)
                                {
                                    var json = await graphResponse.Content.ReadAsStringAsync();
                                    var events = JsonConvert.DeserializeObject<EventModels>(json);
                                    if (events.value.Count() > 0)
                                        responseStr = $"Your next meeting '{events.value.First().subject}' is at {events.value.First().start.DateTime.ToLocalTime().Date.ToShortDateString()} on {events.value.First().start.DateTime.ToShortTimeString()}";
                                    else
                                        responseStr = "You do not have any meeting";
                                }
                                var meetingReply = activity.CreateReply($"{responseStr}");
                                await connector.Conversations.ReplyToActivityAsync(meetingReply);
                            }
                            catch (Exception ex)
                            {
                                var exceptionReply = activity.CreateReply($"Bummer: {ex.Message}");
                                await connector.Conversations.ReplyToActivityAsync(exceptionReply);
                            }
                        }
                        else
                        {
                            var noTokenreply = activity.CreateReply($"goto: https://{Request.RequestUri.Host}:{Request.RequestUri.Port}/home/about?key={activity.From.Id}");
                            await connector.Conversations.ReplyToActivityAsync(noTokenreply);
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}

