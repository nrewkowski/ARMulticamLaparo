using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;

public enum LaparoEndoscopes{LeftHand,RightHand,BottomMiddle,BottomLeft,BottomRight,TopLeft,TopRight};

[Serializable]
public class ParentedViveTrackers{
    public Vector3 fromLeftBaseToRight;
    public Vector3 rightControllerOffset;
    public Vector3 leftControllerOffset;
    public Vector3 rightControllerOffsetRot;
    public Vector3 leftControllerOffsetRot;
    public Vector3 rightBaseOffset;
    public Vector3 rightBaseOffsetRot;
    public Vector3 leftBaseRot;
    public float[] endoArray;
    public Vector3 _manipulatorOffset; //mostly a laziness thing... i'll fix this eventually but for now can calibrate this from unreal without building unity every time
    public Vector3 _pegboardOffset;
    public Vector3 _pegboardOffset2;
    public int mostRecentCamToHaveSeenDonut;

    
    public ParentedViveTrackers(){

    }
    public ParentedViveTrackers(Vector3 f, Vector3 r, Vector3 l,Vector3 rr, Vector3 lr,Vector3 rbl, Vector3 rbr,Vector3 lbr,float[] endoArrayIn,Vector3 _manipulatorOffsetIn, 
    Vector3 _pegboardOffsetIn,int mostRecentCamToHaveSeenDonutIn,Vector3 _pegboardOffset2In){
        fromLeftBaseToRight=f;
        rightControllerOffset=r;
        leftControllerOffset=l;
        rightControllerOffsetRot=rr;
        leftControllerOffsetRot=lr;
        rightBaseOffset=rbl;
        rightBaseOffsetRot=rbr;
        leftBaseRot=lbr;
        //bottomLeftLoc=blloc;
        //bottomLeftRot=blrot;
        //bottomLeftDonutLoc=bldloc;
        //bottomLeftDonutRot=bldrot;
        endoArray=endoArrayIn;
        _manipulatorOffset=_manipulatorOffsetIn;
        _pegboardOffset=_pegboardOffsetIn;
        _pegboardOffset2=_pegboardOffset2In;
        mostRecentCamToHaveSeenDonut=mostRecentCamToHaveSeenDonutIn;
    }
}

[Serializable]
public class EndoscopeObjectCorrespondences{
    public GameObject baseMarker;
    public GameObject donutTrackingCam;
    public GameObject donutForTrackingCam;
    public GameObject baseMarkerTrackingCam;

    public MeshRenderer baseMarkerVisuals;
    public MeshRenderer donutTrackingCamVisuals;
    public MeshRenderer donutForTrackingCamVisuals;
    public MeshRenderer baseMarkerTrackingCamVisuals;

    public EndoscopeObjectCorrespondences(GameObject baseMarkerIn, GameObject donutTrackingCamIn, GameObject donutForTrackingCamIn, GameObject baseMarkerTrackingCamIn,
    MeshRenderer baseMarkerVisualsIn, MeshRenderer donutTrackingCamVisualsIn, MeshRenderer donutForTrackingCamVisualsIn, MeshRenderer baseMarkerTrackingCamVisualsIn){
        baseMarker=baseMarkerIn;
        donutTrackingCam=donutTrackingCamIn;
        donutForTrackingCam=donutForTrackingCamIn;
        baseMarkerTrackingCam=baseMarkerTrackingCamIn;
        baseMarkerVisuals=baseMarkerVisualsIn;
        donutTrackingCamVisuals=donutTrackingCamVisualsIn;
        donutForTrackingCamVisuals=donutForTrackingCamVisualsIn;
        baseMarkerTrackingCamVisuals=baseMarkerTrackingCamVisualsIn;
    }
}

public class __ClassesForUDP : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
