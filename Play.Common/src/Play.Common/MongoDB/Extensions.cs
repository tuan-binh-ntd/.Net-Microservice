using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Play.Common.Settings;

namespace Play.Common.MongoDB
{
    public static class Extensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));


            services.AddSingleton(serviceProvider =>
            {
                // Get configuration
                IConfiguration configuration = serviceProvider.GetService<IConfiguration>()!;
                // Get ServiceSettings Section and binding into instance of ServiceSettings
                ServiceSettings serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>()!;
                // Get MongoDbSettings Section and binding into instance of MongoDbSettings
                MongoDbSettings mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>()!;
                // Connect MongoDb
                MongoClient mongoClient = new(mongoDbSettings!.ConnectionString);
                // Return Database
                return mongoClient.GetDatabase(serviceSettings.ServiceName);
            });

            return services;
        }

        public static IServiceCollection AddMonogoRepository<T>(this IServiceCollection services, string collectioName) where T : IEntity
        {
            services.AddSingleton<IRepository<T>>(serviceProvider =>
            {
                var database = serviceProvider.GetService<IMongoDatabase>();
                return new MongoRepository<T>(database!, collectioName);
            });

            return services;
        }
    }
}
