namespace BB.Di
{
    public interface IEntityStateHandler
    {
        void UpdateEffectiveState();
        void PrepareForSpawn();
        void FinalizeDespawn();
        void PublishSpawnEvent();
        void PublishPostSpawnEvent();
        void PublishEnableEvent();
        void PublishDisableEvent();
        void PublishDestroyedEvent();
        void FinalizeDestroy();
        void PublishDespawnEvent();
    }
}