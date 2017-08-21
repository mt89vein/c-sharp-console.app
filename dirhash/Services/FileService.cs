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

        public bool Insert(List<KeyValuePair<string, string>> files)
        {
            bool status = true;
            try
            {
                List<FileEntity> filesEntities = new List<FileEntity>();
                foreach (KeyValuePair<string, string> file in files)
                {
                    filesEntities.Add(new FileEntity()
                    {
                        Filename = file.Key,
                        Hash = file.Value,
                        CreatedAt = DateTime.Now
                    });
                }
                base.Insert(filesEntities);
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