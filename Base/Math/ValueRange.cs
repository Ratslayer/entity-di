using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public struct ValueRange
{
	[HorizontalGroup]
	[LabelWidth(30)]
	public float _min, _max;
	public ValueRange(float min, float max)
	{
		_min = min;
		_max = max;
	}
	public float Random() => RandomUtils.Range(_min, _max);
	public float Clamp(float value) => Mathf.Clamp(value, _min, _max);
	public float Clamp(float value, float offset)
		=> Mathf.Clamp(value, _min + offset, _max + offset);
	public float Range => _max - _min;
	public float Mid => (_max + _min) * 0.5f;
}