using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    public static Action Start;
    public static Action<int,Vehicle> GrabBall;
    public static Action LoseBall;
    public static Action<int> Goal;
    public static Action Reset;
}
