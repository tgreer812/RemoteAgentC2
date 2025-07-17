// IDatabase.cs
using RemoteAgentServerAPI.Data.Models; // Make sure to include namespace for your models

namespace RemoteAgentServerAPI.Data
{
    public interface IDatabase
    {
        /// <summary>
        /// Initializes the database (e.g., creates database file and schema if needed).
        /// </summary>
        void InitializeDatabase();

        /// <summary>
        /// Retrieves a JobModel from the database by its ID.
        /// Returns null if the job is not found.
        /// </summary>
        /// <param name="id">The ID of the JobModel to retrieve.</param>
        /// <returns>The JobModel object, or null if not found.</returns>
        JobModel? GetJobById(int id);

        /// <summary>
        /// Retrieves all JobModels from the database associated with a given Agent ID.
        /// Returns an empty list if no jobs are found for the agent.
        /// </summary>
        /// <param name="agentId">The agent ID to retrieve jobs for.</param>
        /// <returns>A list of JobModel objects associated with the agent ID. Returns an empty list if no jobs are found.</returns>
        List<JobModel>? GetJobsByAgentId(int agentId);

        /// <summary>
        /// Saves a new JobModel to the database.
        /// The implementation should handle assigning a unique ID to the job.
        /// </summary>
        /// <param name="agentJob">The JobModel object to save.</param>
        /// <returns>The saved JobModel object, potentially with an updated ID assigned by the database.</returns>
        JobModel SaveJob(JobModel agentJob);

        /// <summary>
        /// Gets an agent from the database by its GUID. Returns null if the agent is not found.
        /// </summary>
        /// <param name="agentGuid">The guid to identify the agent by</param>
        /// <returns>The AgentModel object if found or null otherwise.</returns>
        AgentModel? GetAgentByGuid(string agentGuid);

        /// <summary>
        /// Saves the agent to the database. If the agent already exists, it should update the existing record.
        /// </summary>
        /// <param name="agent">The AgentModel object to save or update.</param>
        /// <returns>The saved or updated AgentModel object.</returns>
        AgentModel SaveAgent(AgentModel agent);

        JobModel UpdateJob(JobModel jobModel);

        /// <summary>
        /// Deletes a job from the database by its ID.
        /// </summary>
        /// <param name="jobId">The ID of the job to delete.</param>
        /// <returns>True if the job was successfully deleted, false otherwise.</returns>
        bool DeleteJob(int jobId);

        /// <summary>
        /// Marks jobs as sent to the agent by updating their status.
        /// This can be used to prevent resending the same jobs.
        /// </summary>
        /// <param name="jobIds">The IDs of the jobs to mark as sent.</param>
        /// <returns>True if the jobs were successfully updated, false otherwise.</returns>
        bool MarkJobsAsSent(List<int> jobIds);

        // --- Future methods for other entities can be added here ---
        // Example:
        // Agent? GetAgentById(int id);  // If you had an Agent entity
        // void SaveAgent(Agent agent); // If you had an Agent entity
        // ... etc.
    }
}