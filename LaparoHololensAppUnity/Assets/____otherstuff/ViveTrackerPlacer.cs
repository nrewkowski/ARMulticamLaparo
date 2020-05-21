using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;

using System.Net;
using System.Threading;

#if UNITY_WSA_10_0 && !UNITY_EDITOR
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else
using System.Net.Sockets;
#endif




public class ViveTrackerPlacer : MonoBehaviour
{
    public nickmarker mainMarker;
    
    private ViveTrackerClient Instance;

    public ParentedViveTrackers mpo;

    public TextMesh testVecOut;
    
    public GameObject _viveLeftBase;
    public GameObject _viveRightBase;
    public GameObject _viveLeftHand;
    public GameObject _viveRightHand;
    public GameObject _viveIdealRightBase;
    public GameObject _viveLeftBaseVisuals;
    public GameObject _pegBoard;

    private Vector3 rightHandInitPosition;
    private Vector3 leftHandInitPosition;
    private Vector3 pegBoardInitPosition;
    //public GameObject[] baseMarkers;
    //public GameObject[] donutTrackingCams; //should be as many as there are cams
    //public GameObject[] donutsForEachDonutTrackingCam;
    //public GameObject[] baseMarkerTrackingCams;
    public EndoscopeObjectCorrespondences[] endoscopeObjectCorrespondences; //using this forces alignment! makes code uglier but worth it
    public GameObject[] __endoscopeApparatusPrefabs; //will automatically get the pieces
    //public GameObject donutCam;
    //public GameObject bottomLeftDonutPeg;
    public string test2;
    public string test3;
    bool calibratedOffsets=false;

    float angleLeftBaseNeedsToRotate=0.0f;
    float whichWayToRotate=0.0f;

    float angleLeftBaseNeedsToRotateUp=0.0f;
    float whichWayToRotateUp=0.0f;

    public float opacityWhenMostRecentCam=1.0f;
    public float opacityWhenNotMostRecentCam=0.00f;

    public Color[] donutColors;
    //precomputing these for memory efficiency
    private Color[] donutColorsWhenVisible;
    private Color[] donutColorsWhenNotVisible;

    bool leftHandCamPlaced=false;
    bool rightHandCamPlaced=false;

    public GameObject leftHandCalibratedLocation;
    public GameObject rightHandCalibratedLocation;

    public bool[] rotNeg;

    public Vector3 rotOff;
    bool disabledMesh=false;
    //bool markerFound;
    float angleBetween2DVectors(Vector3 A, Vector3 B){
        A.y=0;
        B.y=0;
        return Mathf.Acos(Vector3.Dot(A.normalized,B.normalized))*Mathf.Rad2Deg;
    }

    float angleBetween2DVectorsNoX(Vector3 A, Vector3 B){
        A.x=0;
        B.x=0;
        return Mathf.Acos(Vector3.Dot(A.normalized,B.normalized))*Mathf.Rad2Deg;
    }

    float angleBetween2DVectorsNoZ(Vector3 A, Vector3 B){
        A.z=0;
        B.z=0;
        return Mathf.Acos(Vector3.Dot(A.normalized,B.normalized))*Mathf.Rad2Deg;
    }

    int vectorLeftOrRightOfAnother(Vector3 pointToTest, Vector3 vectorSource, Vector3 vectorDestination){
        return ((pointToTest.x-vectorSource.x)*(vectorDestination.z-vectorSource.z)-(pointToTest.z-vectorSource.z)*(vectorDestination.x-vectorSource.x))<0?1:-1;
        //return ((pointToTest.x-vectorSource.x)(vectorDestination.z-vectorSource.z)-(pointToTest.z-vectorSource.z)(vectorDestination.x-vectorSource.x))<0?-1:1;
    }

