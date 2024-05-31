using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPManager : MonoBehaviour
{
    public static UDPManager Instance { get; private set; }

    [Header("UDP Settings")]
    [SerializeField] private int UDPPort = 50195;
    [SerializeField] private bool displayUDPMessages = false;
    private UdpClient udpClient;
    private IPEndPoint endPoint;
    private bool isClosing = false;

    // ESP32 Sensor Angles
    public int AngleA { get; private set; } = 0;
    public int AngleB { get; private set; } = 0;
    public int AngleC { get; private set; } = 0;
    public int PlateAngle { get; private set; } = 0;
    public long Distance { get; private set; } = 0;

    public string esp32Ip = "192.168.137.125";

    public SensorScript sensorScript; // Reference to the SensorScript

    public Transform Axis1; // Reference to Axis 1 Transform (Joint A)
    public Transform Axis2; // Reference to Axis 2 Transform (Joint B)
    public Transform Axis3; // Reference to Axis 3 Transform (Joint C)
    public Transform Plate;

    private string receivedData;
    private bool dataReceived;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        DisplayIPAddress();
        endPoint = new IPEndPoint(IPAddress.Any, UDPPort);
        udpClient = new UdpClient(endPoint);
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    void Update()
    {
        if (dataReceived)
        {
            HandleReceivedData(receivedData);
            dataReceived = false;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SendUDPMessage("D:start");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SendUDPMessage("D:stop");
        }
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        if (isClosing)
            return;

        try
        {
            byte[] receivedBytes = udpClient.EndReceive(result, ref endPoint);
            string data = Encoding.UTF8.GetString(receivedBytes);

            if (displayUDPMessages)
            {
                Debug.Log("Received data from " + endPoint.Address.ToString() + ": " + data);
            }

            // Store the received data to be processed in the Update method
            receivedData = data;
            dataReceived = true;

            udpClient.BeginReceive(ReceiveCallback, null);
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving UDP data: " + e.Message);
        }
    }

    public void SendUDPMessage(string message)
    {
        UdpClient client = new UdpClient();
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, esp32Ip, 3002);
            if (displayUDPMessages)
            {
                Debug.Log("UDP message sent: " + message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending UDP message: " + e.Message);
        }
        finally
        {
            client.Close();
        }
    }

    void DisplayIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Debug.Log("Local IP Address: " + ip.ToString());
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error fetching local IP address: " + ex.Message);
        }
    }

    private void OnDestroy()
    {
        isClosing = true;
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }

    private void HandleReceivedData(string data)
    {
        if (data.StartsWith("Sensors"))
        {
            string[] parts = data.Split('|');
            foreach (string part in parts)
            {
                if (part.StartsWith("A:"))
                {
                    float espAngle = float.Parse(part.Substring(2));
                    AngleA = NormalizeAngle((int)MapAngleA(espAngle));
                }
                else if (part.StartsWith("B:"))
                {
                    float espAngle = float.Parse(part.Substring(2));
                    AngleB = NormalizeAngle((int)MapAngleB(espAngle));
                }
                else if (part.StartsWith("C:"))
                {
                    float espAngle = float.Parse(part.Substring(2));
                    AngleC = NormalizeAngle( (int)MapAngleC(espAngle));
                }
                else if (part.StartsWith("Plate:"))
                {
                    PlateAngle = int.Parse(part.Substring(6));
                }
                else if (part.StartsWith("Dist:"))
                {
                    Distance = long.Parse(part.Substring(5));
                }
            }

            if (displayUDPMessages)
            {
                Debug.Log($"Angles - A: {AngleA}, B: {AngleB}, C: {AngleC}, Plate: {PlateAngle}, Distance: {Distance}");
            }

            // Update the distance in the SensorScript and create a point
            if (sensorScript != null)
            {
                sensorScript.distance = Distance / 1.0f; // distance is in cm, convert to meters
                sensorScript.CreatePoint();
            }
            else
            {
                Debug.Log("Sensor script is null");
            }

            // Update the arm angles
            UpdateArmAngles();
        }
        else
        {
            Debug.LogError("Received data is not in the expected format.");
        }
    }


    private float MapAngleA(float espAngle)
    {
        if (espAngle >= 0)
        {
            return Mathf.Lerp(0, -90, Mathf.InverseLerp(0, 84, espAngle));
        }
        else
        {
            return Mathf.Lerp(0, 90, Mathf.InverseLerp(0, -96, espAngle));
        }
    }

    private float MapAngleB(float espAngle)
    {

        float temp = 0f;

        if (espAngle >= 14)
        {
            temp = Mathf.Lerp(0, -120, Mathf.InverseLerp(14, 121, espAngle));
        }
        else
        {
            temp = Mathf.Lerp(1, 50, Mathf.InverseLerp(14, -45, espAngle));
        }
        return temp;

    }



    private float MapAngleC(float espAngle)
    {
        if (espAngle >= 8)
        {
            return Mathf.Lerp(0, -90, Mathf.InverseLerp(8, 90, espAngle));
        }
        else
        {
            return Mathf.Lerp(0, 90, Mathf.InverseLerp(8, -90, espAngle));
        }
    }

    private int NormalizeAngle(float angle)
    {
        while (angle < -180) angle += 360;
        while (angle > 180) angle -= 360;
        return Mathf.RoundToInt(angle);
    }

    private void UpdateArmAngles()
    {
        // Add logging to check the values before they are set
        Debug.Log($"Updating Arm Angles: AngleA: {AngleA}, AngleB: {AngleB}, AngleC: {AngleC}");

        Plate.localEulerAngles = new Vector3(Plate.localEulerAngles.x, Plate.localEulerAngles.y, PlateAngle);

        Quaternion rotationA = Quaternion.Euler(AngleA, 0, 0);
        Quaternion rotationB = Quaternion.Euler(AngleB, 0, 0);
        Quaternion rotationC = Quaternion.Euler(AngleC, 0, 0);

        // Apply rotations using quaternions
        Axis1.localRotation = rotationA;
        Axis2.localRotation = rotationB;
        Axis3.localRotation = rotationC;
    }

}
