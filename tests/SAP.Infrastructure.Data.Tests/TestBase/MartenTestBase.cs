using Marten;
using Microsoft.Extensions.Logging;
using Moq;

namespace SAP.Infrastructure.Data.Tests.TestBase;

/// <summary>
/// Base class for Infrastructure tests.
/// Provides mock document session for testing.
/// </summary>
public abstract class MartenTestBase : IDisposable
{
    protected IDocumentSession Session { get; private set; }
    protected ILogger Logger { get; private set; }

    protected MartenTestBase()
    {
        // Create a logger for testing
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        Logger = loggerFactory.CreateLogger<MartenTestBase>();

        // Create a mock session for testing
        var mockSession = new Mock<IDocumentSession>();
        Session = mockSession.Object;
    }

    /// <summary>
    /// Clean up test resources.
    /// </summary>
    public virtual void Dispose()
    {
        Session?.Dispose();
        GC.SuppressFinalize(this);
    }
} 