using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using dirhash.Models;

namespace dirhash.Mappings
{
    public class LogEntityMap : EntityTypeConfiguration<LogEntity>
    {
        public LogEntityMap()
        {
            ToTable("log");
            HasKey(s => s.Id).Property(s => s.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
