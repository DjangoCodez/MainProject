namespace SoftOne.Soe.Business.Util.API.AvionData.Models
{
    public class AvionResponse<T>
    {
        /// <summary>
        /// Indicates whether the API call was successful.
        /// </summary>
        public bool IsSuccess => Error == null;

        /// <summary>
        /// The result object from the API, if successful.
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// The error information, if the request failed.
        /// </summary>
        public ErrorResponse Error { get; set; }

        /// <summary>
        /// Creates a successful response.
        /// </summary>
        public static AvionResponse<T> Success(T result) => new AvionResponse<T> { Result = result };

        /// <summary>
        /// Creates a failed response.
        /// </summary>
        public static AvionResponse<T> Fail(ErrorResponse error) => new AvionResponse<T> { Error = error };
    }
}
