using System;
using System.Collections.Generic;

namespace Metroidvania.Types
{
    public interface IBaseEntity
    {
        string Id { get; }
        DateTime CreatedAt { get; }
        DateTime UpdatedAt { get; set; }
    }
    public interface IApiResponse<T>
    {
        bool Success { get; }
        T Data { get; }
        string Message { get; }
        DateTime Timestamp { get; }
    }

    public class ApiResponse<T> : IApiResponse<T>
    {
        public bool Success { get; private set; }
        public T Data { get; private set; }
        public string Message { get; private set; }
        public DateTime Timestamp { get; private set; }

        private ApiResponse(bool success, T data, string message)
        {
            Success = success;
            Data = data;
            Message = message;
            Timestamp = DateTime.Now;
        }

        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T>(true, data, message);
        }

 
        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T>(false, default, message);
        }
    }

    public enum UserRole
    {
        ADMIN,
        MANAGER,
        PLAYER
    }

    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARN,
        ERROR
    }


    public enum EnemyType
    {
        NORMAL,
        ELITE,
        BOSS
    }

  
    public enum ItemType
    {
        WEAPON,
        ARMOR,
        CONSUMABLE,
        KEY
    }

    public enum ItemRarity
    {
        COMMON,
        RARE,
        EPIC,
        LEGENDARY
    }


    [Serializable]
    public class CombatRecord : IBaseEntity
    {
        public string Id { get; private set; }
        public string PlayerId { get; set; }
        public string EnemyId { get; set; }
        public float Damage { get; set; }
        public bool IsCritical { get; set; }
        public bool IsFinished { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; set; }

        public CombatRecord(string id, string playerId, string enemyId,
                           float damage, bool isCrit)
        {
            Id = id;
            PlayerId = playerId;
            EnemyId = enemyId;
            Damage = damage;
            IsCritical = isCrit;
            IsFinished = false;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
    }


    public interface ILogEntry
    {
        LogLevel Level { get; }
        string Message { get; }
        DateTime Timestamp { get; }
        object Context { get; }
    }


    public class LogEntry : ILogEntry
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public object Context { get; set; }
    }
}