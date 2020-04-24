/*==============================================================================
Copyright (c) 2017-2019 PTC Inc. All Rights Reserved.

Copyright (c) 2012-2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
==============================================================================*/
using UnityEngine;
using Vuforia;

/// <summary>
/// This script sets up the background shader effect and contains the logic
/// to capture longer touch "drag" events that distort the video background. 
/// </summary>
public class NegativeGrayscaleEffect : MonoBehaviour
{
    #region PRIVATE_MEMBERS;

    Material backgroundMaterial;
    Shader defaultVideoBkgdShader;

    #endregion // PRIVATE_MEMBERS;


    #region MONOBEHAVIOUR_METHODS

    void Awake()
    {
        VuforiaARController.Instance.RegisterVideoBgConfigChangedCallback(OnVideoBackgroundConfigChanged);
        this.defaultVideoBkgdShader = VuforiaConfiguration.Instance.VideoBackground.VideoBackgroundShader;
        VuforiaConfiguration.Instance.VideoBackground.VideoBackgroundShader = Resources.Load<Shader>("NegativeGrayscale");
        VuforiaConfiguration.Instance.VideoBackground.NumDivisions = 20;
    }

    void OnDestroy()
    {
        VuforiaARController.Instance.UnregisterVideoBgConfigChangedCallback(OnVideoBackgroundConfigChanged);
        VuforiaConfiguration.Instance.VideoBackground.VideoBackgroundShader = this.defaultVideoBkgdShader;
        VuforiaConfiguration.Instance.VideoBackground.NumDivisions = 2;
    }

    void Update()
    {
        float touchX = 2.0f;
        float touchY = 2.0f;

        if (Input.GetMouseButton(0))
        {
            Vector2 touchPos = Input.mousePosition;
            // Adjust the touch point for the current orientation
            touchX = ((touchPos.x / Screen.width) - 0.5f) * 2.0f;
            touchY = ((touchPos.y / Screen.height) - 0.5f) * 2.0f;
        }

        if (this.backgroundMaterial)
        {
            // Pass the touch coordinates to the shader
            this.backgroundMaterial.SetFloat("_TouchX", touchX);
            this.backgroundMaterial.SetFloat("_TouchY", touchY);
        }
    }

    #endregion // MONOBEHAVIOUR_METHODS


    #region VUFORIA_CALLBACK_METHODS

    public void OnVideoBackgroundConfigChanged()
    {
        Debug.Log("OnVideoBackgroundConfigChanged() called.");

        this.backgroundMaterial = FindObjectOfType<BackgroundPlaneBehaviour>().Material;
    }

    #endregion // VUFORIA_CALLBACK_METHODS
}
