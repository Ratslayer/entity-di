using System;
using System.Threading;
namespace BB
{
	public class ReusableCancellationTokenSource : IDisposable
	{
		CancellationTokenSource _source;
		public CancellationToken Token
		{
			get
			{
				_source ??= new();
				return _source.Token;
			}
		}
		public bool Canceled => _source?.Token.IsCancellationRequested ?? false;
		public void Cancel()
		{
			_source?.Cancel();
			Dispose();
		}
		public void Dispose()
		{
			_source?.Dispose();
			_source = null;
		}
	}
}
