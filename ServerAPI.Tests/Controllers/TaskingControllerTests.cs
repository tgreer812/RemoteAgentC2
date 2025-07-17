using AgentCommon.AgentPluginCommon;

namespace ServerAPI.Tests.Controllers
{
    public class TaskingControllerTests
    {
        private readonly Mock<IDatabase> _mockDatabase;
        private readonly TaskingController _controller;

        public TaskingControllerTests()
        {
            _mockDatabase = new Mock<IDatabase>();
            _controller = new TaskingController(_mockDatabase.Object);
        }

        [Fact]
        public void Get_WhenNoJobsExist_ShouldReturnEmptyJobsArray()
        {
            // Arrange
            var agentId = 1;
            _mockDatabase.Setup(db => db.GetJobsByAgentId(agentId))
                        .Returns((List<JobModel>?)null);

            // Act
            var result = _controller.Get(agentId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeEquivalentTo(new { jobs = new List<object>() });
        }

        [Fact]
        public void Get_WhenJobsExistButAllSent_ShouldReturnEmptyJobsArray()
        {
            // Arrange
            var agentId = 1;
            var jobs = new List<JobModel>
            {
                new JobModel
                {
                    JobId = 1,
                    JobType = "PluginJob",
                    JobData = new { test = "data" },
                    AgentId = agentId,
                    JobResultStatus = "Sent"
                }
            };

            _mockDatabase.Setup(db => db.GetJobsByAgentId(agentId))
                        .Returns(jobs);

            // Act
            var result = _controller.Get(agentId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeEquivalentTo(new { jobs = new List<object>() });
        }

        [Fact]
        public void Get_WhenPendingJobsExist_ShouldReturnJobsAndMarkAsSent()
        {
            // Arrange
            var agentId = 1;
            var jobs = new List<JobModel>
            {
                new JobModel
                {
                    JobId = 1,
                    JobType = "PluginJob",
                    JobData = new { pluginName = "TestPlugin", pluginArguments = new { path = "C:\\test" } },
                    AgentId = agentId,
                    JobResultStatus = "Created"
                },
                new JobModel
                {
                    JobId = 2,
                    JobType = "PluginJob",
                    JobData = new { pluginName = "SystemInfo" },
                    AgentId = agentId,
                    JobResultStatus = null
                }
            };

            _mockDatabase.Setup(db => db.GetJobsByAgentId(agentId))
                        .Returns(jobs);
            _mockDatabase.Setup(db => db.MarkJobsAsSent(It.IsAny<List<int>>()))
                        .Returns(true);

            // Act
            var result = _controller.Get(agentId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            
            // Verify the response structure
            var responseValue = okResult.Value;
            responseValue.Should().NotBeNull();

            // Verify MarkJobsAsSent was called with correct job IDs
            _mockDatabase.Verify(db => db.MarkJobsAsSent(It.Is<List<int>>(ids => 
                ids.Count == 2 && ids.Contains(1) && ids.Contains(2))), Times.Once);
        }

        [Fact]
        public void Get_WhenMixedJobStatuses_ShouldOnlyReturnPendingJobs()
        {
            // Arrange
            var agentId = 1;
            var jobs = new List<JobModel>
            {
                new JobModel { JobId = 1, JobType = "PluginJob", JobData = new { test = "data1" }, AgentId = agentId, JobResultStatus = "Created" },
                new JobModel { JobId = 2, JobType = "PluginJob", JobData = new { test = "data2" }, AgentId = agentId, JobResultStatus = "Sent" },
                new JobModel { JobId = 3, JobType = "PluginJob", JobData = new { test = "data3" }, AgentId = agentId, JobResultStatus = "Completed" },
                new JobModel { JobId = 4, JobType = "PluginJob", JobData = new { test = "data4" }, AgentId = agentId, JobResultStatus = null }
            };

            _mockDatabase.Setup(db => db.GetJobsByAgentId(agentId))
                        .Returns(jobs);
            _mockDatabase.Setup(db => db.MarkJobsAsSent(It.IsAny<List<int>>()))
                        .Returns(true);

            // Act
            var result = _controller.Get(agentId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

            // Verify only jobs 1 and 4 (Created and null status) are marked as sent
            _mockDatabase.Verify(db => db.MarkJobsAsSent(It.Is<List<int>>(ids => 
                ids.Count == 2 && ids.Contains(1) && ids.Contains(4))), Times.Once);
        }

        [Fact]
        public void Post_WithValidAgentTask_ShouldSaveJobsAndReturnThem()
        {
            // Arrange
            var agentTask = new AgentTask();
            var job1 = new JobModel
            {
                JobType = "PluginJob",
                JobData = new { pluginName = "TestPlugin" },
                AgentId = 1
            };
            var job2 = new JobModel
            {
                JobType = "PluginJob", 
                JobData = new { pluginName = "SystemInfo" },
                AgentId = 1
            };
            agentTask.AddJob(job1);
            agentTask.AddJob(job2);

            var savedJob1 = new JobModel { JobId = 1, JobType = job1.JobType, JobData = job1.JobData, AgentId = job1.AgentId };
            var savedJob2 = new JobModel { JobId = 2, JobType = job2.JobType, JobData = job2.JobData, AgentId = job2.AgentId };

            // Setup sequence for SaveJob calls
            _mockDatabase.SetupSequence(db => db.SaveJob(It.IsAny<JobModel>()))
                        .Returns(savedJob1)
                        .Returns(savedJob2);

            // Act
            var result = _controller.Post(agentTask);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var savedJobs = okResult.Value.Should().BeOfType<List<JobModel>>().Subject;
            savedJobs.Should().HaveCount(2);
            savedJobs.Should().Contain(j => j.JobId == 1);
            savedJobs.Should().Contain(j => j.JobId == 2);

            _mockDatabase.Verify(db => db.SaveJob(It.IsAny<JobModel>()), Times.Exactly(2));
        }

        [Fact]
        public void Post_WithInvalidModel_ShouldReturnBadRequest()
        {
            // Arrange
            var agentTask = new AgentTask();
            _controller.ModelState.AddModelError("Jobs", "Required");

            // Act
            var result = _controller.Post(agentTask);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void Constructor_WithNullDatabase_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TaskingController(null!));
        }
    }
}