    int vectorLeftOrRightOfAnotherXY(Vector3 pointToTest, Vector3 vectorSource, Vector3 vectorDestination){
        return ((pointToTest.x-vectorSource.x)*(vectorDestination.y-vectorSource.y)-(pointToTest.z-vectorSource.y)*(vectorDestination.x-vectorSource.y))<0?1:-1;
        //return ((pointToTest.x-vectorSource.x)(vectorDestination.z-vectorSource.z)-(pointToTest.z-vectorSource.z)(vectorDestination.x-vectorSource.x))<0?-1:1;
    }

    int vectorLeftOrRightOfAnotherYZ(Vector3 pointToTest, Vector3 vectorSource, Vector3 vectorDestination){
        return ((pointToTest.y-vectorSource.y)*(vectorDestination.z-vectorSource.z)-(pointToTest.z-vectorSource.z)*(vectorDestination.y-vectorSource.y))<0?1:-1;
        //return ((pointToTest.x-vectorSource.x)(vectorDestination.z-vectorSource.z)-(pointToTest.z-vectorSource.z)(vectorDestination.x-vectorSource.x))<0?-1:1;
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("vive tracker start");
        
        
        
        //calibMarker0-calibmarker2=forward.  0-1=right

        Vector3 testvec=new Vector3(1.1f,2.2f,3.3f);
        print("***********JSON:"+JsonUtility.ToJson(testvec));
        //ParentedViveTrackers p=new ParentedViveTrackers(testvec,testvec,testvec,testvec,testvec,testvec,testvec,testvec);
        //print("***********JSON2:"+JsonUtility.ToJson(p));
        //test2="{\"fromLeftBaseToRight\":{\"x\":1,\"y\":2,\"z\":3},\"rightControllerOffset\":{\"x\":4,\"y\":5,\"z\":6},\"leftControllerOffset\":{\"x\":7,\"y\":8,\"z\":9},\"rightControllerOffsetRot\":{\"x\":4,\"y\":5,\"z\":6},\"leftControllerOffsetRot\":{\"x\":7,\"y\":8,\"z\":9}}";
        mpo=JsonUtility.FromJson<ParentedViveTrackers>(test3);

        //mpo=

        /*
        {"fromLeftBaseToRight":{"x":1.100000023841858,"y":2.200000047683716,"z":3.299999952316284},"rightControllerOffset":{"x":1.100000023841858,"y":2.200000047683716,"z":3.299999952316284},"leftControllerOffset":{"x":1.100000023841858,"y":2.200000047683716,"z":3.299999952316284}}
        
         */

        //Debug.Log("start now");
        //print("________angle="+angleBetween2DVectors(this.gameObject.transform.right,new Vector3(1,0,1)));
        //print("________d="+vectorLeftOrRightOfAnother(this.gameObject.transform.right+this.gameObject.transform.position,
        //this.gameObject.transform.position,this.gameObject.transform.position+new Vector3(1,0,1)));
        //this.gameObject.transform.localEulerAngles=new Vector3(this.gameObject.transform.localEulerAngles.x,this.gameObject.transform.localEulerAngles.y+
        //angleBetween2DVectors(this.gameObject.transform.right,new Vector3(1,0,1))*vectorLeftOrRightOfAnother(this.gameObject.transform.right+this.gameObject.transform.position,
        //this.gameObject.transform.position,this.gameObject.transform.position+new Vector3(1,0,1)),this.gameObject.transform.localEulerAngles.z);
        this.Instance = new ViveTrackerClient();
        this.Instance.outputText=testVecOut;
        this.Instance.Init(8550);
        
        endoscopeObjectCorrespondences=new EndoscopeObjectCorrespondences[Enum.GetNames(typeof(LaparoEndoscopes)).Length];
        //this.Instance.textMesh1=textMesh1;
        //this.Instance.textMesh2=textMesh2;
        //this.Instance.textMesh3=textMesh3;
        donutColorsWhenVisible=new Color[donutColors.Length];
        donutColorsWhenNotVisible=new Color[donutColors.Length];
        //doing this here b/c gameobject.find is slow and don't want to do it every frame
        print("should be this many apparatus prefabs: "+Enum.GetNames(typeof(LaparoEndoscopes)).Length);
        for (int i=0;i<__endoscopeApparatusPrefabs.Length;i++){
            endoscopeObjectCorrespondences[i]=new EndoscopeObjectCorrespondences(__endoscopeApparatusPrefabs[i],
            __endoscopeApparatusPrefabs[i].transform.Find("donutTrackingCam").gameObject,
            __endoscopeApparatusPrefabs[i].transform.Find("donutTrackingCam").Find("donut").gameObject,
            __endoscopeApparatusPrefabs[i].transform.Find("baseTrackingCam").gameObject,
            __endoscopeApparatusPrefabs[i].transform.Find("baseMarkerVisuals").gameObject.GetComponent<MeshRenderer>(),
            __endoscopeApparatusPrefabs[i].transform.Find("donutTrackingCam").transform.Find("donutTrackingCamVisuals").gameObject.GetComponent<MeshRenderer>(),
            __endoscopeApparatusPrefabs[i].transform.Find("donutTrackingCam").Find("donut").Find("donutVisuals").gameObject.GetComponent<MeshRenderer>(),
            __endoscopeApparatusPrefabs[i].transform.Find("baseTrackingCam").Find("baseTrackingCamVisuals").gameObject.GetComponent<MeshRenderer>()
            );
            donutColorsWhenVisible[i]=new Color(donutColors[i].r,donutColors[i].g,donutColors[i].b,opacityWhenMostRecentCam);
            donutColorsWhenNotVisible[i]=new Color(donutColors[i].r,donutColors[i].g,donutColors[i].b,opacityWhenNotMostRecentCam);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.Instance.ReceiveData){
            //testVecOut.text=""+this.Instance.lastPegLocalTransform.rightControllerOffset;
            positionTrackers(this.Instance.lastPegLocalTransform);
        } else{
            //testVecOut.text="no data";
#if UNITY_EDITOR
            positionTrackers(mpo);
            //print("left->right: "+(_viveLeftBase.gameObject.transform.position-_viveRightBase.gameObject.transform.position).magnitude);
            //print("left->RHand: "+(_viveLeftBase.gameObject.transform.position-_viveRightHand.gameObject.transform.position).magnitude);
            //print("left->Lhand: "+(_viveLeftBase.gameObject.transform.position-_viveLeftHand.gameObject.transform.position).magnitude);
#endif
        }
        if (!disabledMesh){
            Transform BS=GameObject.Find("Target Visualizations").transform.Find("_bigDragonMarker (simulated)");
            if (BS != null){
                BS.GetComponent<MeshRenderer>().enabled=false;
                disabledMesh=true;
            }
        }
        
    }


