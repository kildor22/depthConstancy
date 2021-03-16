using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class runExp : MonoBehaviour
{
    public GameObject stimObj;
    public int trialNumber;
    public int trialTotal;
    public string participantNo;

    public int adjLength;

    public System.TimeSpan trialDuration;
    public System.DateTime trialStartTime;
    public System.DateTime trialEndTime;
    public StringWriter resultStream;
// Set a variable to the Documents path.


    public bool isTrial;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("I am alive!");
        trialNumber = 1;
        trialTotal = 5;
        participantNo = "00";
        isTrial = true;
        trialStartTime = System.DateTime.Now;
        InstantiateStimuli();
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
                adjLength ++;
            }
            else if (Input.GetKeyDown("down"))
            {
                Debug.Log("down pressed");
                adjLength --;
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
                adjLength = 0;
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
            stimElevation + ',' + trialDuration.ToString() + ',' + 
            adjLength.ToString() + Environment.NewLine);

        using (StreamWriter resultStream = File.AppendText(Application.dataPath +
            "/results/p_" + participantNo + ".txt"))
        {
            resultStream.Write(trialResponses);
        }

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

    void InstantiateStimuli()
    {
        Instantiate(stimObj, new Vector3(500f, 1.36144f, 500.8f),
            Quaternion.identity);
        Instantiate(stimObj, new Vector3(499.3f, 1.36144f, 500.8f),
            Quaternion.identity);
    }

}

