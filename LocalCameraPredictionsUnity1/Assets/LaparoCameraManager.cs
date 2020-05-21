using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Vuforia;
using System.Text;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine.UI;
using System.Threading;

#if UNITY_WSA_10_0 && !UNITY_EDITOR
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else
using System.Net.Sockets;

#endif
//using System.Environment;

public class LaparoCameraManager : MonoBehaviour
{
    //wanted to make it hard to accidentally override or reset these values    
    //public DefaultLaparoCameraInfo camInfo;
    //public VuforiaConfiguration c;
    //public TextMesh output;
    //this is pointless to do publicly b/c i can't choose the vuforia camera automatically :/
    public KeyValuePair<LaparoEndoscopes,LaparoInfo> whichEndoscopeToTrack;
    public LaparoEndoscopes nameOfWhichEndo;
    //
    //int PORT = 8551;
    UdpClient udpClient;
    List<UdpClient> receivingClients;
    public List<LaparoEndoscopes> receivingCams;
    //using local PC IP to send to ue4 for ue4 to merge results for hololens to take load from hololens
    public string ipToSendTo="192.168.1.21";

    public nickmarker baseMarker;

    public nickmarker markerToSendInfoFor; //send info for donut marker most confident about or was seen most recently. maybe eventually send all?

    public GameObject vuforiaCam;

    bool sentBaseInfo=false;

    byte[] baseData;

    bool justFoundDonutMarker=false;
    bool donutMarkerIsVisible=false;
    bool imageReceived=false;
    double lastTick=0;
    double lastImageTick=0;
    DateTime lastImage;
    List<double> imageFrequencies;

    public EndoscopeObjectCorrespondences[] endoscopeObjectCorrespondences; //using this forces alignment! makes code uglier but worth it
    public GameObject[] __endoscopeApparatusPrefabs; //will automatically get the pieces


    public ParentedViveTrackers mpo;

    List<IPEndPoint> remoteIpEndPoints;

    public Color lostColor;
    public Color foundColor;
    public Color baseColor;

    public bool showLostMarkers=false;
    public bool showColorPlanes=false;
    public bool showMarkerEdges=true;
    public bool showEntireMarkers=false;
    public GameObject positionedPegboard;
    public GameObject myMarkerToPlace; 
    public float opacityWhenMostRecentCam=0.9f;
    public float opacityWhenNotMostRecentCam=0.05f;
    private Color[] donutColorsWhenVisible;
    private Color[] donutColorsWhenNotVisible;
    public bool receiveOtherCams=false;
    // Start is called before the first frame update
    void Start()
    {
        remoteIpEndPoints= new List<IPEndPoint>();
        receivingClients=new List<UdpClient>();
        lastImage=DateTime.Now;
        imageFrequencies=new List<double>();
        Application.targetFrameRate = 300;
        //i have no idea how anyone would know this property... it doesn't appear in the vuforia api. found it accidentally at
        //https://github.com/simformsolutions/IoTAR/blob/master/unity_ar/unity_project/Assets/Resources/VuforiaConfiguration.asset
        print(VuforiaConfiguration.Instance.WebCam.DeviceNameSetInEditor);
        //VuforiaConfiguration.Instance.WebCam.DeviceNameSetInEditor="Endoscope50";
        //VuforiaConfiguration.WebCamConfiguration.DeviceNameSetInEditor="Endoscope50";
        VuforiaARController.Instance.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);
        //might want to check that there are no duplicates in configuration 

        //basically, just need to figure out which camera to use based on the DefaultLaparoCameraInfo, set in unity, then everything else handled
        //maybe should key by string instead.... not a big deal
        foreach (KeyValuePair<LaparoEndoscopes,LaparoInfo> kv in this.gameObject.GetComponent<DefaultLaparoCameraInfo>().laparoCameraInfoPairs){
            if (kv.Value.vuforiaWebcamName==VuforiaConfiguration.Instance.WebCam.DeviceNameSetInEditor){
                whichEndoscopeToTrack=kv;
                nameOfWhichEndo=kv.Key;
                print("using this endoscope:"+kv.Value.vuforiaWebcamName+", port="+whichEndoscopeToTrack.Value.whichUDPPortToUse);
                udpClient = new UdpClient();
                
                //don't bind; this is for receivers. binding 2 receivers on same machine causes 1 to crash
                //udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, whichEndoscopeToTrack.Value.whichUDPPortToUse));
                //init udp port here
                //break;
            } else{
                
            }
        }
        
