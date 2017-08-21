using Autofac;
using dirhash.DAL;
using dirhash.Services;

namespace dirhash
{
    class IoCConfig
    {

        public IContainer Build()
        {
            var builder = new ContainerBuilder();
            builder.Register<IDbContext>(c => new ApplicationDbContext()).InstancePerDependency();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerDependency();
            builder.RegisterType<LogService>().As<ILogService>().InstancePerDependency();
            builder.RegisterType<FileService>().As<IFileService>().InstancePerDependency();
            return builder.Build();
        }

    }
}
