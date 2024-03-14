using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Predators : MonoBehaviour
{
    [SerializeField]
    public float pursuitPredictionTime = 1.0f;
    [SerializeField]
    private int m_numFish = 4;

    [SerializeField]
    private SharkBoid m_sharkPrefab = null;

    [SerializeField]
    private float m_spawnRadius = 10;

    [SerializeField]
    private BoxCollider m_bounds = null;

    [SerializeField]
    private float m_boundsForceFactor = 5;

    [Header("Boid behaviour data. Experiment with changing these during runtime")]

    [SerializeField]
    private float m_maxSpeed = 8;
    public float MaxSpeed
    {
        get { return m_maxSpeed; }
        set { m_maxSpeed = value; }
    }

    [SerializeField]
    private float m_minSpeed = 2;
    public float MinSpeed
    {
        get { return m_minSpeed; }
        set { m_minSpeed = value; }
    }

    [SerializeField]
    private float m_drag = 0.05f;
    public float Drag
    {
        get { return m_drag; }
        set { m_drag = value; }
    }

    
    [SerializeField]
    private float radius = 20;
    public float NeighborRadius
    {
        get { return radius; }
    }

    [SerializeField]
    private float m_separationForceFactor = 5;
    public float SeparationForceFactor
    {
        get { return m_separationForceFactor; }
        set { m_separationForceFactor = value; }
    }

    [SerializeField]
    private float m_separationRadius = 10;
    public float SeparationRadius
    {
        get { return m_separationRadius; }
        set { m_separationRadius = value; }
    }

    public BoidManager BoidManager { get; set; }
    
    public IEnumerable<SharkBoid> SpawnSharks()
    {
        for (int i = 0; i < m_numFish; ++i)
        {
            Vector3 spawnPoint = transform.position + m_spawnRadius * Random.insideUnitSphere;

            for (int j = 0; j < 3; ++j)
                spawnPoint[j] = Mathf.Clamp(spawnPoint[j], m_bounds.bounds.min[j], m_bounds.bounds.max[j]);

            SharkBoid boid = Instantiate(m_sharkPrefab, spawnPoint, m_sharkPrefab.transform.rotation) as SharkBoid;
            boid.Position = spawnPoint;
            boid.Velocity = Random.insideUnitSphere;
            boid.Predators = this;
            boid.transform.parent = this.transform;
            yield return boid;
        }
    }

    public Vector3 GetForceFromBounds(SharkBoid boid)
    {
        Vector3 force = new Vector3();
        Vector3 centerToPos = (Vector3)boid.Position - transform.position;
        Vector3 minDiff = centerToPos + m_bounds.size * 0.5f;
        Vector3 maxDiff = centerToPos - m_bounds.size * 0.5f;
        float friction = 0.0f;

        for (int i = 0; i < 3; ++i)
        {
            if (minDiff[i] < 0)
                force[i] = minDiff[i];
            else if (maxDiff[i] > 0)
                force[i] = maxDiff[i];
            else
                force[i] = 0;

            friction += Mathf.Abs(force[i]);
        }

        force += 0.1f * friction * (Vector3)boid.Velocity;
        return -m_boundsForceFactor * force;
    }
}
