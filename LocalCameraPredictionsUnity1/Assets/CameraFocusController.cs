// full tutorial here:
// https://medium.com/@harmittaa/setting-camera-focus-mode-for-vuforia-arcamera-in-unity-6b3745297c3d

using UnityEngine;
using System.Collections;
using Vuforia;

public class CameraFocusController: MonoBehaviour {

 // code from  Vuforia Developer Library
 // https://library.vuforia.com/articles/Solution/Camera-Focus-Modes
 void Start() {    
  var vuforia = VuforiaARController.Instance;    
  vuforia.RegisterVuforiaStartedCallback(OnVuforiaStarted);    
  vuforia.RegisterOnPauseCallback(OnPaused);
 }  

 private void OnVuforiaStarted() {    
  CameraDevice.Instance.SetFocusMode(
      CameraDevice.FocusMode.FOCUS_MODE_MACRO);
     /*print("_____________attempt fields");

     // Get the fields
        IEnumerable cameraFields = CameraDevice.Instance.GetCameraFields();
          //print(" fields "+cameraFields.Count());
        // Print fields to device logs
        foreach (CameraDevice.CameraField field in cameraFields)
        {
            Debug.Log("Key: " + field.Key + "; Type: " + field.Type);
        }
 
        // Retrieve a specific field and print to logs
        string focusMode = "";
 
        CameraDevice.Instance.GetField("focus-mode", out focusMode);
 
        Debug.Log("FocusMode: " + focusMode);
        print("_____________done");

  */

 }

 private void OnPaused(bool paused) {    
  if (!paused) // resumed
  {
       // Set again autofocus mode when app is resumed
       CameraDevice.Instance.SetFocusMode(
          CameraDevice.FocusMode.FOCUS_MODE_MACRO);    
  }
 }
}