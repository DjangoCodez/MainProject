using SoftOne.Soe.Common.Attributes;

namespace SoftOne.Soe.Common.DTO
{
  [TSInclude]
  public class ImportFileDTO
  {
    public int DataStorageId { get; set; }
    public string FileName { get; set; }
  }
}
