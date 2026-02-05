using SoftOne.Soe.Common.DTO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public static partial class ExtensionsComp
    {
        public static IEnumerable<FileImportHeadGridDTO> ToGridDTOs(this List<FileImportHead> fileImports, List<SmallGenericType> statusTerms = null)
        {
            foreach (var fileImportHead in fileImports)
            {
                yield return fileImportHead.ToGridDTO(statusTerms);
            }
        }

        public static FileImportHeadGridDTO ToGridDTO(this FileImportHead f, List<SmallGenericType> statusTerms = null)
        {
            if (f == null) return null;

            return new FileImportHeadGridDTO
            {
                FileImportHeadId = f.FileImportHeadId,
                FileName = f.FileName,
                SystemMessage = f.SystemMessage,
                Comment = f.Comment,
                Status = f.Status.Value,
                StatusStr = statusTerms?.FirstOrDefault(x => x.Id == f.Status)?.Name ?? string.Empty,
                Created = f.Created,
                CreatedBy = f.CreatedBy,
                Modified = f.Modified,
                ModifiedBy = f.ModifiedBy
            };
        }

        public static FileImportHeadDTO ToDTO(this FileImportHead f)
        {
            if (f == null) return null;
            return new FileImportHeadDTO
            {
                FileImportHeadId = f.FileImportHeadId,
                ActorCompanyId = (int)f.ActorCompanyId,
                BatchId = f.BatchId,
                EntityType = (int)f.EntityType,
                FileName = f.FileName,
                SystemMessage = f.SystemMessage,
                Comment = f.Comment,
                Status = (int)f.Status
            };
        }
    }
}
