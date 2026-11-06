using System;
using Metroidvania.Types;

namespace Metroidvania.Models
{

    public interface IItem : IBaseEntity
    {
        string Name { get; set; }
        ItemType Type { get; set; }
        ItemRarity Rarity { get; set; }
        float Value { get; set; }
        int Quantity { get; set; }
        bool IsStackable { get; }
        string GetInfo();
    }

    [Serializable]
    public class ItemEntity : IItem
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public ItemRarity Rarity { get; set; }
        public float Value { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; set; }

        public bool IsStackable =>
            Type == ItemType.CONSUMABLE;


        public ItemEntity(string id, string name, ItemType type,
                         ItemRarity rarity = ItemRarity.COMMON,
                         float value = 10f)
        {
            Id = id;
            Name = name;
            Type = type;
            Rarity = rarity;
            Value = value;
            Quantity = 1;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

    
        public string GetInfo()
        {
            return $"[{Id}] {Name} ({Rarity} {Type}) | " +
                   $"Value:{Value} | Qty:{Quantity} | Stackable:{IsStackable}";
        }
    }
}