using System;
using System.Collections.Generic;
using System.Linq;
using Metroidvania.Types;
using Metroidvania.Models;
using Metroidvania.Repositories;

namespace Metroidvania.Services
{

    public class ItemService
    {
        private readonly InMemoryRepository<ItemEntity> _repo;

        public ItemService()
        {
            _repo = new InMemoryRepository<ItemEntity>();
        }


        public IApiResponse<ItemEntity> AddItem(string id, string name,
            ItemType type, ItemRarity rarity = ItemRarity.COMMON, float value = 10f)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ApiResponse<ItemEntity>.Fail("Item ID cannot be empty.");
            if (string.IsNullOrWhiteSpace(name))
                return ApiResponse<ItemEntity>.Fail("Item name cannot be empty.");
            if (value < 0)
                return ApiResponse<ItemEntity>.Fail("Item value cannot be negative.");
            if (_repo.Exists(id))
                return ApiResponse<ItemEntity>.Fail($"Item '{id}' already exists.");

            var item = new ItemEntity(id, name, type, rarity, value);
            _repo.Create(item);
            return ApiResponse<ItemEntity>.Ok(item, $"Item '{name}' added.");
        }


        public IApiResponse<ItemEntity> UpdateItem(string id,
            Action<ItemEntity> updateAction)
        {
            if (updateAction == null)                             
                return ApiResponse<ItemEntity>.Fail("Update action cannot be null."); 
            if (!_repo.Exists(id))
                return ApiResponse<ItemEntity>.Fail("Item not found.");

            var updated = _repo.Update(id, updateAction);
            return ApiResponse<ItemEntity>.Ok(updated, "Item updated.");
        }

        public IApiResponse<bool> RemoveItem(string id)
        {
            if (!_repo.Exists(id))
                return ApiResponse<bool>.Fail("Item not found.");

            _repo.Delete(id);
            return ApiResponse<bool>.Ok(true, "Item removed.");
        }

        public ItemEntity FindById(string id)
        {
            return _repo.GetById(id);
        }

        public List<ItemEntity> GetAllItems()
        {
            return _repo.GetAll();
        }

        public List<ItemEntity> GetByRarity(ItemRarity rarity)
        {
            return _repo.Find(i => i.Rarity == rarity);
        }

        public List<ItemEntity> GetByType(ItemType type)
        {
            return _repo.Find(i => i.Type == type);
        }


        public List<ItemEntity> SearchItems(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAllItems();

            return _repo.Find(i =>
                i.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
            );
        }
    }
}