    void positionTrackers(ParentedViveTrackers trackers){
        //fromlefttoright tells me how much to rotate the cube


        //marker base for manipulator cams is only used to tell me where they're attached to the vivetrackers, nothing else b/c after that, it's attached to the manipulator
        //and the donutseeing camera is attached to that position on the manipulator instead

        //testObject[0].transform.position=trackers.fromLeftBaseToRight;
        //testObject[1].transform.position=trackers.rightControllerOffset;
        //testObject[2].transform.position=trackers.leftControllerOffset;
        if (mainMarker.found || true){
            _viveLeftHand.gameObject.transform.localPosition=trackers.leftControllerOffset+trackers._manipulatorOffset;
            _viveRightHand.gameObject.transform.localPosition=trackers.rightControllerOffset+trackers._pegboardOffset;
            //print("______peg  "+trackers._pegboardOffset);
            _viveLeftHand.gameObject.transform.eulerAngles=new Vector3(rotNeg[0]?(720-trackers.leftControllerOffsetRot.x+rotOff.x):(trackers.leftControllerOffsetRot.x+rotOff.x+360),
            rotNeg[1]?(720-trackers.leftControllerOffsetRot.y+rotOff.y):(trackers.leftControllerOffsetRot.y+rotOff.y+360),
            rotNeg[2]?(720-trackers.leftControllerOffsetRot.z+rotOff.z):(trackers.leftControllerOffsetRot.z+rotOff.z+360));
            _viveRightHand.gameObject.transform.eulerAngles=new Vector3(rotNeg[0]?(720-trackers.rightControllerOffsetRot.x+rotOff.x):(trackers.rightControllerOffsetRot.x+rotOff.x+360),
            rotNeg[1]?(720-trackers.rightControllerOffsetRot.y+rotOff.y):(trackers.rightControllerOffsetRot.y+rotOff.y+360),
            rotNeg[2]?(720-trackers.rightControllerOffsetRot.z+rotOff.z):(trackers.rightControllerOffsetRot.z+rotOff.z+360));
            //_pegBoard.gameObject.transform.localPosition=pegBoardInitPosition+trackers._pegboardOffset;
            //process below is to deal with unity's horrible physics system
            //should: make localcam a child of a donut
            //put localcam localposition relative to this donut
            //move a child visuals donut which will be new position to the same position as the parent donut so that it will move when i move the camera
            //move localcam to the basecam so that the donut will move accordingly


            //bottomleft is index 3
            //int camIndex=3;
            //12 because there are 4 vector3s
            //sets base marker cam transform
            for (int camIndex=0;camIndex<endoscopeObjectCorrespondences.Length;camIndex++){
                //should still be able to do all this for the manipulator b/c i'm basically just figuring out the new peg position. only change is where donutcam gets placed
                //when the base for manipulator first gets seen, attach something at its position/angle on the manipulator to use for future reference
                endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCam.gameObject.transform.localPosition=new Vector3(trackers.endoArray[(12*camIndex)],trackers.endoArray[(12*camIndex)+1],trackers.endoArray[(12*camIndex)+2]);
                endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCam.gameObject.transform.localEulerAngles=new Vector3(trackers.endoArray[(12*camIndex)+3],trackers.endoArray[(12*camIndex)+4],trackers.endoArray[(12*camIndex)+5]);

                //sets donut tracking cams local pos rel to base marker
                endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.localPosition=new Vector3(trackers.endoArray[(12*camIndex)+6],trackers.endoArray[(12*camIndex)+7],trackers.endoArray[(12*camIndex)+8]);
                endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.localEulerAngles=new Vector3(trackers.endoArray[(12*camIndex)+9],trackers.endoArray[(12*camIndex)+10],trackers.endoArray[(12*camIndex)+11]);

                //puts donut at marker pos so that rel to the donut, donut tracking cam is right
                endoscopeObjectCorrespondences[camIndex].donutForTrackingCam.gameObject.transform.position= endoscopeObjectCorrespondences[camIndex].baseMarker.gameObject.transform.position;
                endoscopeObjectCorrespondences[camIndex].donutForTrackingCam.gameObject.transform.eulerAngles=endoscopeObjectCorrespondences[camIndex].baseMarker.gameObject.transform.eulerAngles;

                //moves donut tracking cam to the right position to force donut to follow
                //for manipulators, i only do this the first time & afterwards I place it at the location of the thing I attached to the manipulator itself
                if (camIndex==0 && calibratedOffsets){
                    if (Mathf.Abs(trackers.endoArray[(12*camIndex)])>0.000f){
                        if (leftHandCamPlaced){
                            //then put this at the attachment object
                            endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.position=leftHandCalibratedLocation.transform.position;
                            endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.eulerAngles=leftHandCalibratedLocation.transform.eulerAngles;
                            
                        } else{
                            print("^^^^^^^^^^^^^^^^^^^^placedleft");
                            endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.position=endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCam.gameObject.transform.position;
                            endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.eulerAngles=endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCam.gameObject.transform.eulerAngles;
                            //then attach something to its new position to vivetrackerleft
                            leftHandCalibratedLocation.transform.position=endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCam.gameObject.transform.position;
                            leftHandCalibratedLocation.transform.eulerAngles=endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCam.gameObject.transform.eulerAngles;

                            endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.position=leftHandCalibratedLocation.transform.position;
                            endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.eulerAngles=leftHandCalibratedLocation.transform.eulerAngles;
                            //leftHandCalibratedLocation.transform.parent=_viveLeftHand.transform;
                            leftHandCamPlaced=true;
                        }
                    } else{
                        //print("left cam not ready to be placed");
                    }
                } else if (camIndex==1 && calibratedOffsets){
                    if (Mathf.Abs(trackers.endoArray[(12*camIndex)])>0.000f){
                        if (rightHandCamPlaced){
                            endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.position=rightHandCalibratedLocation.transform.position;
                            endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.eulerAngles=rightHandCalibratedLocation.transform.eulerAngles;
                        } else{
                            print("^^^^^^^^^^^^^^^^^^^^placedright");
                            endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.position=endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCam.gameObject.transform.position;
                            endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.eulerAngles=endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCam.gameObject.transform.eulerAngles;
                            //then attach something to its new position to vivetrackerright
                            rightHandCalibratedLocation.transform.position=endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCam.gameObject.transform.position;
                            rightHandCalibratedLocation.transform.eulerAngles=endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCam.gameObject.transform.eulerAngles;
                            //rightHandCalibratedLocation.transform.parent=_viveRightHand.transform;
                            rightHandCamPlaced=true;
                        }
                    } else{
                        //print("right cam not ready to be placed");
                    }
                } else{
                    endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.position=endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCam.gameObject.transform.position;
                    endoscopeObjectCorrespondences[camIndex].donutTrackingCam.gameObject.transform.eulerAngles=endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCam.gameObject.transform.eulerAngles;
                }
                if (camIndex==trackers.mostRecentCamToHaveSeenDonut){
                    //print("found donut");
                    //change to Line Color?
                    endoscopeObjectCorrespondences[camIndex].donutForTrackingCamVisuals.material.color=donutColorsWhenVisible[camIndex];
                    endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCamVisuals.material.color=donutColorsWhenVisible[camIndex];
                    endoscopeObjectCorrespondences[camIndex].baseMarkerVisuals.material.color=donutColorsWhenNotVisible[camIndex];
                    endoscopeObjectCorrespondences[camIndex].donutTrackingCamVisuals.material.color=donutColorsWhenVisible[camIndex];
                    //ToOpaqueMode(endoscopeObjectCorrespondences[camIndex].donutTrackingCamVisuals.material);
                }else{
                    endoscopeObjectCorrespondences[camIndex].donutForTrackingCamVisuals.material.color=donutColorsWhenNotVisible[camIndex];
                    endoscopeObjectCorrespondences[camIndex].baseMarkerTrackingCamVisuals.material.color=donutColorsWhenNotVisible[camIndex];
                    endoscopeObjectCorrespondences[camIndex].baseMarkerVisuals.material.color=donutColorsWhenNotVisible[camIndex];
                    endoscopeObjectCorrespondences[camIndex].donutTrackingCamVisuals.material.color=donutColorsWhenVisible[camIndex];
                    //ToFadeMode(endoscopeObjectCorrespondences[camIndex].donutTrackingCamVisuals.material);
                    //ToOpaqueMode(endoscopeObjectCorrespondences[camIndex].donutTrackingCamVisuals.material);
                }
            } 
            
            
            /*
            //positions the cam seeing base marker
            camBottomLeft.gameObject.transform.localPosition=trackers.bottomLeftLoc;
            camBottomLeft.gameObject.transform.localEulerAngles=trackers.bottomLeftRot;


            ///this process is basically

            //positions camera that sees donut relative to the donut itself (which will end up being the same as before obv)
            donutCam.gameObject.transform.localPosition=trackers.bottomLeftDonutLoc;
            donutCam.gameObject.transform.localEulerAngles=trackers.bottomLeftDonutRot;
            //puts local peg in position of world peg?
            bottomLeftDonutPeg.gameObject.transform.position=bottomLeftDonutGuess.transform.position;
            bottomLeftDonutPeg.gameObject.transform.eulerAngles=bottomLeftDonutGuess.transform.eulerAngles;
            //moves donut-seeing cam to other cam pos
            donutCam.gameObject.transform.position=camBottomLeft.transform.position;
            donutCam.gameObject.transform.eulerAngles=camBottomLeft.transform.eulerAngles;*/
            //camBottomLeft.gameObject.transform.eulerAngles=Quaternion.LookRotation(markerBottomLeft.gameObject.transform.position-camBottomLeft.gameObject.transform.position,mainMarker.gameObject.transform.up).eulerAngles;
            //camBottomLeft.gameObject.transform.up=
            //print("---bottomleftloc "+trackers.bottomLeftLoc);
            if (!calibratedOffsets){
                print("MARKER UP:"+mainMarker.gameObject.transform.up);
                //_viveLeftBase.gameObject.transform.localEulerAngles=trackers.leftBaseRot; //right base rot (doesn't really matter)
                _viveRightBase.gameObject.transform.localPosition=trackers.fromLeftBaseToRight+_viveLeftBase.gameObject.transform.localPosition; //put right base in right place
                
                print("angle between="+(_viveIdealRightBase.gameObject.transform.localPosition-_viveLeftBase.gameObject.transform.localPosition)
                +", ="+(_viveRightBase.gameObject.transform.localPosition-_viveLeftBase.gameObject.transform.localPosition));
                
                angleLeftBaseNeedsToRotate=angleBetween2DVectors(_viveIdealRightBase.gameObject.transform.localPosition-_viveLeftBase.gameObject.transform.localPosition,
                _viveRightBase.gameObject.transform.localPosition-_viveLeftBase.gameObject.transform.localPosition);
                whichWayToRotate=vectorLeftOrRightOfAnother((_viveRightBase.gameObject.transform.localPosition-_viveLeftBase.gameObject.transform.localPosition)+
                _viveLeftBase.gameObject.transform.localPosition,_viveLeftBase.gameObject.transform.localPosition,_viveIdealRightBase.gameObject.transform.localPosition);
                print("angle="+angleLeftBaseNeedsToRotate+", d="+whichWayToRotate+", pos="+_viveRightBase.gameObject.transform.position);
                //print("________angle="+angleBetween2DVectors(this.gameObject.transform.right,new Vector3(1,0,1)));
                //print("________d="+vectorLeftOrRightOfAnother(this.gameObject.transform.right+this.gameObject.transform.position,
                //this.gameObject.transform.position,this.gameObject.transform.position+new Vector3(1,0,1)));
                //_viveLeftBase.gameObject.transform.localEulerAngles=trackers.leftBaseRot;
                //_viveLeftBase.gameObject.transform.localEulerAngles=new Vector3(trackers.leftBaseRot.x,trackers.leftBaseRot.y+angleLeftBaseNeedsToRotate*whichWayToRotate,trackers.leftBaseRot.z);
                _viveRightBase.transform.parent=_viveLeftBase.transform;
                
                
                //vector excluded is one you rotate around, so this is right vec
                //angleLeftBaseNeedsToRotateUp=angleBetween2DVectorsNoZ(_viveIdealRightBase.gameObject.transform.localPosition-_viveLeftBase.gameObject.transform.localPosition,
                //_viveRightBase.gameObject.transform.localPosition-_viveLeftBase.gameObject.transform.localPosition);
                //whichWayToRotateUp=vectorLeftOrRightOfAnotherXY((_viveRightBase.gameObject.transform.localPosition-_viveLeftBase.gameObject.transform.localPosition)+
                //_viveLeftBase.gameObject.transform.localPosition,_viveLeftBase.gameObject.transform.localPosition,_viveIdealRightBase.gameObject.transform.localPosition);
                //print("anglexy="+angleLeftBaseNeedsToRotateUp+", d="+whichWayToRotateUp+", pos="+_viveRightBase.gameObject.transform.position);
                _viveLeftBase.gameObject.transform.RotateAround(_viveLeftBase.gameObject.transform.position,mainMarker.gameObject.transform.up,angleLeftBaseNeedsToRotate*whichWayToRotate);
                //_viveLeftBase.gameObject.transform.RotateAround(_viveLeftBase.gameObject.transform.position,_viveLeftBase.gameObject.transform.up,angleLeftBaseNeedsToRotateUp*whichWayToRotateUp);
                /*
                //vector excluded is one you rotate around, so this is forward vec
                angleLeftBaseNeedsToRotate=angleBetween2DVectorsNoX(_viveIdealRightBase.gameObject.transform.localPosition-_viveLeftBase.gameObject.transform.localPosition,
                _viveRightBase.gameObject.transform.localPosition-_viveLeftBase.gameObject.transform.localPosition);
                whichWayToRotate=vectorLeftOrRightOfAnotherYZ((_viveRightBase.gameObject.transform.localPosition-_viveLeftBase.gameObject.transform.localPosition)+
                _viveLeftBase.gameObject.transform.localPosition,_viveLeftBase.gameObject.transform.localPosition,_viveIdealRightBase.gameObject.transform.localPosition);
                print("angleyz="+angleLeftBaseNeedsToRotate+", d="+whichWayToRotate+", pos="+_viveRightBase.gameObject.transform.position);
                //_viveLeftBase.gameObject.transform.RotateAround(_viveLeftBase.gameObject.transform.position,mainMarker.gameObject.transform.forward,angleLeftBaseNeedsToRotate*whichWayToRotate);

                */
                //_viveRightBase.gameObject.transform.localEulerAngles=trackers.rightBaseOffsetRot;
                rightHandInitPosition=_viveRightHand.gameObject.transform.localPosition;
                leftHandInitPosition=_viveLeftHand.gameObject.transform.localPosition;
                pegBoardInitPosition=_pegBoard.gameObject.transform.localPosition;
                calibratedOffsets=true;
            }
             //_viveRightHand.gameObject.transform.RotateAround(_viveLeftBase.gameObject.transform.position,_viveLeftBase.gameObject.transform.forward,angleLeftBaseNeedsToRotate*-whichWayToRotate);
            _pegBoard.gameObject.transform.localPosition=pegBoardInitPosition+trackers._pegboardOffset2;
        }else{
                //print("no");
            }
    }

