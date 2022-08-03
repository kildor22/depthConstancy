using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMessagesOnScreen : MonoBehaviour
{
    uint qsize = 15;  // number of messages to keep
    Queue myLogQueue = new Queue();
    GUIStyle style;
    public FontStyle DebugMessageFontStyle;
    void Start() {
        Debug.Log("Started up logging.");
    }

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type) {
        myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();
    }

    void OnGUI() {
        style = new GUIStyle(GUI.skin.label);
        style.normal.textColor = Color.yellow;
        style.fontStyle = DebugMessageFontStyle;
        GUILayout.BeginArea(new Rect(10, 0, 400, Screen.height)); //change x to "Screen.width - 400" to display in upper right
        GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()), style);
        GUILayout.EndArea();
    }
}
