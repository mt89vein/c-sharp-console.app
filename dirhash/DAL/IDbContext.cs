using System.Collections.Generic;
using System.Data.Entity;
using dirhash.Models;

namespace dirhash.DAL
{
    public interface IDbContext
    {
        IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity;

        int SaveChanges();

        IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters)
            where TEntity : BaseEntity, new();

        IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters);

        int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters);

        void Detach(object entity);

        bool ProxyCreationEnabled { get; set; }

        bool AutoDetectChangesEnabled { get; set; }
    }
}