using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.XR;
#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
#endif

public class runExp : MonoBehaviour
{
    /// <summary>
    /// Main class for the Depth Constancy experiment described by Allison and
    /// Wilcox (publication pending.) Code written by Cyan Kuo (2021).
    /// </summary>

    // Game models
    public GameObject mdl = null;
    public GameObject startMenu;
    public GameObject textStartMenu;
    public Material cyanMaterial;
    public SceneSettings sceneSettings;
    public int trialNumber; // from older version, might come in useful
    public Transform camTransform;

    public bool isStarted = false;
    private bool display = true;
    [HideInInspector] public GameObject stimObj;
    [HideInInspector] public GameObject refObj;
    private GameObject selObj = null;

    // Color and mesh size properties
    private Material mat; //highlight color for 2AFC (Default: cyan)
    private float meshSz;

    // Trial duration calculations
    private System.TimeSpan trialDuration;
    private System.DateTime trialStartTime;
    private System.DateTime trialEndTime;


    // Read/write parameters and results
    public CsvManager csv;
    protected StreamWriter resultStream;
    protected StreamReader trialReader = null;
    protected string text = " "; // allow first line to be read below
    protected string[] conds = null;

    private bool isTrial;
    [HideInInspector]
    public bool infoSubmitted = false; //if user has submitted info

    //keyboard controls
    private KeyboardInputActions kbActions;
    private InputAction submitKB;
    private InputAction selectKB;
    private InputAction upKB;
    private InputAction downKB;

    //VR controls
    private XRInputActions xrActions;
    private InputAction submitXR;
    private InputAction selectXR;
    private InputAction upXR;
    private InputAction downXR;
    
    //Conditions:
    /*Trial Type, Displacement Type, Eccentric Object Azimuth, Eccentric Object Elevation, Eccentric Object Length, 
    Reference Object Azimuth, Reference Object Elevation, Reference Object Length,
     Stimuli Distance, UserID
    */
    void Awake()
    {
        kbActions = new KeyboardInputActions();
        xrActions = new XRInputActions();

        // // Create folder path, create folder in user director
        // folderName = Application.persistentDataPath + "/Results";
        // System.IO.Directory.CreateDirectory(folderName);

        // //initialize file path to write output csv
        // filePath = folderName + "/p" + participantID + "_group" + participantGroup + "_session" + 
        // participantSession+ "_results.csv";
    }


