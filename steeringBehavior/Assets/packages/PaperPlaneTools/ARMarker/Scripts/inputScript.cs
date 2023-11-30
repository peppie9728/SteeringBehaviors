using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inputScript : MonoBehaviour
{
    [SerializeField]
    PaperPlaneTools.AR.MainScript mainScript;

    // Start is called before the first frame update
    void Start()
    {
        mainScript = this.gameObject.GetComponent<PaperPlaneTools.AR.MainScript>();
    }

    void Update()
    {
        if (Input.GetKeyDown("left ctrl"))
        {
            print("left ctrl key was pressed");
            mainScript.recordOrigLeftUp = true; // Store Left Up corner
        }

        if (Input.GetKeyDown("left alt"))
        {
            print("left alt key was pressed");
        }
        if (Input.GetKeyDown("right ctrl"))
        {
            print("right ctrl key was pressed");
            mainScript.recordOrigRightDown = true;  // Store Right Bottom corner
        }
        if (Input.GetKeyDown("right alt"))
        {
            print("right alt key was pressed");
        }
    }

}
