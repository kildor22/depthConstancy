using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class CsvManager : MonoBehaviour
{
    public runExp ExperimentManager;
    private string filePath;
    private string folderName;

    private void Awake()
    {
        // Create folder path, create folder in user director
        folderName = Application.persistentDataPath + "/Results";
        System.IO.Directory.CreateDirectory(folderName);
    }

    public void WriteRow(string path, bool append, string trialResponses)
    {
        // If there are any problems with the output file
        try 
        { 
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path, append))
            {
                file.WriteLine(trialResponses); //write response (dont forget to close)
            }
        }
        catch(Exception e)
        {
            Debug.Log("Cannot write to file or generate results");
            Debug.Log(e);
        }
    }
    public void InitializeOutput(string id, string sex, string age, string group, string session)
    {   
        //first CSV row
        string userInfo = "ID: " + id + ',' + "Sex: " + 
        sex + ',' + "Age: " + age + ',' + "Group: " + group + ',' + "Session: " + session;

        //second row
        string csvHeader = "Trial Type,Reference Length (metres),Azimuth (deg),Elevation (deg),Duration,Eccentric Rod Length (metres),Answer";
   
        
        //initialize file path to write output csv
        filePath = folderName + "/p" + id + "_group" + group + "_session" + 
        session+ "_results.csv";

        //write rows in csv
        WriteRow(filePath, false, userInfo);
        WriteRow(filePath, true, csvHeader);
        ExperimentManager.filePath = filePath; //set file path
    }

}
