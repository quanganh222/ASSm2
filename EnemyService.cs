using System;
using System.Collections.Generic;
using System.Linq;
using Metroidvania.Types;
using Metroidvania.Models;
using Metroidvania.Repositories;
using Metroidvania.Constants;

namespace Metroidvania.Services
{
    public class EnemyService
    {
        private readonly InMemoryRepository<EnemyEntity> _repo;

        public EnemyService()
        {
            _repo = new InMemoryRepository<EnemyEntity>();
        }


        public IApiResponse<EnemyEntity> RegisterEnemy(string id, string name,
            float maxHp, float damage, EnemyType type = EnemyType.NORMAL)  
        {
          
            if (string.IsNullOrWhiteSpace(id))
                return ApiResponse<EnemyEntity>.Fail("Enemy ID cannot be empty.");

            if (string.IsNullOrWhiteSpace(name))
                return ApiResponse<EnemyEntity>.Fail("Enemy name cannot be empty.");

            if (maxHp <= 0)
                return ApiResponse<EnemyEntity>.Fail("Max HP must be positive.");

            if (damage <= 0)
                return ApiResponse<EnemyEntity>.Fail("Damage must be positive.");

            if (_repo.Exists(id))
                return ApiResponse<EnemyEntity>.Fail($"Enemy '{id}' already exists.");

            var enemy = new EnemyEntity(id, name, maxHp, damage, type);
            _repo.Create(enemy);
            return ApiResponse<EnemyEntity>.Ok(enemy, $"Enemy '{name}' registered.");
        }

        public IApiResponse<EnemyEntity> UpdateEnemy(string id,
            Action<EnemyEntity> updateAction)
        {
            if (updateAction == null)
                return ApiResponse<EnemyEntity>.Fail("Update action cannot be null.");

            if (!_repo.Exists(id))
                return ApiResponse<EnemyEntity>.Fail("Enemy not found.");

            var updated = _repo.Update(id, updateAction);
            return ApiResponse<EnemyEntity>.Ok(updated, "Enemy updated.");
        }

        public IApiResponse<bool> DeactivateEnemy(string id)
        {
            var enemy = _repo.GetById(id);
            if (enemy == null)
                return ApiResponse<bool>.Fail("Enemy not found.");

            if (!enemy.IsAlive)
                return ApiResponse<bool>.Fail("Enemy is already dead.");

            _repo.Update(id, e =>
            {
                e.CurrentHealth = 0;
            });

            return ApiResponse<bool>.Ok(true, "Enemy deactivated.");
        }

        public EnemyEntity FindById(string id)
        {
            return _repo.GetById(id);
        }

        public List<EnemyEntity> GetAllEnemies()
        {
            return _repo.GetAll();
        }

        public List<EnemyEntity> GetActiveEnemies()
        {
            return _repo.Find(e => e.IsAlive);
        }

        public List<EnemyEntity> GetBosses()
        {
            return _repo.Find(e => e.Type == EnemyType.BOSS);  
        }

        public List<EnemyEntity> SearchEnemies(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAllEnemies();

            return _repo.Find(e =>
                e.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
            );
        }

     

        public bool DecreaseHealth(string id, float amount)
        {
          
            if (amount <= 0) return false;

            var enemy = _repo.GetById(id);
            if (enemy == null || !enemy.IsAlive)
                return false;

         
            _repo.Update(id, e =>
            {
                e.CurrentHealth = Math.Max(0, e.CurrentHealth - amount);
 
            });

            return true;
        }

        public bool TriggerPhase(string id, int newPhase)
        {
            var enemy = _repo.GetById(id);
            if (enemy == null)
                return false;

            if (enemy.Type != EnemyType.BOSS) 
                return false;

            if (newPhase <= enemy.Phase) 
                return false;

            _repo.Update(id, e =>
            {
                e.Phase = newPhase;  
            });

            return true;
        }
    }
}