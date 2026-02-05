using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Project
{
    public static partial class EntityExtensions
    {
        public readonly static Expression<Func<BudgetHead, BudgetHeadProjectDTO>> ProjectBudgetHeadIncludingRows =
        bh => new BudgetHeadProjectDTO()
        {
            BudgetHeadId = bh.BudgetHeadId,
            ParentBudgetHeadId = bh.ParentBudgetHeadId,
            ActorCompanyId = bh.ActorCompanyId,
            Type = bh.Type,
            NoOfPeriods = bh.NoOfPeriods,
            Status = bh.Status,
            ProjectId = bh.ProjectId,
            ProjectNr = bh.Project.Number,
            ProjectName = bh.Project.Name,
            ProjectFromDate = bh.Project.StartDate,
            ProjectToDate = bh.Project.StopDate,
            Name = bh.Name,
            FromDate = bh.FromDate,
            ToDate = bh.ToDate,
            PeriodType = bh.PeriodType ?? (int)TermGroup_ProjectBudgetPeriodType.SinglePeriod,
            Created = bh.Created,
            CreatedBy = bh.CreatedBy,
            Modified = bh.Modified,
            ModifiedBy = bh.ModifiedBy,

            Rows = bh.BudgetRow.Where(r => r.State == (int)SoeEntityState.Active).OrderBy(r => r.Type).Select(r => new BudgetRowProjectDTO()
            {
                BudgetRowId = r.BudgetRowId,
                ParentBudgetRowId = r.ParentBudgetRowId,
                BudgetHeadId = r.BudgetHeadId,
                TimeCodeId = r.TimeCodeId ?? 0,
                TypeCodeName = r.TimeCode.Name ?? "",
                Type = r.Type,
                TotalAmount = r.TotalAmount,
                TotalQuantity = r.TotalQuantity,
                Comment = r.Comment,
                IsLocked = r.Locked,
                HasLogPosts = r.BudgetRowChangeLog.Any(),
                Created = r.Created,
                CreatedBy = r.CreatedBy,
                Modified = r.Modified,
                ModifiedBy = r.ModifiedBy,
            }).ToList(),
        };

        public readonly static Expression<Func<BudgetHead, BudgetHeadProjectDTO>> ProjectBudgetHeadIncludingRowsAndLogs =
        bh => new BudgetHeadProjectDTO()
        {
            BudgetHeadId = bh.BudgetHeadId,
            ParentBudgetHeadId = bh.ParentBudgetHeadId,
            ActorCompanyId = bh.ActorCompanyId,
            Type = bh.Type,
            NoOfPeriods = bh.NoOfPeriods,
            Status = bh.Status,
            ProjectId = bh.ProjectId,
            ProjectNr = bh.Project.Number,
            ProjectName = bh.Project.Name,
            ProjectFromDate = bh.Project.StartDate,
            ProjectToDate = bh.Project.StopDate,
            Name = bh.Name,
            FromDate = bh.FromDate,
            ToDate = bh.ToDate,
            PeriodType = bh.PeriodType ?? (int)TermGroup_ProjectBudgetPeriodType.SinglePeriod,
            Created = bh.Created,
            CreatedBy = bh.CreatedBy,
            Modified = bh.Modified,
            ModifiedBy = bh.ModifiedBy,

            Rows = bh.BudgetRow.Where(r => r.State == (int)SoeEntityState.Active).OrderBy(r => r.Type).Select(r => new BudgetRowProjectDTO()
            {
                BudgetRowId = r.BudgetRowId,
                ParentBudgetRowId = r.ParentBudgetRowId,
                BudgetHeadId = r.BudgetHeadId,
                TimeCodeId = r.TimeCodeId ?? 0,
                TypeCodeName = r.TimeCode.Name ?? "",
                Type = r.Type,
                TotalAmount = r.TotalAmount,
                TotalQuantity = r.TotalQuantity,
                Comment = r.Comment,
                IsLocked = r.Locked,
                HasLogPosts = r.BudgetRowChangeLog.Any(),
                Created = r.Created,
                CreatedBy = r.CreatedBy,
                Modified = r.Modified,
                ModifiedBy = r.ModifiedBy,
                ChangeLogItems = r.BudgetRowChangeLog.OrderByDescending(l => l.Created).Select(l => new BudgetRowProjectChangeLogDTO()
                {
                    BudgetRowChangeLogId = l.BudgetRowChangeLogId,
                    BudgetRowId = l.BudgetRowId,
                    Created = l.Created,
                    FromTotalAmount = l.FromTotalAmount,
                    ToTotalAmount = l.ToTotalAmount,
                    TotalAmountDiff = l.ToTotalAmount - l.FromTotalAmount,
                    FromTotalQuantity = l.FromTotalQuantity,
                    ToTotalQuantity = l.ToTotalQuantity,
                    TotalQuantityDiff = l.ToTotalQuantity - l.FromTotalQuantity,
                    CreatedBy = l.CreatedBy,
                    Comment = l.Comment,
                }).ToList()
            }).ToList(),
        };
    }
}
