using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Vuforia;
using System.Text;
using System.IO;
using System.Linq;
using System.Net;
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
    //
    //int PORT = 8551;
    UdpClient udpClient;
    //using local IP to send to ue4 for ue4 to merge results for hololens to take load from hololens
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
    // Start is called before the first frame update
    void Start()
    {
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
                print("using this endoscope:"+kv.Value.vuforiaWebcamName+", port="+whichEndoscopeToTrack.Value.whichUDPPortToUse);
                udpClient = new UdpClient();
                //don't bind; this is for receivers. binding 2 receivers on same machine causes 1 to crash
                //udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, whichEndoscopeToTrack.Value.whichUDPPortToUse));
                //init udp port here
                break;
            }
        }
        vuforiaCam=GameObject.Find("ARCamera");
    }

    // Update is called once per frame
    void Update()
    {        
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
        Color newColor=Color.green;
        //Transform childCam=foundMarker.gameObject.transform.Find("childCam");
        //childCam.gameObject.GetComponent<MeshRenderer>().material=this.gameObject.GetComponent<MeshRenderer>().material;
        //childCam.gameObject.GetComponent<MeshRenderer>().enabled=false;
        newColor.a=0.5f;
        if (colorPlane){
            if (Array.IndexOf(whichEndoscopeToTrack.Value.nameOfMarkerForBaseCalibration,foundMarker.mTrackableBehaviour.TrackableName)>-1){
                print("___________________________________________found base marker");
                newColor=Color.blue;
                newColor.a=0.5f;
                baseMarker=foundMarker;
            } else{
                //donut marker
                markerToSendInfoFor=foundMarker;
                justFoundDonutMarker=true;
                donutMarkerIsVisible=true;
            }
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
