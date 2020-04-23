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

[Serializable]
public class MarkerPositionObject{
    public float[] dotsPositions;
    public MarkerPositionObject(){

    }
    public MarkerPositionObject(float[] f){
        dotsPositions=f;
    }
}



public class pointsreader : MonoBehaviour
{
    public DefaultTrackableEventHandler marker;
    int fileIndex=0;
    /*
    0-15 col: pose of the cube. Flattened 4*4 matrix
    16-39 col: 8 red dots
    40-66 col: 9 blue dots
    All are relative to the first camera.
     */
    string[] lines;
    string[] singleLineData;

    public GameObject cube;
    public GameObject nickcube;
    public GameObject proposedCube;

    public GameObject[] points;
    public LayerMask redPoints;
    public LayerMask bluePoints;
    //Transform cubeInitialRot;
    int frame=0;
    float xMultiplier=1.0f;
    float yMultiplier=-1.0f;
    float zMultiplier=1.0f;
    bool didInitialPosition=false;
    
    public TextAsset pointsFile;

    public GameObject[] calibMarkers;

    public GameObject calibPlane;

    public GameObject alignmentPlane;

    private PegClient Instance;

    public MarkerPositionObject mpo;

    public TextMesh framestext;

    public bool usePrecordedData;

    public GameObject baseMesh;

    public TextMesh baseoutput;
    
 
    public String GetIP()
    {
    String strHostName = "";
    strHostName = System.Net.Dns.GetHostName();
    
    IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
    
    IPAddress[] addr = ipEntry.AddressList;
    Debug.Log("addrs "+addr.Length);
    foreach (IPAddress address in addr){
        Debug.Log("this address is "+address.ToString());
    }

    return addr[addr.Length-1].ToString();
    
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("ipip");
        Debug.Log("IP!!!! +"+GetIP());
        Debug.Log("LINES="+lines);
        lines=pointsFile.text.Split('\n');
        Debug.Log("FIRST LINE="+lines[0]);
        //cubeInitialRot=nickcube.transform;
        //string[] words=pointsFile.text.Split('\n');
        //Debug.Log("words="+words[0]);
        Vector3 calibCenter=(calibMarkers[0].transform.position+calibMarkers[1].transform.position+calibMarkers[2].transform.position+calibMarkers[3].transform.position)/4.0f;
        calibPlane.transform.position=calibCenter;
        
        //calibPlane.transform.forward=Vector3.Normalize(calibMarkers[0].transform.position-calibMarkers[2].transform.position);
        //calibPlane.transform.right=Vector3.Normalize(calibMarkers[1].transform.position-calibMarkers[0].transform.position);
        //^^^^^It seems that doing it this way doesn't always work...I think Unity's placing a limit on how many times I can reposition the calibplan or something
        //forward, up
        Vector3 newforward=Vector3.Normalize(calibMarkers[0].transform.position-calibMarkers[2].transform.position);
        Vector3 newright=Vector3.Normalize(calibMarkers[1].transform.position-calibMarkers[0].transform.position);
        Vector3 newup=Vector3.Normalize(Vector3.Cross(newforward,newright));
        calibPlane.transform.eulerAngles=Quaternion.LookRotation(newforward,newup).eulerAngles;
        //Debug.DrawLine(calibPlane.transform.position,calibPlane.transform.position+calibPlane.transform.right,Color.green,100);
        //Debug.DrawLine(calibPlane.transform.position,calibPlane.transform.position+calibPlane.transform.forward,Color.blue,100);
        //Debug.DrawLine(calibPlane.transform.position,calibPlane.transform.position+Vector3.Normalize(calibMarkers[0].transform.position-calibMarkers[2].transform.position),Color.red,100);
        //calibPlane.transform.localEulerAngles=new Vector3(calibPlane.transform.localEulerAngles.x+90,calibPlane.transform.localEulerAngles.y,calibPlane.transform.localEulerAngles.z);
        
        
        
        Debug.Log("Calib forward"+calibPlane.transform.forward);
        Debug.Log("Calib up"+calibPlane.transform.up);

        
        this.transform.parent=calibPlane.transform;
        calibMarkers[0].transform.parent=calibPlane.transform;
        calibMarkers[1].transform.parent=calibPlane.transform;
        calibMarkers[2].transform.parent=calibPlane.transform;
        calibMarkers[3].transform.parent=calibPlane.transform;
        calibPlane.transform.parent=alignmentPlane.transform;
        calibPlane.transform.localEulerAngles=new Vector3(0,180,0);
        calibPlane.transform.localPosition=new Vector3(0,0,0);
        
        
        //calibMarker0-calibmarker2=forward.  0-1=right


        string test2="{\"dotsPositions\":[2.1,4.1]}";
        mpo=JsonUtility.FromJson<MarkerPositionObject>(test2);

        //Debug.Log("start now");
        this.Instance = new PegClient();
        this.Instance.Init(8555);
        //this.Instance.textMesh1=textMesh1;
        //this.Instance.textMesh2=textMesh2;
        //this.Instance.textMesh3=textMesh3;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("test");
        frame++;
        baseoutput.text=""+(baseMesh.transform.position*1000.0f).ToString("F4");
        //in instance save number of times it received data. then divide by time to get avg receiverate
        //framestext.text=""+(1.0f/Time.deltaTime)+"\n"+((float)this.Instance.numReceives/Time.realtimeSinceStartup);
        bool condition1=usePrecordedData?(frame%4==0):true;
        if (condition1){
            bool condition2=usePrecordedData?true:this.Instance.ReceiveData;
        //which vector is closest to right vector of cam
        if (condition2){//if (frame % 2==0){
            framestext.text=""+this.Instance.lastMessage;
            //nickcube.transform.eulerAngles=cubeInitialRot;
            //Debug.Log("current line="+lines[fileIndex]);
            if (usePrecordedData){
                MarkerPositionObject pt = JsonUtility.FromJson<MarkerPositionObject>(lines[fileIndex]);
                //Debug.Log("val="+float.Parse(values[0]));
                positionDots(pt.dotsPositions);
                //positionDots(this.Instance.lastPegLocalTransform.dotsPositions);
                //use deltas
            }else{
                Debug.Log("no prerec data");
                positionDots(this.Instance.lastPegLocalTransform.dotsPositions);
            }

            //holes are around 0,1,2,3 and 4,5,6,7
            nickcube.transform.position=(points[0].transform.position+points[1].transform.position+points[2].transform.position+points[3].transform.position+
                points[4].transform.position+points[5].transform.position+points[6].transform.position+points[7].transform.position)/8.0f;
            //01 make forward vector, 02 make up vector

            nickcube.transform.eulerAngles=Quaternion.LookRotation(Vector3.Normalize(points[1].transform.position-points[0].transform.position),Vector3.Normalize(points[2].transform.position-points[0].transform.position)).eulerAngles;

            if (fileIndex<lines.Length-3){
                fileIndex++;
            }else{
                //Debug.Log("end");
                fileIndex=0;
            }
        }
        //frame++;
        }
    }

