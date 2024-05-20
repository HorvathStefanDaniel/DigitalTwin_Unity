using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LED : MonoBehaviour
{
    private string ip = "192.168.217.13";
    private int port = 3002;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            UDPManager.Instance.SendUDPMessage("LED|1", ip, port);    
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            UDPManager.Instance.SendUDPMessage("LED|0", ip, port);
        }
    }
}
