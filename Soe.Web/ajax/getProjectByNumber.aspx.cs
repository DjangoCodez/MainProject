using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getProjectByNumber: JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string projectNr = QS["pnr"];
            if (!string.IsNullOrEmpty(projectNr))
            {
                ProjectManager pm = new ProjectManager(ParameterObject);
                List<Project> projects = pm.GetProjectsByNumberSearch(SoeCompany.ActorCompanyId, projectNr, 100);

                Queue q = new Queue();
                
                if (projects != null)
                {
                    foreach (var project in projects)
                    {
                        q.Enqueue(new
                        {
                            Found = true,
                            ActorCompanyId = project.ActorCompanyId,
                            ProjectNr = project.Number,
                            ProjectName = project.Name,
                        });

                    }                }

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
