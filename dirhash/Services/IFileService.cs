using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dirhash.Services;
using dirhash.Models;

namespace dirhash.Services
{
    public interface IFileService : IBaseService<FileEntity>
    {
        bool Insert(KeyValuePair<string, string> file);
        bool Insert(List<FileEntity> files);
    }
}
