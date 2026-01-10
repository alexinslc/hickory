using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Hickory.Api.Infrastructure.Data;

/// <summary>
/// EF Core interceptor to track query performance metrics
/// </summary>
public class QueryPerformanceInterceptor : DbCommandInterceptor
{
    private readonly ILogger<QueryPerformanceInterceptor> _logger;
    private readonly Histogram<double> _queryDuration;
    private readonly Counter<long> _queryCount;
    private readonly Counter<long> _slowQueryCount;

    public QueryPerformanceInterceptor(ILogger<QueryPerformanceInterceptor> logger, IMeterFactory meterFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var meter = meterFactory?.Create("Hickory.Api.Database") ?? throw new ArgumentNullException(nameof(meterFactory));
        
        _queryDuration = meter.CreateHistogram<double>(
            "db.query.duration",
            "ms",
            "Duration of database queries in milliseconds");
        
        _queryCount = meter.CreateCounter<long>(
            "db.query.count",
            "queries",
            "Total number of database queries executed");
        
        _slowQueryCount = meter.CreateCounter<long>(
            "db.query.slow_count",
            "queries",
            "Number of slow queries (>100ms)");
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        RecordMetrics(command, eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        RecordMetrics(command, eventData);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        RecordMetrics(command, eventData);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        RecordMetrics(command, eventData);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        RecordMetrics(command, eventData);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        RecordMetrics(command, eventData);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void RecordMetrics(DbCommand command, CommandExecutedEventData eventData)
    {
        var duration = eventData.Duration.TotalMilliseconds;
        var commandType = GetCommandType(command.CommandText);
        
        var tags = new TagList
        {
            { "command_type", commandType }
        };

        _queryDuration.Record(duration, tags);
        _queryCount.Add(1, tags);

        // Track slow queries (>100ms)
        if (duration > 100)
        {
            _slowQueryCount.Add(1, tags);
            
            // Log slow queries for investigation
            _logger.LogWarning(
                "Slow query detected ({Duration}ms): {CommandText}",
                duration,
                command.CommandText.Length > 500 
                    ? command.CommandText[..500] + "..." 
                    : command.CommandText);
        }
    }

    private static string GetCommandType(string commandText)
    {
        var firstWord = commandText.TrimStart().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToUpper();
        return firstWord switch
        {
            "SELECT" => "select",
            "INSERT" => "insert",
            "UPDATE" => "update",
            "DELETE" => "delete",
            _ => "other"
        };
    }
}
