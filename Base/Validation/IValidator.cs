namespace BB
{
	public interface IValidator
	{
		bool IsValid(out string message);
	}
}
