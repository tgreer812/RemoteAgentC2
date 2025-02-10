// AgentModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RemoteAgentServerAPI.Data.Models
{
    public class AgentModel
    {
        /// <summary>
        /// Unique identifier for the agent.
        /// Database will auto-generate this (Identity/AutoIncrement).
        /// </summary>
        [Key]
        [JsonPropertyName("agentId")]
        public int AgentId { get; set; }

        /// <summary>
        /// GUID identifier for the agent (e.g., "123e4567-e89b-12d3-a456-426614174000").
        /// This is given to the agent when it is compiled/installed.
        /// </summary>
        [JsonPropertyName("agentGuid")]
        public string AgentGuid { get; set; } = string.Empty;

        /// <summary>
        /// Name of the agent (e.g., "Agent-Node-01", "Windows-Agent-Server").
        /// Required field.
        /// </summary>
        [JsonPropertyName("agentName")]
        public string AgentName { get; set; } = string.Empty;

        /// <summary>
        /// Current status of the agent (e.g., "Online", "Offline", "Idle", "Busy", "Error").
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Offline"; // Default status

        /// <summary>
        /// Last time the agent checked in or communicated with the server.
        /// Useful for monitoring agent liveness.
        /// </summary>
        [JsonPropertyName("lastCheckIn")]
        public DateTime? LastCheckIn { get; set; } // Nullable DateTime as agent might be offline

        /// <summary>
        /// Location or address information of the agent (e.g., IP Address, Hostname, Description).
        /// </summary>
        [JsonPropertyName("location")]
        public string? Location { get; set; } // Optional location information

        /// <summary>
        /// Description of the agent's capabilities or installed plugins (e.g., JSON string or list of capabilities).
        /// Can be used to determine what types of jobs the agent can handle.
        /// </summary>
        [JsonPropertyName("plugins")]
        public List<string>? Plugins { get; set; } // Optional capabilities description

        /// <summary>
        /// The version of the agent software that is running.
        /// </summary>
        [JsonPropertyName("version")]
        public string? Version { get; set; } // Optional version information

        /// <summary>
        /// Timestamp indicating when the agent was first registered or created in the system.
        /// </summary>
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow; // Default to UTC now

        /// <summary>
        /// Timestamp indicating the last time any property of the agent was updated.
        /// Can be used for tracking changes.
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow; // Default to UTC now

    }

}