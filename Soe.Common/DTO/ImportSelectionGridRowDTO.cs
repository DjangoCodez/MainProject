using SoftOne.Soe.Common.Attributes;
namespace SoftOne.Soe.Common.DTO
{
	[TSInclude]
	public class ImportSelectionGridRowDTO
	{
		public string FileName { get; set; }
		public string FileType { get; set; }
		public int DataStorageId { get; set; }
		public ImportDTO Import { get; set; }
		public int? ImportId { get; set; }
		public string ImportName { get; set; }
		public string Message { get; set; }

		public ImportSelectionGridRowDTO(string fileName, string fileType, int dataStorageId, ImportDTO import, string message = "")
		{
			FileName = fileName;
			FileType = fileType;
			DataStorageId = dataStorageId;
			Import = import;
			ImportId = import?.ImportId;
			ImportName = import?.Name;
			Message = message;
		}
	}
}
