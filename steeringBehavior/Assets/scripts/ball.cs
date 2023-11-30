using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ball : MonoBehaviour
{
    public Vehicle currentBallHolder;

    public void Start()
    {
        EventManager.Goal += ResetGame;
        EventManager.GrabBall += HandleBall;
        EventManager.LoseBall += LoseBall;
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "goal":
                EventManager.Goal.Invoke(currentBallHolder.teamID);
                break;
            case "death":
                transform.position = new Vector3(transform.position.x, 5, transform.position.z);
                break;
        }
    }

    public void ResetGame(int i)
    {
        currentBallHolder.targetGrab = null;
        this.transform.position = new Vector3(100, 100, 100);
        this.GetComponent<Rigidbody>().isKinematic = true;
    }
    //Is Invoked when someone catches the ball.
    public void HandleBall(int i, Vehicle v)
    {
        currentBallHolder = v;
    }
    
    public void LoseBall()
    {
        currentBallHolder = null;
    }
}
