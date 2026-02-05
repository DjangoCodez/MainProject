using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Business.Core.PaymentIO.Nets
{
    #region File 

    public class NetsFile
    {
        public StartTransmissionRecord StartTransmissionRecord { get; set; }
        public EndTransmissionRecord EndTransmissionRecord { get; set; }
        public StartAssignmentRecord StartAssignmentRecord { get; set; }
        public EndAssignmentRecord EndAssignmentRecord { get; set; }
        public List<Section> Sections { get; set; }
        public bool IsValid()
        {
            if(!StartTransmissionRecord.IsValid())
                return false;
            if (!EndTransmissionRecord.IsValid())
                return false;
            if (!StartAssignmentRecord.IsValid())
                return false;
            if (!EndTransmissionRecord.IsValid())
                return false;
            foreach (Section section in Sections)
            {
                if (!section.IsValid())
                    return false;
            }
            return true;
        }
        public NetsFile()
        {
            Sections = new List<Section>();
        }
    }

    #endregion

    #region Section - Level 1

    public class Section
    {
        public List<IRecord> Posts { get; set; }
        public bool IsValid()
        {
            foreach (IRecord post in Posts)
            {
                if (!post.IsValid())
                    return false;
            }
            return true;
        }
        public Section()
        {
            Posts = new List<IRecord>();
        }
    }

    #endregion

    #region Posts - Level 3

    public class StartTransmissionRecord
    {
        #region Members

        public int RecordType { get; set; }
        public string DataSender { get; set; }
        public int TransmissionNumber { get; set; }
		
        #endregion

        #region Constructors
        public StartTransmissionRecord(string item)
        {
 
        }

        public StartTransmissionRecord(string dataSender, int transmissionNumber)
        {
            RecordType = (int)NetsRecordType.StartTransmissionRecord;
            DataSender = dataSender;
            TransmissionNumber = transmissionNumber;
           
        }
        #endregion

        #region Help methods
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(Utilities.NETS_FORMAT_CODE);   
            sb.Append(Utilities.NETS_SERVICE_CODE_TRANSMISSION); 
            sb.Append(Utilities.NETS_TRANSMISSION_TYPE);
            sb.Append(RecordType.ToString());
            sb.Append(DataSender.Trim().AddLeadingZeros(8)); 
            sb.Append(TransmissionNumber.ToString("0000000")); 
            sb.Append(Utilities.NETS_DATA_RECIPIENT); 
            sb.Append('0', 49); 
            return sb.ToString();
        }
        public bool IsValid()
        {
            return RecordType == (int)NetsRecordType.StartTransmissionRecord;
        }

        #endregion
    }

    public class StartAssignmentRecord : IRecord
    {
        #region Members
       
        public int RecordType { get; set; }
        public string AgreementId { get; set; }
        public int AssignmentNumber { get; set; }
        public string AssignmentAccount { get; set; }
       
       
        #endregion

        #region Constructors
        public StartAssignmentRecord()
        {
           // PostType = (int)NetsRecordType.StartAssignmentRecord;
        }
        public StartAssignmentRecord(string agreementId, int assignmentNumber, string assignmentAccount)
        {
            RecordType = (int)NetsRecordType.StartAssignmentRecord;
            AgreementId = agreementId;
            AssignmentNumber = assignmentNumber;
            AssignmentAccount = assignmentAccount;
        }
        #endregion

        #region Help methods

        public bool IsValid()
        {
            return RecordType == (int)NetsRecordType.StartAssignmentRecord;
        }

        public override string ToString() 
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(Utilities.NETS_FORMAT_CODE);
            sb.Append(Utilities.NETS_SERVICE_CODE_OTHER);
            sb.Append(Utilities.NETS_ASSIGNMENT_TYPE);
            sb.Append(RecordType.ToString());
            sb.Append(AgreementId.Trim().AddLeadingZeros(9));
            sb.Append(AssignmentNumber.ToString("0000000"));
            sb.Append(AssignmentAccount.Trim().AddLeadingZeros(11));
            sb.Append('0', 45);
            return sb.ToString();
        }
        #endregion
    }
    
    public class EndAssignmentRecord : IRecord
    {
        #region Members
        public int RecordType { get; set; }
        public int NumberOfTransactions { get; set; }
        public int NumberOfRecords { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime EarliestPaymentDate { get; set; }
        public DateTime LatestPaymentDate { get; set; }

        #endregion

        #region Constructors
        public EndAssignmentRecord(string item)
        {
           
        }

        public EndAssignmentRecord(decimal totalAmount, int numberOfTransactions, int numberOfRecords, DateTime earliestPaymentDate, DateTime latestPaymentDate)
        {
            RecordType = (int)NetsRecordType.EndAssignmentRecord;
            TotalAmount = totalAmount;
            NumberOfTransactions = numberOfTransactions;
            NumberOfRecords = numberOfRecords;
            EarliestPaymentDate = earliestPaymentDate;
            LatestPaymentDate = latestPaymentDate;
        }
        #endregion

        #region Help methods
        public bool IsValid()
        {
            return RecordType == (int)NetsRecordType.EndAssignmentRecord;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(Utilities.NETS_FORMAT_CODE);
            sb.Append(Utilities.NETS_SERVICE_CODE_OTHER);
            sb.Append(Utilities.NETS_ASSIGNMENT_TYPE);
            sb.Append(RecordType.ToString());
            sb.Append(NumberOfTransactions.ToString("00000000"));
            sb.Append(NumberOfRecords.ToString("00000000"));
            sb.Append(TotalAmount.ToString("00000000000000000"));
            sb.Append(EarliestPaymentDate.ToString("ddMMyy"));
            sb.Append(LatestPaymentDate.ToString("ddMMyy"));
            sb.Append('0', 27);
            return sb.ToString();
        }
        #endregion
    }

    public class EndTransmissionRecord : IRecord
    {
        #region Members
        public int RecordType { get; set; }
        public int NumberOfTransactions { get; set; }
        public int NumberOfRecords { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime EarliestPaymentDate { get; set; }
       
        #endregion

        #region Constructors
        public EndTransmissionRecord(string item)
        {
            //PostType = Utilities.GetNumeric(item, 0, 1);
            //CustomerNumber = item.Substring(1, 5);
            //SenderAccount = Utilities.GetNumeric(item, 6, 10);
            //SenderCode = item.Substring(16, 2);
            //TotalAmount = Utilities.GetNumeric(item, 18, 13);
            //TotalNumberOfPosts = Utilities.GetNumeric(item, 31, 6);
            //CurrencyCodePocket = item.Substring(54, 3);
            //CurrencyCodeAmount = item.Substring(57, 3);
        }

        public EndTransmissionRecord(decimal totalAmount, int numberOfTransactions, int numberOfRecords, DateTime earliestPaymentDate)
        {
            RecordType = (int)NetsRecordType.EndTransmissionRecord;
            TotalAmount = totalAmount;
            NumberOfTransactions = numberOfTransactions;
            NumberOfRecords = numberOfRecords;
            EarliestPaymentDate = earliestPaymentDate;
        }
        #endregion

        #region Help methods
        public bool IsValid()
        {
            return RecordType == (int)NetsRecordType.EndTransmissionRecord;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(Utilities.NETS_FORMAT_CODE);
            sb.Append(Utilities.NETS_SERVICE_CODE_TRANSMISSION);
            sb.Append(Utilities.NETS_TRANSMISSION_TYPE);
            sb.Append(RecordType.ToString());
            sb.Append(NumberOfTransactions.ToString("00000000"));
            sb.Append(NumberOfRecords.ToString("00000000"));
            sb.Append(TotalAmount.ToString("00000000000000000"));
            sb.Append(EarliestPaymentDate.ToString("ddMMyy"));
            sb.Append('0', 33);
            return sb.ToString();
        }
        #endregion
    }

    public class TransactionRecord1 : IRecord
    {

        #region Members
        public int RecordType { get; set; }
        public int TransactionNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string KID { get; set; }
       
        #endregion

        #region Constructors
        public TransactionRecord1(string item)
        {
            //PostType = Utilities.GetNumeric(item, 0, 1);
            //PaymentMethod = Utilities.GetNumeric(item, 3, 1);
            //ReceiverIdentity = item.Substring(4, 10);
            //NumberOfPostsForEachRecipient = Utilities.GetNumeric(item, 24, 4);
        }
        public TransactionRecord1(int transactionNumber, DateTime paymentDate, string accountNumber, decimal amount, string kid)
        {
            RecordType = (int)NetsRecordType.TransactionRecord1;
            TransactionNumber = transactionNumber;
            PaymentDate = paymentDate;
            AccountNumber = accountNumber;
            Amount = amount;
            KID = kid;
        }

       
        #endregion

        #region Help methods
        public bool IsValid()
        {
            return RecordType == (int)NetsRecordType.TransactionRecord1;
        }
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(Utilities.NETS_FORMAT_CODE);
            sb.Append(Utilities.NETS_SERVICE_CODE_OTHER);
            sb.Append(Utilities.NETS_TRANSACTION_TYPE);
            sb.Append(RecordType.ToString());
            sb.Append(TransactionNumber.ToString("0000000"));
            sb.Append(PaymentDate.ToString("ddMMyy"));
            sb.Append(AccountNumber.Trim().AddLeadingZeros(11));
            sb.Append(Amount.ToString("00000000000000000"));
            sb.Append(KID.PadLeft(25));
            sb.Append('0', 6);
            return sb.ToString();
        }
        #endregion
    }

    public class TransactionRecord2 : IRecord
    {
        #region Members
        public int RecordType { get; set; }
        public int TransactionNumber { get; set; }
        public string RecieverName { get; set; }
        public string InternalReference { get; set; }
        public string ExternalReference { get; set; }
      
        #endregion

        #region Constructors



        public TransactionRecord2(int transactionNumber, string recieverName, string internalReference, string externalReference)
        {
            RecordType = (int)NetsRecordType.TransactionRecord2;
            TransactionNumber = transactionNumber;
            RecieverName = recieverName;
            InternalReference = internalReference;
            ExternalReference = externalReference;
            
        }

      
        #endregion

        #region Help methods
        public bool IsValid()
        {
            return RecordType == (int)NetsRecordType.TransactionRecord2;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(Utilities.NETS_FORMAT_CODE);
            sb.Append(Utilities.NETS_SERVICE_CODE_OTHER);
            sb.Append(Utilities.NETS_TRANSACTION_TYPE);
            sb.Append(RecordType.ToString());
            sb.Append(TransactionNumber.ToString("0000000"));
            if (RecieverName.Length > 10)
                sb.Append(RecieverName.Substring(0,10));
            else
                sb.Append(RecieverName.PadRight(10));
            if (InternalReference.Length > 25)
                sb.Append(InternalReference.Substring(0, 25));
            else
                sb.Append(InternalReference.PadRight(25));
            if (ExternalReference.Length > 25)
                sb.Append(ExternalReference.Substring(0, 25));
            else
                sb.Append(ExternalReference.PadRight(25));
            sb.Append('0', 5);
            return sb.ToString();
        }
        #endregion
    }

    #endregion
    
    #region Interfaces

    public interface IRecord
    {
        int RecordType { get; set; }
        bool IsValid();
    }

    #endregion
}
