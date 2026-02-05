using Soe.WebApi.Controllers;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time")]
    public class TimeCodeController : SoeApiController
    {
        #region Variables

        private readonly TimeCodeManager tcm;
        private readonly ProductManager prm;

        #endregion

        #region Constructor

        public TimeCodeController(TimeCodeManager tcm, ProductManager prm)
        {
            this.tcm = tcm;
            this.prm = prm;
        }

        #endregion

        #region TimeCode

        [HttpGet]
        [Route("TimeCode/{timeCodeType:int}/{onlyActive:bool}/{loadPayrollProducts:bool}/{onlyWithInvoiceProduct:bool}")]
        public IHttpActionResult GetTimeCodes(SoeTimeCodeType timeCodeType, bool onlyActive, bool loadPayrollProducts, bool onlyWithInvoiceProduct)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodes(base.ActorCompanyId, timeCodeType, onlyActive, loadPayrollProducts, onlyWithInvoiceProduct).ToDTOs(loadPayrollProducts, true));
        }
        
        [HttpGet]
        [Route("GetTimeCodesGrid/{timeCodeType:int}/{onlyActive:bool}/{loadPayrollProducts:bool}/{timeCodeId:int?}")]
        public IHttpActionResult GetTimeCodesGrid(int timeCodeType, bool onlyActive, bool loadPayrollProducts, int? timeCodeId = null)
        {
            var codeType = (SoeTimeCodeType)timeCodeType;
            return Content(HttpStatusCode.OK, tcm.GetTimeCodes(base.ActorCompanyId, (SoeTimeCodeType)timeCodeType, onlyActive, loadPayrollProducts, timeCodeId: timeCodeId).ToGridDTOs(loadPayrollProducts, base.GetTermGroupContent(TermGroup.YesNo), base.GetTermGroupContent(TermGroup.TimeCodeClassification)));
        }

        [HttpGet]
        [Route("TimeCode/{timeCodeType:int}/{onlyActive:bool}/{addEmptyRow:bool}/{concatCodeAndName:bool}/{loadPayrollProducts:bool}/{onlyWithInvoiceProduct:bool}")]
        public IHttpActionResult GetTimeCodesDictByType(int timeCodeType, bool onlyActive, bool addEmptyRow, bool concatCodeAndName, bool loadPayrollProducts, bool onlyWithInvoiceProduct)
        {
            SoeTimeCodeType codeType = (SoeTimeCodeType)timeCodeType;

            return Content(HttpStatusCode.OK, tcm.GetTimeCodesDict(base.ActorCompanyId, addEmptyRow, concatCodeAndName, codeType).ToSmallGenericTypes());

            //if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
            //    return Content(HttpStatusCode.OK, tcm.GetTimeCodes(base.ActorCompanyId, timeCodeType, onlyActive, loadPayrollProducts).ToGridDTOs(loadPayrollProducts, base.GetTermGroupContent(TermGroup.YesNo)));
            //else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
            //    if (includeType)
            //        return Content(HttpStatusCode.OK, tcm.GetTimeCodesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("concatCodeAndName"), includeType).ToSmallGenericTypes());
            //    else
            //        return Content(HttpStatusCode.OK, tcm.GetTimeCodesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("concatCodeAndName"), timeCodeType).ToSmallGenericTypes());

            //return Content(HttpStatusCode.OK, tcm.GetTimeCodes(base.ActorCompanyId, timeCodeType, onlyActive, loadPayrollProducts, onlyWithInvoiceProduct).ToDTOs(loadPayrollProducts, true));
        }

        [HttpGet]
        [Route("TimeCode/{timeCodeType:int}/{timeCodeId:int}/{loadInvoiceProducts:bool}/{loadPayrollProducts:bool}/{loadTimeCodeDeviationCauses:bool}/{loadEmployeeGroups:bool}")]
        public IHttpActionResult GetTimeCode(int timeCodeType, int timeCodeId, bool loadInvoiceProducts, bool loadPayrollProducts, bool loadTimeCodeDeviationCauses, bool loadEmployeeGroups)
        {
            TimeCode timeCode = tcm.GetTimeCode(timeCodeId, base.ActorCompanyId, false, loadInvoiceProducts, loadPayrollProducts, true, loadTimeCodeDeviationCauses, loadEmployeeGroups);
            if (timeCode != null)
            {
                switch ((SoeTimeCodeType)timeCodeType)
                {
                    case SoeTimeCodeType.Absense:
                        return Content(HttpStatusCode.OK, timeCode.ToAbsenceDTO());
                    case SoeTimeCodeType.AdditionDeduction:
                        return Content(HttpStatusCode.OK, timeCode.ToAdditionDeductionDTO());
                    case SoeTimeCodeType.Break:
                        return Content(HttpStatusCode.OK, timeCode.ToBreakDTO());
                    case SoeTimeCodeType.Material:
                        return Content(HttpStatusCode.OK, timeCode.ToMaterialDTO());
                    case SoeTimeCodeType.Work:
                        return Content(HttpStatusCode.OK, timeCode.ToWorkDTO());
                    default:
                        return Content(HttpStatusCode.OK, timeCode.ToDTO());
                }
            }
            else
            {
                return Content(HttpStatusCode.OK, timeCode);
            }
        }

        [HttpGet]
        [Route("TimeCode/Break/{addEmptyRow:bool}")]
        public IHttpActionResult GetTimeCodeBreaks(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodeBreaks(base.ActorCompanyId, addEmptyRow).ToSmallBreakDTOs());
        }

        [HttpGet]
        [Route("TimeCode/AdditionDeduction/{checkInvoiceProduct:bool}")]
        public IHttpActionResult GetTimeCodeAdditionDeductions(bool checkInvoiceProduct)
        {
            var timeCodes = tcm.GetTimeCodeAdditionDeductions(base.ActorCompanyId, checkInvoiceProduct).ToAdditionDeductionDTOs();
            return Content(HttpStatusCode.OK, timeCodes);
        }

        [HttpPost]
        [Route("TimeCode/")]
        public IHttpActionResult SaveTimeCode(TimeCodeSaveDTO timeCode)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tcm.SaveTimeCode(timeCode, base.ActorCompanyId, base.RoleId));
        }

        [HttpPost]
        [Route("TimeCode/UpdateState")]
        public IHttpActionResult UpdateTimeCodeState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tcm.UpdateTimeCodeState(model.Dict, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("TimeCode/{timeCodeId:int}")]
        public IHttpActionResult DeleteTimeCode(int timeCodeId)
        {
            return Content(HttpStatusCode.OK, tcm.DeleteTimeCode(timeCodeId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("TimeCode/Dict/{addEmptyRow:bool}/{concatCodeAndName:bool}/{includeType:bool}")]
        public IHttpActionResult GetTimeCodesDict(bool addEmptyRow, bool concatCodeAndName, bool includeType)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodesDict(base.ActorCompanyId, addEmptyRow, concatCodeAndName, includeType).ToSmallGenericTypes());
        }

        #endregion

        #region TimeCodeRanking
        [HttpGet]
        [Route("TimeCodeRankingGrid/{timeCodeRankingGroupId:int?}")]
        public IHttpActionResult GetTimeCodeRankingGrid(int? timeCodeRankingGroupId = null)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodeRankingGroups(timeCodeRankingGroupId).ToGridDTOs());
        }

        [HttpGet]
        [Route("TimeCodeRanking/{id:int}")]
        public IHttpActionResult GetTimeCodeRankings(int id)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodeRankingGroupDTO(id));
        }

        [HttpPost]
        [Route("TimeCodeRanking/Validate/{isDelete:bool}")]
        public IHttpActionResult ValidateTimeCodeRanking(TimeCodeRankingGroupDTO inputRankingGroup, bool isDelete)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tcm.ValidateTimeCodeRankingGroup(inputRankingGroup, isDelete: isDelete));
        }

        [HttpPost]
        [Route("TimeCodeRanking/")]
        public IHttpActionResult SaveTimeCodeRanking(TimeCodeRankingGroupDTO inputRanking)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tcm.SaveTimeCodeRankingsGroup(inputRanking));
        }
        [HttpDelete]
        [Route("TimeCodeRanking/Delete/{id:int}")]
        public IHttpActionResult DeleteTimeCodeRanking(int id)
        {
            return Content(HttpStatusCode.OK, tcm.DeleteTimeCodeRankingGroup(id));
        }
        #endregion

        #region PayrollProducts & InvoiceProducts

        [HttpGet]
        [Route("TimeCode/PayrollProducts")]
        public IHttpActionResult GetTimeCodePayrollProducts()
        {
            return Content(HttpStatusCode.OK, prm.GetPayrollProducts(base.ActorCompanyId, active: null).OrderBy(x => x.State).ThenBy(x => x.Name).ToProductTimeCodeDTOs());
        }

        [HttpGet]
        [Route("TimeCode/InvoiceProducts")]
        public IHttpActionResult GetTimeCodeInvoiceProducts()
        {
            return Content(HttpStatusCode.OK, prm.GetInvoiceProducts(base.ActorCompanyId, active: null).OrderBy(x => x.State).ThenBy(x => x.Name).ToProductTimeCodeDTOs());
        }

        #endregion
    }
}