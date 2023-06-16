using MassTransit;
using Play.Catalog.Contracts;
using Play.Common;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
    {
        private readonly IRepository<CatalogItem> _catalogItemRepo;

        public CatalogItemCreatedConsumer(
            IRepository<CatalogItem> catalogItemRepo
            )
        {
            _catalogItemRepo = catalogItemRepo;
        }

        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            CatalogItemCreated message = context.Message;

            CatalogItem item = await _catalogItemRepo.GetAsync(message.ItemId);

            if (item is not null) 
            {
                return;
            }

            item = new()
            {
                Id = message.ItemId,
                Name = message.Name,
                Description = message.Description
            };

            await _catalogItemRepo.CreateAsync(item);
        }
    }
}
