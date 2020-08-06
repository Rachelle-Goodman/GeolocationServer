using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geoloocation.DB
{
    public interface IDB
    {
        Task Insert<TEntity>(TEntity item) where TEntity : DbEntityBase;
        Task Update<TEntity>(TEntity item, object hashKey, object rangeKey = null) where TEntity : DbEntityBase;
        Task Update<TEntity>(Dictionary<string, (object value, DbType type)> attributesToUpdate, object hashKey, object rangeKey = null) where TEntity : DbEntityBase;
        Task<TEntity> Get<TEntity>(object hashKey, object rangeKey = null, List<string> attributesToGet = null) where TEntity : DbEntityBase;
    }

    public abstract class DbEntityBase { }

    public enum DbType
    {
        Bool,
        Number,
        String,
        Enum,
        DateTime,
        Null,
        List,
        Map,
    }
}
