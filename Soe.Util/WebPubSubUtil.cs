using Azure.Core;
using Azure.Messaging.WebPubSub;
using System;

namespace SoftOne.Soe.Common.Util
{
    public static class WebPubSubUtil
    {
        //https://azure.github.io/azure-webpubsub/demos/clientpubsub.html
        //https://github.com/Azure/azure-sdk-for-net/blob/Azure.Messaging.WebPubSub_1.0.0/sdk/webpubsub/Azure.Messaging.WebPubSub/README.md
        //https://www.youtube.com/watch?v=XEV67QHIFVU

        public static void Init(string connectionString)
        {
            _connectionString = connectionString;
        }
        private static string _connectionString { get; set; }
        private static WebPubSubServiceClient _client;
        private static WebPubSubServiceClient client
        {
            get
            {
                if (_client == null)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_connectionString))
                            _connectionString = "Endpoint=https://terminaltestpubsub.webpubsub.azure.com;AccessKey=ue2NtrmRopEJROIiVn2YvWFjjbGNgVczVifyN5U9woM=;Version=1.0;";

                        _client = new WebPubSubServiceClient(_connectionString, "timeterminal");
                    }
                    catch
                    {
                        // Intentionally ignored, safe to continue
                        // NOSONAR
                    }
                }
                return _client;
            }
        }

        public static void SendTextToAll(string value)
        {
            try { client.SendToAll(value); }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
        }

        public static void SendMessage(string group, string content)
        {
            client.SendToGroup(group, content);
        }

        public static void SendJsonToAll(string json)
        {
            try { client.SendToAll(RequestContent.Create(json), contentType: ContentType.ApplicationJson); }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
        }

        public static void SendJsonToGroup(string group, string json)
        {
            try { client.SendToGroup(group, RequestContent.Create(json), contentType: ContentType.ApplicationJson); }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
        }

        public static void SendJsonToUser(string user, string json)
        {
            try { client.SendToUser(user, RequestContent.Create(json), contentType: ContentType.ApplicationJson); }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
        }

        public static string GetMessageActionKey(WebPubSubMessageAction action)
        {
            switch (action)
            {
                case WebPubSubMessageAction.Insert:
                    return "I";
                case WebPubSubMessageAction.Update:
                    return "U";
                case WebPubSubMessageAction.Delete:
                    return "D";
            }

            return string.Empty;
        }
    }

    public enum WebPubSubMessageAction
    {
        Undefined = 0,
        Insert = 1,
        Update = 2,
        Delete = 3
    }
}
