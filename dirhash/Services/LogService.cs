using System;
using System.Collections.Generic;
using System.Linq;
using dirhash.DAL;
using dirhash.Models;

namespace dirhash.Services
{
    public class LogService : BaseService<LogEntity>, ILogService
    {
        public LogService(IRepository<LogEntity> repository)
            : base(repository)
        {
        }

        public void Add(string message)
        {
            base.Insert(new LogEntity()
            {
                CreatedAt = DateTime.Now,
                Message = message
            });
            
        }
    }
}