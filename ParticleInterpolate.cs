using UnityEngine;

public class ParticleInterpolate : MonoBehaviour
{
	private RaycastCollider playerCollider;

	private ParticleSystem[] particles;

	public float velocityToStartLerp = 30f;

	public float velocityToEndLerp = 80f;

	public bool lerpColor;

	public Color lerpTowardsColor = Color.white;

	public bool lerpSize;

	public float lerpTowardsSize;

	public bool lerpSpeed;

	public float lerpTowardsSpeed;

	public bool lerpEmissionRate;

	public float lerpTowardsEmissionRate;

	private Color[] lerpStartColors;

	private float[] lerpStartSizes;

	private float[] lerpStartSpeeds;

	private float[] lerpStartEmissionRates;

	private void Start()
	{
		if (!base.transform.root.GetComponent<PlatformerPhysics>())
		{
			Debug.LogError("The InterpolatedParticle class must be attached to the player!");
		}
		playerCollider = base.transform.root.GetComponent<RaycastCollider>();
		particles = GetComponentsInChildren<ParticleSystem>();
		lerpStartColors = new Color[particles.Length];
		lerpStartSizes = new float[particles.Length];
		lerpStartSpeeds = new float[particles.Length];
		lerpStartEmissionRates = new float[particles.Length];
		for (int i = 0; i < particles.Length; i++)
		{
			ParticleSystem particleSystem = particles[i];
			ref Color reference = ref lerpStartColors[i];
			reference = particleSystem.startColor;
			lerpStartSizes[i] = particleSystem.startSize;
			lerpStartSpeeds[i] = particleSystem.startSpeed;
			lerpStartEmissionRates[i] = particleSystem.emissionRate;
		}
	}

	private void Update()
	{
		float t = Mathf.InverseLerp(velocityToStartLerp, velocityToEndLerp, playerCollider.velocity.magnitude);
		for (int i = 0; i < particles.Length; i++)
		{
			ParticleSystem particleSystem = particles[i];
			if (lerpColor)
			{
				particleSystem.startColor = Color.Lerp(lerpStartColors[i], lerpTowardsColor, t);
			}
			if (lerpSize)
			{
				particleSystem.startSize = Mathf.Lerp(lerpStartSizes[i], lerpTowardsSize, t);
			}
			if (lerpSpeed)
			{
				particleSystem.startSpeed = Mathf.Lerp(lerpStartSpeeds[i], lerpTowardsSpeed, t);
			}
			if (lerpEmissionRate)
			{
				particleSystem.emissionRate = Mathf.Lerp(lerpStartEmissionRates[i], lerpTowardsEmissionRate, t);
			}
		}
	}
}
