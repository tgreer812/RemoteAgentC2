using RemoteAgentServerAPI.Data;

namespace RemoteAgentServerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            string? dbFilePath = builder.Configuration.GetValue<string>("SqliteDbFilePath");
            if (string.IsNullOrEmpty(dbFilePath))
            {
                // 2. If not configured in appsettings, default to "agent_tasking.db" in the content root
                dbFilePath = "agent_tasking.db";
            }

            // 3. Register IDatabase service, using SqlLiteDatabase as implementation, with Singleton lifetime
            //    Pass the dbFilePath to the SqlLiteDatabase constructor
            builder.Services.AddSingleton<IDatabase>(provider => new SqlLiteDatabase(dbFilePath));

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
