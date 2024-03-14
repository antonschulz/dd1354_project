using UnityEngine;
using System.Collections;

public class Boid : MonoBehaviour 
{
    public School School { get; set; }

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
        Acceleration += (Vector3)School.GetForceFromBounds(this);
        Acceleration += GetConstraintSpeedForce();
        Acceleration += GetSteeringForce();

        //Step simulation
        Velocity += deltaTime * Acceleration;
        Position +=  0.5f * deltaTime * deltaTime * Acceleration + deltaTime * Velocity;
        print("fish");
        print(Position);
    }

    Vector3 GetSteeringForce()
    {
        Vector3 cohesionForce = Vector3.zero;
        Vector3 alignmentForce = Vector3.zero;
        Vector3 separationForce = Vector3.zero;

        int alignmentNeighbors = 0;
        Vector3 velocitySum = Vector3.zero;

        int cohesionNeighbors = 0;
        Vector3 positionSum = Vector3.zero;

        //Boid forces
        foreach (Boid neighbor in School.BoidManager.GetNeighbors(this, School.NeighborRadius))
        {
            float distance = (neighbor.Position - Position).magnitude;

            //Separation force
            if (distance < School.SeparationRadius)
            {
                separationForce += School.SeparationForceFactor * ((School.SeparationRadius - distance) / distance) * (Position - neighbor.Position);
            }

            //Calculate average position/velocity here
            if (distance < School.AlignmentRadius)
            {
                alignmentNeighbors++;
                velocitySum += neighbor.Velocity;
            }

            if (distance < School.CohesionRadius)
            {
                cohesionNeighbors++;
                positionSum += neighbor.Position;
            }

        }

        //Set cohesion/alignment forces here
        if (alignmentNeighbors > 0)
        {
            Vector3 averageVelocity = velocitySum / alignmentNeighbors;
            alignmentForce = School.AlignmentForceFactor * (averageVelocity - Velocity);
        }

        if (cohesionNeighbors > 0)
        {
            Vector3 averagePosition = positionSum / cohesionNeighbors;
            cohesionForce = School.CohesionForceFactor * (averagePosition - Position);
        }
        Vector3 fleeForce = GetFleeingForce();
        return alignmentForce + cohesionForce + separationForce + fleeForce;
    }

    Vector3 GetFleeingForce()
    {
        // check for nearby sharks
        SharkBoid closestShark = null;
        float closestDist = float.MaxValue;
        foreach (SharkBoid shark in School.BoidManager.GetPredators(this, School.PredatorRadius))
        {
            float dist = (shark.Position - Position).magnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closestShark = shark;
            }
        }
        if (closestShark != null)
        {
            return Flee(closestShark);
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Vector3 Seek(Vector3 target_pos)
    {
        Vector3 desired = target_pos - Position;
        desired = desired.normalized * School.MaxSpeed;
        Vector3 steer = desired - Velocity;
        return steer;
    }

    public Vector3 Flee(SharkBoid target)
    {
        Vector3 toTarget = target.Position - Position;
        float T = (toTarget.magnitude / School.MaxSpeed)*1;
        Vector3 futurePosition = target.Position + T * target.Velocity;
        if (toTarget.magnitude < 10.0f)
        {
            return -Seek(target.Position);
        }
        // if the fish is close enough to the shark, eat it
        
        return -Seek(futurePosition);
    }

    Vector3 GetConstraintSpeedForce()
    {
        Vector3 force = Vector3.zero;

        //Apply drag
        force -= School.Drag * Velocity;

        float vel = Velocity.magnitude;
        if (vel > School.MaxSpeed)
        { 
            //If speed is above the maximum allowed speed, apply extra friction force
            force -= (20.0f * (vel - School.MaxSpeed) / vel) * Velocity;
        }
        else if (vel < School.MinSpeed)
        {
            //Increase the speed slightly in the same direction if it is below the minimum
            force += (5.0f * (School.MinSpeed - vel) / vel) * Velocity;
        }

        return force;
    }
}
