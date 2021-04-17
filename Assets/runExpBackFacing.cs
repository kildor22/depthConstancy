using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class runExpBackFacing : MonoBehaviour
{
    /// <summary>
    /// Main class for the Depth Constancy experiment described by Allison and
    /// Wilcox (publication pending.) Code written by Cyan Kuo (2021).
    /// </summary>

    public GameObject mdl;
    public int trialNumber;
    public int trialTotal;
    public string participantNo;

    public GameObject stimObj;
    public GameObject refObj;

    // Trial duration calculations
    public System.TimeSpan trialDuration;
    public System.DateTime trialStartTime;
    public System.DateTime trialEndTime;
    public StreamWriter resultStream;

    private float meshSz;
    private bool isTrial;
    private string folderName;
    


    void Start()
    {
        ///<summary>
        /// Start is called before the first frame update and initializes
        /// the environment in preparation for the experimental procedure
        /// </summary>
        /// 

        //private System.Windows.Forms.OpenFileDialog openFileDialog1;

        string sourceDirectory = "C:\\";


    // Initialize exp control variables and participant details
        trialNumber = 1;
        trialTotal = 5;
        participantNo = "00";

        // Create folder path, create folder in user director
        folderName = Application.persistentDataPath + "/results";
        System.IO.Directory.CreateDirectory(folderName);
        Debug.Log("Path to my file: {0}\n" + folderName);


        // Put reference and stimuli into environment
        InstantiateReference();
        InstantiateStimuli();

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
        /// reached. If no confirmation is pressed, check for a length
        /// manipulation button event and adjust the length of stimObj
        /// accordingly.
        ///</summary>

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
                InstantiateReference();
                InstantiateStimuli();
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
        float adjLen = AbsoluteSize(stimObj);
        float azi = CalcAzimuth(stimObj, refObj);
        float ele = CalcElevation(stimObj, refObj);
        //Debug.Log(azi);
        //Debug.Log(ele);

        // Find trial duration
        trialEndTime = System.DateTime.Now;
        trialDuration = trialEndTime - trialStartTime;

        // Record trial results, output to file
        string trialResponses =
           (refLen.ToString() + ',' + azi.ToString() + ',' +
           ele.ToString() + ',' + trialDuration.ToString() + ',' +
           adjLen.ToString() + Environment.NewLine);


        //using (StreamWriter resultStream = File.AppendText(
        //    folderName + "/p" + participantNo + "_test.csv"));
        //{
        Debug.Log(trialResponses);
        resultStream = new StreamWriter(folderName + "/p" + participantNo + "_test.csv", append: true);
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

        // Range of azimuth and elevation values
        float stimX = RandVal(500.5f, 499.3f);
        float stimY = RandVal(0.95f, 1.78f);

        stimObj = (GameObject)Instantiate(mdl,
           new Vector3(stimX, stimY, 500.8f), Quaternion.Euler(180.0f,0,0));

    }

    void InstantiateReference()
    {
        ///<summary>
        /// Instantiates the reference object from model mdl and with random
        /// length, and fixed position in front of, and in line with the
        /// camera.
        /// </summary>

        // Reference object is fixed, but changes length
        refObj = Instantiate(mdl, new Vector3(500f, 1.36144f, 500.8f),
                    Quaternion.Euler(180.0f, 0, 0));
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
        return (float)val;
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
        go.transform.localScale = new Vector3(1, 1, trScl + adj / meshSz);
    }

    float CalcAzimuth(GameObject go1, GameObject go2)
    {
        ///<summary>
        /// Given two game objects, calculate their azimuth
        /// </summary>

        // get x,z coordinates of objects
        var vec1 = new Vector2(go1.transform.position.x, go1.transform.position.z);
        var vec2 = new Vector2(go2.transform.position.x, go2.transform.position.z);
        Debug.Log(vec1);
        Debug.Log(vec2);
        // get camera offset along the same axes
        var vecCam = new Vector2
            (Camera.main.transform.position.x, Camera.main.transform.position.z);
        // calc angle, removing offset from camera
        return Vector2.SignedAngle(vec1 - vecCam, vec2 - vecCam);

    }

    float CalcElevation(GameObject go1, GameObject go2)
    {
        ///<summary>
        /// Given two game objects, calculate their elevation
        /// </summary>

        // get x,y coordinates of objects
        var vec1 = new Vector2(go1.transform.position.y, go1.transform.position.z);
        var vec2 = new Vector2(go2.transform.position.y, go2.transform.position.z);
        Debug.Log(vec1);
        Debug.Log(vec2);
        // get camera offset along the same axes
        var vecCam = new Vector2
            (Camera.main.transform.position.y, Camera.main.transform.position.z);
        // calc angle, removing offset from camera
        return Vector2.SignedAngle(vec1 - vecCam, vec2 - vecCam);

    }
}

