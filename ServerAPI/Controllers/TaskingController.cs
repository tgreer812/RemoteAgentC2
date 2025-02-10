using Microsoft.AspNetCore.Mvc;
using RemoteAgentServerAPI.Data;
using RemoteAgentServerAPI.Models;
using RemoteAgentServerAPI.Data.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RemoteAgentServerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskingController : ControllerBase
    {
        private IDatabase _database;

        public TaskingController(IDatabase database)
        {
            _database = database;
        }

        [HttpGet("{id}")]
        public AgentTask Get(int id)
        {
            /*var job = new AgentJob()
            { 
                JobId = 1, 
                JobType = "PluginJob", 
                JobData = new { test = "test" } 
            };
            var agentTask = new AgentTask();
            agentTask.AddJob(job);
            return agentTask;*/

            var jobs = _database.GetJobsByAgentId(id);

            if (jobs == null)
            {
                return new AgentTask();
            }

            var agentTask = new AgentTask();
            foreach (var job in jobs)
            {
                agentTask.AddJob(new JobModel()
                {
                    JobId = job.JobId,
                    AgentId = job.AgentId,
                    JobType = job.JobType,
                    JobData = job.JobData,
                    CreatedAt = job.CreatedAt
                });
            }

            return agentTask;
        }

        [HttpPost]
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