    private void positionDots(float[] dotArray){
        //singleLineData = lines[fileIndex].Split(' ');
        int i=0;
        for (int j=0; j<8;j++){
            points[j].transform.localPosition=new Vector3(xMultiplier*dotArray[i], yMultiplier*dotArray[i+1], zMultiplier*dotArray[i+2]);
            i+=3;
        }
    }

    class PegClient : MonoBehaviour
    {
        private int port = 8555;
        private MarkerPositionObject _PegLocalTransform;
        private bool _ReceiveData = false;
        private bool handler_registered = false;
        private object lock_receive = new object();
        private object lock_data = new object();

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

        public MarkerPositionObject lastPegLocalTransform
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
            Debug.Log("RECEIVED VOID");
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
                Debug.Log("MESSAGE: " + text);
                handleMsg(text);

                
            }
        }
#else

#endif

        void receiveMsg(IAsyncResult result)
        {
            Debug.Log("RECEIVING");
            numReceives++;
#if UNITY_WSA_10_0 && !UNITY_EDITOR

#else
            IPEndPoint source = new IPEndPoint(0, 0);
            //byte[] message = udp.EndReceive(result, ref source);
            //Debug.Log("RECV " + Encoding.UTF8.GetString(message) + " from " + source);

            string message = Encoding.UTF8.GetString(udp.EndReceive(result, ref source));

            handleMsg(message);
            // schedule the next receive operation once reading is done:
            udp.BeginReceive(new AsyncCallback(receiveMsg), udp);
           
#endif
        }

        void handleMsg(string msg)
        {
            //textMesh.text=msg;
            //textMesh3.text=msg;
            MarkerPositionObject pt = JsonUtility.FromJson<MarkerPositionObject>(msg);
            lastMessage=msg;
            if (!ReceiveData)
            {
                ReceiveData = true;
                //Debug.Log("here");
            }
            lastPegLocalTransform = pt;
            //textMesh2.text=""+pt;
            Debug.Log("received this msg:"+msg);
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
