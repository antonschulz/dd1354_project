using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoidManager : MonoBehaviour
{
    private List<Boid> m_boids;
    private List<SharkBoid> predator_boids;

    void Start()
    {
        m_boids = new List<Boid>();
        predator_boids = new List<SharkBoid>();

        var schools = GameObject.FindObjectsOfType<School>();
        foreach (var school in schools)
        {
            school.BoidManager = this;
            m_boids.AddRange(school.SpawnFish());
        }

        var predators = GameObject.FindObjectsOfType<Predators>();
        foreach (var predator in predators)
        {
            predator.BoidManager = this;
            predator_boids.AddRange(predator.SpawnSharks());
        }
    }

    void FixedUpdate()
    {
        foreach (Boid boid in m_boids)
        {
            boid.UpdateSimulation(Time.fixedDeltaTime);
        }
        foreach (SharkBoid boid in predator_boids)
        {
            boid.UpdateSimulation(Time.fixedDeltaTime);
        }
    }

    // fish finding nearby fish
    public IEnumerable<Boid> GetNeighbors(Boid boid, float radius)
    {
        float radiusSq = radius * radius;
        foreach (var other in m_boids)
        {
            if (other != boid && (other.Position - boid.Position).sqrMagnitude < radiusSq)
                yield return other;
        }
    }

    // shark finding nearby sharks
    public IEnumerable<SharkBoid> GetNeighbors(SharkBoid boid, float radius)
    {
        float radiusSq = radius * radius;
        foreach (var other in predator_boids)
        {
            if (other != boid && (other.Position - boid.Position).sqrMagnitude < radiusSq)
                yield return other;
        }
    }

    // shark finding nearby fish
    public IEnumerable<Boid> GetFish(SharkBoid boid, float radius)
    {
        float radiusSq = radius * radius;
        foreach (var other in m_boids)
        {
            if ((other.Position - boid.Position).sqrMagnitude < radiusSq)
                yield return other;
        }
    }



    // fish finding nearby sharks
    public IEnumerable<SharkBoid> GetPredators(Boid boid, float radius)
    {
        float radiusSq = radius * radius;
        foreach (var other in predator_boids)
        {
            if ((other.Position - boid.Position).sqrMagnitude < radiusSq)
                yield return other;
        }
    }

}
