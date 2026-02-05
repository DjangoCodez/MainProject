using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class EdiFromFtpJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            EdiManager em = new EdiManager(parameterObject);
            FeatureManager fm = new FeatureManager(parameterObject);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar EDI-jobb");

                    // Execute job
                    var companyEdis = em.GetCompanyEdis().ToDTOs().OrderBy(i => i.CompanyName).ThenBy(i => i.ActorCompanyId).ToList();
                    var companyEdiGroups = companyEdis.GroupBy(i => i.ActorCompanyId).ToList();

                    //Log
                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Hanterar EDI för {0} st företag", companyEdiGroups.Count));

                    foreach (var companyEdiGroup in companyEdiGroups)
                    {
                        #region Prereq

                        //Must have Symbrio EDI
                        var companyEdiSymbrio = companyEdiGroup.FirstOrDefault(i => i.Type == (int)TermGroup_CompanyEdiType.Symbrio);
                        if (companyEdiSymbrio == null)
                            continue;

                        //Company
                        var company = cm.GetCompany(companyEdiGroup.Key);
                        if (company == null)
                            continue;

                        //Re-create EdiManager, language cache needs ParameterObject to be not null
                        parameterObject.SetSoeCompany(company.ToCompanyDTO());
                        em = new EdiManager(parameterObject);

                        #endregion

                        #region Nelfo

                        //Move Nelfo and Lvis files to ftp
                        foreach (var externalCompanyEdi in companyEdiGroup.Where(i => i.Type == (int)TermGroup_CompanyEdiType.Nelfo || i.Type == (int)TermGroup_CompanyEdiType.LvisNet))
                        {
                            if (externalCompanyEdi != null)
                            {
                                if (!Enum.IsDefined(typeof(TermGroup_CompanyEdiType), externalCompanyEdi.Type))
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Okänd edi typ: {0} för företag {1}", externalCompanyEdi.Type, company.Name));
                                    continue;
                                }

                                var ediXE = fm.GetCompanyFeature(company.ActorCompanyId, (int)Feature.Billing_Import_XEEdi);
                                if (ediXE != null && ediXE.SysPermissionId != (int)Permission.None)
                                {
                                    // When new edi is used this is handled in seperate job (EdiAdmin).
                                    continue;
                                }


                                //Log
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Flyttar filer för företag {0}.{1} username {2}) [{3}]", company.ActorCompanyId, company.Name, companyEdiSymbrio.Username, ((TermGroup_CompanyEdiType)externalCompanyEdi.Type).ToString()));

                                // Ignore some files that contains prices, TODO this is a mockup
                                string[] toIgnore = { 
                                    "otrapris-nelfo1.txt",
                                    "V4priserNRF.all",
                                    "V4varefil.zip",
                                    "R4rabatt.txt",
                                    "V4priser.all",
                                    "V4priser.kost",
                                };

                                ActionResult nelfoResult = em.AddEdiFilesToFtp(externalCompanyEdi, sysScheduledJobId: scheduledJob.SysScheduledJobId, batchNr: batchNr, ignoreFilesList: toIgnore);
                                if (nelfoResult.Success)
                                {
                                    //Log
                                    CreateLogEntry(ScheduledJobLogLevel.Success, String.Format("{0} filer funna, {1} filer flyttade", nelfoResult.IntegerValue, nelfoResult.IntegerValue2));
                                }
                                else
                                {
                                    //Log
                                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid flytt av filer: {0} - '{1}'. Företag: {2}", nelfoResult.ErrorNumber, nelfoResult.ErrorMessage, company.Name));
                                }
                            }
                        }

                        #endregion

                        #region Symbrio

                        //Log
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Söker filer för företag {0}.{1} username {2} [Symbrio]", company.ActorCompanyId, company.Name, companyEdiSymbrio.Username));

                        //Add files from ftp
                        ActionResult ediResult = em.AddEdiEntrys(companyEdiSymbrio, TermGroup_EDISourceType.EDI, true, scheduledJob.SysScheduledJobId, batchNr);
                        if (ediResult.Success)
                        {
                            //Log
                            if (ediResult.Keys != null && ediResult.Keys.Count > 0)
                                CreateLogEntry(ScheduledJobLogLevel.Success, String.Format("{0} filer funna, {1} filer behandlade, {2} EDI poster skapade", ediResult.IntegerValue, ediResult.IntegerValue2, ediResult.Keys.Count));
                        }
                        else
                        {
                            //Log
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid skapande av EDI poster: {0} - '{1}'", ediResult.ErrorNumber, ediResult.ErrorMessage));
                        }

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    string msg = String.Format("Fel vid exekvering av jobb: {0}. {1}", ex.Message, SoftOne.Soe.Util.Exceptions.SoeException.GetStackTrace());
                    if (ex.InnerException != null)
                        msg += "\n" + ex.InnerException.Message;
                    CreateLogEntry(ScheduledJobLogLevel.Error, msg);
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
