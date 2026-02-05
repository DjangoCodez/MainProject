using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Core.TimeEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.Communicator.Tests
{
    [TestClass()]
    public class CommunicatorConnectorTests
    {
        [TestMethod()]
        public void SendCommunicatorMessageTest()
        {
            var file = File.ReadAllBytes("C:/Users/mrickardk.SOFTONE/Downloads/billing_invoices_2c92a09a713a5d1101713b08359c734e.pdf");
            var file2 = File.ReadAllBytes("C:/Users/mrickardk.SOFTONE/Downloads/asd.pdf");
            int count = 0;
            CommunicatorMessageBatch communicatorMessageBatch = new CommunicatorMessageBatch()
            {
                CommunicatorMessages = new List<CommunicatorMessage>()
            };

            while (count < 6)
            {
                count++;
                communicatorMessageBatch.CommunicatorMessages.Add(new CommunicatorMessage()
                {
                    Subject = $"Subject test nr {count}",
                    MessageAttachments = new List<CommunicatorMessageAttachment>() { new CommunicatorMessageAttachment() { DataBase64 = (count % 2 == 0 ? file : file2), Name = "Test.pdf", ContentType = "application/pdf" } },
                    Recievers = new List<CommunicatorPerson>() { new CommunicatorPerson() { Email = "rickard.dahlgren@softone.se" } },
                    CommunicatorProvider = CommunicatorProvider.SendGrid,
                    Body = $"Test nr {count} {DateTime.Now}",
                    CommunicatorMessageId = Guid.NewGuid(),
                    Sender = new CommunicatorPerson() { Email = "rickard.karlsson@softone.se" }
                });
            }

            var result = CommunicatorConnector.SendCommunicatorMessageBatch(communicatorMessageBatch);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void SendMailMessageTest()
        {
            var file = File.ReadAllBytes("C:/Users/mrickardk.SOFTONE/Downloads/billing_invoices_2c92a09a713a5d1101713b08359c734e.pdf");
            var file2 = File.ReadAllBytes("C:/Users/mrickardk.SOFTONE/Downloads/asd.pdf");
            int count = 0;

            MailMessageDTO mailMessage = new MailMessageDTO()
            {
                SenderEmail = "Rickard.karlsson@softone.se",
                recievers = new List<string>() { "rickard.dahlgren@softone.se" },
                subject = "Testar mailmessageDTO",
                body = "body!",
                cc = new List<string>(),
                MessageAttachmentDTOs = new List<MessageAttachmentDTO>() { new MessageAttachmentDTO() { Data = count % 2 == 0 ? file : file2, Name = "Test.pdf" } },

            };
            var result = CommunicatorConnector.SendMailMessage(mailMessage);
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void SendSmsMessageTest()
        {
            MailMessageDTO smsMessage = new MailMessageDTO()
            {
                body = "test from soe",
                recievers = new List<string>() { "0703200945" },
                SenderName = "Soe",
                Source = "Softone",
                key = "SmsCopy"
            };
            var result = CommunicatorConnector.SendSMSMessage(smsMessage);
            Assert.IsTrue(result.Success);
        }
    }
}