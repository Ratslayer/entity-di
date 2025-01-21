namespace BB.Di
{
	public interface IEntityLifecycle
	{
		void Update(UpdateTime time);
		void FixedUpdate(UpdateTime time);
		void LateUpdate(UpdateTime time);
	}
}