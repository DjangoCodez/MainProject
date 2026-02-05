using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getAttestStates : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Int32.TryParse(QS["entity"], out int entity);
            if (entity != 0)
            {
                Queue queue = new Queue();
                int i = 0;

                AttestManager am = new AttestManager(ParameterObject);
                var attestStates = am.GetAttestStates(SoeCompany.ActorCompanyId, (TermGroup_AttestEntity)entity, SoeModule.None);
                foreach (var attestState in attestStates)
                {
                    queue.Enqueue(new
                    {
                        Position = i,
                        Found = true,
                        Name = attestState.Name,
                        AttestStateId = attestState.AttestStateId
                    });

                    i++;
                }
                ResponseObject = queue;
            }

            if (ResponseObject == null)
            {
                ResponseObject = new
                {
                    Found = false
                };
            }
        }
    }
}