    public void ToOpaqueMode(Material material)
    {
        material.SetOverrideTag("RenderType", "");
        material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }
   
    public void ToFadeMode(Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
    }

    class ViveTrackerClient : MonoBehaviour
    {
        private int port = 8550;
        private ParentedViveTrackers _PegLocalTransform;
        private bool _ReceiveData = false;
        private bool handler_registered = false;
        private object lock_receive = new object();
        private object lock_data = new object();
        public TextMesh outputText;

        public int numReceives=0;

        //public TextMesh textMesh1;
        //public TextMesh textMesh2;
        //public TextMesh textMesh3;
        public bool ReceiveData
        {
            get
            {
                lock (lock_receive)
                {
                    return _ReceiveData;
                }
            }
            set
            {
                lock (lock_receive)
                {
                    _ReceiveData = value;
                }
            }

        }

        public ParentedViveTrackers lastPegLocalTransform
        {
            get
            {
                lock (lock_data)
                {
                    return _PegLocalTransform;
                }
            }
            private set
            {
                lock (lock_data)
                {
                    _PegLocalTransform = value;
                }
            }
        }

#if UNITY_WSA_10_0 && !UNITY_EDITOR
        public DatagramSocket socket;
        IOutputStream outstream;
        //DataReader reader;
        DataWriter writer;
#else
        UdpClient udp;
#endif

