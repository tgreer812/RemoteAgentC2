using Microsoft.AspNetCore.Mvc;
using RemoteAgentServerAPI.Data;
using RemoteAgentServerAPI.Data.Models;
using RemoteAgentServerAPI.Models;

namespace RemoteAgentServerAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AgentController : Controller
    {
        private IDatabase _database;

        public AgentController(IDatabase database)
        {
            _database = database;
        }

        [HttpGet]
        public string Get()
        {
            return "test";
        }

        [HttpPost("hello")] // Responds to POST /api/agents/hello
        public IActionResult Post([FromBody] AgentHello agentHello)
        {
            var agentGuid = agentHello.AgentGuid;

            var agent = _database.GetAgentByGuid(agentGuid);

            if (agent == null)
            {
                // if null create new agent
                agent = new AgentModel()
                {
                    AgentGuid = agentGuid,
                    Status = "Online",                  // TODO: make this enums
                    Location = "",
                    Plugins = new List<string>(),       // TODO: Add default plugins
                    LastCheckIn = DateTime.UtcNow,
                    Version = ""                        // TODO: Add this to AgentHello
                };

                _database.SaveAgent(agent);
            } 
            else
            {
                // Update the agent's last check-in time
                agent.LastCheckIn = DateTime.UtcNow;
                _database.SaveAgent(agent);
            }
            
            // Handle agent hello handshake logic here
            return Ok(new { message = "Agent Hello Received", agentId = agent.AgentId });
        }
    }
}
