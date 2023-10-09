using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ball : MonoBehaviour
{
    public Vehicle currentBallHolder;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "goal")
        {
            this.GetComponent<Rigidbody>().isKinematic = true;
            this.transform.position = new Vector3(105, 10, 105);
            currentBallHolder.targetGrab = null;
            currentBallHolder.hasBall = false;
            currentBallHolder.aTarget = currentBallHolder.targetBall;
            currentBallHolder.gameManager.ResetPlay();
        }
    }
}
