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

	public MeshFilter meshFilter;
	public Rigidbody rigidbody;

	Vector3 worldVertPos;


	void Update()
	{
		CalculateForces();
	}

	private void CalculateForces()
	{
		underwaterVerts = 0;

		for (var index = 0; index < meshFilter.mesh.normals.Length; index++)
		{
			worldVertPos = transform.position + transform.TransformDirection(meshFilter.mesh.vertices[index]);
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
				Vector3	forceAmount = (transform.TransformDirection(-meshFilter.mesh.normals[index]) * forceScalar) * Time.deltaTime;
				Vector3 forcePosition = transform.position + transform.TransformDirection(meshFilter.mesh.vertices[index]);
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
			rigidbody.drag = (underwaterVerts / (float)meshFilter.mesh.vertices.Length) * dragScalar;
			rigidbody.angularDrag = (underwaterVerts / (float)meshFilter.mesh.vertices.Length) * dragScalar;
		}
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
