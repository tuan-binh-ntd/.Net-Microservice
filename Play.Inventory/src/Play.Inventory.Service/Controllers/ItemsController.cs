using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> _itemsRepository;
        private readonly IRepository<CatalogItem> _catalogItemRepositor;

        public ItemsController(
            IRepository<InventoryItem> itemsRepository,
            IRepository<CatalogItem> catalogItemRepositor
            )
        {
            _itemsRepository = itemsRepository;
            _catalogItemRepositor = catalogItemRepositor;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> Get(Guid userId)
        {
            if(userId == Guid.Empty)
            {
                return BadRequest();
            }

            var inventoryItemEntities = await _itemsRepository.GetAllAsync(item => item.UserId == userId);
            IEnumerable<Guid> itemIds = inventoryItemEntities.Select(i => i.CatalogItemId);
            var catalogItemEntities = await _catalogItemRepositor.GetAllAsync(item => itemIds.Contains(item.Id));

            var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });
            return Ok(inventoryItemDtos);
        }

        [HttpPost]
        public async Task<ActionResult> Create(GrantItemsDto input)
        {
            InventoryItem inventoryItem = await _itemsRepository.GetAsync(item => item.UserId == input.UserId && item.CatalogItemId == input.CatalogItemId);

            if(inventoryItem is null)
            {
                inventoryItem = new InventoryItem()
                {
                    CatalogItemId = input.CatalogItemId,
                    UserId = input.UserId,
                    Quantity = input.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };

                await _itemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += input.Quantity;
                await _itemsRepository.UpdateAsync(inventoryItem);
            }

            return Ok();
        }
    }
}
