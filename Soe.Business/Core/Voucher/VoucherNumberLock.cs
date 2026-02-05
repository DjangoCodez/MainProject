using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class VoucherNumberLock
    {
        private VoucherNumberLockService _locker;
        private VoucherHeadStore _store;
        public VoucherNumberLock(CompEntities entities)
        {
            this._locker = new VoucherNumberLockService(entities);
            this._store = new VoucherHeadStore();
        }
        public VoucherNumberLock AddVoucher(VoucherHead voucherHead)
        {
            if (voucherHead.VoucherSeriesId == 0)
                throw new Exception("Voucher series id is required");

            this._store.AddVoucherHead(voucherHead.VoucherSeriesId, voucherHead);
            return this;
        }
        public VoucherNumberLock AddVouchers(List<VoucherHead> voucherHeads)
        {
            foreach (var voucherHead in voucherHeads)
                this.AddVoucher(voucherHead);
            
            return this;
        }
        public void SetVoucherNumbers()
        {
            /// <summary>
            /// Throws if unable to allocate voucher numbers.
            /// </summary>
            foreach (var (series, heads) in this._store.GetValues())
                this._locker.AllocateVoucherNumbers(series, heads);
        }
    }

    /// <summary>
    /// Used as a wrapper around the process of resering a voucher number.
    /// </summary>
    class VoucherNumberLockService
    {
        private CompEntities _entities;
        public VoucherNumberLockService(CompEntities entities)
        {
            this._entities = entities;
        }
        private VoucherSeries GetVoucherSeries(int voucherSeriesId)
        {
            // An update lock is used to ensure that no other process acquires the voucher number until
            // the current transaction is completed.
            // Rowlock is used to ensure that the lock is not escalated to a table lock.
            var voucherSeries = this._entities.ExecuteStoreQuery<VoucherSeries>(
                "SELECT TOP 1 * FROM VoucherSeries WITH (UPDLOCK, ROWLOCK) WHERE VoucherSeriesId = @p0", voucherSeriesId
                ).FirstOrDefault();

            if (voucherSeries is null)
                throw new ObjectNotFoundException("VoucherSeries");

            return voucherSeries;
        }
        private List<long> AllocateVoucherNumbers(VoucherSeries voucherSeriesRef, int count, DateTime voucherDateLatest)
        {
            //Detach the existing instance, so that we can replace it fresh data.
            this.DetachVoucherSeries(voucherSeriesRef);
            var voucherSeriesId = voucherSeriesRef.VoucherSeriesId;
            var voucherSeries = GetVoucherSeries(voucherSeriesId);
            // Start tracking the new instance, ensuring that the changes are persisted when the transaction completes.
            this.TrackVoucherSeries(voucherSeries);

            var start = voucherSeries.VoucherNrLatest.GetValueOrDefault();
            var end = start + count;
            voucherSeries.VoucherNrLatest = end;
            voucherSeries.VoucherDateLatest = voucherDateLatest;

            var numbers = new List<long>();
            for (long i = start + 1; i <= end; i++)
                numbers.Add(i);

            return numbers;
        }
        public void AllocateVoucherNumbers(VoucherSeries voucherSeries, List<VoucherHead> voucherHeads)
        {
            if (voucherHeads.Count == 0)
                return;

            int numbersToAllocate = voucherHeads.Count;
            DateTime lastDate = voucherHeads.Last().Date;

            // Get available voucher numbers
            var allocatedNumbers = this.AllocateVoucherNumbers(
                voucherSeries,
                numbersToAllocate, 
                lastDate);

            // Apply the voucher numbers.
            foreach (var head in voucherHeads)
                head.VoucherNr = FindNumber(allocatedNumbers, head.VoucherNr);
        }
        private long FindNumber(List<long> numbers, long preliminaryVoucherNumber)
        {
            // The key here is that we try to maintain the voucher number that has been assigned to the voucher head.
            // If that number is not available, we'll assign a new one.
            bool findFirst = preliminaryVoucherNumber == 0;
            int firstFree = -1;
            for (int i = 0; i < numbers.Count; i++)
            {
                var number = numbers[i];
                if (number != -1)
                {
                    if (firstFree == -1)
                        firstFree = i;
                    
                    if (findFirst)
                    {
                        numbers[i] = -1;
                        return number;
                    }
                    if (number == preliminaryVoucherNumber)
                    {
                        numbers[i] = -1;
                        return number;
                    }
                }
            }
            if (firstFree == -1 || numbers[firstFree] == -1)
                throw new Exception("No free voucher numbers available");

            var firstFreeNumber = numbers[firstFree];
            numbers[firstFree] = -1;
            return firstFreeNumber;
        }
        private void TrackVoucherSeries(VoucherSeries voucherSeries)
        {
            this._entities.VoucherSeries.Attach(voucherSeries);
        }
        private void DetachVoucherSeries(VoucherSeries voucherSeries)
        {
            this._entities.VoucherSeries.Detach(voucherSeries);
        }
    }
    class VoucherHeadStore
    {
        private Dictionary<int, List<VoucherHead>> _voucherHeads { get; set; }
        private Dictionary<int, VoucherSeries> _voucherSeries { get; set; }
        public VoucherHeadStore()
        {
            this._voucherHeads = new Dictionary<int, List<VoucherHead>>();
            this._voucherSeries = new Dictionary<int, VoucherSeries>();
        }

        public void AddVoucherHead(int voucherSeriesId, VoucherHead voucherHead)
        {
            // Keep references to both VoucherHeads and corresponding Series.
            // That way we can detach the series & update the heads easily.
            if (voucherSeriesId == 0)
                throw new Exception("Voucher series id is required");

            if (!this._voucherHeads.ContainsKey(voucherSeriesId))
                this._voucherHeads.Add(voucherSeriesId, new List<VoucherHead>());

            this._voucherHeads[voucherSeriesId].Add(voucherHead);
            this.AddVoucherSeries(voucherHead.VoucherSeries);
        }
        public void AddVoucherSeries(VoucherSeries voucherSeries)
        {
            if (voucherSeries is null)
                throw new Exception("Voucher series reference missing on VoucherHead");

            this._voucherSeries[voucherSeries.VoucherSeriesId] = voucherSeries;
        }
        private List<VoucherHead> GetVoucherHeads(int voucherSeriesId)
        {
            return this._voucherHeads.TryGetValue(voucherSeriesId, out var voucherHeads) ? 
                voucherHeads : 
                new List<VoucherHead>();
        }
        public List<(VoucherSeries, List<VoucherHead>)> GetValues()
        {
            return this._voucherSeries.Select(x => (x.Value, this.GetVoucherHeads(x.Key))).ToList();
        }
    }
}
