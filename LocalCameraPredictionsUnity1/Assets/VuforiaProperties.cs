using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vuforia;
 
public class VuforiaProperties : MonoBehaviour
{
    void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
    }
 
    void OnVuforiaStarted()
    {
        print("___________________________________________________________________________________________attempt fields");

     // Get the fields
        IEnumerable cameraFields = CameraDevice.Instance.GetCameraFields();
          //print(" fields "+cameraFields.Count());
        // Print fields to device logs
        foreach (CameraDevice.CameraField field in cameraFields)
        {
            Debug.Log("******************************Key: " + field.Key + "; Type: " + field.Type);
        }
 
        // Retrieve a specific field and print to logs
        string focusMode = "";
 
        CameraDevice.Instance.GetField("focus-mode", out focusMode);
 
        Debug.Log("FocusMode: " + focusMode);
        print("___________________________________________________________________________________________done");
    }
}