namespace RemoteAgentServerAPI.Data.Models
{
    public static class JobResultStatus
    {
        public static readonly string Created = "Created";
        public static readonly string InProgress = "InProgress";
        public static readonly string Success = "Success";
        public static readonly string Failed = "Failed";

        /// <summary>
        /// Converts a string to a JobResultStatus constant using a modern switch expression.
        /// </summary>
        /// <param name="statusString">The string to convert.</param>
        /// <returns>The matching JobResultStatus constant.</returns>
        /// <exception cref="ArgumentException">If the string does not match any valid JobResultStatus.</exception>
        public static string FromString(string statusString)
        {
            if (string.IsNullOrEmpty(statusString))
            {
                throw new ArgumentException("JobResultStatus string cannot be null or empty.", nameof(statusString));
            }

            string lowerStatusString = statusString.ToLowerInvariant();

            return lowerStatusString switch
            {
                "created" => Created,
                "inprogress" => InProgress,
                "success" => Success,
                "failed" => Failed,
                _ => throw new ArgumentException($"Invalid JobResultStatus value: '{statusString}'. Valid values are: Created, InProgress, Success, Failed.", nameof(statusString))
            };
        }
    }
}
