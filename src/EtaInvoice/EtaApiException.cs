using System;

namespace EtaInvoice
{
    /// <summary>
    /// Thrown when the ETA API returns a non-success HTTP status code.
    /// </summary>
    public class EtaApiException : Exception
    {
        /// <summary>HTTP status code returned by the API.</summary>
        public int StatusCode { get; }

        /// <summary>ETA error code from the response body, if present.</summary>
        public string ErrorCode { get; }

        /// <summary>Raw response body.</summary>
        public string ResponseBody { get; }

        public EtaApiException(int statusCode, string responseBody, string errorCode = null, string message = null)
            : base(message ?? $"ETA API request failed with HTTP {statusCode}.{(errorCode != null ? $" Error code: {errorCode}." : string.Empty)}")
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
            ErrorCode = errorCode;
        }
    }
}
