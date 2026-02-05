using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class CompTerm
    {

    }

    public static partial class EntityExtensions
    {
        #region CompTerm

        public static CompTermDTO ToDTO(this CompTerm e, string langName)
        {
            return new CompTermDTO()
            {
                CompTermId = e.CompTermId,
                RecordType = (CompTermsRecordType)e.RecordType,
                RecordId = e.RecordId,
                Lang = e.LangId.ParseToEnum<TermGroup_Languages>(),
                LangName = langName,
                Name = e.Name,
                State = (SoeEntityState)e.State,
            };
        }

        public static List<CompTermDTO> ToDTOs(this IEnumerable<CompTerm> e)
        {
            return e.Select(s => s.ToDTO(string.Empty)).ToList();
        }

        #endregion
    }
}
