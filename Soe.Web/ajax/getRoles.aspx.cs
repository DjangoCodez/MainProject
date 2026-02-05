using SoftOne.Soe.Business.Core;
using System;
using System.Collections;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getRoles : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Int32.TryParse(QS["company"], out int actorCompanyId))
            {
                RoleManager rm = new RoleManager(ParameterObject);
                var roles = rm.GetRolesByCompany(actorCompanyId);
                Queue q = new Queue();
                int i = 0;

                //Add empty row
                q.Enqueue(new
                {
                    Found = true,
                    Position = i,
                    RoleId = 0,
                    Name = " ",
                });
                i++;

                foreach (var role in roles)
                {
                    string roleName = rm.GetRoleNameText(role);

                    q.Enqueue(new
                    {
                        Found = true,
                        Position = i,
                        RoleId = role.RoleId,
                        Name = roleName,
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
