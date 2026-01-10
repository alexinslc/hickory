using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Hickory.Api.Infrastructure.Data;

/// <summary>
/// EF Core interceptor to track query performance metrics
/// </summary>
public class QueryPerformanceInterceptor : DbCommandInterceptor
{
    private static readonly Meter Meter = new("Hickory.Api.Database");
    private static readonly Histogram<double> QueryDuration = Meter.CreateHistogram<double>(
        "db.query.duration",
        "ms",
        "Duration of database queries in milliseconds");
    
    private static readonly Counter<long> QueryCount = Meter.CreateCounter<long>(
        "db.query.count",
        "queries",
        "Total number of database queries executed");
    
    private static readonly Counter<long> SlowQueryCount = Meter.CreateCounter<long>(
        "db.query.slow_count",
        "queries",
        "Number of slow queries (>100ms)");

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

        QueryDuration.Record(duration, tags);
        QueryCount.Add(1, tags);

        // Track slow queries (>100ms)
        if (duration > 100)
        {
            SlowQueryCount.Add(1, tags);
            
            // Log slow queries for investigation using Serilog
            Serilog.Log.Warning(
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
