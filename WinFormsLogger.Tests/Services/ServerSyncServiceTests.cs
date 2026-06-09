using Xunit;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WinFormsLogger.Services;
using WinFormsLogger.DB.Tables;
using WinFormsLogger.DB.Models;

namespace WinFormsLogger.Tests.Services;

public class ServerSyncServiceTests
{
    private readonly Mock<IProcessRepository> _mockProcessRepo;
    private readonly Mock<IScheduleRepository> _mockScheduleRepo;
    private readonly Mock<IPcStatusRepository> _mockPcStatusRepo;
    private readonly Mock<IDeviceIdentityService> _mockDeviceIdentity;
    private readonly Mock<ICredentialService> _mockCredential;
    private readonly Mock<ILogger<ServerSyncService>> _mockLogger;
    private readonly AppSettings _appSettings;

    public ServerSyncServiceTests()
    {
        _mockProcessRepo = new Mock<IProcessRepository>();
        _mockScheduleRepo = new Mock<IScheduleRepository>();
        _mockPcStatusRepo = new Mock<IPcStatusRepository>();
        _mockDeviceIdentity = new Mock<IDeviceIdentityService>();
        _mockCredential = new Mock<ICredentialService>();
        _mockLogger = new Mock<ILogger<ServerSyncService>>();
        
        _appSettings = new AppSettings
        {
            ServerUrl = "http://localhost:8080"
        };
    }

    [Fact]
    public async Task SyncAsync_WhenNoUnsyncedData_ReturnsNoUnsyncedData()
    {
        // Arrange
        _mockProcessRepo.Setup(r => r.GetAllProcesses()).Returns(new List<Process>());
        _mockScheduleRepo.Setup(r => r.GetAll()).Returns(new List<Schedule>());

        var service = new ServerSyncService(
            _mockProcessRepo.Object,
            _mockScheduleRepo.Object,
            _mockPcStatusRepo.Object,
            _mockDeviceIdentity.Object,
            _mockCredential.Object,
            _appSettings,
            _mockLogger.Object
        );

        // Act
        var result = await service.SyncAsync();

        // Assert
        Assert.Equal(SyncStatus.NoUnsyncedData, result);
    }

    [Fact]
    public async Task SyncAsync_WhenNotAuthenticated_ReturnsNotAuthenticated()
    {
        // Arrange
        var unsyncedProcesses = new List<Process>
        {
            new Process { Id = 1, ProcessName = "notepad", WindowsName = "Notes", ProcessStart = DateTime.Now, Duration = 60, IsSynced = false }
        };
        _mockProcessRepo.Setup(r => r.GetAllProcesses()).Returns(unsyncedProcesses);
        _mockScheduleRepo.Setup(r => r.GetAll()).Returns(new List<Schedule>());
        _mockCredential.Setup(c => c.GetCredentials()).Returns(((string, string)?)null);

        var service = new ServerSyncService(
            _mockProcessRepo.Object,
            _mockScheduleRepo.Object,
            _mockPcStatusRepo.Object,
            _mockDeviceIdentity.Object,
            _mockCredential.Object,
            _appSettings,
            _mockLogger.Object
        );

        // Act
        var result = await service.SyncAsync();

        // Assert
        Assert.Equal(SyncStatus.NotAuthenticated, result);
    }

