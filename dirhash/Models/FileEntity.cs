using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dirhash.Models
{
    /// <summary>
    /// Файлы
    /// </summary>
    public class FileEntity : BaseEntity
    {
        /// <summary>
        /// Название файла
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// Хэш сумма файла
        /// </summary>
        public string Hash { get; set; }
        ///<summary>
        /// Время создания записи
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

}