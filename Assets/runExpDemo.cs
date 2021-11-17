using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class runExpDemo : MonoBehaviour
{
    /// <summary>
    /// Main class for the Depth Constancy experiment described by Allison and
    /// Wilcox (publication pending.) Code written by Cyan Kuo (2021).
    /// </summary>
    
    // Game models
    public GameObject mdl;
    public int trialNumber;
    public int trialTotal;
    public string participantNo;

    // Object variables
    public GameObject stimObj;
    public GameObject refObj;
    private GameObject selObj = null;

    // Color and mesh size properties
    private Color col;
    private float meshSz;

    // Trial duration calculations
    public System.TimeSpan trialDuration;
    public System.DateTime trialStartTime;
    public System.DateTime trialEndTime;


    public StreamWriter resultStream;

 
    private bool isTrial;
    private string folderName;

    public string trialType;
    public string dispType;

    
    void Start()
    {
        ///<summary>
        /// Start is called before the first frame update and initializes
        /// the environment in preparation for the experimental procedure
        /// </summary>

        // Initialize exp control variables and participant details
        trialNumber = 1;
        trialTotal = 20;
        participantNo = "00";

        // Create folder path, create folder in user directory
        folderName = Application.persistentDataPath + "/results";
        System.IO.Directory.CreateDirectory(folderName);
        

        // Put reference and stimuli into environment
        InstantiateReference();
        InstantiateStimuli();

        // Mesh size for calculating absolute measurements
        meshSz = mdl.GetComponent<MeshFilter>().sharedMesh.bounds.size.z;

        //Get the starting material color for 2AFC selection
        col = stimObj.GetComponent<Renderer>().material.color;

        // Default selected obj for 2AFC
        selObj = refObj;

        // Start trial and get sys time for trial duration record
        trialStartTime = System.DateTime.Now;
        isTrial = true;

        // Coin flip: adjustment or 2AFC
        var rndT = new System.Random();
        if (rndT.NextDouble() >= 0.5) 
        {
            trialType = "2";
        }
        else
        {
            trialType = "A";
        }

    }
    void Update()
    {
        ///<summary>
        /// The procedures in this method run every frame, checks for the
        /// confirmation button event, in which case the trial ends and the 
        /// next begins or the script stops if the total trials have been
        /// reached. If no confirmation is pressed, check for a length
        /// manipulation button event and adjust the length of stimObj
        /// accordingly.
        ///</summary>

        // A trial is in session
        if (isTrial)
        {
            if (trialType == "2")
            {
                // User confirms their manipulation
                if (OVRInput.GetUp(OVRInput.RawButton.X) | OVRInput.GetUp(OVRInput.RawButton.Y) | OVRInput.GetUp(OVRInput.RawButton.A) | OVRInput.GetUp(OVRInput.RawButton.B))
                // (Input.GetKeyDown("space"))
                {
                    OutputTrialResults();
                    isTrial = false;
                    Destroy(stimObj);
                    Destroy(refObj);
                }
                // Toggle
                else if (Input.GetKeyDown("right") | Input.GetKeyDown("left"))
                // OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger) | OVRInput.GetUp(OVRInput.RawButton.SecondaryIndexTrigger);
                {
                    if (selObj == refObj)
                    {
                        stimObj.GetComponent<Renderer>().material.color = new Color(0, 204, 102);
                        refObj.GetComponent<Renderer>().material.color = col;
                        selObj = stimObj;

                    }
                    else if (selObj == stimObj)
                    {

                        refObj.GetComponent<Renderer>().material.color = new Color(0, 204, 102);
                        stimObj.GetComponent<Renderer>().material.color = col;
                        selObj = refObj;

                    }

                }
            }
            if (trialType == "A") 
            {
                // User confirms their manipulation
                if (OVRInput.GetUp(OVRInput.RawButton.X) | OVRInput.GetUp(OVRInput.RawButton.Y) | OVRInput.GetUp(OVRInput.RawButton.A) | OVRInput.GetUp(OVRInput.RawButton.B))
                // (Input.GetKeyDown("space"))
                {
                    OutputTrialResults();
                    isTrial = false;
                    Destroy(stimObj);
                    Destroy(refObj);
                }
                // Scale stim up
                else if (Input.GetKeyDown("up"))
                // OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp)| OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight);
                {
                    AdjLen(stimObj, 0.01f);

                }
                // Scale stim down
                else if (Input.GetKeyDown("down"))
                // OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown)|;
                {
                    AdjLen(stimObj, -0.01f);
                }
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

                var rndT = new System.Random();
                if (rndT.NextDouble() >= 0.5)
                {
                    trialType = "2";
                }
                else
                {
                    trialType = "A";
                }

                var rndD = new System.Random();
                if (rndD.NextDouble() >= 0.5)
                {
                    dispType = "R";
                }
                else 
                {
                    dispType = "S";
                }

                InstantiateReference();
                InstantiateStimuli();
                selObj = refObj;
                isTrial = true;
            }
        }
    }

    void OutputTrialResults()
    {
        ///<summary>
        /// Runs on confirmation event. Output a line to a .csv file with the
        /// results of the trial. Independent variables: refLen - the length of
        /// the reference object, adjLen - the adjusted length of the stimuli
        /// object, azi - the azimuth from reference to stimuli, ele - the
        /// elevation from reference to stimuli, and the trial duration
        /// </summary>

        // dummy variables for testing
        float refLen = AbsoluteSize(refObj);
        //float adjLen = AbsoluteSize(stimObj);
        string sel = null;
        if (selObj == stimObj)
        {
            sel = "eccentric object";
        }
        else if (selObj == refObj)
        {
            sel = "midline object";
        }
        float azi = CalcAzimuth(stimObj,refObj);
        float ele = CalcElevation(stimObj, refObj);

        
        // Find trial duration
        trialEndTime = System.DateTime.Now;
        trialDuration = trialEndTime - trialStartTime;

        // Record trial results, output to file
        string trialResponses = 
           (refLen.ToString() + ',' + azi.ToString() + ',' +
           ele.ToString() + ',' + trialDuration.ToString() + ',' + 
           sel + Environment.NewLine);

        
        //using (StreamWriter resultStream = File.AppendText(
        //    folderName + "/p" + participantNo + "_test.csv"));
        //{
            //Debug.Log(trialResponses);
            resultStream = new StreamWriter( folderName + "/p" + participantNo + "_test.csv", append: true);
            resultStream.Write(trialResponses);
            resultStream.Close();
        //}


    }

    void InstantiateStimuli()
    {
        ///<summary>
        /// Instantiates the stimuli object from model mdl and with random x
        /// and y values for position (left/right, up/down)
        /// </summary>
  
        // Determine whether or not adjustment or 2AFC

        if (dispType == "R")
        {
            // Range of azimuth and elevation values
            float stimX = RandVal(26.5f, -26.5f);
            float stimY = RandVal(45f, -45f);
            float stimZ = 0.8f;

            Debug.Log(stimX + ',' + stimY);

            stimObj = (GameObject)Instantiate(mdl,
            CalcPosGivenAziEle(
                stimX, stimY, stimZ),
                Quaternion.identity
                );

            // Rotate the object at camera for radial
            stimObj.transform.LookAt(Camera.main.transform);
            stimObj.transform.Rotate(180f, 0, 0);
        }
        else if (dispType == "S")
        {
            
            // Range of azimuth and elevation values
            float stimX = RandVal(26.5f, -26.5f);
            float stimY = RandVal(45f, -45f);
            float stimZ = 0.8f;

            Debug.Log(stimX + ',' + stimY);

            stimObj = (GameObject)Instantiate(mdl, 
                CalcPosGivenAziEle(
                stimX, stimY, stimZ), 
                Quaternion.identity
                );

            // Sets the z distance for straight displacement
            stimObj.transform.position = new Vector3(
                stimObj.transform.position.x, stimObj.transform.position.y,
                500.0f + stimZ
                );
        }

    }

    void InstantiateReference()
    {
        ///<summary>
        /// Instantiates the reference object from model mdl and with random
        /// length, and fixed position in front of, and in line with the
        /// camera.
        /// </summary>
        
        // Reference object is fixed, but changes length
        refObj = Instantiate(mdl, new Vector3(500f, 1.5f, 500.8f),
                    Quaternion.identity);
        refObj.transform.localScale = new Vector3(1, 1,
            RandVal(0.5f, 1.5f));

    }

    float RandVal(float max, float min) 
    {
        ///<summary>
        /// Calculate random float given range max and min
        ///</summary>
        
        System.Random random = new System.Random();
        double val = (random.NextDouble() * (max - min) + min);
        return (float) val;
    }

    float AbsoluteSize(GameObject go)
    {
        ///<summary>
        /// Obtain the absolute size of the unity game object go by multiplying
        /// the model's mesh size by the scale factor
        ///</summary>
        
        var trScl = go.transform.localScale.z;
        return meshSz * trScl;
    }

    void AdjLen(GameObject go, float adj)
    {
        ///<summary> 
        /// Adjust the length of game object go by adj.
        /// Given that absolute size is given by mesh bounds * local scale
        /// Adding a 1m in Unity coordinates to the size of an object is
        /// 1m/mesh bounds + to the local scale
        /// </summary>
        
        float meshSz = go.GetComponent<MeshFilter>().mesh.bounds.size.z;
        var trScl = go.transform.localScale.z;
        // change its local scale
        go.transform.localScale = new Vector3(1, 1, trScl+adj/meshSz);
    }

    float CalcAzimuth(GameObject go1, GameObject go2) 
    {
        ///<summary>
        /// Given two game objects, calculate their azimuth
        /// </summary>
        
        // get x,z coordinates of objects
        var vec1 = new Vector2(go1.transform.position.x, go1.transform.position.z);
        var vec2 = new Vector2(go2.transform.position.x, go2.transform.position.z);

        // Get camera offset along the same axes
        var vecCam = new Vector2
            (Camera.main.transform.position.x, Camera.main.transform.position.z);
        // calc angle, removing offset from camera
        return Vector2.SignedAngle(vec1-vecCam,vec2-vecCam);
        
    }

    float CalcElevation(GameObject go1, GameObject go2)
    {
        ///<summary>
        /// Given two game objects, calculate their elevation
        /// </summary>
        
        // get x,y coordinates of objects
        var vec1 = new Vector2(go1.transform.position.y, go1.transform.position.z);
        var vec2 = new Vector2(go2.transform.position.y, go2.transform.position.z);
        // get camera offset along the same axes
        var vecCam = new Vector2
            (Camera.main.transform.position.y, Camera.main.transform.position.z);
        // calc angle, removing offset from camera
        return Vector2.SignedAngle(vec1 - vecCam, vec2 - vecCam);

    }

    Vector3 CalcPosGivenAziEle(float val1, float val2, float dis)
    {
        ///<summary>
        /// Given elevation and azimuth, calculate its position in Unity coordinates
        ///</summary>

        // Calculate the x,y rotation of object
        Quaternion rotX = Quaternion.AngleAxis(
            Camera.main.transform.eulerAngles.x + val2, Vector3.left
            );
        Quaternion rotY = Quaternion.AngleAxis(
            Camera.main.transform.eulerAngles.y + val1, Vector3.up
            );
        // Incorporate x,y rotation and magnitude to find location
        Vector3 pos = rotX * rotY * Vector3.forward * dis;
        return pos + Camera.main.transform.position;
    }



}

