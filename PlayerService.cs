using System;
using System.Collections.Generic;
using System.Linq;
using Metroidvania.Types;
using Metroidvania.Models;
using Metroidvania.Repositories;
using Metroidvania.Constants;

namespace Metroidvania.Services
{
    public class PlayerService
    {
        private readonly InMemoryRepository<PlayerEntity> _repo;

        public PlayerService()
        {
            _repo = new InMemoryRepository<PlayerEntity>();
        }

   

        public IApiResponse<PlayerEntity> AddPlayer(string id, string name,
            float maxHp = 100f, float atk = 10f)  
        {
       
            if (string.IsNullOrWhiteSpace(id))
                return ApiResponse<PlayerEntity>.Fail("Player ID cannot be empty.");

            if (string.IsNullOrWhiteSpace(name))
                return ApiResponse<PlayerEntity>.Fail("Player name cannot be empty.");

            if (name.Length < 2 || name.Length > 20)
                return ApiResponse<PlayerEntity>.Fail("Name must be 2-20 characters.");

            if (maxHp <= 0)
                return ApiResponse<PlayerEntity>.Fail("Max HP must be positive.");

            if (atk <= 0)
                return ApiResponse<PlayerEntity>.Fail("Attack power must be positive.");

            if (_repo.Exists(id))
                return ApiResponse<PlayerEntity>.Fail($"Player '{id}' already exists.");

           
            var duplicate = _repo.Find(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (duplicate.Count > 0)
                return ApiResponse<PlayerEntity>.Fail($"Name '{name}' is already taken.");

            var player = new PlayerEntity(id, name, maxHp, atk);
            _repo.Create(player);
            return ApiResponse<PlayerEntity>.Ok(player, $"Player '{name}' created.");
        }

        public IApiResponse<PlayerEntity> UpdatePlayer(string id,
            Action<PlayerEntity> updateAction)
        {
           
            if (updateAction == null)
                return ApiResponse<PlayerEntity>.Fail("Update action cannot be null.");

            if (!_repo.Exists(id))
                return ApiResponse<PlayerEntity>.Fail("Player not found.");

            var updated = _repo.Update(id, updateAction);
            return ApiResponse<PlayerEntity>.Ok(updated, "Player updated.");
        }

        public IApiResponse<bool> DeletePlayer(string id)
        {
            if (!_repo.Exists(id))
                return ApiResponse<bool>.Fail("Player not found.");

            _repo.Delete(id);
            return ApiResponse<bool>.Ok(true, "Player deleted.");
        }

        public PlayerEntity FindById(string id)
        {
            return _repo.GetById(id);
        }

        public PlayerEntity FindByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return _repo.Find(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            ).FirstOrDefault();
        }

        public List<PlayerEntity> GetAllPlayers()
        {
            return _repo.GetAll();
        }

        public List<PlayerEntity> SearchPlayers(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAllPlayers();

            return _repo.Find(p =>
                p.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
            );
        }

   

        public bool DecreaseHealth(string id, float amount)
        {
            if (amount <= 0) return false;

            var player = _repo.GetById(id);
            if (player == null || !player.IsAlive)
                return false;

            _repo.Update(id, p =>
            {
                p.CurrentHealth = Math.Max(0, p.CurrentHealth - amount);  
                if (!p.IsAlive)
                    p.DeathCount++;
            });

            return true;
        }

        public bool IncreaseHealth(string id, float amount)
        {
            if (amount <= 0) return false;

            var player = _repo.GetById(id);
            if (player == null || !player.IsAlive)
                return false;

            _repo.Update(id, p =>
            {
                p.CurrentHealth = Math.Min(p.MaxHealth, p.CurrentHealth + amount);  
            });

            return true;
        }

        public bool AddToInventory(string id, string itemId)
        {
        
            if (string.IsNullOrWhiteSpace(itemId)) return false;

            var player = _repo.GetById(id);
            if (player == null) return false;

            if (player.Inventory.Count >= AppConstants.MAX_INVENTORY_SLOTS)
                return false;

            _repo.Update(id, p => p.Inventory.Add(itemId));
            return true;
        }

        public bool UnlockAbility(string id, string abilityName)
        {
            
            if (string.IsNullOrWhiteSpace(abilityName)) return false;

            var player = _repo.GetById(id);
            if (player == null) return false;

            if (player.Abilities.Contains(abilityName))
                return false;

            _repo.Update(id, p => p.Abilities.Add(abilityName));
            return true;
        }

        public bool AddScore(string id, int points)
        {
            if (points <= 0) return false;  

            var player = _repo.GetById(id);
            if (player == null) return false;

            _repo.Update(id, p => p.Score += points);
            return true;
        }
    }
}