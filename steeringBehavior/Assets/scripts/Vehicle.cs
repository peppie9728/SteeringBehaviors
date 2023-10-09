using UnityEngine;
using System.Collections;

public class Vehicle : MovingEntity {

	// a pointer to the steering behavior class
	SteeringBehaviors m_pSteering;
	float maxPower = 10;
	float powerLeftWheel;
	float powerRightWheel;
	float totalPower;
	float totalTorque;
	public Vector2 relativeSteering;
	public float scaling;
	float fPivYLimit = 90.0f;
	public Rigidbody rb;
	Vector2 SteeringForce = new Vector2(0, 0);

	[Header("Avoidance")]
	public float objectRadius;
	[HideInInspector]
	public GameObject[] allRobots;
	public float maxAvoidForce;
	[HideInInspector]
	public bool hasBall;

	[Header("Targets")]
	public float slowingRadius;
	public Transform targetBall;
	public Transform targetGoal;
	public Transform aTarget;
	public GameObject targetGrab;
	public Transform ballPoint;

	
	int counter = 0;
	public int counterValue;

	public SoccerState stateMachine;
	[HideInInspector]
	public GameManager gameManager;
	[HideInInspector]
	public bool isHit;
	float timer;

	public bool waiting;
	// Gidi: the 2D version of position
	public Vector2 Pos2 {
		get
		{
			Vector2 temp = new Vector2();
			temp.x = myTransform.position.x;
			temp.y = myTransform.position.z;

			return temp;
		}
	}

	
	// Gidi: the 2D version of aTarget
	public Vector2 targetPos2 {
		get
		{
			Vector2 temp = new Vector2();
			temp.x = aTarget.position.x;
			temp.y = aTarget.position.z;
			
			return temp;
		}
	}

	// a pointer to the world data enabling the vehicle to access any obstacle
	// path, wall or agent data
	// @@@

