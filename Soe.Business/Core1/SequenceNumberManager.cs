using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class SequenceNumberManager : ManagerBase
    {
        #region Variables

        #endregion

        #region Ctor

        public SequenceNumberManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region SequenceNumber

        /// <summary>
        /// Get next sequence number based on parameter entityName and startNbr
        /// </summary>
        /// <param name="entities">CompEntities context</param>
        /// <param name="actorCompanyId">Company Id</param>
        /// <param name="entityName">Entity name for the sequence number</param>
        /// <param name="defaultStartNbr">Default start number if entity name does not exist</param>
        /// <param name="saveChanges">Save changes to database or just in entities context</param>
        /// <returns>Next number in sequence or defaultStartNbr if new sequence</returns>
        public int GetNextSequenceNumber(CompEntities entities, int actorCompanyId, string entityName, int defaultStartNbr, bool saveChanges)
        {
            // Get last used sequence for specified entity
            SequenceNumber seqNbr = (from sn in entities.SequenceNumber
                                     where sn.ActorCompanyId == actorCompanyId &&
                                     sn.Entity == entityName
                                     select sn).FirstOrDefault();

            if (seqNbr == null && !saveChanges)
            {
                seqNbr = entities.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Where(x => (x.Entity is SequenceNumber)).Select(x => (SequenceNumber)x.Entity).FirstOrDefault(x => x.Entity == entityName);
            }

            // Update last used sequence
            if (seqNbr == null)
            {
                seqNbr = new SequenceNumber()
                {
                    Company = CompanyManager.GetCompany(entities, actorCompanyId),
                    Entity = entityName,
                    Date = DateTime.Now,
                    LatestSequence = defaultStartNbr
                };

                // Decide if changes should be saved to database or only ObjectContext
                if (saveChanges)
                    AddEntityItem(entities, seqNbr, "SequenceNumber");
                else
                    entities.SequenceNumber.AddObject(seqNbr);
            }
            else
            {
                if (defaultStartNbr > (seqNbr.LatestSequence + 1))
                {
                    // Update sequence number for specified entity
                    seqNbr.Date = DateTime.Now;
                    seqNbr.LatestSequence = defaultStartNbr;
                }
                else
                {
                    // Update sequence number for specified entity
                    seqNbr.Date = DateTime.Now;
                    seqNbr.LatestSequence++;
                }

                // Decide if changes should be saved to database or only ObjectContext
                if (saveChanges)
                    SaveEntityItem(entities, seqNbr);
            }

            return seqNbr.LatestSequence;
        }

        /// <summary>
        /// Get next sequence number based on parameter entityName and startNbr
        /// </summary>
        /// <param name="entities">CompEntities context</param>
        /// <param name="actorCompanyId">Company Id</param>
        /// <param name="entityName">Entity name for the sequence number</param>
        /// <param name="defaultStartNbr">Default start number if entity name does not exist</param>
        /// <param name="saveChanges">Save changes to database or just in entities context</param>
        /// <returns>Next number in sequence or defaultStartNbr if new sequence</returns>
        public int GetNextSequenceNumber(CompEntities entities, TransactionScope transaction, int actorCompanyId, string entityName, int defaultStartNbr, bool saveChanges)
        {
            // Get last used sequence for specified entity
            SequenceNumber seqNbr = (from sn in entities.SequenceNumber
                                     where sn.ActorCompanyId == actorCompanyId &&
                                     sn.Entity == entityName
                                     select sn).FirstOrDefault();

            // Update last used sequence
            if (seqNbr == null)
            {
                seqNbr = new SequenceNumber()
                {
                    Company = CompanyManager.GetCompany(entities, actorCompanyId),
                    Entity = entityName,
                    Date = DateTime.Now,
                    LatestSequence = defaultStartNbr
                };

                // Decide if changes should be saved to database or only ObjectContext
                if (saveChanges)
                    AddEntityItem(entities, seqNbr, "SequenceNumber", transaction);
                else
                    entities.SequenceNumber.AddObject(seqNbr);
            }
            else
            {
                // Update sequence number for specified entity
                seqNbr.Date = DateTime.Now;
                seqNbr.LatestSequence++;

                // Decide if changes should be saved to database or only ObjectContext
                if (saveChanges)
                    SaveEntityItem(entities, seqNbr);
            }

            return seqNbr.LatestSequence;
        }

        /// <summary>
        /// Get next sequence number based on parameter entityName and startNbr
        /// </summary>
        /// <param name="entities">CompEntities context</param>
        /// <param name="actorCompanyId">Company Id</param>
        /// <param name="entityName">Entity name for the sequence number</param>
        /// <param name="defaultStartNbr">Default start number if entity name does not exist</param>
        /// <param name="saveChanges">Save changes to database or just in entities context</param>
        /// <returns>Next number in sequence or defaultStartNbr if new sequence</returns>
        public int GetNextSequenceNumberCheckRecords(CompEntities entities, int actorCompanyId, int entity, int defaultStartNbr, string entityName, bool saveChanges)
        {
            //Check for open record
            SequenceNumberRecord openSeqNrRec = (from sn in entities.SequenceNumberRecord
                                                 where sn.ActorCompanyId == actorCompanyId &&
                                                 sn.Entity == entity &&
                                                 sn.State == (int)SoeEntityState.Active
                                                 orderby sn.RecordId descending
                                                 select sn).FirstOrDefault();

            if (openSeqNrRec == null)
            {
                // Get last used sequence for specified entity
                SequenceNumber seqNbr = (from sn in entities.SequenceNumber
                                         where sn.ActorCompanyId == actorCompanyId &&
                                         sn.Entity == entityName
                                         select sn).FirstOrDefault();

                // Update last used sequence
                if (seqNbr == null)
                {
                    seqNbr = new SequenceNumber()
                    {
                        Company = CompanyManager.GetCompany(entities, actorCompanyId),
                        Entity = entityName,
                        Date = DateTime.Now,
                        LatestSequence = defaultStartNbr
                    };

                    // Decide if changes should be saved to database or only ObjectContext
                    if (saveChanges)
                        AddEntityItem(entities, seqNbr, "SequenceNumber");
                    else
                        entities.SequenceNumber.AddObject(seqNbr);
                }
                else
                {
                    // Update sequence number for specified entity
                    seqNbr.Date = DateTime.Now;
                    seqNbr.LatestSequence++;

                    // Decide if changes should be saved to database or only ObjectContext
                    if (saveChanges)
                        SaveEntityItem(entities, seqNbr);
                }

                return seqNbr.LatestSequence;
            }
            else
            {
                return openSeqNrRec.SequenceNumber;
            }
        }

        /// <summary>
        /// Get last used sequence number based on parameter entityName
        /// </summary>
        /// <param name="actorCompanyId">Company Id</param>
        /// <param name="entityName">Entity name for the sequence number</param>
        /// <returns>Last used number in sequence or 0 if new sequence</returns>
        public int GetLastUsedSequenceNumber(int actorCompanyId, string entityName)
        {
            // Get last used sequence for specified entity
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SequenceNumber.NoTracking();
            SequenceNumber seqNbr = (from sn in entities.SequenceNumber
                                     where sn.ActorCompanyId == actorCompanyId &&
                                     sn.Entity == entityName
                                     select sn).FirstOrDefault();

            if (seqNbr != null)
                return seqNbr.LatestSequence;

            return 0;
        }

        public ActionResult UpdateSequenceNumber(CompEntities entities, int actorCompanyId, string entityName, int number)
        {
            // Get sequence number for specified entity
            SequenceNumber seqNbr = (from sn in entities.SequenceNumber
                                     where sn.ActorCompanyId == actorCompanyId &&
                                     sn.Entity == entityName
                                     select sn).FirstOrDefault();

            if (seqNbr == null)
            {
                // Add sequence number for specified entity
                seqNbr = new SequenceNumber()
                {
                    Company = CompanyManager.GetCompany(entities, actorCompanyId),
                    Entity = entityName,
                    Date = DateTime.Now,
                    LatestSequence = number
                };

                return AddEntityItem(entities, seqNbr, "SequenceNumber");
            }
            else
            {
                if (entityName == "Employee" && seqNbr.LatestSequence > number)
                    number = seqNbr.LatestSequence;

                // Update sequence number for specified entity
                seqNbr.Date = DateTime.Now;
                seqNbr.LatestSequence = number;

                return SaveEntityItem(entities, seqNbr);
            }
        }

        public ActionResult UpdateSequenceNumber(int actorCompanyId, string entityName, int number)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                // Get sequence number for specified entity
                SequenceNumber seqNbr = (from sn in entities.SequenceNumber
                                         where sn.ActorCompanyId == actorCompanyId &&
                                         sn.Entity == entityName
                                         select sn).FirstOrDefault();

                if (seqNbr == null)
                {
                    // Add sequence number for specified entity
                    seqNbr = new SequenceNumber()
                    {
                        Entity = entityName,
                        Date = DateTime.Now,
                        LatestSequence = number,

                        //References
                        Company = CompanyManager.GetCompany(entities, actorCompanyId),
                    };

                    result = AddEntityItem(entities, seqNbr, "SequenceNumber");
                }
                else
                {
                    // Update sequence number for specified entity
                    seqNbr.Date = DateTime.Now;
                    seqNbr.LatestSequence = number;

                    result = SaveEntityItem(entities, seqNbr);
                }
            }

            return result;
        }

        /// <summary>
        /// Delete sequence number for specified company and entity
        /// </summary>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="entityName">Name of entity to delete sequence number for</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteSequenceNumber(int actorCompanyId, string entityName)
        {
            // Get sequence number for specified entity
            using (CompEntities entities = new CompEntities())
            {
                SequenceNumber seqNbr = (from sn in entities.SequenceNumber
                                         where sn.ActorCompanyId == actorCompanyId &&
                                         sn.Entity == entityName
                                         select sn).FirstOrDefault();

                // Delete sequence number
                if (seqNbr != null)
                    return DeleteEntityItem(entities, seqNbr);

                // No sequence number found, return success
                return new ActionResult();
            }
        }


        public int GetNextSequenceNumberLock(CompEntities entities, int actorCompanyId, string entityName, int defaultStartNbr, bool saveChanges)
        {
            var seqNbr = GetSequenceNumberLock(entities,actorCompanyId, entityName);

            if (seqNbr == null && !saveChanges)
            {
                seqNbr = entities.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Where(x => (x.Entity is SequenceNumber)).Select(x => (SequenceNumber)x.Entity).FirstOrDefault(x => x.Entity == entityName);
            }

            // Update last used sequence
            if (seqNbr == null)
            {
                seqNbr = new SequenceNumber()
                {
                    Company = CompanyManager.GetCompany(entities, actorCompanyId),
                    Entity = entityName,
                    Date = DateTime.Now,
                    LatestSequence = defaultStartNbr
                };

                // Decide if changes should be saved to database or only ObjectContext
                if (saveChanges)
                    AddEntityItem(entities, seqNbr, "SequenceNumber");
                else
                    entities.SequenceNumber.AddObject(seqNbr);
            }
            else
            {
                if (defaultStartNbr > (seqNbr.LatestSequence + 1))
                {
                    // Update sequence number for specified entity
                    seqNbr.Date = DateTime.Now;
                    seqNbr.LatestSequence = defaultStartNbr;
                }
                else
                {
                    // Update sequence number for specified entity
                    seqNbr.Date = DateTime.Now;
                    seqNbr.LatestSequence++;
                }

                // Decide if changes should be saved to database or only ObjectContext
                if (saveChanges)
                    SaveEntityItem(entities, seqNbr);
            }

#if DEBUG
            Debug.WriteLine("GetNextSequenceNumberLock:" + seqNbr.LatestSequence);
#endif
            return seqNbr.LatestSequence;
        }

        private static SequenceNumber GetSequenceNumberLock(CompEntities entities, int actorCompanyId, string entityName)
        {
            var seqNbr = entities.ObjectStateManager.GetObjectStateEntries(EntityState.Unchanged | EntityState.Modified).Where(x => (x.Entity is SequenceNumber)).Select(x => (SequenceNumber)x.Entity).FirstOrDefault(x => x.Entity == entityName);

            if (seqNbr == null)
            {
                // Rowlock is used to ensure that the lock is not escalated to a table lock.
                seqNbr = entities.ExecuteStoreQuery<SequenceNumber>(
                    "SELECT TOP 1 * FROM SequenceNumber WITH (UPDLOCK, ROWLOCK) WHERE ActorCompanyId = @p0 AND Entity = @p1", actorCompanyId, entityName
                    ).FirstOrDefault();

                if (seqNbr != null)
                    entities.SequenceNumber.Attach(seqNbr);
            }
            

            return seqNbr;
        }

        #endregion

        #region SequenceNumberRecord

        public SequenceNumberRecord GetLastActiveSequenceNumberRecord(CompEntities entities, int actorCompanyId, int entity)
        {
            return (from snr in entities.SequenceNumberRecord
                    where snr.ActorCompanyId == actorCompanyId &&
                    snr.Entity == entity &&
                    snr.State == (int)SoeEntityState.Active
                    orderby snr.SequenceNumberRecordId descending
                    select snr).FirstOrDefault();
        }

        public ActionResult SaveSequenceNumberRecord(CompEntities entities, int entity, int recordId, int sequenceNumber, int actorCompanyId)
        {
            SequenceNumberRecord rec = new SequenceNumberRecord()
            {
                ActorCompanyId = actorCompanyId,
                Entity = entity,
                RecordId = recordId,
                SequenceNumber = sequenceNumber,
                State = (int)SoeEntityState.Active,
            };

            return AddEntityItem(entities, rec, "SequenceNumberRecord");
        }

        public ActionResult InactivateSequenceNumberRecord(CompEntities entities, int sequenceNumberRecordId)
        {
            ActionResult result = new ActionResult(true);

            SequenceNumberRecord rec = (from r in entities.SequenceNumberRecord
                                        where r.SequenceNumberRecordId == sequenceNumberRecordId
                                        select r).FirstOrDefault();

            if (rec != null)
            {
                rec.State = (int)SoeEntityState.Inactive;
                result = SaveEntityItem(entities, rec);
            }

            return result;
        }


        public ActionResult DeleteSequenceNumberRecord(CompEntities entities, int sequenceNumberRecordId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            SequenceNumberRecord rec = (from r in entities.SequenceNumberRecord
                                        where r.SequenceNumberRecordId == sequenceNumberRecordId
                                        select r).FirstOrDefault();

            if (rec != null)
            {
                result = ChangeEntityState(entities, rec, SoeEntityState.Deleted, true);
            }

            return result;
        }

        public ActionResult DeleteSequenceNumberRecords(List<int> records, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SequenceNumberRecord.NoTracking();
            return DeleteSequenceNumberRecords(entities, records, actorCompanyId);
        }

        public ActionResult DeleteSequenceNumberRecords(CompEntities entities, List<int> records, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            foreach (int recId in records)
            {

                SequenceNumberRecord rec = (from r in entities.SequenceNumberRecord
                                            where r.SequenceNumberRecordId == recId
                                            select r).FirstOrDefault();

                if (rec != null)
                {
                    result = ChangeEntityState(entities, rec, SoeEntityState.Deleted, true);
                }
            }

            return result;
        }

        public ActionResult DeleteSequenceNumberRecords(int sequenceNumber, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SequenceNumberRecord.NoTracking();
            return DeleteSequenceNumberRecords(entities, sequenceNumber, actorCompanyId);
        }

        public ActionResult DeleteSequenceNumberRecords(CompEntities entities, int sequenceNumber, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            var records = (from r in entities.SequenceNumberRecord
                           where r.SequenceNumber == sequenceNumber &&
                           r.ActorCompanyId == actorCompanyId &&
                           r.State == (int)SoeEntityState.Active
                           select r);

            foreach (SequenceNumberRecord rec in records)
            {
                result = ChangeEntityState(entities, rec, SoeEntityState.Deleted, true);
            }

            return result;
        }

        #endregion
    }
}
