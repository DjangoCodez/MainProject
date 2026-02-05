using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SoftOne.Soe.Common.DTO.ApiExternal
{
    public class AccountIO
    {
        public string Nr { get; set; } 
        public int Id { get; set; } 
        public int DimId { get; set; } 
        public string DimNr { get; set; }  
    }

    public class AccountStdIO : AccountIO
    {
        public string Unit { get; set; }
    }

    public class VoucherSeriesTypeIO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Nr { get; set; }
    }

    public class AccountBalancePartIO
    {
        public List<AccountIO> InternalAccounts { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }

        public AccountBalancePartIO()
        {
            this.InternalAccounts = new List<AccountIO>();
        }
    }

    public class AccountBalanceSumIO
    {
        public AccountStdIO AccountStd { get; set; }
        public List<AccountBalancePartIO> SubSums { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }

        public AccountBalanceSumIO()
        {
            this.SubSums = new List<AccountBalancePartIO>();
        }
    }

    public class TransactionIO
    {
        public int VoucherId { get; set; }
        public int VoucherNr { get; set; }
        public int VoucherRowId { get; set; }
        public DateTime? Date { get; set; }
        public VoucherSeriesTypeIO VoucherSeriesType { get; set; }
        public List<AccountIO> InternalAccounts { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }

        public TransactionIO()
        {
            this.InternalAccounts = new List<AccountIO>();
        }
    }

    public class AccountTransactionsIO
    {
        public AccountStdIO AccountStd { get; set; }
        public List<TransactionIO> Transactions { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }

        public AccountTransactionsIO()
        {
            this.Transactions = new List<TransactionIO>();
        }
    }

    public class GeneralLedgerIO
    {
        public List<AccountBalanceSumIO> IngoingBalances { get; set; }
        public List<AccountTransactionsIO> AccountTransactions { get; set; }
        public List<AccountBalanceSumIO> OutgoingBalances { get; set; }

        public GeneralLedgerIO()
        {
            this.IngoingBalances = new List<AccountBalanceSumIO>();
            this.AccountTransactions = new List<AccountTransactionsIO>();
            this.OutgoingBalances = new List<AccountBalanceSumIO>();
        }
    }

    public class GeneralLedgerParams
    {
        public bool IncludeTransactions { get; set; }
        public DateTime FromDate { get; set; } 
        public DateTime ToDate { get; set; }
    }
}
