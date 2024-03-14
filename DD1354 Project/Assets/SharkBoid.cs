using UnityEngine;
using System.Collections;

// inherits from Boid
public class SharkBoid : MonoBehaviour
{
    public Predators Predators { get; set; }

    public Vector3 Position;
    public Vector3 Velocity;
    public Vector3 Acceleration;
  
    void Start()
    {
        Velocity = Random.insideUnitSphere * 2;
    }
    public void UpdateSimulation(float deltaTime)
    {
        //Clear acceleration from last frame
        Acceleration = Vector3.zero;

        //Apply forces
        Acceleration += (Vector3)Predators.GetForceFromBounds(this);
        Acceleration += GetConstraintSpeedForce();
        Acceleration += GetSteeringForce();

        //Step simulation
        Velocity += deltaTime * Acceleration;
        Position +=  0.5f * deltaTime * deltaTime * Acceleration + deltaTime * Velocity;
    }

    Vector3 GetConstraintSpeedForce()
    {
        Vector3 force = Vector3.zero;

        //Apply drag
        force -= Predators.Drag * Velocity;

        float vel = Velocity.magnitude;
        if (vel > Predators.MaxSpeed)
        { 
            //If speed is above the maximum allowed speed, apply extra friction force
            force -= (20.0f * (vel - Predators.MaxSpeed) / vel) * Velocity;
        }
        else if (vel < Predators.MinSpeed)
        {
            //Increase the speed slightly in the same direction if it is below the minimum
            force += (5.0f * (Predators.MinSpeed - vel) / vel) * Velocity;
        }

        return force;
    }

    Vector3 GetSteeringForce()
    {
        // pursuit force
        Vector3 pursuitForce = Vector3.zero;
        // check for nearby prey
        Boid closestFish = null;
        float closestDist = float.MaxValue;
        foreach (Boid fish in Predators.BoidManager.GetFish(this, Predators.NeighborRadius))
        {
            float dist = (fish.Position - Position).magnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closestFish = fish;
            }
        }
        if (closestFish != null)
        {
            print("seek");
            print(closestFish.Position);
            pursuitForce = Pursuit(closestFish);
        }
        // separation force from other sharks
        Vector3 separationForce = SeparationForce();
        return pursuitForce + separationForce;
    }

    public Vector3 SeparationForce()
    {
        // separation force from other sharks
        Vector3 separationForce = Vector3.zero;
        foreach (SharkBoid neighbor in Predators.BoidManager.GetNeighbors(this, Predators.NeighborRadius))
        {
            float distance = (neighbor.Position - Position).magnitude;

            //Separation force
            if (distance < Predators.SeparationRadius)
            {
                separationForce += Predators.SeparationForceFactor * ((Predators.SeparationRadius - distance) / distance) * (Position - neighbor.Position);
            }
        }
        return separationForce;
    }

    public Vector3 Seek(Vector3 target_pos)
    {
        Vector3 desired = target_pos - Position;
        desired = desired.normalized * Predators.MaxSpeed;
        Vector3 steer = desired - Velocity;
        return steer;
    }

    // predict the future position of the target and seek that position
    public Vector3 Pursuit(Boid target)
    {
        Vector3 toTarget = target.Position - Position;
        float T = (toTarget.magnitude / Predators.MaxSpeed)*Predators.pursuitPredictionTime;
        Vector3 futurePosition = target.Position + T * target.Velocity;
        // make the shark keep a distance from the target
        Vector3 direction = futurePosition;
        direction.Normalize();
        direction *= 3;
        Vector3 desired = futurePosition - direction;
        // if we are close enough, increase separation factor from other sharks
        if (toTarget.magnitude < 3.0f)
        {
            Vector3 separationForce = SeparationForce();
            separationForce *= 10;
            return Seek(desired) + separationForce;
        }else{
            return Seek(desired);
        }
    }
}