    void OnEnable()
    {
        selectKB = kbActions.Player.Select; //left or right key
        selectKB.Enable();

        submitKB = kbActions.Player.Submit; //spacebar
        submitKB.Enable();

        upKB = kbActions.Player.ScaleUp; //up key
        upKB.Enable();

        downKB = kbActions.Player.ScaleDown; //down key
        downKB.Enable();

        submitXR = xrActions.Controllers.Submit;
        submitXR.Enable();

        selectXR = xrActions.Controllers.Select;
        selectXR.Enable();

        upXR = xrActions.Controllers.ScaleUp;
        upXR.Enable();

        downXR = xrActions.Controllers.ScaleDown;
        downXR.Enable();

    }
    void OnDisable()
    {
        selectKB.Disable();
        submitKB.Disable();
        upKB.Disable();
        downKB.Disable();
        submitXR.Disable();
        selectXR.Disable();
        upXR.Disable();
        downXR.Disable();
    }


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
            FileInfo sourceFile = new FileInfo(Application.dataPath +
                "/Input Files/input.csv");
            trialReader = sourceFile.OpenText();

        }
        catch(Exception e)
        {
            Debug.Log("Cannot open file");
            Debug.Log(e);
        }
        //read header once
        text = trialReader.ReadLine();
        conds = text.Split(',');
        // participantID = conds[6].Replace("Particpant ID: ", "");
        // participantGroup = conds[7].Replace("Group: ", "");
        // participantSession = conds[8].Replace("Session: ", "");


        //read for first command
        text = trialReader.ReadLine();
        conds = text.Split(','); //split text returns array of substrings of row


        // Initialize exp control variables and participant details
        trialNumber = 1;
        

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

        if (submitXR.triggered) Debug.Log("Trigger pressed");
        if (selectXR.triggered) Debug.Log("Select pressed");

        if (!isStarted)
        {
           if (startMenu.activeSelf && submitXR.triggered)
           {
            startMenu.SetActive(false);
            sceneSettings.UseDefaultSkybox = true; //once started, load default skybox

            // Put reference and stimuli into environment
            InstantiateReference(conds[7], conds[8], conds[5], conds[6]);
            InstantiateStimuli(conds[2], conds[3], conds[8], conds[4]);

            // Get the starting material for objs to use later for 2AFC selection
            mat = stimObj.GetComponent<Renderer>().material;

            // Default selected object for 2 AFC
            selObj = refObj;

            // Mesh size for calculating absolute measurements
            meshSz = stimObj.GetComponent<MeshFilter>().mesh.bounds.size.z;

            // Start trial and get sys time for trial duration record
            trialStartTime = System.DateTime.Now; 
            isStarted = true;
            isTrial = true;
            Debug.Log("Trial " + trialNumber + " | " + conds[0] + " | " + "Displacement: " + conds[1]);
           }
           else
           {
                if (infoSubmitted)
                {
                    startMenu.SetActive(true);
                }
           }

        }
        else
        {
            // A trial is in session
            if (isTrial)
            {
                if (display)
                {
                    Debug.Log("EccObj Azi & Ele: " + conds[2] + ", " + conds[3]);
                    display = false;
                } 
                if (conds[0] == "Adjustment")
                {
                    TrialAdjustment();
                }
                if (conds[0] == "2AFC")
                {
                    Trial2AFC();
                }
            }
            else
            {
                // Total number of trials completed, quit
                if (trialReader.EndOfStream)
                {
                    // startMenu.SetActive(true);
                    // textStartMenu.GetComponent<Text>().text = "Test over";
                    // sceneSettings.UseDefaultSkybox = false; //once started, load default skybox
                    Application.Quit();
                }
                // Initialize new trial
                else
                {
                    display = true;
                    NextTrial();
                }
            }
        }

    }

    void TrialAdjustment()
    {
        // User confirms their manipulation
        if (submitKB.triggered || submitXR.triggered)
        {
            OutputTrialResults();
            isTrial = false;
            Destroy(stimObj);
            Destroy(refObj);
        }
        // Scale stim up
        else if (upKB.triggered || upXR.triggered)
        {
            AdjLen(stimObj, 0.01f);

        }
        // Scale stim down
        else if (downKB.triggered || downXR.triggered)
        {
            AdjLen(stimObj, -0.01f);
        }
    }

    void Trial2AFC()
    {
        // User confirms their manipulation
        if (submitKB.triggered || submitXR.triggered)
        {
            OutputTrialResults();
            isTrial = false;
            Destroy(stimObj);
            Destroy(refObj);
        }
        // Toggle between objects
        else if (selectKB.triggered || selectXR.triggered)
        {
            // Highlight selected object
            if (selObj == refObj)
            {
                stimObj.GetComponent<Renderer>().material = cyanMaterial;
                refObj.GetComponent<Renderer>().material= mat;
                selObj = stimObj;
            }
            else if (selObj == stimObj)
            {
                // refObj.GetComponent<Renderer>().material.color = new Color(0, 204, 102);
                refObj.GetComponent<Renderer>().material= cyanMaterial;
                stimObj.GetComponent<Renderer>().material = mat;
                selObj = refObj;

            }
        }
    }

    void NextTrial()
    {
        trialStartTime = System.DateTime.Now; //get current date and time

        //read next line
        text = trialReader.ReadLine();
        conds = text.Split(',');


        trialNumber++;

        InstantiateReference(conds[7], conds[8], conds[5], conds[6]);
        InstantiateStimuli(conds[2], conds[3], conds[8], conds[4]);
        selObj = refObj;
        isTrial = true;
        Debug.Log("Trial " + trialNumber + " | " + conds[0] + " | " + "Displacement: " + conds[1]);
        Debug.Log("Local scale: " + meshSz * float.Parse(conds[8]));
    }

    //BUG: When endofline is reached, close file
    void OutputTrialResults()
    {
        ///<summary>
        /* Runs on confirmation event. Output a line to a .csv file with the
        results of the trial. 
        Independent variables: 
        refLen - the length of the reference object, 
        adjLen - the adjusted length of the stimuli object, 
        azi - the azimuth from reference to stimuli, 
        ele - the elevation from reference to stimuli, 
        the trial duration 
        sel - the currently selected object */
        /// </summary>

        string trialResponses = string.Empty;
        double refLen = Math.Round(AbsoluteSize(refObj),3); //are length and absolute size the same?
        double adjLen = Math.Round(AbsoluteSize(stimObj),3);
        double azi = Math.Round(CalcAzimuth(stimObj, refObj),3);
        double ele = Math.Round(CalcElevation(stimObj, refObj),3);
        // Find trial duration
        trialEndTime = System.DateTime.Now;
        trialDuration = trialEndTime - trialStartTime;

        // Change output string depending on whether the trial is adjustment
        // or 2AFC
        if (conds[0] == "Adjustment")
        {
            trialResponses =
               ("Adjustment" + ',' + refLen.ToString() + ',' + azi.ToString() + ',' +
               ele.ToString() + ',' + trialDuration.ToString() + ',' +
               adjLen.ToString());
        }
        else if (conds[0] == "2AFC")
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
            ("2AFC" + ',' +refLen.ToString() + ',' + azi.ToString() + ',' +
            ele.ToString() + ',' + trialDuration.ToString() + ',' +
            adjLen.ToString() + ',' + sel);
        }
        csv.WriteRow(filePath, true, trialResponses); //append trial result to initialized output CSV
    }

    void InstantiateStimuli(string azimuth, string elevation, string distance)
    {
        ///<summary>
        /// Instantiates the stimuli object from model mdl and with x and y 
        /// values for position (left/right, up/down) and, in the case of 
        /// radial displaecment trial parameter, rotation, too
        /// </summary>

        if (conds[1] == "Radial")
        {
            stimObj = (GameObject)Instantiate(mdl,
            CalcPosGivenAziEle(float.Parse(azimuth), float.Parse(elevation), float.Parse(distance)), 
            Quaternion.identity, camTransform);

            // Rotate the object at camera for radial
            stimObj.transform.LookAt(Camera.main.transform);
            stimObj.transform.Rotate(180f, 0, 0);
        }
        if (conds[1] == "Straight")
        {
            stimObj = (GameObject)Instantiate(mdl,
            CalcPosGivenAziEle(float.Parse(azimuth), float.Parse(elevation), float.Parse(distance)), 
            Quaternion.identity, camTransform
                );

            // Sets the z distance for straight displacement
            stimObj.transform.position = new Vector3(
                stimObj.transform.position.x, stimObj.transform.position.y,
                500.0f + float.Parse(distance)
                );
        }
    }
    //Overload of InstantiateStimuli with string param "length" to customize length of stimuli object
    void InstantiateStimuli(string azimuth, string elevation, string distance, string length)
    {
        ///<summary>
        /// Instantiates the stimuli object from model mdl and with x and y 
        /// values for position (left/right, up/down) and, in the case of 
        /// radial displaecment trial parameter, rotation, too
        /// </summary>

        if (conds[1] == "Radial")
        {
            stimObj = (GameObject)Instantiate(mdl,
            CalcPosGivenAziEle(float.Parse(azimuth), float.Parse(elevation), float.Parse(distance)), 
            Quaternion.identity, camTransform);

            // Rotate the object at camera for radial
            stimObj.transform.LookAt(Camera.main.transform);
            stimObj.transform.Rotate(180f, 0, 0);
        }
        if (conds[1] == "Straight")
        {
            stimObj = (GameObject)Instantiate(mdl,
            CalcPosGivenAziEle(float.Parse(azimuth), float.Parse(elevation), float.Parse(distance)), 
            Quaternion.identity, camTransform
                );

            // Sets the z distance for straight displacement
            stimObj.transform.position = new Vector3(
                stimObj.transform.position.x, stimObj.transform.position.y,
                500.0f + float.Parse(distance)
                );
        }
        stimObj.transform.localScale = new Vector3(1, 1, float.Parse(length)); //change stim obj length along z axis
    }

    void InstantiateReference(string len, string dist)
    {
        ///<summary>
        /// Instantiates the reference object from model mdl and with defined
        /// length, and fixed position in front of, and in line with the
        /// camera.
        /// </summary>

        // Reference object is fixed, but changes length
        float val = float.Parse(len);
        refObj = Instantiate(mdl, new Vector3(500f, 1.6f, (500.0f + float.Parse(dist))), Quaternion.identity, camTransform);
        
        refObj.transform.localScale = new Vector3(1, 1, val);

    }
    // Overload of function InstantiateReference with params "azimuth" and "elevation" to change reference obj position in 3D space
    void InstantiateReference(string len, string distance, string azimuth, string elevation)
    {
        ///<summary>
        /// Instantiates the reference object from model mdl and with defined
        /// length, and fixed position in front of, and in line with the
        /// camera.
        /// </summary>

        // Reference object is fixed, but changes length
        float val = float.Parse(len);
        refObj = Instantiate(mdl, CalcPosGivenAziEle(float.Parse(azimuth), float.Parse(elevation), float.Parse(distance)), Quaternion.identity, camTransform);
        
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
        Debug.Log("Mesh Size: " + meshSz + "| transform scale: " + trScl);
    }

    //Check calculations for azimuth and elevation
    float CalcAzimuth(GameObject go1, GameObject go2)
    {
        ///<summary>
        /// Given two game objects, calculate their azimuth
        /// go1 = test rod, go2 = reference rod
        ///</summary>

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


    Vector3 CalcPosGivenAziEle(float azimuth, float elevation, float distance)
    {
        ///<summary>
        /// Given elevation and azimuth, calculate its position in Unity coordinates
        ///</summary>
        
        // Calculate the x,y rotation of object !! Azimuth and elevation were swapped earlier and has now been fixed
        Quaternion rotX = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.x + elevation, Vector3.left); //rotates around X axis 

        Quaternion rotY = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y + azimuth, Vector3.up); //rotates around y axis

        // Incorporate x,y rotation and magnitude to find location
        Vector3 pos = rotX * rotY * Vector3.forward * distance;
        return pos+Camera.main.transform.position;
    }

    public string filePath{ get; set; }

    // private void WriteRow(string path, bool append, string trialResponses)
    // {
    //     // If there are any problems with the output file
    //     try 
    //     { 
    //         using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path, append))
    //         {
    //             file.WriteLine(trialResponses); //write response (dont forget to close)
    //         }
    //     }
    //     catch(Exception e)
    //     {
    //         Debug.Log("Cannot write to file or generate results");
    //         Debug.Log(e);
    //     }
        
    // }
    // private void InitializeOutput()
    // {
    //     string csvHeader = "Trial Type,Reference Length (metres),Azimuth (deg),Elevation (deg),Duration,Eccentric Rod Length (metres),Answer";
    //     WriteRow(filePath, false, csvHeader);
    // }

    /*Note about CSV units:
    Input - input sizes related to rod are multipliers, not unit sizes. So 0.5 Reference Rod Length 
    would be 0.5 * unit length of imported mesh (use Mesh.bounds.size.z)

    Output - output sizes are in unit size (cm). 
    */

}

