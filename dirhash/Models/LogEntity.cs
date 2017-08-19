using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dirhash.Models
{
    /// <summary>
    /// Логирование
    /// </summary>
    public class LogEntity : BaseEntity
    {
        /// <summary>
        /// Сообщение
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Время ошибки
        /// </summary>
        public DateTime CreatedAt { get; set; }

    }
}