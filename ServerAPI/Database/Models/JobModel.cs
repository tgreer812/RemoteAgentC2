// JobModel.cs
using RemoteAgentServerAPI.Database.Serializers;
using System.ComponentModel.DataAnnotations; // For data annotations
using System.Text.Json.Serialization;

namespace RemoteAgentServerAPI.Data.Models
{
    public class JobModel
    {
        /// <summary>
        /// Unique identifier for the job.
        /// Database will auto-generate this (Identity/AutoIncrement).
        /// </summary>
        [Key] // Specifies this is the primary key
        [JsonPropertyName("jobId")]
        public int JobId { get; set; }

        /// <summary>
        /// Type of the job (e.g., "PluginJob", "FileCopy", "DatabaseBackup").
        /// Required field.
        /// </summary>
        [Required(ErrorMessage = "JobType is required.")]
        [JsonPropertyName("jobType")]
        public string JobType { get; set; } = string.Empty; // Use string.Empty for non-nullable reference types

        /// <summary>
        /// Data associated with the job. Can be a complex object serialized to JSON.
        /// Required field.
        /// </summary>
        [Required(ErrorMessage = "JobData is required.")]
        [JsonPropertyName("jobData")]
        public object JobData { get; set; } = default!; // Use default! for non-nullable reference types, as it will be initialized

        /// <summary>
        /// Status of the job result (e.g., "Success", "Failed", "Created", "InProgress").
        /// </summary>
        [JsonPropertyName("jobResultStatus")]
        [JsonConverter(typeof(JobResultStatusConverter))]
        public string JobResultStatus { get; set; } = "Created";

        /// <summary>
        /// Job output data (e.g., log messages, results, etc.).
        /// Must be formatted as a JSON string.
        /// </summary>
        [JsonPropertyName("jobOutput")]
        public object JobOutput { get; set; } = default!;

        /// <summary>
        /// ID of the Agent that this job is assigned to or associated with.
        /// Foreign key to the Agents table (if you have an Agent entity).
        /// </summary>
        [JsonPropertyName("agentId")]
        public int AgentId { get; set; }

        // --- Navigation Property (if you have an Agent entity defined) ---
        // If you have an Agent model defined, you could add a navigation property like this:
        // public Agent Agent { get; set; } // Represents the associated Agent

        /// <summary>
        /// Timestamp indicating when the job was created.
        /// Database can set this automatically (e.g., using DEFAULT CURRENT_TIMESTAMP in SQLite).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Example: Set default to UTC now
    }
}