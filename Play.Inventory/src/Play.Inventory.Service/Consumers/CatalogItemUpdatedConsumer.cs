using MassTransit;
using Play.Catalog.Contracts;
using Play.Common;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
    {
        private readonly IRepository<CatalogItem> _catalogItemRepo;

        public CatalogItemUpdatedConsumer(
            IRepository<CatalogItem> catalogItemRepo
            )
        {
            _catalogItemRepo = catalogItemRepo;
        }

        public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
        {
            CatalogItemUpdated message = context.Message;

            CatalogItem item = await _catalogItemRepo.GetAsync(message.ItemId);

            if (item is null)
            {
                item = new()
                {
                    Id = message.ItemId,
                    Name = message.Name,
                    Description = message.Description
                };

                await _catalogItemRepo.CreateAsync(item);
            }

            item.Name = message.Name;
            item.Description = message.Description;

            await _catalogItemRepo.UpdateAsync(item);
        }
    }
}
