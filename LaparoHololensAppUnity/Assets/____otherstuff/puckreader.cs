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
public class PuckPositionObject{
    //structured as puckvectorxy, puck2xy relative to puck1 ground truth, righthandpos xyz
    //use the vector to figure out yaw offset 
    public float[] dotsPositions;
    public PuckPositionObject(){

    }
    public PuckPositionObject(float[] f){
        dotsPositions=f;
    }
}


public class puckreader : MonoBehaviour
{
    
    int frame=0; 
    private PegClient Instance;
    public TextMesh framestext;

    bool completedInitialDeltaYaw=false;

    public GameObject puck1;
    public GameObject puck2;
    public GameObject rightHand;

    public float angleBetweenVectors2D(Vector2 v1, Vector2 v2){
        return Mathf.Rad2Deg*Mathf.Acos(Vector2.Dot(v1.normalized,v2.normalized));
    }

    //from https://math.stackexchange.com/questions/274712/calculate-on-which-side-of-a-straight-line-is-a-given-point-located
    //d=(x−x1)(y2−y1)−(y−y1)(x2−x1). d<0 means left. x,y is the point. line from AtoB. A is x1,y1
    public bool pointIsLeftOfVector2D(Vector2 point, Vector2 startOfVector, Vector2 vectorDirection){
        float d=((point.x-startOfVector.x)*(vectorDirection.y-startOfVector.y))-((point.y-startOfVector.y)*(vectorDirection.x-startOfVector.x));
        return d<0;
    }

    // Start is called before the first frame update
    void Start()
    {
                //Debug.Log("start now");
        this.Instance = new PegClient();
        this.Instance.Init(8550);
        //this.Instance.textMesh1=textMesh1;
        //this.Instance.textMesh2=textMesh2;
        //this.Instance.textMesh3=textMesh3;
    }

    // Update is called once per frame
    void Update()
    {
        //in instance save number of times it received data. then divide by time to get avg receiverate
        //framestext.text=""+(1.0f/Time.deltaTime)+"\n"+((float)this.Instance.numReceives/Time.realtimeSinceStartup);
        if (true){
            if (this.Instance.ReceiveData){//if (frame % 2==0){
                //attach gameobjects in unity the same way I have it in unreal to the predefined puck1 location
                //set their relative positions like in unreal
                //find the yaw difference (abs value) and whether or not it's to the left.
                //rotate puck1 correctly
                //the rotation only needs to be done in the 1st message receive b/c yaw offset should always be the same
                //afterwards, I only need the relative pos of the hand trackers
                string messageToProcess=this.Instance.lastMessage;
                framestext.text=messageToProcess;
                char[] delimiterChar={' '};
                string[] parsedMessage=messageToProcess.Split(delimiterChar);
                //0 and 1 are vector. 2 and 3 are puck2 rel to puck1. 4-6 are right hand
                
                if (!completedInitialDeltaYaw){
                    //get the vector from puck1 to puck2 in unity
                    //find angle between those 2 pucks. assume vector start is 0,0
                    //find vector left of other
                    //rotate left or right by the deltayaw based on that
                    Vector2 puck1toPuck2Unity=(puck2.transform.position-puck1.transform.position); //implicit conversion to vector2
                    Vector2 puck1toPuck2Unreal=new Vector2(float.Parse(parsedMessage[0]),float.Parse(parsedMessage[1]));
                    float angle=angleBetweenVectors2D(puck1toPuck2Unity,puck1toPuck2Unreal);
                    bool unrealLeftOfUnity=pointIsLeftOfVector2D(puck1toPuck2Unreal,new Vector2(0,0),puck1toPuck2Unity);
                    //float finalAngle=angle*(unrealLeftOfUnity?)
                    //puck1.transform.eulerAngles=new Vector3(puck1.transform.eulerAngles.x,puck1.transform.eulerAngles.y+)
                }
            }
        }
    }


    class PegClient : MonoBehaviour
    {
        private int port = 8550;
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

        public string lastMessage="NOTHING2";

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
            Debug.Log("RECEIVED VOID PUCK");
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

                handleMsg(text);

                //Debug.Log("MESSAGE: " + text);
            }
        }
#else

#endif

        void receiveMsg(IAsyncResult result)
        {
            Debug.Log("RECEIVING PUCK");
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
            //MarkerPositionObject pt = JsonUtility.FromJson<MarkerPositionObject>(msg);
            lastMessage=msg;
            if (!ReceiveData)
            {
                ReceiveData = true;
                //Debug.Log("here");
            }
            //lastPegLocalTransform = pt;
            //textMesh2.text=""+pt;
            Debug.Log(msg);
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
