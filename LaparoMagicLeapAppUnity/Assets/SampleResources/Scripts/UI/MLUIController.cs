/*===============================================================================
Copyright (c) 2020 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class MLUIController : MonoBehaviour
{
    public MLCanvas mainCanvas;
    public MLCanvas homeCanvas;
    public MLCanvas bumperCanvas;

    private ML6DOFController controller;

    public void Awake()
    {
        controller = FindObjectOfType<ML6DOFController>();
    }

    public void Start()
    {
        MLInput.OnControllerButtonDown += OnControllerButtonDown;
    }

    public void Stop()
    {
        MLInput.OnControllerButtonDown -= OnControllerButtonDown;
    }

    public void GoToMain()
    {
        if (mainCanvas != null)
        {
            mainCanvas.SetInteractionEnabled(true);
        }

        if (homeCanvas != null)
        {
            homeCanvas.gameObject.SetActive(false);
        }

        if (bumperCanvas != null)
        {
            bumperCanvas.gameObject.SetActive(false);
        }
    }

    public void GoToHome()
    {
        if (homeCanvas != null)
        {
            if (bumperCanvas != null)
            {
                bumperCanvas.gameObject.SetActive(false);
            }

            homeCanvas.gameObject.SetActive(true);

            if (mainCanvas != null)
            {
                mainCanvas.SetInteractionEnabled(false);
            }
        }
    }

    public void GoToBumper()
    {
        if (bumperCanvas != null)
        {
            if (homeCanvas != null)
            {
                homeCanvas.gameObject.SetActive(false);
            }

            bumperCanvas.gameObject.SetActive(true);

            if (mainCanvas != null)
            {
                mainCanvas.SetInteractionEnabled(false);
            }
        }
    }

    private void OnControllerButtonDown(byte controllerId, MLInputControllerButton button)
    {
        if (button == MLInputControllerButton.HomeTap)
        {
            if (homeCanvasVisible)
            {
                GoToMain();
            }
            else
            {
                GoToHome();
            }
        }
        else if (button == MLInputControllerButton.Bumper)
        {
            if (bumperCanvasVisible)
            {
                GoToMain();
            }
            else
            {
                GoToBumper();
            }
        }
    }

    private bool homeCanvasVisible
    {
        get
        {
            return (homeCanvas != null) && homeCanvas.gameObject.activeSelf;
        }
    }

    private bool bumperCanvasVisible
    {
        get
        {
            return (bumperCanvas != null) && bumperCanvas.gameObject.activeSelf;
        }
    }
}
