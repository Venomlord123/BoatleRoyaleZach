using System;
using UnityEngine;

// Cams mostly hack buoyancy
public class Buoyancy : MonoBehaviour
{
	public float splashVelocityThreshold;
	public float forceScalar;
	public float waterLineHack; // HACK

	public int underwaterVerts;
	public float dragScalar;

	public static event Action<GameObject, Vector3, Vector3> OnSplash;
	public static event Action<GameObject> OnDestroyed;

	

	Vector3 worldVertPos;
	//My Changes to code
	public MeshFilter meshFilter;
	public Rigidbody rigidbody;
	private Vector3 forceAmount;
	private Vector3 forcePosition;
	public Transform t;
	private int index;
	private float deltaTime;


	private Vector3[] meshNormals;
	private Vector3[] meshVertices;

	private void Start()
	{
		meshNormals = GetComponent<MeshFilter>().mesh.normals;
		meshVertices = GetComponent<MeshFilter>().mesh.vertices;
		t = GetComponent<Transform>();
	}

	void Update()
	{
		deltaTime = Time.deltaTime;
		CalculateForces();
	}

	private void CalculateForces()
	{
		underwaterVerts = 0;
		t.position = transform.position;
		for (index = 0; index < meshNormals.Length; index++)
		{
			worldVertPos = t.position + t.TransformDirection(meshVertices[index]);
			if (worldVertPos.y < waterLineHack)
			{
				// Splashes only on surface of water plane
				if (worldVertPos.y > waterLineHack - 0.1f)
				{
					if (rigidbody.velocity.magnitude > splashVelocityThreshold || rigidbody.angularVelocity.magnitude > splashVelocityThreshold)
					{
						print(rigidbody.velocity.magnitude);
						if (OnSplash != null)
						{
							OnSplash.Invoke(gameObject, worldVertPos, rigidbody.velocity);
						}
					}
				}
				forceAmount = (t.TransformDirection(-meshNormals[index]) * forceScalar) * deltaTime;
				forcePosition = transform.position + t.TransformDirection(meshVertices[index]);
				rigidbody.AddForceAtPosition(forceAmount, forcePosition, ForceMode.Force);
				underwaterVerts++;
			}
			// HACK to remove sunken boats
			if (worldVertPos.y < waterLineHack - 10f)
			{
				DestroyParentGO();
				break;
			}
			// Drag for percentage underwater
			
		}
		rigidbody.drag = (underwaterVerts / (float)meshVertices.Length) * dragScalar;
		rigidbody.angularDrag = (underwaterVerts / (float)meshVertices.Length) * dragScalar;
	}

	private void DestroyParentGO()
	{
		if (OnDestroyed != null)
		{
			OnDestroyed.Invoke(gameObject);
		}
		Destroy(gameObject);
	}
}
