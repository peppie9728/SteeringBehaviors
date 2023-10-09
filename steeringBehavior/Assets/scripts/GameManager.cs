using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Vehicle[] team1;
    public Vehicle[] team2;
    public GameObject[] ResetPointsTeam1;
    public GameObject[] ResetPointsTeam2;
    public Vehicle ballCarrier;
    public GameObject ball;
    public bool resetting;
    public GameObject spawnPointPrefab;

    public void Start()
    {
        SetRespawnPoints();
    }


    public void TeamHasBall(Vehicle ballCarrier_)
    {
        ballCarrier = ballCarrier_;
        bool team = false;
        foreach(Vehicle v in team1)
        {
            if (v == ballCarrier_)
            {
                team = true;
            }
        }
        if (team)
        {
            foreach (Vehicle v in team1)
            {
                v.stateMachine.ChangeState(v.stateMachine.inPossesionState);
            }
            foreach (Vehicle v in team2)
            {
                v.stateMachine.ChangeState(v.stateMachine.notInPossesionState);
            }
        }
        else
        {
            foreach (Vehicle v in team2)
            {
                v.stateMachine.ChangeState(v.stateMachine.inPossesionState);
            }
            foreach (Vehicle v in team1)
            {
                v.stateMachine.ChangeState(v.stateMachine.notInPossesionState);
            }
        }
    }

    public void ResetPlay()
    {
        resetting = true;
        for (int i = 0; i < team1.Length; i++)
        {
            team1[i].aTarget = ResetPointsTeam1[i].transform;
            team1[i].stateMachine.ChangeState(team1[i].stateMachine.resetPositionState);
        }

        for (int i = 0; i < team2.Length; i++)
        {
            team2[i].aTarget = ResetPointsTeam2[i].transform;
            team2[i].stateMachine.ChangeState(team2[i].stateMachine.resetPositionState);
        }
    }

    public bool checkReady()
    {
        bool result = true;
        foreach(Vehicle v in team1)
        {
            if (!v.waiting)
            {
                result = false;
            }
         
        }
        foreach (Vehicle v in team2)
        {
            if (!v.waiting)
            {
                result = false;
            }
        }
        if (result)
        {
            ball.transform.ResetTransformation();
            ball.GetComponent<Rigidbody>().isKinematic = false;
            ball.transform.position = new Vector3(0, 6, 0);
            resetting = false;
        }
        return result;
    }

    public void SetRespawnPoints()
    {
        ResetPointsTeam1 = new GameObject[team1.Length];
        int i = 0;
        foreach (Vehicle v in team1)
        {
            ResetPointsTeam1[i] = Instantiate(spawnPointPrefab);
            ResetPointsTeam1[i].transform.position = v.transform.position;
            i++;
        }
        ResetPointsTeam2 = new GameObject[team2.Length];
        i = 0;
        foreach (Vehicle v in team2)
        {
            ResetPointsTeam2[i] = Instantiate(spawnPointPrefab);
            ResetPointsTeam2[i].transform.position = v.transform.position;
            i++;
        }
    }
}
