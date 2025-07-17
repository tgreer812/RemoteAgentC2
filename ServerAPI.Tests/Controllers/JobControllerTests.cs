using AgentCommon.AgentPluginCommon;

namespace ServerAPI.Tests.Controllers
{
    public class JobControllerTests
    {
        private readonly Mock<IDatabase> _mockDatabase;
        private readonly JobController _controller;

        public JobControllerTests()
        {
            _mockDatabase = new Mock<IDatabase>();
            _controller = new JobController(_mockDatabase.Object);
        }

        [Fact]
        public void Get_WhenJobExists_ShouldReturnJob()
        {
            // Arrange
            var jobId = 1;
            var expectedJob = new JobModel
            {
                JobId = jobId,
                JobType = "PluginJob",
                JobData = new { pluginName = "TestPlugin" },
                AgentId = 1,
                JobResultStatus = "Created"
            };

            _mockDatabase.Setup(db => db.GetJobById(jobId))
                        .Returns(expectedJob);

            // Act
            var result = _controller.Get(jobId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedJob);
        }

        [Fact]
        public void Get_WhenJobDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var jobId = 999;
            _mockDatabase.Setup(db => db.GetJobById(jobId))
                        .Returns((JobModel?)null);

            // Act
            var result = _controller.Get(jobId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Post_WithValidJob_ShouldSaveAndReturnJob()
        {
            // Arrange
            var job = new JobModel
            {
                JobType = "PluginJob",
                JobData = new { pluginName = "TestPlugin", pluginArguments = new { path = "C:\\test" } },
                AgentId = 1
            };

            var savedJob = new JobModel
            {
                JobId = 1,
                JobType = job.JobType,
                JobData = job.JobData,
                AgentId = job.AgentId,
                JobResultStatus = "Created"
            };

            _mockDatabase.Setup(db => db.SaveJob(job))
                        .Returns(savedJob);

            // Act
            var result = _controller.Post(job);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedJob = okResult.Value.Should().BeOfType<JobModel>().Subject;
            returnedJob.Should().BeEquivalentTo(savedJob);

            _mockDatabase.Verify(db => db.SaveJob(job), Times.Once);
        }

        [Fact]
        public void Post_WithNullJob_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.Post(null!);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Request body is missing or invalid.");
        }

        [Fact]
        public void Post_WithInvalidAgentId_ShouldReturnBadRequest()
        {
            // Arrange
            var job = new JobModel
            {
                JobType = "PluginJob",
                JobData = new { pluginName = "TestPlugin" },
                AgentId = 0 // Invalid agent ID
            };

            // Act
            var result = _controller.Post(job);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Agent ID is required and must be a positive integer.");
        }

        [Fact]
        public void Post_WithEmptyJobType_ShouldReturnBadRequest()
        {
            // Arrange
            var job = new JobModel
            {
                JobType = "",
                JobData = new { pluginName = "TestPlugin" },
                AgentId = 1
            };

            // Act
            var result = _controller.Post(job);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Job type is required.");
        }

        [Fact]
        public void Post_WithNullJobData_ShouldReturnBadRequest()
        {
            // Arrange
            var job = new JobModel
            {
                JobType = "PluginJob",
                JobData = null!,
                AgentId = 1
            };

            // Act
            var result = _controller.Post(job);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Job data is required.");
        }

        [Fact]
        public void Post_WhenDatabaseSaveReturnsNull_ShouldReturnInternalServerError()
        {
            // Arrange
            var job = new JobModel
            {
                JobType = "PluginJob",
                JobData = new { pluginName = "TestPlugin" },
                AgentId = 1
            };

            _mockDatabase.Setup(db => db.SaveJob(job))
                        .Returns((JobModel?)null);

            // Act
            var result = _controller.Post(job);

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
            statusResult.Value.Should().Be("Failed to save job to the database. Please try again later.");
        }

        [Fact]
        public void Post_WhenDatabaseThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var job = new JobModel
            {
                JobType = "PluginJob",
                JobData = new { pluginName = "TestPlugin" },
                AgentId = 1
            };

            _mockDatabase.Setup(db => db.SaveJob(job))
                        .Throws(new Exception("Database error"));

