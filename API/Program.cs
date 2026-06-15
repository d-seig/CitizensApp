using System.Data;
using API.MinimalAPI;
using Microsoft.Data.SqlClient;

namespace API
{
    public static partial class Program
    {
        static async Task DBInit(string masterConnectionString)
        {
            await using var masterConnection = new SqlConnection(masterConnectionString);
            await masterConnection.OpenAsync();
            var createDbCmd = masterConnection.CreateCommand();
            createDbCmd.CommandText = @"
                    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CitizenDB')
                    BEGIN
                        CREATE DATABASE CitizenDB;
                    END";
            await createDbCmd.ExecuteNonQueryAsync();
        }
        static async Task TableInit(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            var createTableCmd = connection.CreateCommand();
            createTableCmd.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Citizen' AND xtype='U')
                    CREATE TABLE Citizen (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Famil NVARCHAR(20) NOT NULL,
                        Imja NVARCHAR(20) NOT NULL,
                        Otch NVARCHAR(20) NOT NULL,
                        BirthDate DATETIME NOT NULL
                    );";
            await createTableCmd.ExecuteNonQueryAsync();
        }
        private static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddOpenApi();
            builder.Services.AddTransient<IDbConnection>(c => new SqlConnection(builder.Configuration.GetConnectionString("Connection")));
            var app = builder.Build();
            await DBInit(builder.Configuration.GetConnectionString("ConnectionMaster"));
            await TableInit(builder.Configuration.GetConnectionString("Connection"));
            if (app.Environment.IsDevelopment())
                app.MapOpenApi();
            app.UseHttpsRedirection();
            app.MapCitizenEndpoints();
            await app.RunAsync();
        }
    }
}
