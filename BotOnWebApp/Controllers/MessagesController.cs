using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using BotOnWebApp.Services;
using BotOnWebApp.Models;

namespace BotOnWebApp.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            var responseStr = "";
            var endPoint = "https://graph.microsoft.com/v1.0/me/events";
            if (activity.Type == ActivityTypes.Message)
            {
                var token = BotRegistryHelper.GetUserToken("12345");
                if (token != null)
                {
                    try
                    {
                        var client = new HttpClient();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        var graphResponse = await client.GetAsync(endPoint);

                        if (graphResponse.IsSuccessStatusCode)
                        {
                            var json = await graphResponse.Content.ReadAsStringAsync();
                            var events = JsonConvert.DeserializeObject<EventModels>(json);
                            responseStr = $"Your next meeting '{events.value.First().subject}' is at {events.value.First().start.DateTime.ToLocalTime().Date.ToShortDateString()} on {events.value.First().start.DateTime.ToShortTimeString()}";
                        }
                        ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        var reply = activity.CreateReply($"{responseStr}");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    catch(Exception ex)
                    {
                        int i = 1;
                    }
                }
                else
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    var reply = activity.CreateReply($"goto: https://{Request.RequestUri.Host}:{Request.RequestUri.Port}/home/about?key=12345");
                    await connector.Conversations.ReplyToActivityAsync(reply);
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

