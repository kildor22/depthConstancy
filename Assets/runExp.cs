using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
//using StreamWriter;

public class runExp : MonoBehaviour
{
public GameObject eccentricObj;
public int trialNumber;
public int trialTotal;

public System.TimeSpan trialDuration;
public System.DateTime trialStartTime;
public System.DateTime trialEndTime;
//public var outputFile;
// Set a variable to the Documents path.


public bool isTrial;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("I am alive!");
        trialNumber = 1;
        trialTotal = 5;
        isTrial = true;
        trialStartTime = System.DateTime.Now;
    }
    void Update()
    {
        if (isTrial)
        {
            if (Input.GetKeyDown("space"))
            {
                Debug.Log("space pressed");
                OutputTrialResults();
                isTrial = false;
                
            }
            else if (Input.GetKeyDown("up"))
            {
                Debug.Log("up pressed");
            }
            else if (Input.GetKeyDown("down"))
            {
                Debug.Log("down pressed");
            }
        }
        else
        {
            if (trialNumber == trialTotal)
            {
                Application.Quit();
            }
            else
            {
                trialStartTime = System.DateTime.Now;
                trialNumber++;
                isTrial = true;
            }
        }
    }

    void OutputTrialResults()
    {
        
        
        // dummy variables for testing
        string stimLength = "0";
        string stimAzimuth = "0";
        string stimElevation = "0";

        trialEndTime = System.DateTime.Now;
        trialDuration = trialStartTime - trialEndTime;
        string trialResponses = (stimLength + ',' + stimAzimuth + ',' +
                stimElevation + ',' + trialDuration.ToString());

        
        Debug.Log(trialResponses);

    }

    void CalcObjDisplacement()
    {
        // linear or radial displacement
        // random coinflip selection of radial vs linear
        // if linear:
            // calc displacement
        // if radial
            // calc displacement
        //
    }

}

