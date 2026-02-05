using SoftOne.Soe.Business.Altinn.IntermediaryInboundBasic;
using SoftOne.Soe.Business.Altinn.PreFillEUSExternalBasic;
using SoftOne.Soe.Business.Altinn.ReceiptExternalBasic;
using SoftOne.Soe.Business.Altinn.ServiceMetaDataExternalBasic;
using SoftOne.Soe.Business.Altinn.SystemAuthentication;
using SoftOne.Soe.Common.DTO;
using System;
using System.IO;
using System.Xml.Serialization;

namespace SoftOne.Soe.Business.Util.AltInn
{

    public class AltInn
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userSSN">Fødselsnummer til bruker i sluttbrukersystemet som skal autentiseres</param>
        /// <param name="endUserSystemPassword">Passordet person har registrert for sin bruker i Altinn</param>
        /// <param name="endUserSystemId">Id som unikt identifiserer sluttbrukersystemet i Altinn</param>
        /// <param name="authMethod">Angir hvilken engangskodetype bruker ønskes utfordret på</param>
        /// <returns></returns>
        public GetAuthenticationChallengeResponse GetAuthenticationChallenge(AltInnUser user)
        {
            //AuthenticationChallengeRequestBE
            var authRequestBE = new AuthenticationChallengeRequestBE();
            authRequestBE.UserSSN = user.UserSSN;
            authRequestBE.UserPassword = user.UserPassword;
            authRequestBE.SystemUserName = user.EndUserSystemId; // TODO what is endusersystemid?
            authRequestBE.AuthMethod = user.LogInMethod.ToString(); //or AltinnPin or TaxPin

            var authRequest = new GetAuthenticationChallengeRequest(authRequestBE);

            //AuthenticationChallengeBE
            GetAuthenticationChallengeResponse authBE = null;

            //Create client
            using (SystemAuthenticationExternalClient client = new SystemAuthenticationExternalClient())
            {
                //GetAuthenticationChallenge
                authBE = client.GetAuthenticationChallenge(authRequest);
            }

            //Output AuthenticationChallengeBE
            if (authBE != null)
            {
                var xmlSerializer = new XmlSerializer(authBE.GetType());
                var stringWriter = new StringWriter();
                xmlSerializer.Serialize(stringWriter, authBE);
                var retString = stringWriter.ToString();
                return authBE;
            }

            return null;
        }

        public GetAvailableServicesBasicResponse GetAvailableServices(AltInnUser user)
        {
            var request = new GetAvailableServicesBasicRequest(user.EndUserSystemId, user.EndUserSystemPassword, 0);
            var response = new GetAvailableServicesBasicResponse();

            //Create client
            using (var client = new ServiceMetaDataExternalBasicClient())
            {
                response = client.GetAvailableServicesBasic(request);
            }

            return response;
        }