        IPEndPoint ep;

        public string lastMessage="NOTHING";

#if UNITY_WSA_10_0 && !UNITY_EDITOR
        public async void Init(int _port)
        {
            this.port = _port;

            socket = new DatagramSocket();
            Debug.Log("port number is " + port);
            RegisterMsgHandler();
            await socket.BindServiceNameAsync(port.ToString());
            //outstream = socket.GetOutputStreamAsync(new HostName(ep.Address.ToString()), port.ToString()).GetResults();
            //writer = new DataWriter(outstream);
        }
#else
        public void Init(int _port)
        {
            udp = new UdpClient(port);
            udp.BeginReceive(new AsyncCallback(receiveMsg), null);
            Debug.Log("pc port number is " + port);
            //outputText.text="pc port number is " + port;
        }

#endif


#if UNITY_WSA_10_0 && !UNITY_EDITOR
        public void UnregisterMsgHandler()
        {
            handler_registered = false;
            socket.MessageReceived -= SocketOnMessageReceived;
        }

        public void RegisterMsgHandler()
        {
            socket.MessageReceived += SocketOnMessageReceived;
            handler_registered = true;
        }

        //private async void SendMessage(string message)
        //{
        //    var socket = new DatagramSocket();

        //    using (var stream = await socket.GetOutputStreamAsync(new HostName(ep.Address.ToString()), port.ToString()))
        //    {
        //        using (var writer = new DataWriter(stream))
        //        {
        //            var data = Encoding.UTF8.GetBytes(message);

