using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> _itemRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public ItemsController(
            IRepository<Item> itemRepository,
            IPublishEndpoint publishEndpoint
            )
        {
            _itemRepository = itemRepository;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> Get()
        {
            IEnumerable<ItemDto> items = (await _itemRepository.GetAllAsync()).Select(e => e.AsDto());

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetById(Guid id)
        {
            var item = await _itemRepository.GetAsync(id);
            if(item is null)
            {
                return NotFound();
            }
            return Ok(item.AsDto());
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> Create(CreateItemDto input)
        {
            Item item = new()
            {
                Name = input.Name,
                Description = input.Description,
                Price = input.Price,
                CreatedDate = DateTimeOffset.UtcNow,
            };

            await _itemRepository.CreateAsync(item);

            await _publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateItemDto input)
        {
            Item existingItem = await _itemRepository.GetAsync(id);

            if(existingItem is null)
            {
                return NotFound();
            }

            existingItem.Name = input.Name;
            existingItem.Description = input.Description;
            existingItem.Price = input.Price;

            await _itemRepository.UpdateAsync(existingItem);

            await _publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));


            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            Item existingItem = await _itemRepository.GetAsync(id);

            if (existingItem is null)
            {
                return NotFound();
            }

            await _itemRepository.RemoveAsync(id);

            await _publishEndpoint.Publish(new CatalogItemDeleted(existingItem.Id));

            return NoContent();
        }
    }
}