        //RF0002 1046 313
        public GetFormTaskSchemaDefinitionsBasicResponse GetFormTaskSchemaDefinitions(AltInnUser user)
        {
            var request = new GetFormTaskSchemaDefinitionsBasicRequest(user.EndUserSystemId, user.EndUserSystemPassword, RF002_VatDeclaration.ExternalServiceCode, RF002_VatDeclaration.ExternalServiceEditionCode);
            var response = new GetFormTaskSchemaDefinitionsBasicResponse();

            using (var client = new ServiceMetaDataExternalBasicClient())
            {
                response = client.GetFormTaskSchemaDefinitionsBasic(request);
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userSSN"></param>
        /// <param name="userPassword"></param>
        /// <param name="userPinCode"></param>
        /// <param name="method"></param>
        /// <param name="ReporteeNumber">Fødselsnummer eller organisasjonsnummer det skal hentes ut prefill for.</param>
        public GetPrefillDataBasicResponse GetPrefillData(AltInnUser user, string ReporteeNumber)
        {
            var requestBody = new GetPrefillDataBasicRequestBody(user.EndUserSystemId, user.EndUserSystemPassword, user.UserSSN, user.UserPassword, user.UserPinCode, user.LogInMethod.ToString(), ReporteeNumber, RF002_VatDeclaration.ExternalServiceCode, RF002_VatDeclaration.ExternalServiceEditionCode);
            var request = new GetPrefillDataBasicRequest(requestBody);
            var response = new GetPrefillDataBasicResponse();

            using (var client = new PreFillEUSExternalBasicClient())
            {
                response = client.GetPrefillDataBasic(request);
            }

            return response;
        }

        public GetReceiptListBasicResponse GetReciepts(AltInnUser user)
        {
            var requestBody = new GetReceiptListBasicRequest(user.EndUserSystemId, user.EndUserSystemPassword, ReceiptTypeEnum.FormTask, DateTime.Now.AddDays(-7), DateTime.Now);
            var response = new GetReceiptListBasicResponse();

            using (var client = new ReceiptExternalBasicClient())
            {
                response = client.GetReceiptListBasic(requestBody);
            }

            return response;
        }

        public GetReceiptBasicResponse GetReciept(AltInnUser user, int recieptId)
        {
            var search = new ReceiptSearchExternal()
            {
                ReceiptId = recieptId,
            };
            var request = new GetReceiptBasicRequest(user.EndUserSystemId, user.EndUserSystemPassword, search);
            var response = new GetReceiptBasicResponse();
            using (var client = new ReceiptExternalBasicClient())
            {
                response = client.GetReceiptBasic(request);
            }

            return response;
        }

        public SubmitFormTaskBasicResponse SubmitVatDeclarationFormTaskBasic(AltInnUser user, RF002_VatDeclaration vatDeclaration)
        {
            var formData = vatDeclaration.ToXmlDocument();

            //Form
            var form = new Form();
            form.Completed = vatDeclaration.Completed;
            form.DataFormatId = vatDeclaration.DataFormatId; // "212";
            form.DataFormatVersion = vatDeclaration.DataFormatVersion; // 10420;
            // Referanse til signaturen som ble utført i sluttbrukersystem (satt av sluttbrukersystem, bør være unik).
            form.EndUserSystemReference = String.Format("EUS_{0}", Guid.NewGuid().ToString());
            form.ParentReference = 0;
            form.FormData = formData.InnerXml; //This will encode the XML data. It's recommended to use CDATA elements to encode XML data.

            //FormTask
            var formtask = new FormTask();
            // Unik tjenestekode for tjenesten. For å sende skjema inn i AltinnI må dette feltet være tomt. */
            formtask.ServiceCode = RF002_VatDeclaration.ExternalServiceCode;

            // Tjenesteutgavekode. For å sende skjema inn i AltinnI må dette feltet være tomt.
            formtask.ServiceEdition = RF002_VatDeclaration.ExternalServiceEditionCode;
            formtask.Forms = new Form[1];
            formtask.Forms[0] = form;

            //FormTaskShipment
            var ftShipment = new FormTaskShipmentBE();
            
            ftShipment.Reportee = user.OrginizationNumber; // Unik identifikator for avgiver for denne forsendelsen, fødselsnummer eller organisasjonsnummer. "REPORTEE_SSN_OR_ORG"; //Change this to a valid value
            //ftShipment.Users = new DelegatedUserBE[1];
            //ftShipment.Users[0] = new DelegatedUserBE()
            //{
            //    SSN = user.UserSSN,
            //    Name = "Borg",
            //    NumberOfSignaturesAllowed = 1,
            //};

            //ftShipment.Signatures = new Signature()
            //{
            //    EndUserSystemReference = String.Format("EUS_{0}", Guid.NewGuid().ToString()),
            //    // Identifikator for bruker som var logget på sluttbrukersystemet og gjennomførte signeringen.
            //    EndUserSystemUserId = user.EndUserSystemId,
            //    // Dato og tidspunkt for når bruker logget inn i sluttbrukersystem
            //    EndUserSystemLoginDateTime = DateTime.Now,
            //    // Versjonsnummer for sluttbrukersystemet.
            //    // EndUserSystemVersion,
            //    // Unik id for logginnslag for signeringen.
            //    EndUserSystemSignatureLogId = Guid.NewGuid().ToString(),
            //};

            /* Referanse for forsendelsen. Referansen settes av sluttbrukersystem og kan dermed benyttes ved senere forespørsler mot Altinn for denne forsendelsen, bør derfor være unik. */
            ftShipment.ExternalShipmentReference = String.Format("ESR_{0}", Guid.NewGuid().ToString()); //Must be unique per End User System submission
           //ftShipment.UserDefinedNumberOfSignaturesRequired = 1;

            ftShipment.FormTasks = formtask;

            var requestBody = new SubmitFormTaskBasicRequestBody(user.EndUserSystemId, user.EndUserSystemPassword, user.UserSSN, user.UserPassword, user.UserPinCode, user.LogInMethod.ToString(), ftShipment);
            var request = new SubmitFormTaskBasicRequest(requestBody);

            var response = new SubmitFormTaskBasicResponse();

            using (var client = new IntermediaryInboundExternalBasicClient())
            {
                response = client.SubmitFormTaskBasic(request);
            }

            return response;
        }
    }
}
