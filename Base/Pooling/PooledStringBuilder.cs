using System.Text;
namespace BB
{
	public sealed class PooledStringBuilder : PooledObject<PooledStringBuilder>
	{
		readonly StringBuilder _sb = new();
		public void Append(string str) => _sb.Append(str);
		public void AppendLine(string str) => _sb.AppendLine(str);
		public void AppendLine() => _sb.AppendLine();

		public override void Dispose()
		{
			_sb.Clear();
			base.Dispose();
		}
		public override string ToString()
			=> _sb.ToString();
	}
}