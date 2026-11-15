using System.Data;
using Dapper;

namespace Infrastructure.Persistence.Repositories;

public sealed record FeatureSummaryReportDto
{
    public Guid ItemId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public long TotalTransactions { get; init; }
    public decimal TotalAmount { get; init; }
}

public interface IFeatureReportRepository
{
    Task<IEnumerable<FeatureSummaryReportDto>> GetSummaryReportAsync(
        Guid companyId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}

public sealed class FeatureReportRepository : IFeatureReportRepository
{
    private readonly IDbConnection _dbConnection;

    public FeatureReportRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IEnumerable<FeatureSummaryReportDto>> GetSummaryReportAsync(
        Guid companyId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // Regla: Cero SQL complejo inline en C#. Solo invocación directa de la función PostgreSQL.
        const string sql = "SELECT * FROM get_feature_summary_report(@CompanyId, @StartDate, @EndDate);";

        var parameters = new
        {
            CompanyId = companyId,
            StartDate = startDate,
            EndDate = endDate
        };

        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        return await _dbConnection.QueryAsync<FeatureSummaryReportDto>(command);
    }
}
