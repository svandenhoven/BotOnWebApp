using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BotOnWebApp.Models
{
    public class UserMappings : TableEntity
    {
        public UserMappings()
        { }

        public UserMappings(string userid, string accessToken)
        {
            this.PartitionKey = "MSFT";
            this.RowKey = userid;
            this.AccessToken = accessToken;
            this.ServiceUrl = "";
            this.Key = "";
        }

        public UserMappings(string userid, string accessToken, string serviceUrl)
        {
            this.PartitionKey = "MSFT";
            this.RowKey = userid;
            this.AccessToken = accessToken;
            this.ServiceUrl = serviceUrl;
            this.Key = "";
        }

        public UserMappings(string userid, string accessToken, string serviceUrl, string key)
        {
            this.PartitionKey = "MSFT";
            this.RowKey = userid;
            this.AccessToken = accessToken;
            this.ServiceUrl = serviceUrl;
            this.PartitionKey = "";
            this.Key = key;
        }

        public string OrgUserId { get; set; }
        public string AccessToken { get; set; }
        public string ServiceUrl { get; set; }
        public string Key { get; set; }
    }
}