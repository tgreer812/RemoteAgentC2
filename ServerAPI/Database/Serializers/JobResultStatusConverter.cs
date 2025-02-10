using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using RemoteAgentServerAPI.Data.Models;

namespace RemoteAgentServerAPI.Database.Serializers
{
    public class JobResultStatusConverter : JsonConverter<string> // Or JsonConverter<JobResultStatus> if using Option 2
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? statusString = reader.GetString();
            if (string.IsNullOrEmpty(statusString))
            {
                // Not sure if this is the desired behavior
                // but it's easier to test with an exception and correct the behavior later
                // than to silently return null and wonder why it's not working
                throw new JsonException($"Invalid JobResultStatus value: {statusString}");
                //return null; // Or throw an exception if empty strings are not allowed
            }

            if (statusString == JobResultStatus.Created ||
                statusString == JobResultStatus.InProgress ||
                statusString == JobResultStatus.Success ||
                statusString == JobResultStatus.Failed
            )
            {
                return statusString;   
            }
            else
            {
                throw new JsonException($"Invalid JobResultStatus value: {statusString}");
            }
        }

        public override void Write(Utf8JsonWriter writer, string statusString, JsonSerializerOptions options)
        {
            writer.WriteStringValue(statusString);
        }
    }
}
