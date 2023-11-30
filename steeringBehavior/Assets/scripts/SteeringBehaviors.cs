using UnityEngine;
using System.Collections;

public enum Behaviors
{
	Seek,Flee,Arrive,Allign,Support
};

public class SteeringBehaviors {

	private Vehicle m_pVehicle; // the vehicle attached to this class
	private float timer = 0.25f;

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
			case Behaviors.Allign:
				temp = Allign(m_pVehicle.targetPos2);
				break;
			case Behaviors.Support:
				temp = Support();
				break;
		}
		// if the steeringbehavior has to avoid other obstacles
		if (avoidance)
		{
			temp = temp + AvoidObstacles(m_pVehicle.allRobots);
		}
		// if the steering force exceeds the max force
		if (temp.sqrMagnitude > (m_pVehicle.m_fMaxForce * m_pVehicle.m_fMaxForce))
		{
			temp = temp.normalized.MultiplyBy(m_pVehicle.m_fMaxForce).Truncate();
		}
		return temp;
	}
	//------------------------------- Seek -----------------------------------
	//
	//  Given a target, this behavior returns a steering force which will
	//  direct the agent towards the target
	//
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
	//
	//------------------------------------------------------------------------
	Vector2 Flee( Vector2 targetPos) 
	{
		Vector2 temp = m_pVehicle.Pos2 - targetPos;
		
		Vector2 DesiredVelocity = temp.normalized.MultiplyBy (m_pVehicle.m_fMaxSpeed);
		
		return (DesiredVelocity);
	}
	//----------------------------- Arrive -------------------------------------
	//
	// 
	//
	//------------------------------------------------------------------------

	Vector2 Arrive(Vector2 targetPos)
    {
		Vector2 temp = targetPos - m_pVehicle.Pos2;

		float distance = temp.magnitude;

		Vector2 DesiredVelocity;
		if (distance < m_pVehicle.slowingRadius)
		{
			//Inside the slowing area 
			DesiredVelocity = temp.normalized * m_pVehicle.m_fMaxSpeed * (distance /m_pVehicle.slowingRadius);
		}
		else
		{
			//Outside the slowing area. 
			DesiredVelocity = temp.normalized.MultiplyBy(m_pVehicle.m_fMaxSpeed);
		}
		return (DesiredVelocity);
	}
	
	//-----------------------------allign---------------------------------------
	//
	// alligns Itself with a another steeringforce
	//
	//-------------------------------------------------------------------------

	Vector2 Allign(Vector2 targetForce)
    {
		Vector2 earlyTarget = FindInterceptionPoint();
		Vector2 result = new Vector2();
		if (Vector2.Distance(m_pVehicle.m_vPos, earlyTarget) > 1)
        {
			// If the distance is greater than 1, adjust the maximum speed and apply the Seek behavior towards the interception point
			m_pVehicle.m_fMaxSpeed = 5;
			result = Seek(earlyTarget);
        }
        else
		{
			// Adjust the maximum speed and move forward
			m_pVehicle.m_fMaxSpeed = 4f;
			result = new Vector2(m_pVehicle.transform.forward.x, m_pVehicle.transform.forward.z);
			
			if (timer <= 0)
			{
				// If the timer has elapsed, invoke the LoseBall event and reset the timer
				EventManager.LoseBall.Invoke();
				timer = 0.25f;
			}
			else
			{
				timer -= Time.deltaTime;
			}
		}
		return result;
    }

	//this function calculates the two points where a robot can intercept another robot
	public Vector2 FindInterceptionPoint()
	{
		// Calculate vectors to the left and right of the chase target
		Vector2 leftBehindTarget = m_pVehicle.ChaseTarget.transform.forward.RotateVector(90, 3);
		Vector2 rightBehindTarget = m_pVehicle.ChaseTarget.transform.forward.RotateVector(-90, 3);

		// Transform these vectors to world space and extract the x and z components
		Vector3 tempLeft = m_pVehicle.ChaseTarget.transform.TransformPoint(new Vector3(leftBehindTarget.x, 0, leftBehindTarget.y));
		leftBehindTarget = new Vector2(tempLeft.x, tempLeft.z);

		Vector3 tempRight = m_pVehicle.ChaseTarget.transform.TransformPoint(new Vector3(rightBehindTarget.x, 0, rightBehindTarget.y));
		rightBehindTarget = new Vector2(tempRight.x, tempRight.z);

		// Compare distances and return the appropriate point
		if (Vector2.Distance(rightBehindTarget, m_pVehicle.m_vPos) < Vector2.Distance(leftBehindTarget, m_pVehicle.m_vPos))
		{
			return rightBehindTarget;
		}
		else
		{
			return leftBehindTarget;
		}

	}

	//-----------------------------avoid---------------------------------------
	//
	// avoids other objects in scene
	//
	//-------------------------------------------------------------------------

	Vector2 AvoidObstacles(GameObject[] obstacles)
    {
		Vector2 result;
		// Calculate a dynamic length based on the vehicle's total power, maximum speed, and a constant factor
		float dynamicLength = m_pVehicle.totalPower /m_pVehicle.m_fMaxSpeed * 5;

		// Calculate two points ahead of the vehicle based on its velocity and dynamic length
		Vector2 ahead = m_pVehicle.m_vPos + (new Vector2(m_pVehicle.transform.forward.x, m_pVehicle.transform.forward.z)  * dynamicLength);
		Vector2 ahead2 = m_pVehicle.m_vPos + ((new Vector2(m_pVehicle.transform.forward.x, m_pVehicle.transform.forward.z) * dynamicLength) * 0.5f);

		// Find the most threatening obstacle in the calculated path
		GameObject mostThreat = FindMostThreathening(obstacles, ahead, ahead2);

		Vector2 avoidance = new Vector2(0, 0);

		// If there is a threatening obstacle, calculate the avoidance vector
		if (mostThreat != null)
		{
			Vector2 temp = new Vector2(0,0);
			temp.x = ahead.x - mostThreat.transform.position.x;
			temp.y = ahead.y - mostThreat.transform.position.z;
			avoidance = temp.normalized * m_pVehicle.maxAvoidForce;
		}
		return avoidance;
    }

	// function to find the most threatening obstacle in the given path
	GameObject FindMostThreathening(GameObject[] obstacles,Vector2 ahead,Vector2 ahead2)
    {
		GameObject mostThreat = null;
		// Iterate through the obstacles to find the most threatening one
		foreach (GameObject obs in obstacles)
		{
			if (obs != m_pVehicle.gameObject)
			{
				bool collision = LineIntersectsCircle(ahead, ahead2, obs);

				// If there is a collision and it's the most threatening or the only one so far, update mostThreat
				if (collision && (mostThreat == null || Vector3.Distance(m_pVehicle.transform.position, obs.transform.position) < Vector3.Distance(m_pVehicle.transform.position, mostThreat.transform.position)))
				{
					mostThreat = obs;
				}
			}
		}
		return mostThreat;
	}

	// function to check if a line between two points intersects with a circular obstacle
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

	//-----------------------------Support---------------------------------------
	//
	// supports the other teammate when he has the ball
	//
	//-------------------------------------------------------------------------

	Vector2 Support()
    {
		Vector2 earlyTarget = FindInterceptionPoint();
		Vector2 result = new Vector2();
		if (Vector2.Distance(m_pVehicle.m_vPos, earlyTarget) > 1)
		{
			m_pVehicle.m_fMaxSpeed = 5;
			result = Seek(earlyTarget);
		}
		else
		{
			m_pVehicle.m_fMaxSpeed = 4;
			result = new Vector2(m_pVehicle.transform.forward.x, m_pVehicle.transform.forward.z);
		}

		return result;
	}
}
