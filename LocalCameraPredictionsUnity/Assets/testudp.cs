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

public class testudp : MonoBehaviour
{
    int PORT = 8563;
    UdpClient udpClient;
    public string ip="192.168.1.2";
    public nickmarker normalMarker;
    // Start is called before the first frame update
    void Start()
    {
        udpClient = new UdpClient();
        //udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));
    }

    // Update is called once per frame
    void Update()
    {
        //print("dist: "+(this.gameObject.transform.position-normalMarker.gameObject.transform.position).magnitude);
        var data = Encoding.UTF8.GetBytes("ABCD");
        udpClient.Send(data, data.Length, ip, PORT);
    }
}
