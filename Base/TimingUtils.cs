//using System;
//using System.Collections;
//using System.Collections.Generic;
//using BB.Di;
//using UnityEngine;
//namespace BB
//{
//	public struct CustomCancellationToken
//	{
//		readonly ulong _id;
//		readonly IPooledCancelable _cancelable;
//		Entity _entity;
//		public CustomCancellationToken(IPooledCancelable cancelable)
//		{
//			_cancelable = cancelable;
//			_id = _cancelable.Id;
//			_entity = default;
//			_cancelable.OnEnd += OnEnd;
//		}
//		public void Cancel()
//		{
//			if (_cancelable == null)
//				return;
//			OnEnd();
//			if (_id == _cancelable.Id)
//				_cancelable.Cancel();
//		}
//		public void Bind(Entity entity)
//		{
//			entity.AddOnDespawn(Cancel);
//		}
//		void OnEnd()
//		{
//			_cancelable.OnEnd -= OnEnd;
//			_entity.RemoveOnDespawn(Cancel);
//		}
//	}
//	public interface IPooledCancelable
//	{
//		ulong Id { get; set; }
//		event Action OnEnd;
//		bool Active { get; }
//		void Cancel();
//	}
//	public class Cooldown
//	{
//		CustomCancellationToken _timer;
//		public bool Active { get; private set; }
//		public void Clear()
//		{
//			Active = false;
//			_timer.Cancel();
//			_timer = default;
//		}
//		public void Activate(float duration)
//		{
//			Clear();
//			Active = true;
//			_timer = TimingUtils.DelayedInvoke(duration, Clear);
//		}
//		public static implicit operator bool(Cooldown c) => c.Active;
//	}
//	public sealed class Timer : IPooledCancelable
//	{
//		public ulong Id { get; set; }
//		public bool Active { get; private set; }
//		public event Action OnEnd;
//		CoroutineCancelationToken _coroutine;

//		public void Start(float delay, Action action)
//		{
//			_coroutine = TimingUtils.StartCoroutine(Coroutine());
//			IEnumerator Coroutine()
//			{
//				Active = true;
//				yield return new WaitForSeconds(delay);
//				Active = false;
//				action();
//				OnEnd?.Invoke();
//			}
//		}
//		public void Cancel()
//		{
//			_coroutine.Cancel();
//			_coroutine = default;
//			CancelablePoolManager.ReturnToPool(this);
//		}
//	}
//	public static class CancelablePoolManager
//	{
//		static readonly Dictionary<Type, List<IPooledCancelable>> _pools = new();
//		static ulong _lastId;
//		public static T GetFromPool<T>()
//			where T : class, IPooledCancelable, new()
//		{
//			if (!_pools.TryGetValue(typeof(T), out var pool))
//			{
//				pool = new();
//				_pools.Add(typeof(T), pool);
//			}
//			var result = pool.Count > 0 ? pool.RemoveLast() : new T();
//			_lastId++;
//			result.Id = _lastId;
//			return result as T;
//		}
//		public static void ReturnToPool(IPooledCancelable cancelable)
//		{
//			if (!_pools.TryGetValue(cancelable.GetType(), out var pool))
//			{
//				Log.Logger.Error($"Trying to return a non-pooled cancelable. Dafuq?");
//				return;
//			}
//			cancelable.Id = 0;
//			pool.Add(cancelable);
//		}
//	}
//	public static class TimingUtils
//	{
//		private static CoroutineManager _manager;
//		private static readonly WaitForEndOfFrame _waitForEndOfFrame = new();
//		private static void Init()
//		{
//			if (_manager)
//				return;
//			_manager = new GameObject("Coroutine Manager").AddComponent<CoroutineManager>();
//			UnityEngine.Object.DontDestroyOnLoad(_manager.gameObject);
//		}
//		public static Coroutine InvokeEndOfFrame(Action action)
//		{
//			Init();
//			return _manager.StartCoroutine(Coroutine());
//			IEnumerator Coroutine()
//			{
//				yield return _waitForEndOfFrame;
//				action();
//			}
//		}
//		public static CoroutineCancelationToken StartCoroutine(IEnumerator coroutine)
//		{
//			Init();
//			return new(_manager.StartCoroutine(coroutine), _manager);
//		}
//		public static CustomCancellationToken DelayedInvoke(float delay, Action action)
//		{
//			var timer = CancelablePoolManager.GetFromPool<Timer>();
//			timer.Start(delay, action);
//			return new(timer);
//		}
//	}
//	public readonly struct CoroutineCancelationToken
//	{
//		readonly Coroutine _coroutine;
//		readonly MonoBehaviour _owner;
//		public CoroutineCancelationToken(Coroutine coroutine, MonoBehaviour owner)
//		{
//			_coroutine = coroutine;
//			_owner = owner;
//		}
//		public void Cancel()
//		{
//			if (_owner && _coroutine is not null)
//				_owner.StopCoroutine(_coroutine);
//		}
//	}
//}