    [Fact]
    public async Task SyncAsync_WhenSyncSucceeds_UpdatesSyncedFlagsAndReturnsSuccess()
    {
        // Arrange
        var unsyncedProcesses = new List<Process>
        {
            new Process { Id = 1, ProcessName = "notepad", WindowsName = "Notes", ProcessStart = new DateTime(2026, 1, 1, 12, 0, 0), Duration = 60, IsSynced = false }
        };
        _mockProcessRepo.Setup(r => r.GetAllProcesses()).Returns(unsyncedProcesses);
        _mockScheduleRepo.Setup(r => r.GetAll()).Returns(new List<Schedule>());
        
        _mockCredential.Setup(c => c.GetCredentials()).Returns(("user@example.com", "fake-jwt-token"));
        _mockDeviceIdentity.Setup(d => d.GetDeviceId()).Returns("test-device-id");

        // Set up mock HTTP handler returning OK (200)
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"status\":\"success\"}")
            })
            .Verifiable();

        var mockHttpClient = new HttpClient(handlerMock.Object);

        var service = new ServerSyncService(
            _mockProcessRepo.Object,
            _mockScheduleRepo.Object,
            _mockPcStatusRepo.Object,
            _mockDeviceIdentity.Object,
            _mockCredential.Object,
            _appSettings,
            _mockLogger.Object,
            mockHttpClient
        );

        // Act
        var result = await service.SyncAsync();

        // Assert
        Assert.Equal(SyncStatus.Success, result);
        _mockProcessRepo.Verify(r => r.UpdateProcess(It.Is<Process>(p => p.Id == 1 && p.IsSynced)), Times.Once);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().EndsWith("/api/v1/pcs") &&
                req.Headers.Authorization!.Scheme == "Bearer" &&
                req.Headers.Authorization!.Parameter == "fake-jwt-token"
            ),
            ItExpr.IsAny<CancellationToken>()
        );
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().Contains("/api/v1/pcs/test-device-id/processes") &&
                req.Headers.Authorization!.Scheme == "Bearer" &&
                req.Headers.Authorization!.Parameter == "fake-jwt-token"
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task LoginAsync_WhenSuccessful_ReturnsToken()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-login-token\"}")
            })
            .Verifiable();

        var mockHttpClient = new HttpClient(handlerMock.Object);

        var service = new ServerSyncService(
            _mockProcessRepo.Object,
            _mockScheduleRepo.Object,
            _mockPcStatusRepo.Object,
            _mockDeviceIdentity.Object,
            _mockCredential.Object,
            _appSettings,
            _mockLogger.Object,
            mockHttpClient
        );

        // Act
        var result = await service.LoginAsync("user@example.com", "password123");

        // Assert
        Assert.Equal("fake-login-token", result);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().Contains("/api/v1/auth/login")
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task LoginAsync_WhenFailed_ThrowsException()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Invalid credentials")
            })
            .Verifiable();

        var mockHttpClient = new HttpClient(handlerMock.Object);

        var service = new ServerSyncService(
            _mockProcessRepo.Object,
            _mockScheduleRepo.Object,
            _mockPcStatusRepo.Object,
            _mockDeviceIdentity.Object,
            _mockCredential.Object,
            _appSettings,
            _mockLogger.Object,
            mockHttpClient
        );

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.LoginAsync("user@example.com", "wrong-password"));
    }

    [Fact]
    public async Task SyncAsync_WhenScheduleSyncSucceeds_UpdatesSyncedFlagsAndReturnsSuccess()
    {
        // Arrange
        var unsyncedSchedules = new List<Schedule>
        {
            new Schedule { Id = 1, PcStatusId = 2, Timestamp = new DateTime(2026, 1, 1, 12, 0, 0), IsSynced = false }
        };
        var pcStatuses = new List<PcStatus>
        {
            new PcStatus { Id = 2, Status = "PowerOn" }
        };

        _mockProcessRepo.Setup(r => r.GetAllProcesses()).Returns(new List<Process>());
        _mockScheduleRepo.Setup(r => r.GetAll()).Returns(unsyncedSchedules);
        _mockPcStatusRepo.Setup(r => r.GetAll()).Returns(pcStatuses);
        
        _mockCredential.Setup(c => c.GetCredentials()).Returns(("user@example.com", "fake-jwt-token"));
        _mockDeviceIdentity.Setup(d => d.GetDeviceId()).Returns("test-device-id");

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"status\":\"success\"}")
            })
            .Verifiable();

        var mockHttpClient = new HttpClient(handlerMock.Object);

        var service = new ServerSyncService(
            _mockProcessRepo.Object,
            _mockScheduleRepo.Object,
            _mockPcStatusRepo.Object,
            _mockDeviceIdentity.Object,
            _mockCredential.Object,
            _appSettings,
            _mockLogger.Object,
            mockHttpClient
        );

        // Act
        var result = await service.SyncAsync();

        // Assert
        Assert.Equal(SyncStatus.Success, result);
        _mockScheduleRepo.Verify(r => r.Update(It.Is<Schedule>(s => s.Id == 1 && s.IsSynced)), Times.Once);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().EndsWith("/api/v1/pcs") &&
                req.Headers.Authorization!.Scheme == "Bearer" &&
                req.Headers.Authorization!.Parameter == "fake-jwt-token"
            ),
            ItExpr.IsAny<CancellationToken>()
        );
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().Contains("/api/v1/pcs/test-device-id/schedules") &&
                req.Headers.Authorization!.Scheme == "Bearer" &&
                req.Headers.Authorization!.Parameter == "fake-jwt-token"
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task SyncAsync_WhenBothSucceed_UpdatesAllSyncedFlagsAndReturnsSuccess()
    {
        // Arrange
        var unsyncedProcesses = new List<Process>
        {
            new Process { Id = 10, ProcessName = "chrome", WindowsName = "Google", ProcessStart = new DateTime(2026, 1, 1, 12, 0, 0), Duration = 30, IsSynced = false }
        };
        var unsyncedSchedules = new List<Schedule>
        {
            new Schedule { Id = 20, PcStatusId = 5, Timestamp = new DateTime(2026, 1, 1, 12, 10, 0), IsSynced = false }
        };
        var pcStatuses = new List<PcStatus>
        {
            new PcStatus { Id = 5, Status = "Unlocked" }
        };

        _mockProcessRepo.Setup(r => r.GetAllProcesses()).Returns(unsyncedProcesses);
        _mockScheduleRepo.Setup(r => r.GetAll()).Returns(unsyncedSchedules);
        _mockPcStatusRepo.Setup(r => r.GetAll()).Returns(pcStatuses);
        
        _mockCredential.Setup(c => c.GetCredentials()).Returns(("user@example.com", "fake-jwt-token"));
        _mockDeviceIdentity.Setup(d => d.GetDeviceId()).Returns("test-device-id");

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"status\":\"success\"}")
            })
            .Verifiable();

        var mockHttpClient = new HttpClient(handlerMock.Object);

        var service = new ServerSyncService(
            _mockProcessRepo.Object,
            _mockScheduleRepo.Object,
            _mockPcStatusRepo.Object,
            _mockDeviceIdentity.Object,
            _mockCredential.Object,
            _appSettings,
            _mockLogger.Object,
            mockHttpClient
        );

        // Act
        var result = await service.SyncAsync();

        // Assert
        Assert.Equal(SyncStatus.Success, result);
        _mockProcessRepo.Verify(r => r.UpdateProcess(It.Is<Process>(p => p.Id == 10 && p.IsSynced)), Times.Once);
        _mockScheduleRepo.Verify(r => r.Update(It.Is<Schedule>(s => s.Id == 20 && s.IsSynced)), Times.Once);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(3), // 1 for registration, 1 for processes, 1 for schedules
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task SyncAsync_WhenProcessSyncFailsButScheduleSucceeds_ReturnsPartiallyFailed()
    {
        // Arrange
        var unsyncedProcesses = new List<Process>
        {
            new Process { Id = 10, ProcessName = "chrome", WindowsName = "Google", ProcessStart = new DateTime(2026, 1, 1, 12, 0, 0), Duration = 30, IsSynced = false }
        };
        var unsyncedSchedules = new List<Schedule>
        {
            new Schedule { Id = 20, PcStatusId = 5, Timestamp = new DateTime(2026, 1, 1, 12, 10, 0), IsSynced = false }
        };
        var pcStatuses = new List<PcStatus>
        {
            new PcStatus { Id = 5, Status = "Unlocked" }
        };

        _mockProcessRepo.Setup(r => r.GetAllProcesses()).Returns(unsyncedProcesses);
        _mockScheduleRepo.Setup(r => r.GetAll()).Returns(unsyncedSchedules);
        _mockPcStatusRepo.Setup(r => r.GetAll()).Returns(pcStatuses);
        
        _mockCredential.Setup(c => c.GetCredentials()).Returns(("user@example.com", "fake-jwt-token"));
        _mockDeviceIdentity.Setup(d => d.GetDeviceId()).Returns("test-device-id");

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        
        // Setup registration to succeed
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().EndsWith("/api/v1/pcs")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"status\":\"success\"}")
            });

        // Setup processes sync to fail with InternalServerError (500)
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/api/v1/pcs/test-device-id/processes")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Server Error")
            });

        // Setup schedules sync to succeed
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/api/v1/pcs/test-device-id/schedules")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"status\":\"success\"}")
            });

        var mockHttpClient = new HttpClient(handlerMock.Object);

        var service = new ServerSyncService(
            _mockProcessRepo.Object,
            _mockScheduleRepo.Object,
            _mockPcStatusRepo.Object,
            _mockDeviceIdentity.Object,
            _mockCredential.Object,
            _appSettings,
            _mockLogger.Object,
            mockHttpClient
        );

        // Act
        var result = await service.SyncAsync();

        // Assert
        Assert.Equal(SyncStatus.PartiallyFailed, result);
        _mockProcessRepo.Verify(r => r.UpdateProcess(It.IsAny<Process>()), Times.Never);
        _mockScheduleRepo.Verify(r => r.Update(It.Is<Schedule>(s => s.Id == 20 && s.IsSynced)), Times.Once);
    }

    [Fact]
    public async Task SyncAsync_WhenExceptionOccurs_ReturnsFailed()
    {
        // Arrange
        _mockProcessRepo.Setup(r => r.GetAllProcesses()).Throws(new Exception("Database connection lost"));

        var service = new ServerSyncService(
            _mockProcessRepo.Object,
            _mockScheduleRepo.Object,
            _mockPcStatusRepo.Object,
            _mockDeviceIdentity.Object,
            _mockCredential.Object,
            _appSettings,
            _mockLogger.Object
        );

        // Act
        var result = await service.SyncAsync();

        // Assert
        Assert.Equal(SyncStatus.Failed, result);
    }
}
