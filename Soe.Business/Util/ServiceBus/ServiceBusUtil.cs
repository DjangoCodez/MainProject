using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.ServiceBus
{
    public static class ServiceBusUtil
    {
        private static string ConnectionString = "YourConnectionString"; // Replace with your Service Bus connection string
        private static string TopicName = "YourTopicName"; // Replace with your topic name
        private static string QueueName = "YourQueueName"; // Replace with your queue name

        public static void AddMessageToTopic(ServiceBusKeyMessage message)
        {
            var task = AddMessageToTopicAsync(message);
            task.Wait();
        }

        public static async Task AddMessageToTopicAsync(ServiceBusKeyMessage message)
        {
            await Task.Run(async () =>
            {
                var client = new ServiceBusClient(ConnectionString);

                var sender = client.CreateSender(TopicName);

                string messageBody = $"{message.Key}-{message.Time}";
                var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));

                await sender.SendMessageAsync(serviceBusMessage);
                await client.DisposeAsync();
            });
        }

        public static List<ServiceBusKeyMessage> CheckForMessagesInQueue()
        {
            return CheckForMessagesInQueueAsync().Result;
        }

        public static async Task<List<ServiceBusKeyMessage>> CheckForMessagesInQueueAsync()
        {
            var messages = new List<ServiceBusKeyMessage>();

            await Task.Run(async () =>
            {
                var client = new ServiceBusClient(ConnectionString);

                var receiver = client.CreateReceiver(QueueName);

                while (true)
                {
                    ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();

                    if (receivedMessage != null)
                    {
                        string messageBody = Encoding.UTF8.GetString(receivedMessage.Body.ToArray());
                        Console.WriteLine($"Received message: {messageBody}");

                        var parts = messageBody.Split('-');
                        if (parts.Length == 2 && DateTime.TryParse(parts[1], out DateTime time))
                        {
                            var keyMessage = new ServiceBusKeyMessage
                            {
                                Key = parts[0],
                                Time = time
                            };
                            messages.Add(keyMessage);

                            await receiver.CompleteMessageAsync(receivedMessage);
                        }
                        else
                        {
                            // Message format is not valid, consider handling it
                        }
                    }
                    else
                    {
                        // No more messages in the queue, exit the loop
                        break;
                    }
                }
                await client.DisposeAsync();
            });

            return messages;
        }
    }

    public class ServiceBusKeyMessage
    {
        public string Key { get; set; }
        public DateTime Time { get; set; }
    }
}
