using Xunit;
using Moq;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using WinFormsLogger.Services;
using WinFormsLogger.DB.Tables;
using WinFormsLogger.DB.Models;

namespace WinFormsLogger.Tests;

public class Form1Tests
{
    private readonly Mock<ILogger<Form1>> _mockLogger = new();
    private readonly Mock<IProcessRepository> _mockProcessRepo = new();
    private readonly Mock<IProcessTracer> _mockProcessTracer = new();
    private readonly Mock<ICredentialService> _mockCredentialService = new();
    private readonly Mock<ISystemEventWatcher> _mockSystemEventWatcher = new();
    private readonly Mock<IServerSyncService> _mockServerSyncService = new();
    private readonly Mock<IConfigRepository> _mockConfigRepo = new();
    private readonly Mock<IDeviceIdentityService> _mockDeviceIdentityService = new();
    private readonly AppSettings _appSettings = new();

    private void RunInSTA(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }

    [Fact]
    public void FormClosing_WhenExiting_TriggersServerSync()
    {
        RunInSTA(() =>
        {
            // Arrange
            _mockServerSyncService.Setup(s => s.SyncAsync())
                .ReturnsAsync(SyncStatus.Success)
                .Verifiable();

            var form = new Form1(
                _mockLogger.Object,
                _mockProcessRepo.Object,
                _mockProcessTracer.Object,
                _mockCredentialService.Object,
                _mockSystemEventWatcher.Object,
                _mockServerSyncService.Object,
                _mockConfigRepo.Object,
                _mockDeviceIdentityService.Object,
                _appSettings
            );

            // Access form handle to force control initialization
            var handle = form.Handle;

            // Act: Simulate Application Exit (CloseReason.ApplicationExitCall)
            var args = new FormClosingEventArgs(CloseReason.ApplicationExitCall, false);
            var method = typeof(Form1).GetMethod("Form1_FormClosing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(form, new object[] { form, args });

            // Assert
            Assert.False(args.Cancel); // Exit should proceed
            _mockServerSyncService.Verify(s => s.SyncAsync(), Times.Once);
        });
    }

    [Fact]
    public void FormClosing_WhenUserClosingAndNotExiting_CancelsCloseAndHides()
    {
        RunInSTA(() =>
        {
            // Arrange
            var form = new Form1(
                _mockLogger.Object,
                _mockProcessRepo.Object,
                _mockProcessTracer.Object,
                _mockCredentialService.Object,
                _mockSystemEventWatcher.Object,
                _mockServerSyncService.Object,
                _mockConfigRepo.Object,
                _mockDeviceIdentityService.Object,
                _appSettings
            );

            // Access form handle to force control initialization
            var handle = form.Handle;

            // Act: Simulate UserClosing (clicking the 'X')
            var args = new FormClosingEventArgs(CloseReason.UserClosing, false);
            var method = typeof(Form1).GetMethod("Form1_FormClosing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(form, new object[] { form, args });

            // Assert
            Assert.True(args.Cancel); // Close should be cancelled
            Assert.False(form.Visible); // Form should be hidden
            _mockServerSyncService.Verify(s => s.SyncAsync(), Times.Never); // Should not sync
        });
    }
}
