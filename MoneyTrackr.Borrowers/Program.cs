using Microsoft.EntityFrameworkCore;
using MoneyTrackr.Borrowers.Repository;
using MoneyTrackr.Borrowers.Services;


namespace MoneyTrackr.Borrowers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<MoneyTrackrDbContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("MoneyTrackrDatabase"),
                    new MySqlServerVersion(new Version(8, 0, 36))  // Adjust version if needed
                )
            );

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<ILoanService, LoanService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
