using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class CubemapBlur : CubemapRenderer
{
	[SerializeField]
	private int kernelRadius = 4;

	[SerializeField]
	private float sampleRadius = 0.4f;

	[SerializeField]
	private Cubemap blurredCube;

	private float[] kernel1D;

	protected override void ManipulateCubemap()
	{
		base.enabled = false;
		if (!blurredCube || kernelRadius <= 1 || sampleRadius <= 0f)
		{
			return;
		}
		kernel1D = GetKernel(kernelRadius);
		Color[][] array = new Color[6][];
		Color[][] rawData = new Color[6][];
		for (int i = 0; i < 6; i++)
		{
			rawData[i] = cache.GetPixels((CubemapFace)i);
		}
		for (int j = 0; j < 6; j++)
		{
			array[j] = new Color[resolution * resolution];
			for (int k = 0; k < resolution; k++)
			{
				float v = (float)k / (float)resolution;
				for (int l = 0; l < resolution; l++)
				{
					float u = (float)l / (float)resolution;
					Vector3 unitVector = GetUnitVector((CubemapFace)j, u, v);
					GetOffsetVectors(unitVector, out var tangent, out var binormal);
					ref Color reference = ref array[j][l + k * resolution];
					reference = SampleKernel(ref rawData, unitVector, tangent, binormal);
				}
			}
		}
		for (int m = 0; m < 6; m++)
		{
			blurredCube.SetPixels(array[m], (CubemapFace)m);
		}
		blurredCube.Apply();
	}

	private void GetOffsetVectors(Vector3 sampler, out Vector3 tangent, out Vector3 binormal)
	{
		sampler = sampler.normalized;
		Vector3 vector = new Vector3(Mathf.Abs(sampler.x), Mathf.Abs(sampler.y), Mathf.Abs(sampler.z));
		if (vector.x > vector.y && vector.x > vector.z)
		{
			tangent = Vector3.up;
			binormal = Vector3.forward;
		}
		else if (vector.y > vector.z)
		{
			tangent = Vector3.right;
			binormal = Vector3.forward;
		}
		else
		{
			tangent = Vector3.up;
			binormal = Vector3.right;
		}
	}

	private float[] GetKernel(int radius)
	{
		float[] array = new float[radius];
		float num = (float)Math.E;
		float num2 = 2 * radius * radius;
		float num3 = 1f / Mathf.Sqrt((float)Math.PI * num2) * num;
		for (int i = 0; i < radius; i++)
		{
			array[i] = num3 * ((float)(i * i) / num2);
		}
		return array;
	}

	private Vector3 GetUnitVector(CubemapFace face, float u, float v)
	{
		return face switch
		{
			CubemapFace.NegativeX => new Vector3(-0.5f, u - 0.5f, v - 0.5f).normalized, 
			CubemapFace.NegativeY => new Vector3(u - 0.5f, -0.5f, v - 0.5f).normalized, 
			CubemapFace.NegativeZ => new Vector3(u - 0.5f, v - 0.5f, -0.5f).normalized, 
			CubemapFace.PositiveX => new Vector3(0.5f, u - 0.5f, v - 0.5f).normalized, 
			CubemapFace.PositiveY => new Vector3(u - 0.5f, 0.5f, v - 0.5f).normalized, 
			_ => new Vector3(u - 0.5f, v - 0.5f, 0.5f).normalized, 
		};
	}

	private Vector3 GetCubeSampleData(Vector3 sampler)
	{
		Vector3 vector = new Vector3(Mathf.Abs(sampler.x), Mathf.Abs(sampler.y), Mathf.Abs(sampler.z));
		CubemapFace cubemapFace;
		float y;
		float z;
		if (vector.x > vector.y && vector.x > vector.z)
		{
			cubemapFace = ((sampler.x < 0f) ? CubemapFace.NegativeX : CubemapFace.PositiveX);
			y = Mathf.Asin(sampler.y) / ((float)Math.PI / 2f) + 0.5f;
			z = Mathf.Asin(sampler.z) / ((float)Math.PI / 2f) + 0.5f;
		}
		else if (vector.y > vector.z)
		{
			cubemapFace = ((!(sampler.y < 0f)) ? CubemapFace.PositiveY : CubemapFace.NegativeY);
			y = Mathf.Asin(sampler.x) / ((float)Math.PI / 2f) + 0.5f;
			z = Mathf.Asin(sampler.z) / ((float)Math.PI / 2f) + 0.5f;
		}
		else
		{
			cubemapFace = ((!(sampler.z < 0f)) ? CubemapFace.PositiveZ : CubemapFace.NegativeZ);
			y = Mathf.Asin(sampler.x) / ((float)Math.PI / 2f) + 0.5f;
			z = Mathf.Asin(sampler.y) / ((float)Math.PI / 2f) + 0.5f;
		}
		return new Vector3((float)cubemapFace, y, z);
	}

	private Color SampleCube(ref Color[][] rawData, Vector3 meta, bool isMeta)
	{
		int num = resolution - 1;
		int num2 = (int)Mathf.Clamp(meta.y * (float)num, 0f, num);
		int num3 = (int)Mathf.Clamp(meta.z * (float)num, 0f, num);
		return rawData[(int)meta.x][num3 * resolution + num2];
	}

	private Color SampleCube(ref Color[][] rawData, Vector3 sampler)
	{
		Vector3 cubeSampleData = GetCubeSampleData(sampler);
		return SampleCube(ref rawData, cubeSampleData, isMeta: true);
	}

	private Vector3 FixFace(Vector3 faulty, Vector3 reference)
	{
		if (faulty[0] == reference[0])
		{
			return faulty;
		}
		CubemapFace cubemapFace = (CubemapFace)faulty[0];
		switch ((CubemapFace)(int)reference[0])
		{
		case CubemapFace.PositiveY:
		case CubemapFace.NegativeY:
			switch (cubemapFace)
			{
			case CubemapFace.PositiveX:
			case CubemapFace.NegativeX:
			{
				float num3 = faulty[1];
				float num4 = faulty[2];
				faulty[1] = 1f - num4;
				faulty[2] = 1f - num3;
				break;
			}
			case CubemapFace.PositiveZ:
			case CubemapFace.NegativeZ:
			{
				float num = faulty[1];
				float num2 = faulty[2];
				faulty[1] = 1f - num;
				faulty[2] = 1f - num2;
				break;
			}
			}
			break;
		case CubemapFace.NegativeZ:
			faulty[1] = 1f - faulty[1];
			goto case CubemapFace.PositiveZ;
		case CubemapFace.PositiveZ:
			switch (cubemapFace)
			{
			case CubemapFace.PositiveX:
			case CubemapFace.NegativeX:
			{
				float value = faulty[1];
				float value2 = faulty[2];
				faulty[1] = value2;
				faulty[2] = value;
				break;
			}
			case CubemapFace.PositiveY:
				faulty[0] = 3f;
				break;
			case CubemapFace.NegativeY:
				faulty[0] = 2f;
				break;
			}
			break;
		default:
			switch (cubemapFace)
			{
			case CubemapFace.NegativeY:
				faulty[0] = 5f;
				break;
			case CubemapFace.NegativeZ:
				faulty[0] = 2f;
				break;
			case CubemapFace.PositiveY:
				faulty[0] = 4f;
				break;
			case CubemapFace.PositiveZ:
				faulty[0] = 3f;
				break;
			}
			break;
		}
		return faulty;
	}

	private Color SampleKernel(ref Color[][] rawData, Vector3 direction, Vector3 axisX, Vector3 axisY)
	{
		float num = sampleRadius * 45f / (float)(kernelRadius - 1);
		float num2 = kernel1D[kernelRadius - 1] * kernel1D[kernelRadius - 1];
		Vector3 cubeSampleData = GetCubeSampleData(direction);
		Color black = Color.black;
		for (int i = 1; i < kernelRadius; i++)
		{
			for (int j = 1; j < kernelRadius; j++)
			{
				float num3 = kernel1D[kernelRadius - j - 1] * kernel1D[kernelRadius - i - 1];
				Quaternion quaternion = Quaternion.AngleAxis((float)j * num, axisX) * Quaternion.AngleAxis((float)i * num, axisY);
				Vector3 cubeSampleData2 = GetCubeSampleData(quaternion * direction);
				cubeSampleData2 = FixFace(cubeSampleData2, cubeSampleData);
				black += SampleCube(ref rawData, cubeSampleData2, isMeta: true) * num3;
				quaternion = Quaternion.AngleAxis((float)(-j) * num, axisX) * Quaternion.AngleAxis((float)i * num, axisY);
				cubeSampleData2 = GetCubeSampleData(quaternion * direction);
				cubeSampleData2 = FixFace(cubeSampleData2, cubeSampleData);
				black += SampleCube(ref rawData, cubeSampleData2, isMeta: true) * num3;
				quaternion = Quaternion.AngleAxis((float)(-j) * num, axisX) * Quaternion.AngleAxis((float)(-i) * num, axisY);
				cubeSampleData2 = GetCubeSampleData(quaternion * direction);
				cubeSampleData2 = FixFace(cubeSampleData2, cubeSampleData);
				black += SampleCube(ref rawData, cubeSampleData2, isMeta: true) * num3;
				quaternion = Quaternion.AngleAxis((float)j * num, axisX) * Quaternion.AngleAxis((float)(-i) * num, axisY);
				cubeSampleData2 = GetCubeSampleData(quaternion * direction);
				cubeSampleData2 = FixFace(cubeSampleData2, cubeSampleData);
				black += SampleCube(ref rawData, cubeSampleData2, isMeta: true) * num3;
				num2 += num3 * 4f;
			}
		}
		return black / num2;
	}
}
