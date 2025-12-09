namespace BB.Di
{
    public record SingleSystemInstaller<TSystem>(
        string Name,
        params object[] SystemArgs)
        : IEntityInstaller
        where TSystem : new()
    {

        public void Install(IDiContainer container)
        {
            container.System<TSystem>(SystemArgs);
        }
    }
}
