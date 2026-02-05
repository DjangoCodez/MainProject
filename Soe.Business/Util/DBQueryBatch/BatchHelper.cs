using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Util.BatchHelper
{
    public class BatchHelper
    {        
        private List<int> AllIds { get; set; }
        private List<int> BatchIds { get; set; }

        private int Position;
        private readonly int BatchLength;
        private int Length { get { return this.AllIds.Count; } }

        private BatchHelper(List<int> ids, int batchLength)
        {
            this.AllIds = ids ?? new List<int>();
            this.Position = 0;
            this.BatchLength = batchLength;
        }

        public static BatchHelper Create(List<int> ids, int batchLength = Constants.LINQMAXCOUNTCONTAINS)
        {
            return new BatchHelper(ids, batchLength);
        }

        public bool HasMoreBatches()
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
        public List<int> GetCurrentBatchIds()
        {
            return this.BatchIds.ToList();
        }

    }    
}
