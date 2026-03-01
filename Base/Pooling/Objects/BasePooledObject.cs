using System;
namespace BB
{
    public abstract class BasePooledObject : IPooledDisposable
    {
        public abstract ulong Counter { get; }

        public abstract void Dispose();
        public static DisposableBag operator +(BasePooledObject l, IDisposable r)
        {
            var bag = DisposableBag.GetPooled();
            bag.Add(l);
            bag.Add(r);
            return bag;
        }
        public static DisposableBag operator +(IDisposable l, BasePooledObject r)
            => r + l;
    }
}