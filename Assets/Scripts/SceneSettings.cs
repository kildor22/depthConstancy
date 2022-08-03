using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class SceneSettings : MonoBehaviour
{

    public bool enableDebugLog = false;
    public bool enableTerrain = false;
    public bool enableHeadTracking = false;
    [HideInInspector] public bool UseDefaultSkybox;
    public GameObject terrain;
    public GameObject floor;
    public Material defaultSkybox;
    public Material darkSkybox;
    public runExp mainScript;
    public TrackedPoseDriver trackedPoseDriver;

    // Awake is called even when the script is inactive
    void Awake()
    {
    }

    void Start()
    {
        EnableHeadTracking();
        EnableDebugLog();
    }

    void Update()
    {
        if (mainScript.isStarted)
        {
            EnableTerrain();
        }
        EnableDefaultSkybox(UseDefaultSkybox);
    }

    void EnableHeadTracking()
    {
        switch(enableHeadTracking)
        {
            case true: trackedPoseDriver.enabled = true;
                break;
            case false: trackedPoseDriver.enabled = false;
                break;
        }
    }

    void EnableTerrain()
    {
        if (enableTerrain)
        {
            terrain.SetActive(true);
            floor.SetActive(false);
        }
        else
        {
            terrain.SetActive(false);
            floor.SetActive(true);
        }
    }
    
    void EnableDebugLog()
    {
        if (enableDebugLog)
        {
            gameObject.GetComponent<DebugMessagesOnScreen>().enabled = true;
        }
        else
        {
            gameObject.GetComponent<DebugMessagesOnScreen>().enabled = false;
        }
    }

    void EnableDefaultSkybox(bool UseDefaultSkybox)
    {
        if (UseDefaultSkybox) RenderSettings.skybox = defaultSkybox;
        else RenderSettings.skybox = darkSkybox;
    }
}
