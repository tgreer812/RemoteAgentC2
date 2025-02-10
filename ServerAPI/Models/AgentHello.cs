using System.Text.Json.Serialization;

namespace RemoteAgentServerAPI.Models
{
    public class AgentHello
    {
        [JsonPropertyName("agentGuid")]
        public string AgentGuid { get; set; }

        public AgentHello()
        {
        }
    }
}
