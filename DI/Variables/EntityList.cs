namespace BB
{
    public abstract class EntityList<SelfType> : ListVariable<SelfType, Entity>
        where SelfType : EntityList<SelfType>
    {
        public void RemoveAllDespawnedEntities()
        {
            foreach (var i in -Count)
                if (!this[i])
                    RemoveAt(i);
        }
        [OnEvent]
        public void OnDespawn(EntityDespawnedEvent _) => RemoveAllDespawnedEntities();
    }
}