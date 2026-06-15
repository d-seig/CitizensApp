using Microsoft.Data.SqlClient;
using System.Data;
using API.MinimalAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddTransient<IDbConnection>(c => new SqlConnection(builder.Configuration.GetConnectionString("Connection")));

var app = builder.Build();

await DBInit(builder.Configuration.GetConnectionString("ConnectionMaster"));
await TableInit(builder.Configuration.GetConnectionString("Connection"));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapCitizenEndpoints();

app.Run();

static async Task DBInit(string masterConnectionString)
{

    await using (var masterConnection = new SqlConnection(masterConnectionString))
    {
        masterConnection.Open();

        var createDbCmd = masterConnection.CreateCommand();

        createDbCmd.CommandText = @"
                    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CitizenDB')
                    BEGIN
                        CREATE DATABASE CitizenDB;
                    END";

        createDbCmd.ExecuteNonQuery();
    }
}
static async Task TableInit(string connectionString)
{
    using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();

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

        createTableCmd.ExecuteNonQuery();
    }
}
