using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;

public class SerialDataReceiver : MonoBehaviour
{
    [SerializeField] private string PortName = "COM3";
    [SerializeField] private int BaudRate = 115200;
    [SerializeField] private Saber SaberScript;

    private SerialPort _serialPort;
    private Thread _readThread;
    private bool _isRunning = false;
    private string _receivedData = "";
    private object _lockObject = new object();

    private void Start()
    {
        if (SaberScript == null)
        {
            Debug.LogError("Saber script not assigned to SerialDataReceiver! Please assign it in the Inspector.");
            enabled = false;
            return;
        }
        OpenSerialPort();
    }

    private void OpenSerialPort()
    {
        _serialPort = new SerialPort(PortName, BaudRate);
        _serialPort.ReadTimeout = 500;
        _serialPort.NewLine = "\n";

        try
        {
            _serialPort.Open();
            _isRunning = true;
            _readThread = new Thread(ReadSerial);
            _readThread.Start();
            Debug.Log("Serial port opened: " + PortName);
        }
        catch (Exception e)
        {
            Debug.LogError("Could not open serial port: " + e.Message);
        }
    }

    private void ReadSerial()
    {
        while (_isRunning && _serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                var line = _serialPort.ReadLine();
                lock (_lockObject)
                {
                    _receivedData = line;
                }
            }
            catch (TimeoutException) { }
            catch (Exception e)
            {
                if (_isRunning)
                {
                    Debug.LogError("Error reading serial port: " + e.Message);
                }
                break;
            }
        }
        Debug.Log("Serial read thread stopped.");
    }

    private void Update()
    {
        lock (_lockObject)
        {
            if (!string.IsNullOrEmpty(_receivedData))
            {
                ParseData(_receivedData);
                _receivedData = "";
            }
        }
    }

    private void ParseData(string data)
    {
        var values = data.Split(',');

        if (values.Length == 6)
        {
            try
            {
                var ax = float.Parse(values[0]);
                var ay = float.Parse(values[1]);
                var az = float.Parse(values[2]);

                var gx = float.Parse(values[3]);
                var gy = float.Parse(values[4]);
                var gz = float.Parse(values[5]);

                SaberScript.SetSensorData(new Vector3(ax, ay, az), new Vector3(gx, gy, gz));
            }
            catch (FormatException fex)
            {
                Debug.LogError("Error parsing data: " + fex.Message + " Data: " + data);
            }
            catch (IndexOutOfRangeException iex)
            {
                Debug.LogError("Index out of range when parsing data: " + iex.Message + " Data: " + data);
            }
        }
        else
        {
            Debug.LogWarning("Received malformed data: " + data);
        }
    }

    private void OnApplicationQuit()
    {
        CloseSerialPort();
    }

    private void OnDisable()
    {
        CloseSerialPort();
    }

    private void CloseSerialPort()
    {
        if (_isRunning)
        {
            _isRunning = false;
            if (_readThread != null && _readThread.IsAlive)
            {
                _readThread.Join();
            }
        }

        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
            _serialPort.Dispose();
            Debug.Log("Serial port closed.");
        }
    }
}