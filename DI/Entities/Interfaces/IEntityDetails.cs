using System.Collections.Generic;
namespace BB.Di
{
    public interface IEntityDetails : IEntity
    {
        IReadOnlyCollection<EntityComponentData> GetElements();
        EntityComponentData GetComponentData(in GetComponentDataContext context);
        IEntityInstaller Installer { get; }
        IEntityInjector Injector { get; }
        IEntityPool Pool { get; }
        bool OneShot { get; set; }
    }
}