        //            writer.WriteBytes(data);
        //            writer.StoreAsync();
        //            //Debug.Log("sent " + data.Length);
        //        }
        //    }
        //}

        private async void SocketOnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            //Debug.Log("RECEIVED VOID");
            if (!handler_registered)
            {
                return;
            }
            //Debug.Log("RECEIVED VOID");
            //if(Thread.CurrentThread != mainThread)
            //{
            //    Debug.Log("cast receiver handler is not on main thread");
            //}
            var result = args.GetDataStream();
            var resultStream = result.AsStreamForRead(1400);

            using (var reader = new StreamReader(resultStream))
            {
                var text = await reader.ReadToEndAsync();
                //var text = reader.ReadToEnd();
                //Debug.Log("MESSAGE: " + text);
                handleMsg(text);

                
            }
        }
#else

#endif

        void receiveMsg(IAsyncResult result)
        {
           // Debug.Log("RECEIVING");
            numReceives++;
#if UNITY_WSA_10_0 && !UNITY_EDITOR

#else
            IPEndPoint source = new IPEndPoint(IPAddress.Any, 0);
            //byte[] message = udp.EndReceive(result, ref source);
            //Debug.Log("RECV " + Encoding.UTF8.GetString(message) + " from " + source);
            string message = Encoding.UTF8.GetString(udp.EndReceive(result, ref source));
            //outputText.text="RECV " + message + " from " + source;
            //string message = Encoding.UTF8.GetString(udp.EndReceive(result, ref source));

            handleMsg(message);
            // schedule the next receive operation once reading is done:
            udp.BeginReceive(new AsyncCallback(receiveMsg), udp);
           
#endif
        }

        void handleMsg(string msg)
        {
            //textMesh.text=msg;
            //textMesh3.text=msg;
            ParentedViveTrackers pt = JsonUtility.FromJson<ParentedViveTrackers>(msg);
            lastMessage=msg;
            if (!ReceiveData)
            {
                ReceiveData = true;
                Debug.Log("here");
            }
            //print("handle");
            lastPegLocalTransform = pt;
            //textMesh2.text=""+pt;
            //Debug.Log("received this msg:"+msg);
            return;
        }

        public void Stop()
        {
#if UNITY_WSA_10_0 && !UNITY_EDITOR

#else
            if (udp != null)
                udp.Close();
#endif
        }
    }

}
