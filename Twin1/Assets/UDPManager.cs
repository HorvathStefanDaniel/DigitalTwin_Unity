using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPManager : MonoBehaviour
{
    // Static variable that holds the instance
    public static UDPManager Instance { get; private set; }

    // UDP Settings
    [Header("UDP Settings")]
    [SerializeField] private int UDPPort = 50195;
    [SerializeField] private bool displayUDPMessages = true; // Enable message display for debugging
    private UdpClient udpClient;
    private IPEndPoint endPoint;
    private bool isClosing = false;

    // ESP32 Sensor Angles
    public int AngleA { get; private set; } = 0;
    public int AngleB { get; private set; } = 0;
    public int AngleC { get; private set; } = 0;
    public int PlateAngle { get; private set; } = 0;
    public long Distance { get; private set; } = 0;

    private string esp32Ip = "192.168.137.172";

    void Awake()
    {
        // Assign the instance to this instance, if it is the first one
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get IP Address
        DisplayIPAddress();

        // UDP begin
        endPoint = new IPEndPoint(IPAddress.Any, UDPPort);
        udpClient = new UdpClient(endPoint);
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SendUDPMessage("Servo|A:90|B:90|C:90|D:stop", esp32Ip, 3002);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SendUDPMessage("Servo|A:45|B:45|C:45|D:stop", esp32Ip, 3002);
        }
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        if (isClosing)
            return;

        try
        {
            byte[] receivedBytes = udpClient.EndReceive(result, ref endPoint);
            string receivedData = Encoding.UTF8.GetString(receivedBytes);

            // Log UDP message
            if (displayUDPMessages)
            {
                Debug.Log("Received data from " + endPoint.Address.ToString() + ": " + receivedData);
            }

            // Handle the received data
            HandleReceivedData(receivedData);

            // Begin receiving again
            udpClient.BeginReceive(ReceiveCallback, null);
        }
        catch (ObjectDisposedException)
        {
            // Ignore if the client has been disposed
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving UDP data: " + e.Message);
        }
    }

    // Function to send UDP message
    public void SendUDPMessage(string message, string ipAddress, int port)
    {
        UdpClient client = new UdpClient();
        try
        {
            // Convert the message string to bytes
            byte[] data = Encoding.UTF8.GetBytes(message);

            // Send the UDP message
            client.Send(data, data.Length, ipAddress, port);
            Debug.Log("UDP message sent: " + message);
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
        // Example message format: "Sensors|A:45|B:90|C:135|Plate:180|Dist:50"
        if (data.StartsWith("Sensors"))
        {
            string[] parts = data.Split('|');
            foreach (string part in parts)
            {
                if (part.StartsWith("A:"))
                {
                    AngleA = int.Parse(part.Substring(2));
                }
                else if (part.StartsWith("B:"))
                {
                    AngleB = int.Parse(part.Substring(2));
                }
                else if (part.StartsWith("C:"))
                {
                    AngleC = int.Parse(part.Substring(2));
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

            // Display the received values
            if (displayUDPMessages)
            {
                Debug.Log($"Angles - A: {AngleA}, B: {AngleB}, C: {AngleC}, Plate: {PlateAngle}, Distance: {Distance}");
            }
        }
        else
        {
            Debug.LogError("Received data is not in the expected format.");
        }
    }
}
