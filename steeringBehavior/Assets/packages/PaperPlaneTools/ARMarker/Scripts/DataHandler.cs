namespace PaperPlaneTools.AR
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using OpenCvSharp.Demo;
    public class DataHandler : MonoBehaviour
    {
        public MainScript mainScript;
        public Vector2 ballLocation;
        private void Update()
        {
            ballLocation = mainScript.GetBallCoordinates();
        }
    }
}
