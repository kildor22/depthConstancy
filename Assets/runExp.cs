using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class runExp : MonoBehaviour
{
    public GameObject mdl;
    public int trialNumber;
    public int trialTotal;
    public string participantNo;

    public GameObject stimObj;
    public int adjLength;

    // Trial duration calculations
    public System.TimeSpan trialDuration;
    public System.DateTime trialStartTime;
    public System.DateTime trialEndTime;
    public StringWriter resultStream;

    private float stimAzi;
    private float stimEle;

    public bool isTrial;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize exp control variables
        Debug.Log("I am alive!");
        trialNumber = 1;
        trialTotal = 5;

        // Set participant number
        participantNo = "00";

        //Put reference into environment
        //GameObject refObj = Instantiate(mdl2, new Vector3(500f, 1.36144f, 500.8f), 
            //Quaternion.identity);

        // Put stimuli into environment
        InstantiateStimuli();

        // Start trial and get sys time for trial duration record
        trialStartTime = System.DateTime.Now;
        isTrial = true;

        Debug.Log(AbsSz(stimObj));
        
    }
    void Update()
    {
        // A trial is in session
        if (isTrial)
        {
            // User confirms their manipulation
            if (Input.GetKeyDown("space"))
            {
                Debug.Log("space pressed");
                OutputTrialResults();
                isTrial = false;
                Destroy(stimObj);
            }
            // Scale stim up
            else if (Input.GetKeyDown("up"))
            {
                Debug.Log("up pressed");
                adjLength ++;
            }
            // Scale stim down
            else if (Input.GetKeyDown("down"))
            {
                Debug.Log("down pressed");
                adjLength --;
            }
        }
        else
       { 
            // Total number of trials completed, quit
            if (trialNumber == trialTotal)
            {
                Application.Quit();
            }
            // Initialize new trial
            else
            {
                trialStartTime = System.DateTime.Now;
                trialNumber++;
                
                isTrial = true;
                adjLength = 0;
                InstantiateStimuli();
            }
        }
    }

    void OutputTrialResults()
    {
        // dummy variables for testing
        float stimLen = AbsSz(stimObj);
        //Debug.Log(stimLen);
        
        // Find trial duration
        trialEndTime = System.DateTime.Now;
        trialDuration = trialStartTime - trialEndTime;

        // Record trial results, output to file
        string trialResponses = (stimLen + ',' + stimAzi.ToString() + ',' +
            stimEle.ToString() + ',' + trialDuration.ToString() + ',' + 
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
        // Range of azimuth and elevation values
        stimAzi = RandVal(500.5f, 499.3f);
        stimEle = RandVal(0.95f, 1.78f);

        stimObj = (GameObject)Instantiate(mdl, new Vector3(stimAzi, stimEle, 500.8f),
           Quaternion.identity);
        
    }

    float RandVal(float max, float min) 
    {
        // Calculate random given range
        System.Random random = new System.Random();
        double val = (random.NextDouble() * (max - min) + min);
        return (float) val;
    }

    float AbsSz(GameObject go)
    {
        //TODO: Debug
        // Calculate absolute length of object
        var meshSz = go.GetComponent<MeshFilter>().mesh.bounds.size.z;
        var trScl = go.transform.localScale.z;
        return meshSz * trScl;
    }


}

