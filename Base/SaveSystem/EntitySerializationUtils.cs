namespace BB
{
	public static class EntitySerializationUtils
    {
        public static void RegisterAsSerializedEntity(Entity entity, string serializedName)
        {
            entity._ref.SerializationName = serializedName;
            //var serializedEntities = World.Require<ISerializedEntities>();
            //serializedEntities.Add(entity, serializedName);
        }
    }
}