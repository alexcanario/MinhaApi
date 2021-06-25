using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevIO.Api.Extensions {
    public class SqlServerHealthCheck : IHealthCheck {
        private readonly string _connection;
        public SqlServerHealthCheck(string connection) {
            _connection = connection;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken()) {
            try {
                using (var connection = new SqlConnection(_connection)) {
                    await connection.OpenAsync(cancellationToken);

                    var command = connection.CreateCommand();
                    command.CommandText = "select count(1) from Produtos";

                    return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) < 0
                        ? HealthCheckResult.Healthy()
                        : HealthCheckResult.Degraded();
                }
            } catch (Exception) {
                return HealthCheckResult.Degraded();
            }
        }
    }
}