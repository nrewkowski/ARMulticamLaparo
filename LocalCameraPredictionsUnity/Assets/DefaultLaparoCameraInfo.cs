using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum LaparoEndoscopes{LeftHand,RightHand,BottomMiddle,BottomLeft,BottomRight,TopLeft,TopRight};

[Serializable]
public class LaparoInfo{
    //made this an array so you can give both regular & defocused markers
    public string[] nameOfMarkerForBaseCalibration;
    //generally these should be the same since i forced them to be in regedit except for yellow
    //only saving this for dev to know which camera goes where. cams get labelled by port in vuforia
    public int usbPortNumber;
    public string vuforiaWebcamName;
    public int whichUDPPortToUse;
    public LaparoInfo(string[] nameOfMarkerForBaseCalibrationIn, int usbPortNumberIn, string vuforiaWebcamNameIn,int whichUDPPortToUseIn){
        nameOfMarkerForBaseCalibration=nameOfMarkerForBaseCalibrationIn;
        usbPortNumber=usbPortNumberIn;
        vuforiaWebcamName=vuforiaWebcamNameIn;
        whichUDPPortToUse=whichUDPPortToUseIn;
    }
}

[Serializable]
public class LaparoEndoscopesInfoPairs:SerializableDictionary<LaparoEndoscopes,LaparoInfo>{}

[Serializable]
public class DefaultLaparoCameraInfo:MonoBehaviour{
    [SerializeField]
    public LaparoEndoscopesInfoPairs laparoCameraInfoPairs = new LaparoEndoscopesInfoPairs
        {
            //appears defective
            { LaparoEndoscopes.LeftHand, new LaparoInfo(new[]{"wolf"},52,"Endoscope52",8560) },
            { LaparoEndoscopes.RightHand, new LaparoInfo(new[]{"tiger"},4,"Endoscope4",8561) },
            //port doesn't matter here b/c there's only 1 of these
            { LaparoEndoscopes.BottomMiddle, new LaparoInfo(new[]{"wolfmarkerblurred", "wolf2"},-1,"YPCendoscope",8562) },
            { LaparoEndoscopes.BottomLeft, new LaparoInfo(new[]{"wolfmarkerblurred", "wolf2"},25,"Endoscope25",8563) },
            { LaparoEndoscopes.BottomRight, new LaparoInfo(new[]{"tiger"},3,"Endoscope3",8564) },
            { LaparoEndoscopes.TopLeft, new LaparoInfo(new[]{"tiger"},1,"Endoscope1",8565) },
            { LaparoEndoscopes.TopRight, new LaparoInfo(new[]{"wolfmarkerblurred", "wolf2"},2,"Endoscope2",8566) }
        };
}