        vuforiaCam=GameObject.Find("ARCamera");
        Text t=(Text)GameObject.Find("camName").GetComponent<Text>();
        UnityEngine.UI.Image im=(UnityEngine.UI.Image)GameObject.Find("camBorder").GetComponent<UnityEngine.UI.Image>();
        t.text=""+whichEndoscopeToTrack.Key;
        t.color=whichEndoscopeToTrack.Value.colorForDisplay;
        im.color=whichEndoscopeToTrack.Value.colorForDisplay;

        /*
        if (whichEndoscopeToTrack.Key==LaparoEndoscopes.BottomRight){
            receiveingClient = new UdpClient(this.gameObject.GetComponent<DefaultLaparoCameraInfo>().laparoCameraInfoPairs[LaparoEndoscopes.TopRight].whichUDPPortToUse);
            receiveingClient.Client.Blocking = false;
            print("__________________________________________i am bottom right");
            //this.Instance = new ViveTrackerClient();
            //this.Instance.Init(this.gameObject.GetComponent<DefaultLaparoCameraInfo>().laparoCameraInfoPairs[LaparoEndoscopes.TopRight].whichUDPPortToUse);
            RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

        }*/
        
        endoscopeObjectCorrespondences=new EndoscopeObjectCorrespondences[Enum.GetNames(typeof(LaparoEndoscopes)).Length];
        //this.Instance.textMesh1=textMesh1;
        //this.Instance.textMesh2=textMesh2;
        //this.Instance.textMesh3=textMesh3;
        donutColorsWhenVisible=new Color[this.gameObject.GetComponent<DefaultLaparoCameraInfo>().laparoCameraInfoPairs.Count];
        donutColorsWhenNotVisible=new Color[this.gameObject.GetComponent<DefaultLaparoCameraInfo>().laparoCameraInfoPairs.Count];
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
            LaparoInfo currentInfo=(this.gameObject.GetComponent<DefaultLaparoCameraInfo>().laparoCameraInfoPairs)[(LaparoEndoscopes)i];
            donutColorsWhenVisible[i]=new Color(currentInfo.colorForDisplay.r,
            currentInfo.colorForDisplay.g,
            currentInfo.colorForDisplay.b,
            opacityWhenMostRecentCam);
            donutColorsWhenNotVisible[i]=new Color(currentInfo.colorForDisplay.r,
            currentInfo.colorForDisplay.g,
            currentInfo.colorForDisplay.b,
            opacityWhenNotMostRecentCam);
            endoscopeObjectCorrespondences[i].donutForTrackingCamVisuals.material.color=donutColorsWhenVisible[i];
            endoscopeObjectCorrespondences[i].baseMarkerTrackingCamVisuals.material.color=donutColorsWhenVisible[i];
            endoscopeObjectCorrespondences[i].baseMarkerVisuals.material.color=donutColorsWhenVisible[i];
            endoscopeObjectCorrespondences[i].donutTrackingCamVisuals.material.color=donutColorsWhenVisible[i];
        }
        if (receiveOtherCams){//currently creates too many threads; breaks vuforia; need limit on num listeners
            foreach (KeyValuePair<LaparoEndoscopes,LaparoInfo> kv in this.gameObject.GetComponent<DefaultLaparoCameraInfo>().laparoCameraInfoPairs){
                //creates listeners of other cameras to display their predictions (not sure if accurate)
                if (kv.Key != whichEndoscopeToTrack.Key && whichEndoscopeToTrack.Value.nearbyCams.Contains(kv.Key)){ //currently creates too many threads; breaks vuforia
                    receivingClients.Add(new UdpClient(kv.Value.whichUDPPortToUse));
                    receivingClients[receivingClients.Count-1].Client.Blocking=false;
                    remoteIpEndPoints.Add(new IPEndPoint(IPAddress.Any, kv.Value.whichUDPPortToUse));
                    receivingCams.Add(kv.Key);
                }
                else{
                    endoscopeObjectCorrespondences[(int)kv.Key].donutForTrackingCamVisuals.GetComponent<MeshRenderer>().enabled=false;
                    endoscopeObjectCorrespondences[(int)kv.Key].baseMarkerTrackingCamVisuals.GetComponent<MeshRenderer>().enabled=false;
                    endoscopeObjectCorrespondences[(int)kv.Key].baseMarkerVisuals.GetComponent<MeshRenderer>().enabled=false;
                    endoscopeObjectCorrespondences[(int)kv.Key].donutTrackingCamVisuals.GetComponent<MeshRenderer>().enabled=false;
                }
            }
        } else{
            //positionedPegboard.SetActive(false);
            foreach (KeyValuePair<LaparoEndoscopes,LaparoInfo> kv in this.gameObject.GetComponent<DefaultLaparoCameraInfo>().laparoCameraInfoPairs){
                endoscopeObjectCorrespondences[(int)kv.Key].donutForTrackingCamVisuals.GetComponent<MeshRenderer>().enabled=false;
                endoscopeObjectCorrespondences[(int)kv.Key].baseMarkerTrackingCamVisuals.GetComponent<MeshRenderer>().enabled=false;
                endoscopeObjectCorrespondences[(int)kv.Key].baseMarkerVisuals.GetComponent<MeshRenderer>().enabled=false;
                endoscopeObjectCorrespondences[(int)kv.Key].donutTrackingCamVisuals.GetComponent<MeshRenderer>().enabled=false;
            }
        }

        myMarkerToPlace=((GameObject)__endoscopeApparatusPrefabs[(int)whichEndoscopeToTrack.Key]).transform.Find("baseMarkerVisuals").gameObject;
        myMarkerToPlace.transform.parent=null;
        __endoscopeApparatusPrefabs[(int)whichEndoscopeToTrack.Key].transform.parent=myMarkerToPlace.transform;
        positionedPegboard.transform.SetParent(myMarkerToPlace.transform);
    }

    // Update is called once per frame
    void Update()
    {      
        //myMarkerToPlace.transform.position=baseMarker.transform.position;
        //myMarkerToPlace.transform.eulerAngles=baseMarker.transform.eulerAngles;  
        //positionedPegboard.transform.SetParent(myMarkerToPlace.transform);
        //Application.targetFrameRate = 300;
       // print("deltaTime:"+Time.deltaTime+", "+System.DateTime.Now.Ticks+", "+
        //System.DateTime.Now.ToString("HH:mm:ss.fffffff")+", ___"+((System.DateTime.Now.Ticks-lastTick)/ TimeSpan.TicksPerMillisecond));
        
        if (imageReceived){
            double delta=(double)((System.DateTime.Now.Ticks-lastImageTick)/ TimeSpan.TicksPerMillisecond);
            //TimeSpan time=DateTime.Now-lastImage; basically same as delta
            //print("IMAGE RECEIVED: "+ delta+", "+time.Milliseconds);
            imageReceived=false;
            lastImageTick=System.DateTime.Now.Ticks;
            //imageFrequencies.Add(delta);
            //print(imageFrequencies[imageFrequencies.Count-1]);
            //lastImage=DateTime.Now;
        }
        lastTick=System.DateTime.Now.Ticks;
        //string[] arguments = Environment.GetCommandLineArgs();
        //it should only constantly send data about the donut. base marker only sends once. does not send donut/camera info til base found
        //if (markerToSendInfoFor){  

        //}
        
        if (sentBaseInfo){
            if (markerToSendInfoFor){        
                //print("sending donut");        
                sendInfoAboutMarker("DONUT",markerToSendInfoFor,true, false);
                //sendInfoAboutMarker("BASE",baseMarker,true);
                udpClient.Send(baseData, baseData.Length, ipToSendTo, whichEndoscopeToTrack.Value.whichUDPPortToUse);
                //disable tracking for base marker and still send position
            } else if (whichEndoscopeToTrack.Key==LaparoEndoscopes.LeftHand || whichEndoscopeToTrack.Key==LaparoEndoscopes.RightHand){
                //then tell unreal we don't have it
                byte[] lostData=Encoding.UTF8.GetBytes(whichEndoscopeToTrack.Key+" LOST");
                udpClient.Send(lostData, lostData.Length, ipToSendTo, whichEndoscopeToTrack.Value.whichUDPPortToUse);
            }
        } else{
            //print("sending base");
            if (baseMarker){                
                sendInfoAboutMarker("BASE",baseMarker,true, true);
                sentBaseInfo=true;
            }
        }
        /*if (this.Instance){
            if (this.Instance.ReceiveData){
                //testVecOut.text=""+this.Instance.lastPegLocalTransform.rightControllerOffset;
                //positionTrackers(this.Instance.lastPegLocalTransform);
                print("___________________received");
            }
        }*/
        
        for(int otherCamID=0;otherCamID<receivingClients.Count;otherCamID++){
            if (remoteIpEndPoints[otherCamID] != null){
                try{
                    IPEndPoint thisEndpoint=remoteIpEndPoints[otherCamID];
                    byte[] data=receivingClients[otherCamID].Receive(ref thisEndpoint);
                    //print("____________received "+Encoding.UTF8.GetString(data));
                    string[] splitMessage=Encoding.UTF8.GetString(data).Split(' ');
                    if (splitMessage[1].Equals("BASE")){
                        //print("_____________base");
                        EndoscopeObjectCorrespondences endo=endoscopeObjectCorrespondences[(int)Enum.Parse(typeof(LaparoEndoscopes), splitMessage[0])];
                        endo.baseMarkerTrackingCam.transform.localPosition=new Vector3(float.Parse(splitMessage[2]),float.Parse(splitMessage[3]),float.Parse(splitMessage[4]));
                        endo.baseMarkerTrackingCam.transform.localEulerAngles=new Vector3(float.Parse(splitMessage[5]),float.Parse(splitMessage[6]),float.Parse(splitMessage[7]));
                    } else{
                        int camID=(int)Enum.Parse(typeof(LaparoEndoscopes), splitMessage[0]);
                        EndoscopeObjectCorrespondences endo=endoscopeObjectCorrespondences[camID];

                        endo.donutTrackingCam.transform.localPosition=new Vector3(float.Parse(splitMessage[2]),float.Parse(splitMessage[3]),float.Parse(splitMessage[4]));
                        endo.donutTrackingCam.transform.localEulerAngles=new Vector3(float.Parse(splitMessage[5]),float.Parse(splitMessage[6]),float.Parse(splitMessage[7]));
                        endo.donutForTrackingCam.transform.position=endo.baseMarker.transform.position;
                        endo.donutForTrackingCam.transform.eulerAngles=endo.baseMarker.transform.eulerAngles;
                        endo.donutTrackingCam.transform.position=endo.baseMarkerTrackingCam.transform.position;
                        endo.donutTrackingCam.transform.eulerAngles=endo.baseMarkerTrackingCam.transform.eulerAngles;
                        endo.donutForTrackingCamVisuals.material.color=donutColorsWhenVisible[camID];
                        endo.baseMarkerTrackingCamVisuals.material.color=donutColorsWhenVisible[camID];
                        endo.baseMarkerVisuals.material.color=donutColorsWhenVisible[camID];
                        endo.donutTrackingCamVisuals.material.color=donutColorsWhenVisible[camID];
                    }
                } catch(Exception e){
                    //print("_received nothing "+otherCamID);
                    //int camID=otherCamID>=(int)whichEndoscopeToTrack.Key?otherCamID+1:otherCamID;
                    int camID=(int)receivingCams[otherCamID]; //technically could have done this above; was an afterthought
                    EndoscopeObjectCorrespondences endo=endoscopeObjectCorrespondences[camID];
                    endo.donutForTrackingCamVisuals.material.color=donutColorsWhenNotVisible[camID];
                    endo.baseMarkerTrackingCamVisuals.material.color=donutColorsWhenNotVisible[camID];
                    endo.baseMarkerVisuals.material.color=donutColorsWhenNotVisible[camID];
                    endo.donutTrackingCamVisuals.material.color=donutColorsWhenNotVisible[camID];
                }
            }
        }
        /*
        if (RemoteIpEndPoint != null){
            //print("try receive");
            try{
                byte[] data=receiveingClient.Receive(ref RemoteIpEndPoint);
                print("____________received "+Encoding.UTF8.GetString(data));
                string[] splitMessage=Encoding.UTF8.GetString(data).Split(' ');
                if (splitMessage[1].Equals("BASE")){
                    print("_____________base");
                    EndoscopeObjectCorrespondences endo=endoscopeObjectCorrespondences[(int)Enum.Parse(typeof(LaparoEndoscopes), splitMessage[0])];
                    endo.baseMarkerTrackingCam.transform.localPosition=new Vector3(float.Parse(splitMessage[2]),float.Parse(splitMessage[3]),float.Parse(splitMessage[4]));
                    endo.baseMarkerTrackingCam.transform.localEulerAngles=new Vector3(float.Parse(splitMessage[5]),float.Parse(splitMessage[6]),float.Parse(splitMessage[7]));
                    //endo.donutTrackingCam.transform.position=endo.baseMarkerTrackingCam.transform.position;
                    //endo.donutTrackingCam.transform.eulerAngles=endo.baseMarkerTrackingCam.transform.eulerAngles;
                } else{
                    EndoscopeObjectCorrespondences endo=endoscopeObjectCorrespondences[(int)Enum.Parse(typeof(LaparoEndoscopes), splitMessage[0])];

                    endo.donutTrackingCam.transform.localPosition=new Vector3(float.Parse(splitMessage[2]),float.Parse(splitMessage[3]),float.Parse(splitMessage[4]));
                    endo.donutTrackingCam.transform.localEulerAngles=new Vector3(float.Parse(splitMessage[5]),float.Parse(splitMessage[6]),float.Parse(splitMessage[7]));
                    endo.donutForTrackingCam.transform.position=endo.baseMarker.transform.position;
                    endo.donutForTrackingCam.transform.eulerAngles=endo.baseMarker.transform.eulerAngles;
                    endo.donutTrackingCam.transform.position=endo.baseMarkerTrackingCam.transform.position;
                    endo.donutTrackingCam.transform.eulerAngles=endo.baseMarkerTrackingCam.transform.eulerAngles;
                }
            } catch(Exception e){

            }
        }*/
/*
        try{

            // Blocks until a message returns on this socket from a remote host.
            Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);

            string returnData = Encoding.ASCII.GetString(receiveBytes);

            print("This is the message you received " +
                                    returnData.ToString());
            print("This message was sent from " +
                                        RemoteIpEndPoint.Address.ToString() +
                                        " on their port number " +
                                        RemoteIpEndPoint.Port.ToString());
        }
        catch ( Exception e ){
            print(e.ToString());
        }*/
    }

    void OnApplicationQuit()
    {
        //Debug.Log("Application ending after " + Time.time + " seconds");
        //string path = "Assets/Resources/____timingData.txt";
        File.WriteAllLines(
            "____timingData.txt" // <<== Put the file name here
        ,   imageFrequencies.ToArray().Select(d => d.ToString("0.####"))
        );
    }

    void sendInfoAboutMarker(string header, nickmarker marker, bool useChildCam, bool isBase){
        //send position of camera
        Transform childCam=marker.gameObject.transform.Find("childCam");
        if (useChildCam){
            if (childCam){
                childCam.transform.position=vuforiaCam.transform.position;
                childCam.transform.eulerAngles=vuforiaCam.transform.eulerAngles;
                //should also send camera info so unreal knows which camera to position. just need to send position
                //also need rot

                //ue4 vis
                //var data = Encoding.UTF8.GetBytes(whichEndoscopeToTrack.Key+" "+header+" " + (-childCam.transform.localPosition.z)+" "+childCam.transform.localPosition.x+" "+
                //(-childCam.transform.localPosition.y)+" "+(childCam.transform.localEulerAngles.z-180)+" "+(-childCam.transform.localEulerAngles.x)+" "+(-childCam.transform.localEulerAngles.y));

                byte[] data = Encoding.UTF8.GetBytes(whichEndoscopeToTrack.Key+" "+header+" " + childCam.transform.localPosition.x+" "+childCam.transform.localPosition.y+" "+
                childCam.transform.localPosition.z+" "+childCam.transform.localEulerAngles.x+" "+childCam.transform.localEulerAngles.y+" "+childCam.transform.localEulerAngles.z+" "+
                (justFoundDonutMarker?1:0)+" "+(donutMarkerIsVisible?1:0));

                if (isBase){
                    baseData=data;
                } else{
                    if (justFoundDonutMarker){
                        justFoundDonutMarker=false;
                    }
                }
                udpClient.Send(data, data.Length, ipToSendTo, whichEndoscopeToTrack.Value.whichUDPPortToUse);
            }else{
                print("no child cam attached");
                UnityEditor.EditorApplication.isPlaying = false;
            }
        } else{
            //just send donut itself
            childCam.transform.position=vuforiaCam.transform.position;
            childCam.transform.eulerAngles=vuforiaCam.transform.eulerAngles;
            marker.gameObject.transform.parent=childCam;
            var data = Encoding.UTF8.GetBytes(whichEndoscopeToTrack.Key+" "+header+" " + marker.transform.localPosition.x+" "+marker.transform.localPosition.y+" "+
            marker.transform.localPosition.z+" "+marker.transform.localEulerAngles.x+" "+marker.transform.localEulerAngles.y+" "+marker.transform.localEulerAngles.z);

            print(marker.transform.localPosition);

            udpClient.Send(data, data.Length, ipToSendTo, whichEndoscopeToTrack.Value.whichUDPPortToUse);
            childCam.gameObject.transform.parent=marker.gameObject.transform;
        }
    }

    public void markerFound(nickmarker foundMarker){
        //replace sending marker
        print("!!!!!!!!!!!!!!!!!! found foundMarker="+foundMarker.mTrackableBehaviour.TrackableName+ ", base="+string.Join(",", whichEndoscopeToTrack.Value.nameOfMarkerForBaseCalibration));
        Transform colorPlane=foundMarker.gameObject.transform.Find("colorPlane");
        Color newColor=foundColor;
        //Transform childCam=foundMarker.gameObject.transform.Find("childCam");
        //childCam.gameObject.GetComponent<MeshRenderer>().material=this.gameObject.GetComponent<MeshRenderer>().material;
        //childCam.gameObject.GetComponent<MeshRenderer>().enabled=false;
        //newColor.a=0.5f;
        //foundMarker.transform.Find("markerEdges").GetComponent<MeshRenderer>().enabled=showMarkerEdges;
        //foundMarker.GetComponent<MeshRenderer>().enabled=showEntireMarkers;
        if (colorPlane){
            if (Array.IndexOf(whichEndoscopeToTrack.Value.nameOfMarkerForBaseCalibration,foundMarker.mTrackableBehaviour.TrackableName)>-1){
                print("___________________________________________found base marker");
                newColor=baseColor;
                baseMarker=foundMarker;
                //positionedPegboard.transform.parent=myMarkerToPlace.transform;
                myMarkerToPlace.transform.parent=baseMarker.transform;
                myMarkerToPlace.transform.localPosition=Vector3.zero;
                myMarkerToPlace.transform.localEulerAngles=new Vector3(-90,180,0);
            } else{
                //donut marker
                markerToSendInfoFor=foundMarker;
                justFoundDonutMarker=true;
                donutMarkerIsVisible=true;
            }
            colorPlane.GetComponent<MeshRenderer>().enabled=showColorPlanes;
        }else{
            print("no color plane");
            UnityEditor.EditorApplication.isPlaying = false;
        }

        MeshRenderer meshRenderer=colorPlane.gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer){
            colorPlane.gameObject.GetComponent<MeshRenderer>().material.color=newColor;
        } else{
            print("no mesh");
        }

        
    }

    public void markerLost(nickmarker lostMarker){
        //lostMarker.transform.Find("markerEdges").GetComponent<MeshRenderer>().enabled=showLostMarkers && showMarkerEdges;
        lostMarker.transform.Find("markerEdges").GetComponent<MeshRenderer>().enabled=false;
        //lostMarker.GetComponent<MeshRenderer>().enabled=showLostMarkers && showEntireMarkers;
        lostMarker.GetComponent<MeshRenderer>().enabled=false;
        Transform colorPlane=lostMarker.gameObject.transform.Find("colorPlane");
        if (colorPlane){
            Color newColor=Color.red;
            newColor.a=0.25f;
            MeshRenderer meshRenderer=colorPlane.gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer){
                colorPlane.gameObject.GetComponent<MeshRenderer>().material.color=newColor;
                colorPlane.GetComponent<MeshRenderer>().enabled=showLostMarkers && showColorPlanes;
            } else{
                print("no mesh");
            }
        }


        //stop it from sending info IF it's a donut marker and is markerToSendInfoFor
        if (lostMarker==markerToSendInfoFor){
            markerToSendInfoFor=null;
        }

        if (!(Array.IndexOf(whichEndoscopeToTrack.Value.nameOfMarkerForBaseCalibration,lostMarker.mTrackableBehaviour.TrackableName)>-1)){
            //don't care if base marker gets lose; this can really only happen with the manipulators anyway
            donutMarkerIsVisible=false;
        }
    }

    void OnTrackablesUpdated(){
        imageReceived=true;
    }


    
}
