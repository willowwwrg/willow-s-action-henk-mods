using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Radial Blur")]
[RequireComponent(typeof(Camera))]
public class CC_RadialBlur : MonoBehaviour
{
	public float amount = 0.1f;

	public Vector2 center = new Vector2(0.5f, 0.5f);

	public int quality = 1;

	public Shader shaderLow;

	public Shader shaderMed;

	public Shader shaderHigh;

	private Shader _currentShader;

	private Material _material;

	private Material material
	{
		get
		{
			if (quality == 0)
			{
				_currentShader = shaderLow;
			}
			else if (quality == 1)
			{
				_currentShader = shaderMed;
			}
			else if (quality == 2)
			{
				_currentShader = shaderHigh;
			}
			if (_material == null)
			{
				_material = new Material(_currentShader);
				_material.hideFlags = HideFlags.HideAndDontSave;
			}
			else
			{
				_material.shader = _currentShader;
			}
			return _material;
		}
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
	}

	private bool CheckShader()
	{
		if (!_currentShader || !_currentShader.isSupported)
		{
			return false;
		}
		return true;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetFloat("amount", amount);
		_material.SetVector("center", center);
		if (!CheckShader())
		{
			Graphics.Blit(source, destination);
		}
		else
		{
			Graphics.Blit(source, destination, _material);
		}
	}

	private void OnDisable()
	{
		if ((bool)_material)
		{
			Object.DestroyImmediate(_material);
		}
	}
}
