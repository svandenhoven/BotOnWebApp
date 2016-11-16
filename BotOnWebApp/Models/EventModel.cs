using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BotOnWebApp.Models
{
    public class EventModels
    {
        public List<EventModel> value { get; set; }
    }
    public class EventModel
    {
        public string id { get; set; }
        public string subject { get; set; }
        public DateTimeTimeZone start { get; set; }
        public DateTimeTimeZone end { get; set; }
        public string showAs { get; set; }
        public bool isAllDay { get; set; }
        public EventBody body { get; set; }
        public bool isReminderOn { get; set; }
        public List<Attendee> attendees { get; set; }
        public Organizer organizer { get; set; }
        public bool isOrganizer { get; set; }
        public List<BookingExtension> extensions { get; set; }
    }

    public class DateTimeTimeZone
    {
        public DateTime DateTime { get; set; }
        public string TimeZone
        {
            get
            {
                return System.TimeZone.CurrentTimeZone.StandardName;
            }
        }
    }

    public class EventBody
    {
        public string contentType { get; set; }
        public string content { get; set; }
    }

    public class EmailAddress
    {
        public string name { get; set; }
        public string address { get; set; }
    }

    public class Status
    {
        public string response { get; set; }
        public string time { get; set; }
    }

    public class Attendee
    {
        public Status status { get; set; }
        public string type { get; set; }
        public EmailAddress emailAddress { get; set; }
    }
    public class Organizer
    {
        public EmailAddress emailAddress { get; set; }
    }

    public abstract class OpenTypeExtensionModel
    {
        [JsonProperty("@odata.type")]
        public string Type { get { return "Microsoft.Graph.OpenTypeExtension"; } }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        public string ExtensionName { get; set; }
    }

    public enum ItemType
    {
        Booking,
        Reservation,
        Option
    }

    public class BookingExtension : OpenTypeExtensionModel
    {
        public ItemType ItemType { get; set; }

        public BookingExtension(string extensionName)
        {
            Id = Guid.NewGuid().ToString();
            ExtensionName = extensionName;
        }
    }
}