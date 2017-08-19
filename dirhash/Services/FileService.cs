using System;
using System.Collections.Generic;
using System.Linq;
using dirhash.DAL;
using dirhash.Models;

namespace dirhash.Services
{
    public class FileService : BaseService<FileEntity>, IFileService
    {


        public FileService(IRepository<FileEntity> repository)
            : base(repository)
        {
        }


        public void Insert(KeyValuePair<string, string> file)
        {
            base.Insert(new FileEntity()
            {
                Filename = file.Key,
                Hash = file.Value,
                CreatedAt = DateTime.Now
            });

        }

    }
}