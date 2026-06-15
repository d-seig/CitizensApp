using System.Data;
using DTO;
using Microsoft.Data.SqlClient;

namespace API.MinimalAPI
{
    public static class ApiEndpoints
    {
        private static async Task<IResult> ReadList(IDbConnection connection, string sortBy = "id", bool isDesc = false, int page = 1, int pageSize = 5)
        {
            if (page < 1)
                page = 1;
            if (pageSize < 1 || pageSize > 50)
                pageSize = 5;
            string sortColumn = sortBy.ToLower() switch
            {
                "famil" => "Famil",
                "imja" => "Imja",
                "otch" => "Otch",
                "birthdate" => "BirthDate",
                _ => "Id"
            };
            string direction = isDesc ? "DESC" : "ASC";

            var items = new List<CitizenDTO>();
            int totalCount = 0;

            using (connection)
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Citizen";
                totalCount = cmd.ExecuteScalar() as int? ?? 0;
                cmd = connection.CreateCommand();
                cmd.CommandText = $@"
                        SELECT Id, Famil, Imja, Otch, BirthDate
                        FROM Citizen 
                        ORDER BY {sortColumn} {direction} 
                        OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

                cmd.Parameters.Add(new SqlParameter("@Offset", (page - 1) * pageSize));
                cmd.Parameters.Add(new SqlParameter("@Limit", pageSize));
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(new CitizenDTO
                    {
                        Id = reader.GetInt32(0),
                        Famil = reader.GetString(1),
                        Imja = reader.GetString(2),
                        Otch = reader.GetString(3),
                        BirthDate = reader.GetDateTime(4)
                    });
                }
            }
            return Results.Ok(new { Items = items, TotalCount = totalCount });
        }
        private static async Task<IResult> Read(IDbConnection connection, int id)
        {
            CitizenDTO citizen;
            using (connection)
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"SELECT * FROM Citizen WHERE Id = @Id";
                cmd.Parameters.Add(new SqlParameter("@Id", id));
                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                    return Results.NotFound();
                citizen = new CitizenDTO
                {
                    Id = reader.GetInt32(0),
                    Famil = reader.GetString(1),
                    Imja = reader.GetString(2),
                    Otch = reader.GetString(3),
                    BirthDate = reader.GetDateTime(4)
                };
            }
            return Results.Ok(citizen);
        }
        private static async Task<IResult> Create(IDbConnection connection, CitizenDTO citizen)
        {
            if (string.IsNullOrWhiteSpace(citizen?.Famil))
                return Results.BadRequest("Данные гражданина неверны.");
            using (connection)
            {
                connection.Open();
                var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                        INSERT INTO Citizen (Famil, Imja, Otch, BirthDate) 
                        VALUES (@Famil, @Imja, @Otch, @BirthDate);
                        SELECT SCOPE_IDENTITY();";

                cmd.Parameters.Add(new SqlParameter("@Famil", citizen.Famil));
                cmd.Parameters.Add(new SqlParameter("@Imja", citizen.Imja));
                cmd.Parameters.Add(new SqlParameter("@Otch", citizen.Otch));
                cmd.Parameters.Add(new SqlParameter("@BirthDate", citizen.BirthDate));
                if (cmd.ExecuteScalar() is int newId)
                    citizen.Id = newId;
            }

            return Results.Created($"/{citizen.Id}", citizen);
        }
        private static async Task<IResult> Update(IDbConnection connection, CitizenDTO updated)
        {
            using (connection)
            {
                connection.Open();
                var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                        UPDATE Citizen 
                        SET Famil = @Famil, Imja = @Imja, Otch = @Otch, BirthDate = @BirthDate
                        WHERE Id = @Id";

                cmd.Parameters.Add(new SqlParameter("@Famil", updated.Famil));
                cmd.Parameters.Add(new SqlParameter("@Imja", updated.Imja));
                cmd.Parameters.Add(new SqlParameter("@Otch", updated.Otch));
                cmd.Parameters.Add(new SqlParameter("@BirthDate", updated.BirthDate));
                cmd.Parameters.Add(new SqlParameter("@Id", updated.Id));
                if (cmd.ExecuteNonQuery() is 0)
                    return Results.NotFound();
            }
            return Results.Ok();
        }
        private static async Task<IResult> Delete(IDbConnection connection, int id)
        {
            using (connection)
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM Citizen WHERE Id = @Id";
                cmd.Parameters.Add(new SqlParameter("@Id", id));
                if (cmd.ExecuteNonQuery() is 0)
                    return Results.NotFound();
            }
            return Results.NoContent();
        }
        public static void MapCitizenEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/citizens");
            group.MapGet("/", ReadList);
            group.MapGet("/{id}", Read);
            group.MapPost("/", Create);
            group.MapPut("/", Update);
            group.MapDelete("/{id}", Delete);
        }
    }
}
