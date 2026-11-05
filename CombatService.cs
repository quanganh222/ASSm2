using System;
using System.Collections.Generic;
using System.Linq;
using Metroidvania.Types;
using Metroidvania.Models;
using Metroidvania.Repositories;
using Metroidvania.Constants;

namespace Metroidvania.Services
{
    public class CombatService
    {
        private readonly PlayerService _playerService;
        private readonly EnemyService _enemyService;
        private readonly List<CombatRecord> _combatHistory; 
        private readonly Random _random;

        public CombatService(PlayerService playerService, EnemyService enemyService)
        {
            _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
            _enemyService = enemyService ?? throw new ArgumentNullException(nameof(enemyService));
            _combatHistory = new List<CombatRecord>();  
            _random = new Random();
        }


        public IApiResponse<CombatRecord> Attack(string playerId, string enemyId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
                return ApiResponse<CombatRecord>.Fail("Player ID cannot be empty.");
            if (string.IsNullOrWhiteSpace(enemyId))
                return ApiResponse<CombatRecord>.Fail("Enemy ID cannot be empty.");

            var player = _playerService.FindById(playerId);
            var enemy = _enemyService.FindById(enemyId);

            if (player == null)
                return ApiResponse<CombatRecord>.Fail("Player not found.");
            if (enemy == null)
                return ApiResponse<CombatRecord>.Fail("Enemy not found.");
            if (!player.IsAlive)
                return ApiResponse<CombatRecord>.Fail("Player is dead, cannot attack.");
            if (!enemy.IsAlive)
                return ApiResponse<CombatRecord>.Fail("Enemy is already defeated.");

          
            float baseDamage = AppConstants.BASE_DAMAGE * (player.Strength / 10f);

           
            float defense = enemy.Damage * 0.1f;
            float finalDamage = Math.Max(1f, baseDamage - defense);

            
            bool isCrit = _random.NextDouble() < AppConstants.CRIT_CHANCE;
            if (isCrit)
            {
                finalDamage *= AppConstants.CRIT_MULTIPLIER;
            }

            finalDamage = (float)Math.Round(finalDamage, 1);

            
            _enemyService.DecreaseHealth(enemyId, finalDamage);

           
            var updatedEnemy = _enemyService.FindById(enemyId);
            if (updatedEnemy.Type == EnemyType.BOSS && updatedEnemy.IsAlive)  
            {
                float hpPercent = updatedEnemy.CurrentHealth / updatedEnemy.MaxHealth;  
                if (hpPercent <= AppConstants.BOSS_PHASE2_HP_PERCENT
                    && updatedEnemy.Phase == 1)  
                {
                    _enemyService.TriggerPhase(enemyId, 2);
                }
            }

            var record = new CombatRecord(
                Guid.NewGuid().ToString(),
                playerId,
                enemyId,
                finalDamage,
                isCrit);
            _combatHistory.Add(record);  

            string msg = isCrit
                ? $"CRITICAL HIT! {player.Name} dealt {finalDamage:F1} damage to {enemy.Name}!"
                : $"{player.Name} dealt {finalDamage:F1} damage to {enemy.Name}.";

            if (!updatedEnemy.IsAlive)
                msg += $" {enemy.Name} has been defeated!";

            return ApiResponse<CombatRecord>.Ok(record, msg);
        }

        public IApiResponse<CombatRecord> EnemyAttack(string enemyId, string playerId)
        {
            if (string.IsNullOrWhiteSpace(enemyId) || string.IsNullOrWhiteSpace(playerId))
                return ApiResponse<CombatRecord>.Fail("Invalid IDs.");

            var player = _playerService.FindById(playerId);
            var enemy = _enemyService.FindById(enemyId);

            if (player == null || enemy == null)
                return ApiResponse<CombatRecord>.Fail("Invalid combatants.");
            if (!player.IsAlive)
                return ApiResponse<CombatRecord>.Fail("Player is already dead.");
            if (!enemy.IsAlive)
                return ApiResponse<CombatRecord>.Fail("Enemy is already dead.");

            float damage = Math.Max(1f, enemy.Damage - player.Defense);
            damage = (float)Math.Round(damage, 1);

            _playerService.DecreaseHealth(playerId, damage);

            var record = new CombatRecord(
                Guid.NewGuid().ToString(),
                playerId,
                enemyId,
                damage,
                false);
            _combatHistory.Add(record);

            var updatedPlayer = _playerService.FindById(playerId);
            string msg = $"{enemy.Name} dealt {damage:F1} damage to {player.Name}.";
            if (!updatedPlayer.IsAlive)
                msg += $" {player.Name} has died!";

            return ApiResponse<CombatRecord>.Ok(record, msg);
        }

        public List<CombatRecord> GetRecordsByPlayer(string playerId)
        {
            return _combatHistory.FindAll(r => r.PlayerId == playerId);
        }

        public List<CombatRecord> GetRecordsByEnemy(string enemyId)
        {
            return _combatHistory.FindAll(r => r.EnemyId == enemyId);
        }

        public List<CombatRecord> GetCriticalRecords()
        {
            return _combatHistory.FindAll(r => r.IsCritical);
        }

        public List<CombatRecord> GetAllRecords()
        {
            return _combatHistory;
        }

        public float GetTotalDamageDealt(string playerId)
        {
            return _combatHistory.FindAll(r => r.PlayerId == playerId)
                                 .Sum(r => r.Damage);  
        }
    }
}