namespace ImageSearch.Test;
using Microsoft.Extensions.Logging;

public class TestBase
{
    protected ILogger<T> ArrangeConsoleLogger<T>()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(Configure);
        ILogger<T> logger = loggerFactory.CreateLogger<T>();
        return logger;

        void Configure(ILoggingBuilder builder) =>
            builder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole();
    }
}