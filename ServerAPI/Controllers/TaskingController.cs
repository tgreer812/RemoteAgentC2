using Microsoft.AspNetCore.Mvc;
using RemoteAgentServerAPI.Data;
using RemoteAgentServerAPI.Models;
using RemoteAgentServerAPI.Data.Models;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RemoteAgentServerAPI.Controllers
{
    /// <summary>
    /// Controller for managing agent task assignment and job retrieval
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TaskingController : ControllerBase
    {
        private IDatabase _database;

        public TaskingController(IDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// Gets pending jobs for a specific agent
        /// </summary>
        /// <param name="id">The agent ID to retrieve jobs for</param>
        /// <returns>A list of pending jobs for the agent</returns>
        /// <response code="200">Returns the list of pending jobs</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult Get(int id)
        {
            var jobs = _database.GetJobsByAgentId(id);

            if (jobs == null || jobs.Count == 0)
            {
                // Return empty jobs array when no jobs are found
                return Ok(new { jobs = new List<object>() });
            }

            // Filter for jobs that haven't been sent yet (status is "Created" or null)
            var pendingJobs = jobs.Where(job => 
                string.IsNullOrEmpty(job.JobResultStatus) || 
                job.JobResultStatus == "Created").ToList();

            if (pendingJobs.Count == 0)
            {
                return Ok(new { jobs = new List<object>() });
            }

            // Convert jobs to the format expected by the agent
            var jobsResponse = pendingJobs.Select(job => new
            {
                jobId = job.JobId,    // Include jobId as expected by agent
                jobType = job.JobType,
                jobData = job.JobData
            }).ToList();

            // Mark these jobs as sent to prevent resending
            var jobIds = pendingJobs.Select(j => j.JobId).ToList();
            _database.MarkJobsAsSent(jobIds);

            // Return in the format the agent expects: { jobs: [...] }
            return Ok(new { jobs = jobsResponse });
        }

        /// <summary>
        /// Submits a new agent task containing multiple jobs
        /// </summary>
        /// <param name="agentTask">The agent task containing jobs to be executed</param>
        /// <returns>The list of saved jobs</returns>
        /// <response code="200">Returns the saved jobs</response>
        /// <response code="400">If the agent task is invalid</response>
        [HttpPost]
        [ProducesResponseType(typeof(List<JobModel>), 200)]
        [ProducesResponseType(400)]
        public IActionResult Post([FromBody] AgentTask agentTask)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return 400 Bad Request with validation errors
            }

            List<JobModel> savedJobs = new List<JobModel>();
            foreach (var job in agentTask.Jobs)
            {
                var savedJob = _database.SaveJob(new JobModel()
                {
                    JobType = job.JobType,
                    JobData = job.JobData,
                    AgentId = job.AgentId
                });
                savedJobs.Add(savedJob); // Capture saved jobs to return in response
            }

            // Return 200 OK status code with the list of saved jobs in the response body
            return Ok(savedJobs);
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
