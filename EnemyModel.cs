using System;
using Metroidvania.Types;

namespace Metroidvania.Models
{

    public interface IEnemy : IBaseEntity
    {
        string Name { get; set; }
        float CurrentHealth { get; set; }
        float MaxHealth { get; set; }
        float Damage { get; set; }
        float Speed { get; set; }
        EnemyType Type { get; set; }
        int KillCount { get; set; }
        int Phase { get; set; }
        bool IsAlive { get; }
        string GetInfo();
    }

    [Serializable]
    public class EnemyEntity : IEnemy
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }
        public float Damage { get; set; }
        public float Speed { get; set; }
        public EnemyType Type { get; set; }
        public int KillCount { get; set; }
        public int Phase { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; set; }

        public bool IsAlive => CurrentHealth > 0;

        public EnemyEntity(string id, string name, float maxHealth,
                          float damage, EnemyType type = EnemyType.NORMAL)
        {
            Id = id;
            Name = name;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            Damage = damage;
            Speed = 3f;
            Type = type;
            KillCount = 0;
            Phase = 1;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        public string GetInfo()
        {
            return $"[{Id}] {Name} ({Type}) | HP:{CurrentHealth}/{MaxHealth} | " +
                   $"DMG:{Damage} | Phase:{Phase} | Alive:{IsAlive}";
        }
    }
}
