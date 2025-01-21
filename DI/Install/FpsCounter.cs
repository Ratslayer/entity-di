using TMPro;
using UnityEngine;

public sealed class FpsCounter : BaseBehaviour
{
	[SerializeField] TextMeshProUGUI _text;
	private void Update()
	{
		_text.text = $"{1f / Time.deltaTime:N0}";
	}
}
