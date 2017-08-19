using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using dirhash.DAL;
using dirhash.Models;

namespace dirhash.Services
{
    public abstract class BaseService<TEntity> : IBaseService<TEntity>
        where TEntity : BaseEntity
    {
        protected readonly IRepository<TEntity> _repository;

        public BaseService(IRepository<TEntity> repository)
        {
            this._repository = repository;
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            return this._repository.Table.AsEnumerable();
        }
        public virtual TEntity GetById(int id)
        {
            return this._repository.GetById(id);
        }

        IEnumerable<TEntity> IBaseService<TEntity>.GetByQuery(Expression<Func<TEntity, bool>> query = null)
        {
            IQueryable<TEntity> queryResult = this._repository.Table.AsQueryable();

            if (query != null)
                queryResult = queryResult.Where(query);

            return queryResult;
        }

        protected IEnumerable<TEntity> GetByQuery(Expression<Func<TEntity, bool>> query = null)
        {
            return ((IBaseService<TEntity>)this).GetByQuery(query);
        }

        TEntity IBaseService<TEntity>.GetFirst(Expression<Func<TEntity, bool>> predicate)
        {
            return this._repository.Table.FirstOrDefault(predicate);
        }

        protected TEntity GetFirst(Expression<Func<TEntity, bool>> predicate)
        {
            return ((IBaseService<TEntity>)this).GetFirst(predicate);
        }

        public virtual int Insert(TEntity entity)
        {
            return this._repository.Insert(entity);
        }

        public virtual void Update(TEntity entity)
        {
            this._repository.Update(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            this._repository.Delete(entity);
        }

        public virtual void Delete(int id)
        {
            this.Delete(this.GetById(id));
        }
    }
}