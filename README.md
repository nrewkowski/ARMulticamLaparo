# COMP776Final

In the Hololens Unity project, the script doing most of the "application thread" kinematic stuff is in LaparoHololensAppUnity/Assets/ViveTrackerPlacer.cs (although quite a bit of it is controlled by the parenting structure in the scene itself)

In the Pooler Thread, most of the code is in a blueprint script called trackingpawn in LaparoViveTrackersPooling/Content/trackingpawn.uasset (only openable in UE4)

In the individual cam project, the code that does most of the work finding and sending the marker positions through UDP is in LocalCameraPredictionsUnity/Assets/LaparoCameraManager.cs