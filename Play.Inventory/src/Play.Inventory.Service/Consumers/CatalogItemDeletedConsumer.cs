using MassTransit;
using Play.Catalog.Contracts;
using Play.Common;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
    {
        private readonly IRepository<CatalogItem> _catalogItemRepo;

        public CatalogItemDeletedConsumer(
            IRepository<CatalogItem> catalogItemRepo
            )
        {
            _catalogItemRepo = catalogItemRepo;
        }

        public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
        {
            CatalogItemDeleted message = context.Message;

            CatalogItem item = await _catalogItemRepo.GetAsync(message.ItemId);

            if(item is null)
            {
                return;
            }

            await _catalogItemRepo.RemoveAsync(message.ItemId);
        }
    }
}
