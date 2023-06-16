using Play.Catalog.Service.Entities;
using Play.Common.MongoDB;
using Play.Common.MassTransit;

namespace Play.Catalog.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddMongo()
                .AddMonogoRepository<Item>("items")
                .AddMassTransitWithRabbitMq();

            //builder.Services.AddMassTransitHostedService();

            builder.Services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //Add MVC Lowercase URL
            builder.Services.AddRouting(options => options.LowercaseUrls = true);
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