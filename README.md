# COMP776Final

In the Hololens Unity project (LaparoHololensAppUnity), the script doing most of the "application thread" kinematic stuff is in LaparoHololensAppUnity/Assets/ViveTrackerPlacer.cs (although quite a bit of it is controlled by the parenting structure in the scene itself). The MagicLeap project is the same as the HoloLens project (it has a symbolic link to most of the HL directories), but its settings are changed to export to MagicLeap correctly.

In the Pooler Thread UE4 project (LaparoViveTrackersPooling), most of the code is in a blueprint script called trackingpawn in LaparoViveTrackersPooling/Content/trackingpawn.uasset (only openable in UE4)

In the individual cam Unity project (LocalCameraPredictionsUnity1-8), the code that does most of the work finding and sending the marker positions through UDP is in LocalCameraPredictionsUnity1/Assets/LaparoCameraManager.cs. Camera info pairing is saved in DefaultLaparoCameraInfo.cs. Only LocalCameraPredictionsUnity1 has a REAL Assets directory. The other folders are Unity projects that have a symbolic link to the Assets of LocalCameraPredictionsUnity1 to trick Unity into thinking they're different projects (Unity will not open multiple of the same project). (makelink is done from within a copied project's folder with "mklink /D Assets ..\LocalCameraPredictionsUnity1\Assets")

Pipeline described here:
![Pipeline](__images/pipeline.png)

More apparatus images

vivetracker
![vivetracker](__images/vivetracker.jpg)

hololens
![hololens](__images/hololens.png)

vuforia features
![vuforia features](__images/vuforiafeatures.jpg)

vuforia image trackability ranks
![vuforia image trackability ranks](__images/vuforiaranks.jpg)

before translation or yaw correction
![before translation or yaw correction](__images/beforetrans.jpg)

after translation, before yaw correction
![after translation, before yaw correction](__images/beforerot.jpg)

after correction
![after correction](__images/corrected.jpg)

entire visualization
![endoscopes](__images/vis.png)

individual cams
![individual cams](__images/indivcams.png)

static cam kinematic chain
![static cam kinematic chain](__images/staticcam.png)

zoom in on local peg
![zoom in on local peg](__images/localpeg.jpg)

manipulator cam before calib
![manipulator cam before calib](__images/dyncam1.png)

manipulator cam after calib
![manipulator cam after calib](__images/dyncam2.png)

virtual pegboard locations
![virtual pegboard locations](__images/pegcalibnames.jpg)

vive tracker positioning
![vive tracker positioning](__images/markercalib.jpg)

more apparatus
![more apparatus](__images/teaser2.png)

main marker
![main marker](__images/mainmarker.png)

endoscopes/borescopes: https://www.ebay.com/itm/Endoscope-5-5mm-Borescope-Camera-Inspection-For-Android-PC-Laptop-Waterproof-LED/272969087508?hash=item3f8e39ba14:m:mKOxQf8Ih-aXqAamVBQcwWA 
![endoscopes](__images/endo.png)

cam placement
![cam placement](__images/cams.jpg)