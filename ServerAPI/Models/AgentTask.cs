using RemoteAgentServerAPI.Data.Models;
using System.Text.Json.Serialization;

namespace RemoteAgentServerAPI.Models
{
    public class AgentTask
    {
        [JsonPropertyName("jobs")]
        public List<JobModel> Jobs { get; set; }

        public AgentTask()
        {
            Jobs = new List<JobModel>();
        }

        public void AddJob(JobModel job)
        {
            Jobs.Add(job);
        }
    }
}
