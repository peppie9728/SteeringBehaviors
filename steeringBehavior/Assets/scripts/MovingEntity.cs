using UnityEngine;
using System.Collections;

public class MovingEntity : BaseGameEntity {

	[HideInInspector]
	public Vector2 m_vPos;
	[HideInInspector]
	public Vector2 m_vVelocity;

	// a normalized vector pointing in the direction the entity is heading
	[HideInInspector]
	public Vector2 m_vHeading;

	// the mass of the moving entity
	[Header("Vehicle stats")]
	public float m_fMass = 1 ;

	// the maximum speed at which this entity may travel;
	public float m_fMaxSpeed;

	// the maximum force this entity can produce to power itself
	// (think rockets and thrust)
	public float m_fMaxForce;

	// the maximum rate (radians per second) at which this vehicle can rotate
	public float m_fMaxTurnRate;

	// the team to wich the robot belongs
	public int teamID;

	// Use this for initialization
	public void Start () {
		base.Start ();
		Debug.Log ("Start in MovingEntity");
	}	

}
