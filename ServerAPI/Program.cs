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

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Remote Agent Server API",
                    Version = "v1",
                    Description = "API for managing remote agent tasks and job execution",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Remote Agent Team"
                    }
                });
                
                // Enable XML comments for better documentation
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            // Add CORS support for development
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

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
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Remote Agent Server API v1");
                    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
                });
            }

            app.UseCors();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
