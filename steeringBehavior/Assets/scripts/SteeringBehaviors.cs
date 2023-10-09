using UnityEngine;
using System.Collections;

public enum Behaviors
{
	Seek,Flee,Arrive
};

public class SteeringBehaviors {

	private Vehicle m_pVehicle; // the vehicle attached to this class

	// constructor
	public SteeringBehaviors(Vehicle aVehicle)
	{
		m_pVehicle = aVehicle;
	}

	// calculate the combined force from each steering behavior in the vehicles list
	//
	// The calculate method sums all the vehicles active steering behaviors and 
	// returns the total steering force
	// 
	public Vector2 Calculate(bool avoidance,Behaviors behavior) 
	{
		Vector2 temp = new Vector2();
		switch (behavior)
        {
			case Behaviors.Seek:
				temp = Seek(m_pVehicle.targetPos2);
				break;
			case Behaviors.Flee:
				temp = Flee(m_pVehicle.targetPos2);
				break;
			case Behaviors.Arrive:
				temp = Arrive(m_pVehicle.targetPos2);
				break;
		}
		// if the steering has to avoid other obstacles
		if (avoidance)
		{
			temp = temp + AvoidObstacles(m_pVehicle.allRobots);
		}
		// if the steering force exceeds the max force
		if (temp.sqrMagnitude > (m_pVehicle.m_fMaxForce * m_pVehicle.m_fMaxForce))

		{ temp = temp.normalized.MultiplyBy(m_pVehicle.m_fMaxForce).Truncate(); }
		return temp;
	}
	//------------------------------- Seek -----------------------------------
	//
	//  Given a target, this behavior returns a steering force which will
	//  direct the agent towards the target
	//------------------------------------------------------------------------
	Vector2 Seek( Vector2 targetPos) 
	{
		Vector2 temp = targetPos - m_pVehicle.Pos2;
		Vector2 DesiredVelocity = temp.normalized.MultiplyBy (m_pVehicle.m_fMaxSpeed);
		Vector2 vehicleVelocity = new Vector2(m_pVehicle.rb.velocity.x, m_pVehicle.rb.velocity.z);
		return (DesiredVelocity - vehicleVelocity);
	}

	//----------------------------- Flee -------------------------------------
	//
	//  Does the opposite of Seek
	//------------------------------------------------------------------------
	Vector2 Flee( Vector2 targetPos) 
	{
		Vector2 temp = m_pVehicle.Pos2 - targetPos;
		
		Vector2 DesiredVelocity = temp.normalized.MultiplyBy (m_pVehicle.m_fMaxSpeed);
		
		return (DesiredVelocity - m_pVehicle.m_vVelocity);
	}
	//----------------------------- Arrive -------------------------------------
	//
	// 
	//------------------------------------------------------------------------

	Vector2 Arrive(Vector2 targetPos)
    {
		Vector2 temp = targetPos - m_pVehicle.Pos2;

		float distance = temp.magnitude;

		Vector2 DesiredVelocity;
		//Debug.Log(distance);
		if (distance < m_pVehicle.slowingRadius)
		{
			// Inside the slowing area 
			DesiredVelocity = temp.normalized * m_pVehicle.m_fMaxSpeed * (distance /m_pVehicle.slowingRadius);
		}
		else
		{
			// Outside the slowing area. 
			DesiredVelocity = temp.normalized.MultiplyBy(m_pVehicle.m_fMaxSpeed);
		}
		return (DesiredVelocity - m_pVehicle.m_vVelocity);
	}

	//-----------------------------avoid---------------------------------------
	//
	// avoids other objects in scene
	//
	//-------------------------------------------------------------------------

	Vector2 AvoidObstacles(GameObject[] obstacles)
    {
		Vector2 result;
		float dynamicLength = m_pVehicle.m_vVelocity.magnitude /m_pVehicle.m_fMaxSpeed * 10;
		Vector2 ahead = m_pVehicle.m_vPos + (new Vector2(m_pVehicle.transform.forward.x,m_pVehicle.transform.forward.z) * dynamicLength);
		
		Vector2 ahead2 = m_pVehicle.m_vPos + (m_pVehicle.m_vVelocity.normalized * dynamicLength * 0.5f);
		GameObject mostThreat = FindMostThreathening(obstacles, ahead, ahead2);

		Vector2 avoidance = new Vector2(0, 0);
		if (mostThreat != null)
		{
			Vector2 temp = new Vector2(0,0);
			temp.x = ahead.x - mostThreat.transform.position.x;
			temp.y = ahead.y - mostThreat.transform.position.z;
			avoidance = temp.normalized * m_pVehicle.maxAvoidForce;
		}
		return avoidance;
    }

	GameObject FindMostThreathening(GameObject[] obstacles,Vector2 ahead,Vector2 ahead2)
    {
		GameObject mostThreat = null;
		foreach (GameObject obs in obstacles)
		{
			bool collision = LineIntersectsCircle(ahead, ahead2, obs);
			if (collision && (mostThreat == null || Vector3.Distance(m_pVehicle.transform.position, obs.transform.position) < Vector3.Distance(m_pVehicle.transform.position, mostThreat.transform.position)))
			{
				mostThreat = obs;
			}
		}
		return mostThreat;
	}

	bool LineIntersectsCircle(Vector2 ahead,Vector2 ahead2, GameObject obstacle)
    {
		if (Vector2.Distance(ahead,new Vector2(obstacle.transform.position.x,obstacle.transform.position.z))<m_pVehicle.objectRadius || Vector2.Distance(ahead2, new Vector2(obstacle.transform.position.x, obstacle.transform.position.z)) < m_pVehicle.objectRadius || Vector2.Distance(m_pVehicle.m_vPos,new Vector2(obstacle.transform.position.x, obstacle.transform.position.z)) < m_pVehicle.objectRadius)
		{
			return true;
		}
        else
        {
			return false;
        }
    }
}
