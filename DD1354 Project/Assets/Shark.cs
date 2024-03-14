using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SharkBoid))]
public class Shark : MonoBehaviour 
{
    private SharkBoid m_boid = null;

	void Start () 
    {
        m_boid = GetComponent<SharkBoid>();
	}
	
	void Update () 
    {
        Vector3 velocity = m_boid.Velocity;
        if (velocity.sqrMagnitude > 0.001f)
        {
            Quaternion target = Quaternion.LookRotation(velocity, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * 6);
        }

        transform.position = m_boid.Position;
        //transform.position += new Vector3(0, 0, transform.parent.position.z); //inherit z-position
	}
}
