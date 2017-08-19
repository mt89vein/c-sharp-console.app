using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using dirhash.Models;

namespace dirhash.Mappings
{
    public class FileEntityMap : EntityTypeConfiguration<FileEntity>
    {
        public FileEntityMap()
        {
            ToTable("file");
            HasKey(s => s.Id).Property(s => s.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
