using UnityEngine;

[ExecuteInEditMode]
public class PlayerPredicter : MonoBehaviour
{
	public float startVelocity;

	public float predictionTime = 1f;

	public float horizontalSpeedIndicatorRate = 100f;

	public float randomVelocityLoss;

	public int numberOfSimulations = 100;

	private int curSimulation;

	public InputMoment[] playerInputs;

	private float timer;

	private float prevPredictionTime;

	private PredictionData[] predictionData = new PredictionData[0];

	private PredictionData[,] extensivePredictionData = new PredictionData[0, 0];

	private void Awake()
	{
		if (Application.isPlaying)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		CalculatePrediction(extensiveCheck: false);
	}

	public void CalculateExtended()
	{
		int num = Mathf.FloorToInt(predictionTime / Time.fixedDeltaTime);
		extensivePredictionData = new PredictionData[num, numberOfSimulations];
		for (curSimulation = 0; curSimulation < numberOfSimulations; curSimulation++)
		{
			CalculatePrediction(extensiveCheck: true);
		}
	}

	public void RemovePaths()
	{
		extensivePredictionData = new PredictionData[0, 0];
	}

	public void CalculatePrediction(bool extensiveCheck)
	{
		RaycastCollider component = GetComponent<RaycastCollider>();
		PlatformerPhysics component2 = GetComponent<PlatformerPhysics>();
		PlayerWaypointManager component3 = GetComponent<PlayerWaypointManager>();
		PlatformerInput input = component2.input;
		component.drawRays = false;
		component2.drawRays = false;
		Vector3 vector = Vector3.right;
		if ((bool)component3)
		{
			component3.OnReset();
			component3.RotateToWaypoint(snap: true);
			vector = component3.GetDirection();
		}
		component2.OnReset();
		component.velocity = startVelocity * vector;
		if (extensiveCheck)
		{
			component.velocity -= randomVelocityLoss * vector * ((float)curSimulation / (float)numberOfSimulations);
		}
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		Vector3 position = base.transform.position;
		timer = 0f;
		int num = Mathf.FloorToInt(predictionTime / Time.fixedDeltaTime);
		if (!extensiveCheck)
		{
			predictionData = new PredictionData[num];
		}
		input.walkInput = 0f;
		input.jumpInput = false;
		input.slideInput = false;
		InputMoment[] array = playerInputs;
		foreach (InputMoment inputMoment in array)
		{
			inputMoment.triggered = false;
			inputMoment.releasedJump = false;
			if (Random.value > inputMoment.triggerChance && extensiveCheck)
			{
				inputMoment.triggered = true;
				inputMoment.releasedJump = true;
			}
			inputMoment.origXPos = inputMoment.triggerXPos;
			if (inputMoment.origXPos != -1000f && extensiveCheck)
			{
				inputMoment.triggerXPos = inputMoment.origXPos + Random.Range(0f - inputMoment.triggerXPosVariation, inputMoment.triggerXPosVariation);
			}
		}
		for (int j = 0; j < num; j++)
		{
			if (extensiveCheck)
			{
				extensivePredictionData[j, curSimulation] = new PredictionData(base.transform.position, component.velocity);
			}
			else
			{
				predictionData[j] = new PredictionData(base.transform.position, component.velocity);
			}
			array = playerInputs;
			foreach (InputMoment inputMoment2 in array)
			{
				float num2 = (float)inputMoment2.triggerTime * 0.001f;
				bool flag = timer >= num2 && inputMoment2.triggerXPos == -1000f;
				bool flag2 = base.transform.position.x > inputMoment2.triggerXPos && inputMoment2.triggerXPos != -1000f;
				if ((bool)component3)
				{
					flag2 = component3.GetOffset() > inputMoment2.triggerXPos && inputMoment2.triggerXPos != -1000f;
				}
				if (!inputMoment2.triggered && (flag || flag2) && inputMoment2.triggerChance != 0f)
				{
					if (flag2)
					{
						inputMoment2.triggerTime = Mathf.FloorToInt(timer * 1000f);
					}
					input.walkInput = inputMoment2.walkInput;
					input.jumpInput = inputMoment2.jumpInput;
					input.slideInput = inputMoment2.slideInput;
					inputMoment2.triggered = true;
					if (!extensiveCheck)
					{
						predictionData[j].isEvent = true;
					}
				}
				if (!inputMoment2.releasedJump && inputMoment2.jumpInput && timer >= num2 + 0.25f)
				{
					input.jumpInput = false;
					inputMoment2.releasedJump = true;
				}
			}
			component2.FixedUpdate();
			component.FixedUpdate();
			if ((bool)component3)
			{
				component3.FixedUpdate();
			}
			timer += Time.fixedDeltaTime;
		}
		array = playerInputs;
		foreach (InputMoment obj in array)
		{
			obj.triggerXPos = obj.origXPos;
		}
		base.transform.position = position;
		base.transform.localEulerAngles = localEulerAngles;
	}

	private void OnDrawGizmos()
	{
		float num = 0f;
		if (this.predictionData.Length != 0)
		{
			num = this.predictionData[0].position.x;
		}
		for (int i = 0; i < this.predictionData.Length - 1; i++)
		{
			PredictionData predictionData = this.predictionData[i];
			PredictionData predictionData2 = this.predictionData[i + 1];
			float t = Mathf.InverseLerp(0f, 70f, predictionData.velocity.magnitude);
			Gizmos.color = Color.Lerp(Color.green, Color.red, t);
			Gizmos.DrawLine(predictionData.position, predictionData2.position);
			if (predictionData.position.x > num + horizontalSpeedIndicatorRate)
			{
				Gizmos.DrawRay(predictionData.position, Vector3.up * predictionData.velocity.magnitude * 0.2f);
				num += horizontalSpeedIndicatorRate;
			}
			if (this.predictionData[i].isEvent)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(predictionData.position, 0.25f);
			}
			if (i == this.predictionData.Length - 2)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(predictionData2.position, 0.25f);
			}
		}
		for (int j = 0; j < extensivePredictionData.GetLength(1); j++)
		{
			for (int k = 0; k < extensivePredictionData.GetLength(0) - 1; k++)
			{
				PredictionData predictionData3 = extensivePredictionData[k, j];
				PredictionData predictionData4 = extensivePredictionData[k + 1, j];
				float t2 = Mathf.InverseLerp(0f, 70f, predictionData3.velocity.magnitude);
				Color color = Color.Lerp(Color.green, Color.red, t2);
				color.a = 0.25f;
				Gizmos.color = color;
				Gizmos.DrawLine(predictionData3.position, predictionData4.position);
			}
		}
	}
}
