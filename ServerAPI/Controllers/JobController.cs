using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RemoteAgentServerAPI.Data;
using RemoteAgentServerAPI.Data.Models;
using AgentCommon.AgentPluginCommon;

namespace RemoteAgentServerAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class JobController : Controller
    {
        private IDatabase _database;

        public JobController(IDatabase database)
        {
            _database = database;
        }

        [HttpGet("{id}")]
        public JobModel? Get(int id)
        {
            var job = _database.GetJobById(id);
            if (job == null)
            {
                return null;
            }

            return job;
        }

        [HttpPost]
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

/*        [HttpPut("{id}")] // PUT request to update a job, using ID in the route
        public IActionResult Put(int id, [FromBody] JobModel updatedJob)
        {
            if (updatedJob == null)
            {
                return BadRequest("Request body is missing or invalid."); // Or "Updated job data is required."
            }

            if (id <= 0) // Validate the ID from the route
            {
                return BadRequest("Job ID is invalid. It must be a positive integer.");
            }

            if (updatedJob.AgentId <= 0) // Validate properties from the body
            {
                return BadRequest("Agent ID is required and must be a positive integer.");
            }

            if (string.IsNullOrEmpty(updatedJob.JobType))
            {
                return BadRequest("Job type is required.");
            }

            if (updatedJob.JobData == null)
            {
                return BadRequest("Job data is required.");
            }

            try
            {
                // Check if the job with the given ID exists in the database
                var existingJob = _database.GetJobById(id); // Assuming you have a method to get job by ID

                if (existingJob == null)
                {
                    return NotFound($"Job with ID {id} not found."); // Return 404 if not found
                }

                // Update the existing job with the values from updatedJob
                // You might choose to update all properties from updatedJob, or selectively update specific ones
                existingJob.AgentId = updatedJob.AgentId;
                existingJob.JobType = updatedJob.JobType;
                existingJob.JobData = updatedJob.JobData;
                // ... Update other properties as needed from updatedJob to existingJob ...

                var isUpdated = _database.UpdateJob(id, existingJob); // Assuming you have an UpdateJob method in your service

                if (!isUpdated) // Check if the update was successful (UpdateJob might return boolean or updated object)
                {
                    return StatusCode(500, $"Failed to update job with ID {id} in the database. Please try again later.");
                }

                return NoContent(); // 204 No Content - successful update, no response body needed
                // Alternatively, you could return the updated job with Ok(existingJob); if you want to return the updated resource.
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred while processing your request to update job ID {id}. Please try again later.");
            }
        }*/


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
