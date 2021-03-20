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
    public GameObject refObj;
    public float adjLength;

    // Trial duration calculations
    public System.TimeSpan trialDuration;
    public System.DateTime trialStartTime;
    public System.DateTime trialEndTime;
    public StringWriter resultStream;

    public float meshSz;

    private float stimAzi;
    private float stimEle;

    public bool isTrial;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize exp control variables
        trialNumber = 1;
        trialTotal = 5;

        // Set participant number
        participantNo = "00";

        //Put reference into environment
        InstantiateReference();

        // Put stimuli into environment
        InstantiateStimuli();
        float meshSz = stimObj.GetComponent<MeshFilter>().mesh.bounds.size.z;

        // Start trial and get sys time for trial duration record
        trialStartTime = System.DateTime.Now;
        isTrial = true;
        
    }
    void Update()
    {
        // A trial is in session
        if (isTrial)
        {
            // User confirms their manipulation
            if (Input.GetKeyDown("space"))
            {
                OutputTrialResults();
                isTrial = false;
                Destroy(stimObj);
            }
            // Scale stim up
            else if (Input.GetKeyDown("up"))
            {
                AdjLen(stimObj, 0.01f);
                
            }
            // Scale stim down
            else if (Input.GetKeyDown("down"))
            {
                AdjLen(stimObj, -0.01f);
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
                InstantiateReference();
                InstantiateStimuli();
            }
        }
    }

    void OutputTrialResults()
    {
        // dummy variables for testing
        float refLen = AbsoluteSize(refObj);
        Debug.Log(refLen);
        float adjLen = AbsoluteSize(stimObj);
        Debug.Log(adjLen);
        
        // Find trial duration
        trialEndTime = System.DateTime.Now;
        trialDuration = trialEndTime - trialStartTime;

        // Record trial results, output to file
        string trialResponses = 
            (refLen.ToString() + ',' + stimAzi.ToString() + ',' +
            stimEle.ToString() + ',' + trialDuration.ToString() + ',' + 
            adjLen.ToString() + Environment.NewLine);

        using (StreamWriter resultStream = File.AppendText(Application.dataPath +
            "/results/p_" + participantNo + ".txt"))
        {
            resultStream.Write(trialResponses);
        }


    }

    void InstantiateStimuli()
    {
        // Range of azimuth and elevation values
        stimAzi = RandVal(500.5f, 499.3f);
        stimEle = RandVal(0.95f, 1.78f);

        // TODO: change these into angles rather than point-coordinates
        stimObj = (GameObject)Instantiate(mdl, 
            new Vector3(stimAzi, stimEle, 500.8f), Quaternion.identity);
        
    }

    void InstantiateReference()
    {
        refObj = Instantiate(mdl, new Vector3(500f, 1.36144f, 500.8f),
                    Quaternion.identity);
        refObj.transform.localScale = new Vector3(1, 1,
            RandVal(0.5f, 1.5f));

    }

    float RandVal(float max, float min) 
    {
        // Calculate random given range
        System.Random random = new System.Random();
        double val = (random.NextDouble() * (max - min) + min);
        return (float) val;
    }

    float AbsoluteSize(GameObject go)
    {
        //TODO: Debug
        // Calculate absolute length of object
        float meshSz = go.GetComponent<MeshFilter>().mesh.bounds.size.z;
        var trScl = go.transform.localScale.z;
        return meshSz * trScl;
    }

    void AdjLen(GameObject go, float adj)
    {
        // Adjust the length of game object go by adj
        // Given that absolute size is given by mesh bounds * local scale
        // Adding a 1m in Unity coordinates to the size of an object is
        // 1m/mesh bounds + to the local scale

        float meshSz = go.GetComponent<MeshFilter>().mesh.bounds.size.z;
        var trScl = go.transform.localScale.z;
        // change its local scale
        go.transform.localScale = new Vector3(1, 1, trScl+adj/meshSz);
    }


}

