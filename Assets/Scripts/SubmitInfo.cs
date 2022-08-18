using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubmitInfo : MonoBehaviour
{
    public GameObject experiment;
    public CsvManager csv;
    public Text ageField;
    public Text sexField;
    public Text idField;
    public Text groupField;
    public Text sessionField;

    //starts the game when "Next" is clicked
    public void Next()
    {
        string id = idField.text;
        string sex = sexField.text;
        string age = ageField.text;
        string group = groupField.text;
        string session = sessionField.text;
        csv.InitializeOutput(id, sex, age, group, session);
        experiment.GetComponent<runExp>().infoSubmitted = true; //move to instructions
        GameObject.Find("RegistrationCanvas").SetActive(false);
    }   
}