	//
	private Transform myTransform;
	// Use this for initialization
	public void Start() 
	{
		base.Start();
		Debug.Log("Start in Vehicle");
		m_pSteering = new SteeringBehaviors(this);
		m_vPos = new Vector2(transform.position.x, transform.position.z);
		myTransform = this.gameObject.transform;
		gameManager = GameObject.FindWithTag("gameManager").GetComponent<GameManager>();
		//find all other robots except this robot.
		GameObject[] tempRob = GameObject.FindGameObjectsWithTag("Player");
		allRobots = new GameObject [tempRob.Length - 1];
		int i = 0;
		foreach (GameObject o in tempRob)
		{
			if (o != this.gameObject) { allRobots[i] = o; i++; }
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		HandleStun();
        RotateRobot();
		GrabBall();
    }
    private void FixedUpdate()
    {
		//GetVelocity();
	}

    public void differentialSteering(Vector2 velocity)
	{
		velocity = velocity.normalized * 128;
		float nMotPremixL;    // Motor (left)  premixed output        (-128..+127)
		float nMotPremixR;    // Motor (right) premixed output        (-128..+127)
							  // 1] Calculate Drive Turn output due to Joystick X input
		if (velocity.x >= 0)
		{
			// Forward
			nMotPremixL = (velocity.y >= 0) ? 127.0f : (127.0f + velocity.y); 
			nMotPremixR = (velocity.y >= 0) ? (127.0f - velocity.y) : 127.0f;
		}
		else
		{
			// Reverse
			nMotPremixL = (velocity.y >= 0) ? (127.0f + velocity.y) : 127.0f;
			nMotPremixR = (velocity.y >= 0) ? 127.0f : (127.0f + velocity.y);
		}

		// 2] Scale Drive output due to Joystick Y input (throttle)
		// (The more velocity.y, the more drive)
		nMotPremixL = nMotPremixL * velocity.x / 128.0f;
		nMotPremixR = nMotPremixR * velocity.x / 128.0f;

		// 3] Calculate pivot amount
		// - Strength of pivot (nPivSpeed) based on Joystick X input
		// - Blending of pivot vs drive (fPivScale) based on Joystick Y input (0 .. 1)
		int nPivSpeed = (int)velocity.y;
		// 4] Calculate drive vs pivot scale due to joystic Y input
		float fPivScale = (Mathf.Abs(velocity.x) > fPivYLimit) ? 0.0f : (1.0f - Mathf.Abs(velocity.x) / fPivYLimit);
		// if abs(velocity.y) too large, fPivScale is 0 (no pivot, but turn and drive), else pivot action

		// 5] Calculate final mix of Drive and Pivot
		float mixL = (1.0f - fPivScale) * nMotPremixL + fPivScale * (nPivSpeed);   // (-128 .. 127)
		float mixR = (1.0f - fPivScale) * nMotPremixR + fPivScale * -(nPivSpeed);   // (-128 .. 127)

		// 6] (Gidi) Adjust range from (-128 .. 127) to (-100 .. 100)
		powerLeftWheel = mixL * (scaling / 128.0f); ;
		powerRightWheel = mixR * (scaling / 128.0f); ;
		
		// 7] (Pepijn) Calculate the total torque and linear force of the robot
		totalTorque = powerLeftWheel + powerRightWheel;
		totalPower = powerLeftWheel - powerRightWheel;
	}

	public void RotateRobot()
    {
		// Handle the rotation
		float HoekVersnelling = totalTorque * m_fMass;
		transform.Rotate(0,HoekVersnelling * Time.deltaTime,0);
	}
	public Vector2 GetVelocity(bool avoidance,Behaviors bh)
    {
		// define position in 2D Vector
		m_vPos = new Vector2(transform.position.x, transform.position.z);
		
		// calculate the combined force from each steering behavior in the vehicles list
		if (counter == 0)
		{
			SteeringForce = m_pSteering.Calculate(avoidance,bh);
			counter = counterValue;
		}
        else
        {
			counter--;
        }
		relativeSteering = RelitiveVector(SteeringForce, transform.right, transform.forward);
		
		// Acceleration = Force / Mass (Newton's Second Law of Motion)
		Vector2 acceleration = SteeringForce.DivideBy(m_fMass);

		// update velocity 
		m_vVelocity += acceleration * Time.deltaTime;

		differentialSteering(relativeSteering);

		// make sure vehicle does not exceed maximum velocity
		if (m_vVelocity.sqrMagnitude > (m_fMaxSpeed * m_fMaxSpeed))
		{
			m_vVelocity = m_vVelocity.normalized.MultiplyBy(m_fMaxSpeed);
			//new Vector2 ((float) (m_vVelocity.normalized.x * m_dMaxSpeed), (float) (m_vVelocity.normalized.y * m_dMaxSpeed)) ;
		}

		return m_vVelocity;
	}

	public void MoveRobot()
    {
		Vector3 myTranslate = new Vector3(myTransform.position.x + (transform.forward.x *totalPower* Time.deltaTime),
										  myTransform.position.y,
										  myTransform.position.z + (transform.forward.z* totalPower * Time.deltaTime));
		myTransform.position = myTranslate;
	}


	//Handle all collisions
    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
			case "ball":
				gameManager.TeamHasBall(this);
				hasBall = true;
				targetGrab = targetBall.gameObject;
				collision.gameObject.GetComponent<ball>().currentBallHolder = this;
				aTarget = targetGoal;
				break;

			case "Player":
				Vehicle temp = collision.gameObject.GetComponent<Vehicle>();
				temp.targetGrab = null;
				temp.isHit = true;
				if (!gameManager.resetting)
				{
					aTarget = targetBall;
				}
				break;
		}
    }


	//changes global vector to local vector
	public Vector2 RelitiveVector(Vector2 input,Vector3 right,Vector3 forward)
    {
		Vector2 temp = new Vector2(0,0);
		temp.x = input.x * right.x + input.y * right.z;
		temp.y = input.y * forward.z + input.x * forward.x;
		return temp;
    }

	public void GrabBall()
    {
		if (targetGrab != null)
        {
			targetGrab.transform.position = ballPoint.position;	
        }
    }


	public void HandleStun()
    {
		if (isHit)
        {
			m_fMaxSpeed = 0;
			timer += Time.deltaTime;
			if (timer >= 5)
            {
				m_fMaxSpeed = 5;
				isHit = false;
            }
        }
    }
	
}