            // Act
            var result = _controller.Post(job);

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
            statusResult.Value.Should().Be("An unexpected error occurred while processing your request. Please try again later.");
        }

        [Fact]
        public void Put_WithValidPluginResult_ShouldUpdateJob()
        {
            // Arrange
            var jobId = 1;
            var pluginResult = new PluginResult
            {
                CorrelationId = 1,
                Status = PluginStatus.Success,
                OutputData = new { files = new[] { "file1.txt", "file2.txt" } },
                ErrorMessage = null
            };

            var existingJob = new JobModel
            {
                JobId = jobId,
                JobType = "PluginJob",
                JobData = new { pluginName = "TestPlugin" },
                AgentId = 1,
                JobResultStatus = "Sent"
            };

            var updatedJob = new JobModel
            {
                JobId = jobId,
                JobType = "PluginJob", 
                JobData = new { pluginName = "TestPlugin" },
                AgentId = 1,
                JobResultStatus = "Success",
                JobOutput = pluginResult.OutputData
            };

            _mockDatabase.Setup(db => db.GetJobById(jobId))
                        .Returns(existingJob);
            _mockDatabase.Setup(db => db.UpdateJob(It.IsAny<JobModel>()))
                        .Returns(updatedJob);

            // Act
            var result = _controller.Put(jobId, pluginResult);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedJob = okResult.Value.Should().BeOfType<JobModel>().Subject;
            returnedJob.Should().BeEquivalentTo(updatedJob);

            _mockDatabase.Verify(db => db.GetJobById(jobId), Times.Once);
            _mockDatabase.Verify(db => db.UpdateJob(It.Is<JobModel>(j => 
                j.JobId == jobId && 
                j.JobResultStatus == "Success" && 
                j.JobOutput == pluginResult.OutputData)), Times.Once);
        }

        [Fact]
        public void Put_WithNullPluginResult_ShouldReturnBadRequest()
        {
            // Arrange
            var jobId = 1;

            // Act
            var result = _controller.Put(jobId, null!);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Request body is missing or invalid.");
        }

        [Fact]
        public void Put_WhenJobDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var jobId = 999;
            var pluginResult = new PluginResult
            {
                CorrelationId = (uint)jobId,
                Status = PluginStatus.Success,
                OutputData = new { result = "success" }
            };

            _mockDatabase.Setup(db => db.GetJobById(jobId))
                        .Returns((JobModel?)null);

            // Act
            var result = _controller.Put(jobId, pluginResult);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be($"Job with ID {jobId} not found.");
        }

        [Fact]
        public void Put_WhenUpdateFails_ShouldReturnInternalServerError()
        {
            // Arrange
            var jobId = 1;
            var pluginResult = new PluginResult
            {
                CorrelationId = 1,
                Status = PluginStatus.Failed,
                ErrorMessage = "Plugin failed"
            };

            var existingJob = new JobModel
            {
                JobId = jobId,
                JobType = "PluginJob",
                AgentId = 1
            };

            _mockDatabase.Setup(db => db.GetJobById(jobId))
                        .Returns(existingJob);
            _mockDatabase.Setup(db => db.UpdateJob(It.IsAny<JobModel>()))
                        .Returns((JobModel?)null);

            // Act
            var result = _controller.Put(jobId, pluginResult);

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
            statusResult.Value.Should().Be($"Failed to update job with ID {jobId} in the database. Please try again later.");
        }

        [Fact]
        public void Put_WhenDatabaseThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var jobId = 1;
            var pluginResult = new PluginResult
            {
                CorrelationId = 1,
                Status = PluginStatus.Success
            };

            var existingJob = new JobModel
            {
                JobId = jobId,
                JobType = "PluginJob",
                AgentId = 1
            };

            _mockDatabase.Setup(db => db.GetJobById(jobId))
                        .Returns(existingJob);
            _mockDatabase.Setup(db => db.UpdateJob(It.IsAny<JobModel>()))
                        .Throws(new Exception("Database error"));

            // Act
            var result = _controller.Put(jobId, pluginResult);

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
            statusResult.Value.Should().Be($"An unexpected error occurred while processing your request to update job ID {jobId}. Please try again later.");
        }

        [Fact]
        public void Constructor_WithNullDatabase_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new JobController(null!));
        }
    }
}
