# Remote Agent Server API

A RESTful API server for managing remote agent tasks and job execution in the RemoteAgent system. This API provides endpoints for job management, task distribution, and agent communication.

## 🚀 Quick Start

### Running the Application

```bash
# From the ServerAPI directory
dotnet run

# Or with specific project path
dotnet run --project RemoteAgentServerAPI.csproj
```

### Accessing the API

The API will start on the following URLs:
- **HTTP**: `http://localhost:5148`
- **HTTPS**: `https://localhost:7093`

## 📖 API Documentation (Swagger)

### Swagger UI Access

The **Swagger UI** is available at the **root URL** of the application:

- **HTTP**: **http://localhost:5148**
- **HTTPS**: **https://localhost:7093**

> ⚠️ **Note**: The Swagger UI is configured to be at the root URL (`/`), **NOT** at `/swagger`. 

### Swagger Features

- 🧪 **Interactive Testing** - Test all endpoints directly from the browser
- 📋 **API Documentation** - Complete endpoint documentation with examples
- 🔍 **Request/Response Schemas** - View data models and validation rules
- 📊 **HTTP Status Codes** - See all possible response codes for each endpoint

## 🛠 API Endpoints

### TaskingController (`/api/Tasking`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/Tasking/{agentId}` | Get pending jobs for a specific agent |
| `POST` | `/api/Tasking` | Submit new agent tasks |

### JobController (`/api/Job`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/Job/{id}` | Get a specific job by ID |
| `POST` | `/api/Job` | Create a new job |
| `PUT` | `/api/Job/{id}` | Update job with plugin execution results |

## 📊 Data Models

### JobModel
```csharp
{
  "jobId": 1,
  "jobType": "PluginJob",
  "jobData": {
    "pluginName": "SystemInfo",
    "arguments": {}
  },
  "agentId": 1,
  "jobResultStatus": "Created",
  "createdAt": "2025-07-16T10:30:00Z",
  "jobOutput": null
}
```

### AgentTask
```csharp
{
  "jobs": [
    {
      "jobType": "PluginJob",
      "jobData": {
        "pluginName": "SystemInfo"
      },
      "agentId": 1
    }
  ]
}
```

### PluginResult
```csharp
{
  "outputData": {
    "result": "Command executed successfully"
  },
  "errorMessage": "",
  "isSuccess": true
}
```

## 🗃 Database

The API uses **SQLite** for data persistence with the following key features:

- **Database File**: `agent_tasking.db` (created automatically)
- **ORM**: Custom SQLite implementation
- **Models**: JobModel, AgentModel
- **Automatic Schema Creation**: Database and tables created on first run

### Database Configuration

Configure the database path in `appsettings.json`:

```json
{
  "SqliteDbFilePath": "custom_database.db"
}
```

## 🏗 Project Structure

```
ServerAPI/
├── Controllers/
│   ├── JobController.cs       # Job management endpoints
│   └── TaskingController.cs   # Agent task distribution
├── Database/
│   ├── IDatabase.cs          # Database interface
│   ├── SqlLiteDatabase.cs    # SQLite implementation
│   ├── Models/               # Data models
│   └── Serializers/          # JSON converters
├── Models/
│   ├── AgentTask.cs          # Task submission model
│   ├── AgentHello.cs         # Agent registration
│   └── PluginResult.cs       # Job result model
├── Properties/
│   └── launchSettings.json   # Launch configuration
├── appsettings.json          # Application configuration
└── Program.cs                # Application startup
```

## 🔧 Configuration

### Application Settings (`appsettings.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "SqliteDbFilePath": "agent_tasking.db"
}
```

### Launch Settings

The application is configured with multiple launch profiles:

- **HTTP**: `http://localhost:5148`
- **HTTPS**: `https://localhost:7093;http://localhost:5148`
- **IIS Express**: Available for development

## 🧪 Testing

### Unit Tests

Comprehensive unit tests are available in the `ServerAPI.Tests` project:

```bash
# Run all tests
dotnet test ../ServerAPI.Tests/

# Run with detailed output
dotnet test ../ServerAPI.Tests/ --verbosity normal
```

### Manual Testing with Swagger

1. **Start the application**: `dotnet run`
2. **Open Swagger UI**: Navigate to `http://localhost:5148`
3. **Test endpoints**: Use the "Try it out" feature for each endpoint

### Example API Calls

#### Create a Job
```bash
POST /api/Job
Content-Type: application/json

{
  "jobType": "PluginJob",
  "jobData": {
    "pluginName": "SystemInfo",
    "arguments": {}
  },
  "agentId": 1
}
```

#### Get Jobs for Agent
```bash
GET /api/Tasking/1
```

#### Update Job Results
```bash
PUT /api/Job/1
Content-Type: application/json

{
  "outputData": {
    "systemInfo": "Windows 11, 16GB RAM, Intel i7"
  },
  "errorMessage": "",
  "isSuccess": true
}
```

## 🔐 Security & CORS

The API is configured with:

- **CORS**: Enabled for all origins (development setup)
- **HTTPS**: Available on port 7093
- **Development Environment**: Swagger enabled only in development

## 🚨 Common Issues

### Application Already Running
If you see "file is locked" errors, another instance is already running:
```bash
# Find and kill the process
tasklist | findstr RemoteAgentServerAPI
taskkill /PID <process_id> /F
```

### Swagger Not Loading
- Ensure you're using the **root URL** (`http://localhost:5148`)
- **NOT** `/swagger` endpoint
- Check that the application is running in Development environment

### Database Issues
- Database file is created automatically in the application directory
- Check file permissions if database creation fails
- Default location: `agent_tasking.db` in the project root

## 📝 Development Notes

- **Framework**: ASP.NET Core 8.0
- **Architecture**: RESTful API with controller-based routing
- **Database**: SQLite with custom ORM implementation
- **Documentation**: Swagger/OpenAPI with XML comments
- **Testing**: xUnit with comprehensive controller tests

## 🤝 Contributing

1. Follow the existing code structure and naming conventions
2. Add appropriate XML documentation for new endpoints
3. Include unit tests for new functionality
4. Update this README for any new features or configuration changes

## 📋 TODO / Future Enhancements

- [ ] Add authentication and authorization
- [ ] Implement rate limiting
- [ ] Add structured logging (Serilog)
- [ ] Add health check endpoints
- [ ] Implement API versioning
- [ ] Add integration tests
- [ ] Add Docker support
- [ ] Implement caching for frequently accessed data
