using SoftOne.Soe.Common.Attributes;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO.Communicator
{
    [TSInclude]
    public class IncomingEmailFilterDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string SenderEmail { get; set; }
        public string RecipientEmails { get; set; } 
        public List<int> DeliveryStatus { get; set; } = new List<int>();
        public int NoOfRecords { get; set; }
    }

    [TSInclude]
    public class IncomingEmailGridDTO
    {
        public int IncomingEmailId { get; set; }
        public string SenderEmail { get; set; }
        public string RecipientEmails { get; set; }
        public DateTime Date { get; set; }
        public string AttachementNames { get; set; }
        public int? DeliveryStatus { get; set; } // 0: Pending, 1: Delivered, 2: Failed
        public string DeliveryStatusText { get; set; }
    }

    [TSInclude]
    public class IncomingEmailDTO
    {
        public int IncomingEmailId { get; set; }
        public DateTime Received { get; set; }
        public string Subject { get; set; }
        public decimal? SpamScore { get; set; }
        public Guid UniqueIdentifier { get; set; }
        public string From { get; set; }
        public string Text { get; set; }
        public string Html { get; set; }
        public List<IncomingEmailAddressDTO> InboundEmails { get; set; }
        public List<IncomingEmailAttachmentDTO> Attachments { get; set; }
        public List<IncomingEmailLogDTO> Logs { get; set; }
    }

    [TSInclude]
    public class IncomingEmailAddressDTO
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string EmailAddress { get; set; }
        public string DeliveryStatus { get; set; }
        public int Retries { get; set; }
        public DateTime LastUpdated { get; set; }
        public string DispatcherId { get; set; }
    }

    [TSInclude]
    public class IncomingEmailAttachmentDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public int Size { get; set; }
        public byte[] Content { get; set; }
    }

    [TSInclude]
    public class IncomingEmailLogDTO
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Message { get; set; }
    }
}
