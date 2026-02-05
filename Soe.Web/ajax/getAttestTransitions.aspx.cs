using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getAttestTransitions : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Int32.TryParse(QS["entity"], out int entity);
            Int32.TryParse(QS["module"], out int module);

            if (entity != 0)
            {
                Queue q = new Queue();
                int i = 0;

                AttestManager am = new AttestManager(ParameterObject);
                var attestTransitions = am.GetAttestTransitions((TermGroup_AttestEntity)entity, (SoeModule)module, false, SoeCompany.ActorCompanyId);
                foreach (var attestTransition in attestTransitions)
                {
                    q.Enqueue(new
                    {
                        Position = i,
                        Found = true,
                        Name = attestTransition.Name,
                        AttestTransitionId = attestTransition.AttestTransitionId
                    });

                    i++;
                }
                ResponseObject = q;
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
