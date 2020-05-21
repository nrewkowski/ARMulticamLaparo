/*===============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.MagicLeap;

[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

public class ML6DOFController : MonoBehaviour
{
    private MLInputController inputController;

    void Start()
    {
        MLInput.Start();
        inputController = MLInput.GetController(MLInput.Hand.Left);
    }

    void OnDestroy()
    {
        MLInput.Stop();
    }

    void Update()
    {
        if (inputController != null)
        {
            transform.position = inputController.Position;
            transform.rotation = inputController.Orientation;
        }
    }

    public void Vibrate(MLInputControllerFeedbackPatternVibe patternVibe)
    {
        inputController.StartFeedbackPatternVibe(patternVibe, MLInputControllerFeedbackIntensity.Medium);
    }
}
