using System;
using System.Collections.Generic;
using Metroidvania.Types;
namespace Metroidvania.Models
{
    public interface IPlayer : IBaseEntity
    {
        string Name { get; set; }
        float CurrentHealth { get; set; }
        float MaxHealth { get; set; }
        //float Overload { get; set; }
        //float MaxOverload { get; set; }
        float Strength { get; set; }
        float Defense { get; set; }
        float Speed { get; set; }
        int Level { get; set; }
        int Score { get; set; }
        int KillCount { get; set; }
        int DeathCount { get; set; }
        List<string> Abilities { get; set; }
        List<string> Inventory { get; set; }
        bool IsAlive { get; }
        string GetInfo();
//        int Experience { get; set; }
//        int Gold { get; set; }
    }
    [Serializable]
    public class PlayerEntity : IPlayer
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }
        //public float Overload { get; set; }
        //public float MaxOverload { get; set; }
        public float Strength { get; set; }
        public float Defense { get; set; }
        public float Speed { get; set; }
        public int Level { get; set; }
        public int Score { get; set; }
        public int KillCount { get; set; }
        public int DeathCount { get; set; }
        public List<string> Abilities { get; set; }
        public List<string> Inventory { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; set; }

        public bool IsAlive => CurrentHealth > 0;

        public PlayerEntity(string id, string name,
                           float maxHealth = 100f,
                           float strength = 10f)
        {
            Id = id;
            Name = name;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            //MaxMana = maxMana;
            //Mana = maxMana;
            Strength = strength;
            Defense = 5f;
            Speed = 5f;
            Level = 1;
            Score = 0;
            KillCount = 0;
            DeathCount = 0;
//            Experience = 0;
//            Gold = 0;
            Abilities = new List<string>();
            Inventory = new List<string>();
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }


        public string GetInfo()
        {
            return $"[{Id}] {Name} | Lv.{Level} | HP:{CurrentHealth}/{MaxHealth} | " +
                   $"ATK:{Strength} | DEF:{Defense} | Kills:{KillCount} | " +
                   $"Deaths:{DeathCount} | Alive:{IsAlive}";
        }
    }
}