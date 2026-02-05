using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class ContactECom : ICreatedModified
    {

    }

    public static partial class EntityExtensions
    {
        #region ContactEcom

        public static ContactECom GetLastCreatedOrModified(this List<ContactECom> ecoms)
        {
            if (ecoms.IsNullOrEmpty())
                return null;

            ContactECom lastCreated = ecoms.OrderByDescending(o => o.Created).FirstOrDefault();
            ContactECom lastModified = ecoms.OrderByDescending(o => o.Modified).FirstOrDefault();
            return lastModified.IsModifiedAfterCreated() ? lastModified : lastCreated;
        }

        public static List<ContactECom> GetLastCreatedOrModifiedList(this List<ContactECom> ecoms, int take)
        {
            return ecoms?.OrderByDescending(o => o.GetLastCreatedOrModifiedDate()).Take(take).ToList() ?? new List<ContactECom>();
        }

        public static bool IsModifiedAfterCreated(this ContactECom e)
        {
            return e != null && e.Modified.HasValue && e.Created.HasValue && e.Modified.Value > e.Created.Value;
        }

        public static DateTime GetLastCreatedOrModifiedDate(this ContactECom e)
        {
            return (e.IsModifiedAfterCreated() ? e.Modified : e.Created) ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public static string GetEComText(this IEnumerable<ContactECom> contactEcoms, TermGroup_SysContactEComType type)
        {
            return contactEcoms?.GetEComByType(type)?.Text ?? string.Empty;
        }

        public static ContactECom GetEComByType(this IEnumerable<ContactECom> ecoms, TermGroup_SysContactEComType type)
        {
            return ecoms?.Where(i => i.SysContactEComTypeId == (int)type).OrderBy(i => i.Created).FirstOrDefault();
        }

        public static void GetClosestRelative(this IEnumerable<ContactECom> l, out string closestRelativeNr, out string closestRelativeName, out string closestRelativeRelation)
        {
            var e = l.GetEComByType(TermGroup_SysContactEComType.ClosestRelative);
            if (e != null)
            {
                closestRelativeNr = e.Text;
                SplitClosestRelative(e, out closestRelativeName, out closestRelativeRelation);
            }
            else
            {
                closestRelativeNr = "";
                closestRelativeName = "";
                closestRelativeRelation = "";
            }
        }

        public static void SplitClosestRelative(this ContactECom e, out string name, out string relation)
        {
            name = "";
            relation = "";

            if (e != null && !String.IsNullOrEmpty(e.Description))
            {
                string[] split = e.Description.Split(';');
                if (split.Any())
                    name = split[0];
                if (split.Count() >= 2)
                    relation = split[1];
            }
        }

        #endregion
    }
}
