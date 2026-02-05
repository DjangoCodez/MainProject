using System;

namespace SoftOne.Soe.Business.Util.API.AvionData.Models
{
    public sealed class AvionException
    {
        public static ErrorResponse JsonParser(string statusCode) => new ErrorResponse
        {
            Timestamp = DateTime.UtcNow.ToString("o"),
            Message = "Failed to parse success response JSON.",
            Errorcode = statusCode
        };

        public static ErrorResponse Unknown(Exception ex, string statusCode = "") => new ErrorResponse
        {
            Timestamp = DateTime.UtcNow.ToString("o"),
            Message = $"Unexpected error: {ex.Message}",
            Errorcode = statusCode
        };
    }
}
