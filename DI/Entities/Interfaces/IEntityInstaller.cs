namespace BB.Di
{
    public interface IEntityInstallerBase
    {
        string Name { get; }
        void Install(IDiContainer container);
    }
    public interface IEntityInstaller : IEntityInstallerBase
    {
    }
}