using Oracle.ManagedDataAccess.Client;
using System.Runtime.CompilerServices;

namespace Branch.API.Extensions
{
    public static class HostExtension
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, int? retry = 0)
        {
            int retryForAvailability = retry.Value;

            using (var scope = host.Services.CreateScope())
            {
                var service = scope.ServiceProvider;
                var config = service.GetRequiredService<IConfiguration>();
                var logger = service.GetRequiredService<ILogger<TContext>>();

                try
                {
                    logger.LogInformation("Migrated Oracle database!");
                    OracleConnection connection = new OracleConnection(config.GetConnectionString("OracleSettings:ConnectionString"));

                    connection.Open();
                    OracleCommand command = new OracleCommand();
                    command.Connection = connection;
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "DROP TABLE IF EXISTS BRANCH";
                    command.ExecuteNonQuery();

                    command.CommandText = @"Create Table Branch(
                        Id INT NOT NULL,
                        Code Number(5) NOT NULL,
                        Region VARCHAR2(100 BYTE) NOT NULL,
                        Name VARCHAR2(100 BYTE) NOT NULL,
                        Specialisation VARCHAR2(100 BYTE) NOT NULL,
                        Email VARCHAR2(100 BYTE) NOT NULL,
                        Address VARCHAR2(255 BYTE) NULL,
                        Manager VARCHAR2(100 BYTE)  NULL,
                        Manager_net VARCHAR2(100 BYTE) NULL,
                        Phone VARCHAR2(100 BYTE) NULL,
                        Fax VARCHAR2(100 BYTE) NULL,
                        Status VARCHAR2(100 BYTE) NOT NULL,
                        CONSTRAINT PK_ID PRIMARY KEY (Id),
                        CONSTRAINT CH_STATUS Check (Status in ('Opened','Closed')),
                        CONSTRAINT UQ_NAME UNIQUE(Name),
                        CONSTRAINT UQ_Phone UNIQUE(Phone),
                    )";
                    command.ExecuteNonQuery();

                    logger.LogInformation("Migrated Oracle database!");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);

                    if (retryForAvailability < 5)
                    {
                        retryForAvailability++;
                        System.Threading.Thread.Sleep(2000);
                        MigrateDatabase<TContext>(host, retryForAvailability);
                    }
                }
            }
            return host;
        }
    }
}
