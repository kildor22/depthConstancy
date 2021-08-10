using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class runExp : MonoBehaviour
{
    /// <summary>
    /// Main class for the Depth Constancy experiment described by Allison and
    /// Wilcox (publication pending.) Code written by Cyan Kuo (2021).
    /// </summary>

    // Game models
    public GameObject mdl;
    public int trialNumber; // from older version, might come in useful
    public string participantNo;

    public GameObject stimObj;
    public GameObject refObj;
    private GameObject selObj = null;

    // Color and mesh size properties
    private Color col;
    private float meshSz;

    // Trial duration calculations
    private System.TimeSpan trialDuration;
    private System.DateTime trialStartTime;
    private System.DateTime trialEndTime;


    // Read/write parameters and results
    private string folderName;
    protected StreamWriter resultStream;
    protected StreamReader trialReader = null;
    protected string text = " "; // allow first line to be read below
    protected string[] conds = null;

    private bool isTrial;
    
    


    void Start()
    {
        ///<summary>
        /// Start is called before the first frame update and initializes
        /// the environment in preparation for the experimental procedure
        /// </summary>
        /// 

        // Read in trial conditions from file, potential error reading in

        try
        {
            FileInfo sourceFile = new FileInfo(Application.dataPath + "/Resources/test_ang.csv");
            trialReader = sourceFile.OpenText();

        }
        catch(Exception e)
        {
            Debug.Log("Cannot open file");
            Debug.Log(e);

        }
        text = trialReader.ReadLine();
        conds = text.Split(',');
        participantNo = conds[6];        
        text = trialReader.ReadLine();
        conds = text.Split(',');


        // Initialize exp control variables and participant details
        trialNumber = 1;
        participantNo = "00";

        // Create folder path, create folder in user directory
        folderName = Application.persistentDataPath + "/results";
        System.IO.Directory.CreateDirectory(folderName);


        // Put reference and stimuli into environment
        InstantiateReference(conds[4], conds[5]);
        InstantiateStimuli(conds[2], conds[3], conds[5]);

        // Get the starting material for objs to use later for 2AFC selection
        col = stimObj.GetComponent<Renderer>().material.color;

        // Default selected object for 2 AFC
        selObj = refObj;

        // Mesh size for calculating absolute measurements
        meshSz = stimObj.GetComponent<MeshFilter>().mesh.bounds.size.z;

        // Start trial and get sys time for trial duration record
        trialStartTime = System.DateTime.Now;
        isTrial = true;

    }


    void Update()
    {
        ///<summary>
        /// The procedures in this method run every frame, checks for the
        /// confirmation button event, in which case the trial ends and the 
        /// next begins or the script stops if the total trials have been
        /// reached. 
        /// In the case of an adjustment trial, if no confirmation is pressed, 
        /// check for a length manipulation button event and adjust the length
        /// of stimObj accordingly.
        /// In the case of a 2AFC trial, if no confirmation is pressed, check
        /// for a change in selection. Highlight the selected object in a
        /// chosen colour.
        ///</summary>

        // A trial is in session
        if (isTrial)
        {
            if (conds[0] == "A")
            {
                // User confirms their manipulation
                if (Input.GetKeyDown("space"))
                {
                    OutputTrialResults();
                    isTrial = false;
                    Destroy(stimObj);
                    Destroy(refObj);
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
            if (conds[0] == "2")
            {
                // User confirms their manipulation
                if (Input.GetKeyDown("space"))
                {
                    OutputTrialResults();
                    isTrial = false;
                    Destroy(stimObj);
                    Destroy(refObj);
                }
                // Toggle between objects
                else if (Input.GetKeyDown("right") | Input.GetKeyDown("left"))
                {
                    // Highlight selected object
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
        }
        else
        {
            // Total number of trials completed, quit
            if (trialReader.EndOfStream)
            {
                Application.Quit();
            }
            // Initialize new trial
            else
            {
                Destroy(stimObj);
                Destroy(refObj);
                trialStartTime = System.DateTime.Now;
                text = trialReader.ReadLine();
                conds = text.Split(',');
                trialNumber++;
                InstantiateReference(conds[4], conds[5]);
                InstantiateStimuli(conds[2], conds[3], conds[5]);
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
        /// elevation from reference to stimuli, and the trial duration, sel - 
        /// the currently selected object
        /// </summary>

        string trialResponses = string.Empty;
        double refLen = Math.Round(AbsoluteSize(refObj),3);
        double adjLen = Math.Round(AbsoluteSize(stimObj),3);
        double azi = Math.Round(CalcAzimuth(stimObj, refObj),3);
        double ele = Math.Round(CalcElevation(stimObj, refObj),3);

        // Find trial duration
        trialEndTime = System.DateTime.Now;
        trialDuration = trialEndTime - trialStartTime;

        // Change output string depending on whether the trial is adjustment
        // or 2AFC
        if (conds[0] == "A")
        {
            trialResponses =
               (refLen.ToString() + ',' + azi.ToString() + ',' +
               ele.ToString() + ',' + trialDuration.ToString() + ',' +
               adjLen.ToString() + Environment.NewLine);
        }
        else if (conds[0] == "2")
        {
            // Check which object is currently selected
            string sel = "null";
            if (selObj == stimObj)
            {
                sel = "eccentric object";
            }
            else if (selObj == refObj)
            {
                sel = "midline object";
            }
            trialResponses =
            (refLen.ToString() + ',' + azi.ToString() + ',' +
            ele.ToString() + ',' + trialDuration.ToString() + ',' +
            adjLen.ToString() + ',' + sel + Environment.NewLine);
        }

        // If there are any problems with the output file
        try 
        { 
        resultStream = new StreamWriter
            (
            folderName + "/p" + participantNo + "_results.csv", append: true
            );
        resultStream.Write(trialResponses);
        resultStream.Close();
        }
        catch(Exception e)
        {
            Debug.Log("Cannot write to file or generate results");
            Debug.Log(e);
        }

    }

    void InstantiateStimuli(string val1, string val2, string val3)
    {
        ///<summary>
        /// Instantiates the stimuli object from model mdl and with x and y 
        /// values for position (left/right, up/down) and, in the case of 
        /// radial displaecment trial parameter, rotation, too
        /// </summary>

        if (conds[1] == "R")
        {
            stimObj = (GameObject)Instantiate(mdl,
            CalcPosGivenAziEle(
                float.Parse(val1), float.Parse(val2), float.Parse(val3)), 
                Quaternion.identity
                );

            // Rotate the object at camera for radial
            stimObj.transform.LookAt(Camera.main.transform);
            stimObj.transform.Rotate(180f, 0, 0);
        }
        if (conds[1] == "S")
        {
            stimObj = (GameObject)Instantiate(mdl,
            CalcPosGivenAziEle(
                float.Parse(val1), float.Parse(val2), float.Parse(val3)), 
                Quaternion.identity
                );

            // Sets the z distance for straight displacement
            stimObj.transform.position = new Vector3(
                stimObj.transform.position.x, stimObj.transform.position.y,
                500.0f + float.Parse(val3)
                );
        }
    }


    void InstantiateReference(string len, string dist)
    {
        ///<summary>
        /// Instantiates the reference object from model mdl and with random
        /// length, and fixed position in front of, and in line with the
        /// camera.
        /// </summary>

        // Reference object is fixed, but changes length
        float val = float.Parse(len);
        refObj = Instantiate(mdl, new Vector3(500f, 1.6f, 
            (500.0f + float.Parse(dist))), Quaternion.identity);
        refObj.transform.localScale = new Vector3(1, 1, val);

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
        // Change local scale
        go.transform.localScale = new Vector3(1, 1, trScl + adj / meshSz);
    }

    float CalcAzimuth(GameObject go1, GameObject go2)
    {
        ///<summary>
        /// Given two game objects, calculate their azimuth
        /// </summary>

        // Get x,z coordinates of objects
        var vec1 = new Vector2(go1.transform.position.x, go1.transform.position.z);
        var vec2 = new Vector2(go2.transform.position.x, go2.transform.position.z);
        // Get camera offset along the same axes
        var vecCam = new Vector2
            (Camera.main.transform.position.x, Camera.main.transform.position.z);
        // Calc angle, removing offset from camera
        return Vector2.SignedAngle(vec1 - vecCam, vec2 - vecCam);

    }

    float CalcElevation(GameObject go1, GameObject go2)
    {
        ///<summary>
        /// Given two game objects, calculate their elevation
        /// </summary>

        // Get x,y coordinates of objects
        var vec1 = new Vector2(go1.transform.position.z, go1.transform.position.y);
        var vec2 = new Vector2(go2.transform.position.z, go2.transform.position.y);
        var vecCam = new Vector2
            (Camera.main.transform.position.x, Camera.main.transform.position.y);
        // Calc angle, removing offset from camera
        return Vector2.SignedAngle(vec2 - vecCam, vec1 - vecCam);

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
        Vector3 pos = rotX*rotY * Vector3.forward * dis;
        return pos+Camera.main.transform.position;
    }

    

}

