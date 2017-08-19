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
            builder.Register<IDbContext>(c => new ApplicationDbContext()).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();
            builder.RegisterType<LogService>().As<ILogService>().InstancePerLifetimeScope();
            builder.RegisterType<FileService>().As<IFileService>().InstancePerLifetimeScope();
            return builder.Build();
        }

    }
}
