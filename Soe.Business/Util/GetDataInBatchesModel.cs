using System;
using System.Collections.Generic;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Business.Util
{
    public class GetDataInBatchesModel
    {
        public CompEntities Entities { get; }
        public int ActorCompanyId { get; }
        private List<int> AllIds { get; set; }
        public List<int> BatchIds { get; private set; }
        public DateTime? StartDate { get; }
        public DateTime? StopDate { get; }        
        
        private bool IsStarted;
        private int Position;
        private int BatchLength;
        private int Length { get { return this.AllIds.Count; } }

        private GetDataInBatchesModel(CompEntities entities, int actorCompanyId, List<int> ids, DateTime? startDate, DateTime? stopDate)
        {
            this.Entities = entities;
            this.ActorCompanyId = actorCompanyId;
            this.AllIds = ids ?? new List<int>();
            this.StartDate = startDate?.Date;
            this.StopDate = stopDate?.Date;
        }

        public static GetDataInBatchesModel Create(CompEntities entities, int actorCompanyId, List<int> ids)
        {
            return new GetDataInBatchesModel(entities, actorCompanyId, ids, null, null);
        }
        public static GetDataInBatchesModel Create(CompEntities entities, int actorCompanyId, List<int> ids, DateTime startDate, DateTime stopDate)
        {
            return new GetDataInBatchesModel(entities, actorCompanyId, ids, startDate, stopDate);
        }

        public void Start(int batchLength)
        {
            this.IsStarted = true;
            this.Position = 0;
            this.BatchLength = batchLength;
        }
        public bool HasHasMoreBatches()
        {
            bool hasMoreBatches = this.Position <= this.Length;
            if (hasMoreBatches)
                this.BatchIds = this.AllIds.SkipAndTake(this.Position, this.BatchLength);
            return hasMoreBatches; 
        }
        public void MoveToNextBatch()
        {
            this.Position += this.BatchLength;
        }
        public bool HasIds()
        {
            return this.IsStarted ? !this.BatchIds.IsNullOrEmpty() : !this.AllIds.IsNullOrEmpty();
        }
    }

    public static class GetDataInBatchesModelExtensions
    {
        public static bool IsValid(this GetDataInBatchesModel e, bool requireDates = false)
        {
            if (e == null || e.Entities == null || e.ActorCompanyId <= 0 || !e.HasIds())
                return false;
            if (requireDates)
            { 
                if (!e.StartDate.HasValue || !e.StopDate.HasValue)
                    return false;
                if (e.StartDate > e.StopDate)
                    return false;
            }
            return true;
        }
    }
}
