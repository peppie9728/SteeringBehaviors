using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Vehicle[] allVehicles;
    public Vehicle[] team1;
    public Vehicle[] team2;
    public TextMeshProUGUI[] team1UI;
    public TextMeshProUGUI[] team2UI;

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

    public void Update()
    {
        UpdateStateUI();
    }

    public bool checkReady()
    {
        //checks each team for a bool that is only fufillled when the robot is at its reset point.
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
            //reaets all of the balls transformations and drops the ball in the middle of the playing field.
            ball.transform.ResetTransformation();
            ball.GetComponent<Rigidbody>().isKinematic = false;
            ball.transform.position = new Vector3(0, 6, 0);
            resetting = false;
        }
        return result;
    }

    public void SetRespawnPoints()
    {
        //assigns each robot its own respawn point

        ResetPointsTeam1 = new GameObject[team1.Length];
        int i = 0;
        foreach (Vehicle v in team1)
        {
            ResetPointsTeam1[i] = Instantiate(spawnPointPrefab);
            v.resetPoint = ResetPointsTeam1[i].transform;
            ResetPointsTeam1[i].transform.position = v.transform.position;
            i++;
        }
        ResetPointsTeam2 = new GameObject[team2.Length];
        i = 0;
        foreach (Vehicle v in team2)
        {
            ResetPointsTeam2[i] = Instantiate(spawnPointPrefab);
            v.resetPoint = ResetPointsTeam2[i].transform;
            ResetPointsTeam2[i].transform.position = v.transform.position;
            i++;
        }
    }

    public void UpdateStateUI()
    {
        for (int i = 0; i < team1.Length; i++)
        {
            team1UI[i].text = team1[i].name + ": "+ team2[i].stateMachine.currentStateName;
        }
        for (int i = 0; i < team2.Length; i++)
        {
            team2UI[i].text = team2[i].name + ": " + team2[i].stateMachine.currentStateName;
        }
    }
}
