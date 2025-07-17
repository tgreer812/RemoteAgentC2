using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RemoteAgentServerAPI.Data;
using RemoteAgentServerAPI.Data.Models;
using AgentCommon.AgentPluginCommon;

namespace RemoteAgentServerAPI.Controllers
{
    /// <summary>
    /// Controller for managing individual job operations
    /// </summary>
    [ApiController]
    [Route("/api/[controller]")]
    public class JobController : Controller
    {
        private IDatabase _database;

        public JobController(IDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// Gets a specific job by ID
        /// </summary>
        /// <param name="id">The job ID to retrieve</param>
        /// <returns>The job details if found</returns>
        /// <response code="200">Returns the job details</response>
        /// <response code="404">If the job is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(JobModel), 200)]
        [ProducesResponseType(404)]
        public JobModel? Get(int id)
        {
            var job = _database.GetJobById(id);
            if (job == null)
            {
                return null;
            }

            return job;
        }

        /// <summary>
        /// Creates a new job
        /// </summary>
        /// <param name="job">The job to create</param>
        /// <returns>The created job with assigned ID</returns>
        /// <response code="200">Returns the created job</response>
        /// <response code="400">If the job data is invalid</response>
        /// <response code="500">If there was an error saving the job</response>
        [HttpPost]
        [ProducesResponseType(typeof(JobModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public IActionResult Post([FromBody] JobModel job)
        {
            if (job == null)
            {
                return BadRequest("Request body is missing or invalid."); // Improved message for null request body
            }

            if (job.AgentId <= 0)
            {
                return BadRequest("Agent ID is required and must be a positive integer."); // More specific message
            }

            if (string.IsNullOrEmpty(job.JobType))
            {
                return BadRequest("Job type is required.");
            }

            if (job.JobData == null)
            {
                return BadRequest("Job data is required.");
            }

            try
            {
                // Save the job to the database
                var savedJob = _database.SaveJob(job);

                if (savedJob == null) // Check if SaveJob returned null, indicating failure
                {
                    // Log the error here for debugging purposes - important in real applications
                    // _logger.LogError("Failed to save job to database for AgentId: {AgentId}, JobType: {JobType}", job.AgentId, job.JobType);
                    return StatusCode(500, "Failed to save job to the database. Please try again later."); // Return 500 for server error
                }

                return Ok(savedJob); // Return 200 OK with the saved job
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging purposes - very important in real applications
                // _logger.LogError(ex, "Error saving job to database for AgentId: {AgentId}, JobType: {JobType}", job.AgentId, job.JobType);
                return StatusCode(500, "An unexpected error occurred while processing your request. Please try again later."); // Return 500 for server error
            }
        }

        /// <summary>
        /// Updates a job with plugin execution results
        /// </summary>
        /// <param name="id">The job ID to update</param>
        /// <param name="result">The plugin execution result</param>
        /// <returns>Success message or error details</returns>
        /// <response code="200">If the job was successfully updated</response>
        /// <response code="400">If the result data is invalid</response>
        /// <response code="404">If the job is not found</response>
        /// <response code="500">If there was an error updating the job</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult Put(int id, [FromBody] PluginResult result)
        {
            if (result == null)
            {
                return BadRequest("Request body is missing or invalid.");
            }

            try
            {
                // Check if the job with the given ID exists in the database
                var existingJob = _database.GetJobById(id);

                if (existingJob == null)
                {
                    return NotFound($"Job with ID {id} not found.");
                }

                // Update the existing job with the plugin result
                existingJob.JobResultStatus = result.Status.ToString();
                existingJob.JobOutput = result.OutputData;

                var updatedJob = _database.UpdateJob(existingJob);

                if (updatedJob == null)
                {
                    return StatusCode(500, $"Failed to update job with ID {id} in the database. Please try again later.");
                }

                return Ok(updatedJob);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred while processing your request to update job ID {id}. Please try again later.");
            }
        }


        [HttpPost("/{id}/result")] // POST request to update job result, using ID in the route
        public IActionResult Post(int id, [FromBody] PluginResult result)
        {
            // Create a new Job object from the PluginResult
            var job = new JobModel
            {
                JobId = id,
                JobResultStatus = result.Status.ToString(),
                JobOutput = result.OutputData // Assuming OutputData is the job output
            };

            try
            {
                job = _database.UpdateJob(job);
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging purposes - very important in real applications
                // _logger.LogError(ex, "Error updating job result for JobId: {JobId}", id);
                return StatusCode(500, "An unexpected error occurred while processing your request. Please try again later."); // Return 500 for server error
            }

            return Ok(job);
        }
    }
}
