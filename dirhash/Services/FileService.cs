using System;
using System.Collections.Generic;
using System.Linq;
using dirhash.DAL;
using dirhash.Models;

namespace dirhash.Services
{
    public class FileService : BaseService<FileEntity>, IFileService
    {
        private readonly ILogService _logService;

        public FileService(IRepository<FileEntity> repository, ILogService logService)
            : base(repository)
        {
            _logService = logService;
        }

        public bool Insert(KeyValuePair<string, string> file)
        {
            int id = base.Insert(new FileEntity()
            {
                Filename = file.Key,
                Hash = file.Value,
                CreatedAt = DateTime.Now
            });

            //возвращает true если вставка удалась
            return id > 0;
        }

        public bool Insert(List<FileEntity> files)
        {
            bool status = true;
            try
            {
                base.Insert(files);
            }
            catch (Exception e)
            {
                status = false;
                _logService.Add(e.Message);
            }
            return status;
        }
    }
}