using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dirhash.Services;
using dirhash.Models;

namespace dirhash.Services
{
    public interface ILogService : IBaseService<LogEntity>
    {
        void Add(string message);
    }
}
