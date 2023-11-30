using UnityEngine;
using System.Collections;

public class Vehicle : MovingEntity {

	// a pointer to the steering behavior class
	SteeringBehaviors m_pSteering;
	//TankControl & SteeringBehavior variables
	float powerLeftWheel;
	float powerRightWheel;
	public float totalPower;
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
	public Transform resetPoint;

	//Finite StateMachine
	public SoccerStateMachine stateMachine;
	[HideInInspector]
	public GameManager gameManager;
	[HideInInspector]

	//variables for the timer used in stun function.
	public bool isHit;
	float timer;

	//this variable is used in the Steering behavior script to find the enemy with the ball.
	public Vehicle ChaseTarget;

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

	private Transform myTransform;
	// Use this for initialization
	public void Start() 
	{
		base.Start();
		Debug.Log("Start in Vehicle");
		m_pSteering = new SteeringBehaviors(this);
		m_vPos = new Vector2(transform.position.x, transform.position.z);
		myTransform = this.gameObject.transform;
		
		//Find object in scene and get the component GameManager.
		gameManager = GameObject.FindWithTag("gameManager").GetComponent<GameManager>();
		
		//find all other robots except this robot, necessary for the avoidance behavior.
		GameObject[] tempRob = GameObject.FindGameObjectsWithTag("Player");
		allRobots = new GameObject [tempRob.Length - 1];
		int i = 0;
		foreach (GameObject o in tempRob)
		{
			if (o != this.gameObject) { allRobots[i] = o; i++; }
		}

		//Assign Event listeners
		EventManager.GrabBall += HandleBallGrab;
		EventManager.Goal += ResetPosition;
		EventManager.LoseBall += LoseBall;
	}
	

	void Update () 
	{
		//handleStun maybe in a state of its own
		HandleStun();
		RotateRobot(totalTorque);
		GrabBall();
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

		// 8] (Pepijn) make sure the total power doesn't exceed the max speed.
		if (totalPower > m_fMaxSpeed)
        {
			totalPower = m_fMaxSpeed;
        }
	}

	public void RotateRobot(float torque)
    {
		//Handle the rotation
		float HoekVersnelling = torque * m_fMass;
		transform.Rotate(0,HoekVersnelling * Time.deltaTime,0);
	}
	public void GetVelocity(bool avoidance,Behaviors bh)
    {
		// define position in 2D Vector
		m_vPos = new Vector2(transform.position.x, transform.position.z);
		
		// calculate the combined force from each steering behavior
		SteeringForce = m_pSteering.Calculate(avoidance,bh);
		
		//convert the global steeringForce into a local vector relative to the robot.
		relativeSteering = SteeringForce.RelativeVector(transform.right, transform.forward);

		differentialSteering(relativeSteering);
	}

	public void MoveRobot(float power)
    {
		Vector3 myTranslate = new Vector3(myTransform.position.x + (transform.forward.x * power * Time.deltaTime),
										  myTransform.position.y,
										  myTransform.position.z + (transform.forward.z * power * Time.deltaTime));
		myTransform.position = myTranslate;
	}

	//Handle all collisions
    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
			case "ball":
				if (gameManager.ballCarrier == null && !isHit)
				{
					EventManager.GrabBall.Invoke(teamID, this);
					targetGrab = targetBall.gameObject;
					gameManager.ballCarrier = this;
				}
				break;
		}
    }

	
	//this function is called in update to update the ball position when the ball is grabbed
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
				timer = 0;
            }
        }
    }
	
	//this function is called as the result of the GrabBall event.
	public void HandleBallGrab(int i, Vehicle vehicle)
    {

		Debug.Log(this.name);
		if (i == teamID)
        {
			if (vehicle == this)
            {
				stateMachine.ChangeState(((SoccerStateMachine)stateMachine).inPossWithBallState);
            }
			else
            {
				stateMachine.ChangeState(((SoccerStateMachine)stateMachine).inPossNoBallState);
				ChaseTarget = gameManager.ballCarrier;
			}
        }
        else
        {
			stateMachine.ChangeState(((SoccerStateMachine)stateMachine).notInPossPlayerState);
			ChaseTarget = vehicle;
		}
    }


	//this function is called after a goal is scored.
	//and can be used for resetting the game.
	public void ResetPosition(int i)
    {
		gameManager.ballCarrier = null;
		stateMachine.ChangeState(((SoccerStateMachine)stateMachine).resetPositionState);
    }

	//this function is called after a robot loses the ball
	public void LoseBall()
    {
		if (gameManager.ballCarrier == this)
        {
			hasBall = false;
			targetGrab = null;
			gameManager.ballCarrier = null;
			isHit = true;
        }
		stateMachine.ChangeState(((SoccerStateMachine)stateMachine).notInPossOpenState);
    }
}