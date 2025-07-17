// SqlLiteDatabase.cs
using RemoteAgentServerAPI.Data;
using RemoteAgentServerAPI.Data.Models;
using System;
using Microsoft.Data.Sqlite; // Changed to Microsoft.Data.Sqlite
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace RemoteAgentServerAPI.Data
{
    public class SqlLiteDatabase : IDatabase
    {
        private readonly string _dbFilePath; // Path to the SQLite database file
        // private static int _nextJobId = 1; // No longer needed - DB auto-increments

        public SqlLiteDatabase(string dbFilePath)
        {
            _dbFilePath = dbFilePath;
            InitializeDatabase();
        }

        public void InitializeDatabase()
        {
            Console.WriteLine("Initializing SQLite Database (using Microsoft.Data.Sqlite)...");

            if (!File.Exists(_dbFilePath))
            {
                Console.WriteLine($"Database file not found at: {_dbFilePath}. Creating...");
                using (var connection = new SqliteConnection($"Data Source={_dbFilePath}")) // Changed to SqliteConnection
                {
                    connection.Open();
                    connection.Close(); // Create the file just by opening and closing an empty connection
                }
                Console.WriteLine($"Database file created at: {_dbFilePath}");
                CreateDatabaseSchema();
            }
            else
            {
                Console.WriteLine($"Database file already exists at: {_dbFilePath}.");
            }

            Console.WriteLine("SQLite Database Initialization Complete (using Microsoft.Data.Sqlite).");
        }

        private void CreateDatabaseSchema()
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                // Create Agents Table
                using (var command = new SqliteCommand(@"
                            CREATE TABLE IF NOT EXISTS AgentModels (
                                AgentId INTEGER PRIMARY KEY AUTOINCREMENT,
                                AgentGuid TEXT NOT NULL,
                                AgentName TEXT,
                                Status TEXT,
                                LastCheckIn DATETIME,
                                Location TEXT,
                                Plugins TEXT,
                                Version TEXT,
                                RegisteredAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                                LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP
                            );
                        ", connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table 'AgentModels' created (if not exists).");
                }

                // Create Jobs Table
                using (var command = new SqliteCommand(@"
                            CREATE TABLE IF NOT EXISTS JobModels (
                                JobId INTEGER PRIMARY KEY AUTOINCREMENT,
                                JobType TEXT NOT NULL,
                                JobData TEXT NOT NULL, -- Store JobData as JSON string
                                AgentId INTEGER NOT NULL,
                                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                                FOREIGN KEY (AgentId) REFERENCES AgentModels(AgentId)
                            );
                        ", connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table 'JobModels' created (if not exists).");
                }

                connection.Close();
            }
        }

        public JobModel? GetJobById(int id)
        {
            Console.WriteLine($"Retrieving JobModel from SQLite (Microsoft.Data.Sqlite) for ID: {id}");
            JobModel? jobModel = null;

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqliteCommand("SELECT JobId, JobType, JobData, AgentId, CreatedAt FROM JobModels WHERE JobId = @JobId", connection))
                {
                    command.Parameters.AddWithValue("@JobId", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            jobModel = new JobModel()
                            {
                                JobId = reader.GetInt32(0),
                                JobType = reader.GetString(1),
                                JobData = JsonSerializer.Deserialize<object>(reader.GetString(2))!, // Deserialize JSON
                                AgentId = reader.GetInt32(3),
                                CreatedAt = reader.GetDateTime(4)
                            };
                        }
                    }
                }
                connection.Close();
            }
            return jobModel;
        }

        public JobModel SaveJob(JobModel jobModel)
        {
            Console.WriteLine("Saving JobModel to SQLite database (Microsoft.Data.Sqlite):");

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqliteCommand(@"
                            INSERT INTO JobModels (JobType, JobData, AgentId)
                            VALUES (@JobType, @JobData, @AgentId);
                            SELECT last_insert_rowid(); -- Get the auto-generated JobId
                        ", connection))
                {
                    command.Parameters.AddWithValue("@JobType", jobModel.JobType);
                    command.Parameters.AddWithValue("@JobData", JsonSerializer.Serialize(jobModel.JobData)); // Serialize JobData to JSON
                    command.Parameters.AddWithValue("@AgentId", jobModel.AgentId);

                    jobModel.JobId = Convert.ToInt32(command.ExecuteScalar()); // ExecuteScalar to get last inserted ID
                }
                connection.Close();
            }

            Console.WriteLine($"JobModel saved to database with JobId: {jobModel.JobId}");
            return jobModel;
        }

        public JobModel UpdateJob(JobModel jobModel)
        {
            Console.WriteLine($"Updating JobModel in SQLite database (Microsoft.Data.Sqlite) for JobId: {jobModel.JobId}");

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqliteCommand(@"
                            UPDATE JobModels
                            SET JobResultStatus = @JobResultStatus,
                                JobOutput = @JobOutput,
                            WHERE JobId = @JobId;
                        ", connection))
                {
                    command.Parameters.AddWithValue("@JobResultStatus", jobModel.JobResultStatus);
                    command.Parameters.AddWithValue("@JobOutput", JsonSerializer.Serialize(jobModel.JobOutput)); // Serialize JobOutput to JSON

                    command.ExecuteNonQuery(); // ExecuteNonQuery for UPDATE statements
                }
                connection.Close();
            }

            Console.WriteLine($"JobModel with JobId '{jobModel.JobId}' updated in database.");
            return jobModel;
        }

        private SqliteConnection GetConnection()
        {
            return new SqliteConnection($"Data Source={_dbFilePath}");
        }

        public List<JobModel>? GetJobsByAgentId(int agentId)
        {
            Console.WriteLine($"Retrieving JobModels by AgentId from SQLite (Microsoft.Data.Sqlite) for AgentId: {agentId}");
            List<JobModel> jobModels = new List<JobModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqliteCommand(
                    "SELECT JobId, JobType, JobData, AgentId, CreatedAt FROM JobModels WHERE AgentId = @AgentId"
                    ,connection
                ))
                {
                    command.Parameters.AddWithValue("@AgentId", agentId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            jobModels.Add(new JobModel()
                            {
                                JobId = reader.GetInt32(0),
                                JobType = reader.GetString(1),
                                JobData = JsonSerializer.Deserialize<object>(reader.GetString(2))!, // Deserialize JSON
                                AgentId = reader.GetInt32(3),
                                CreatedAt = reader.GetDateTime(4)
                            });
                        }
                    }
                }
                connection.Close();
            }
            
            return jobModels.Count > 0 ? jobModels : null;
        }

        public AgentModel? GetAgentByGuid(string agentGuid)
        {
            Console.WriteLine($"Retrieving AgentModel by GUID: {agentGuid}");
            AgentModel? agentModel = null;

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqliteCommand(
                    "SELECT AgentId, AgentGuid, AgentName, Status, LastCheckIn, Location, Plugins, Version, RegisteredAt, LastUpdated FROM AgentModels WHERE AgentGuid = @agentGuid"
                    , connection
                ))
                {
                    command.Parameters.AddWithValue("@agentGuid", agentGuid);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            agentModel = new AgentModel()
                            {
                                AgentId = reader.GetInt32(0),          // Ordinal 0: AgentId
                                AgentGuid = reader.GetString(1),        // Ordinal 1: AgentGuid
                                AgentName = reader.GetString(2),      // Ordinal 2: AgentName
                                Status = reader.GetString(3),         // Ordinal 3: Status
                                LastCheckIn = reader.IsDBNull(4) ? null : reader.GetDateTime(4), // Ordinal 4: LastCheckIn (nullable DateTime)
                                Location = reader.IsDBNull(5) ? null : reader.GetString(5),      // Ordinal 5: Location (nullable string)
                                Plugins = reader.IsDBNull(6) ? null : JsonSerializer.Deserialize<List<string>>(reader.GetString(6)), // Ordinal 6: Plugins (nullable JSON List<string>)
                                Version = reader.IsDBNull(7) ? null : reader.GetString(7),      // Ordinal 7: Version (nullable string)
                                RegisteredAt = reader.GetDateTime(8),     // Ordinal 8: RegisteredAt
                                LastUpdated = reader.GetDateTime(9)    // Ordinal 9: LastUpdated
                            };
                        }
                    }
                }
                connection.Close();
            }
            return agentModel;
        }

        public AgentModel SaveAgent(AgentModel agentModel)
        {
            Console.WriteLine("Saving AgentModel to SQLite database (Microsoft.Data.Sqlite) - UPSERT logic:");

            using (var connection = GetConnection())
            {
                connection.Open();

                // 1. Check if an agent with the given AgentGuid already exists
                AgentModel? existingAgent = GetAgentByGuid(agentModel.AgentGuid);

                if (existingAgent != null)
                {
                    // 2. Agent exists - UPDATE the existing row
                    Console.WriteLine($"Agent with AgentGuid '{agentModel.AgentGuid}' already exists. Updating...");
                    using (var command = new SqliteCommand(@"
                                    UPDATE AgentModels
                                    SET AgentName = @AgentName,
                                        Status = @Status,
                                        LastCheckIn = @LastCheckIn,
                                        Location = @Location,
                                        Plugins = @Plugins,
                                        Version = @Version,
                                        LastUpdated = CURRENT_TIMESTAMP
                                    WHERE AgentGuid = @AgentGuid;
                                ", connection))
                    {
                        command.Parameters.AddWithValue("@AgentGuid", agentModel.AgentGuid);
                        command.Parameters.AddWithValue("@AgentName", agentModel.AgentName);
                        command.Parameters.AddWithValue("@Status", agentModel.Status);
                        command.Parameters.AddWithValue("@LastCheckIn", agentModel.LastCheckIn);
                        command.Parameters.AddWithValue("@Location", agentModel.Location);
                        command.Parameters.AddWithValue("@Plugins", JsonSerializer.Serialize(agentModel.Plugins)); // Serialize Plugins to JSON
                        command.Parameters.AddWithValue("@Version", agentModel.Version);

                        command.ExecuteNonQuery(); // ExecuteNonQuery for UPDATE statements
                    }
                    agentModel.AgentId = existingAgent.AgentId; // Keep the existing AgentId
                    Console.WriteLine($"AgentModel with AgentGuid '{agentModel.AgentGuid}' updated in database.");
                }
                else
                {
                    // 3. Agent does not exist - INSERT a new row (original INSERT logic)
                    Console.WriteLine($"Agent with AgentGuid '{agentModel.AgentGuid}' does not exist. Inserting new...");
                    using (var command = new SqliteCommand(@"
                                    INSERT INTO AgentModels (AgentGuid, AgentName, Status, LastCheckIn, Location, Plugins, Version)
                                    VALUES (@AgentGuid, @AgentName, @Status, @LastCheckIn, @Location, @Plugins, @Version);
                                    SELECT last_insert_rowid(); -- Get the auto-generated AgentId
                                ", connection))
                    {
                        command.Parameters.AddWithValue("@AgentGuid", agentModel.AgentGuid);
                        command.Parameters.AddWithValue("@AgentName", agentModel.AgentName);
                        command.Parameters.AddWithValue("@Status", agentModel.Status);
                        command.Parameters.AddWithValue("@LastCheckIn", agentModel.LastCheckIn);
                        command.Parameters.AddWithValue("@Location", agentModel.Location);
                        command.Parameters.AddWithValue("@Plugins", JsonSerializer.Serialize(agentModel.Plugins)); // Serialize Plugins to JSON
                        command.Parameters.AddWithValue("@Version", agentModel.Version);

                        agentModel.AgentId = Convert.ToInt32(command.ExecuteScalar()); // ExecuteScalar for INSERT to get new ID
                    }
                    Console.WriteLine($"New AgentModel saved to database with AgentId: {agentModel.AgentId}");
                }
                connection.Close();
            }

            return agentModel;
        }
    }
}