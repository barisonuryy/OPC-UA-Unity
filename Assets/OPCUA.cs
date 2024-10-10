using System;
using System.Collections.Generic;
using UnityEngine;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Threading.Tasks;
using TMPro;

public class OPCUA : MonoBehaviour
{
    public List<TMP_Text> tagValueTextMeshProList;
    private List<ObjectMata> matchedTagValues = new List<ObjectMata>();
    private ApplicationInstance application;
    private Session session;
    private JsonHandler jsonHandler;  // Reference to JsonHandler script
    bool isConnected;
    private bool isTimerRunning = false;
    private bool isTimerCompleted = false;

    private string endpointUrl = "opc.tcp://127.0.0.1:49320";

    async void Start()
    {
        jsonHandler = FindObjectOfType<JsonHandler>();  // Initialize JsonHandler
        await InitializeClient();

        if (session != null && session.Connected)
        {
            isConnected = true;
            NodeId channelNodeId = new NodeId("Channel1", 2); // Channel1's NodeId
            BrowseDevicesInChannel(channelNodeId);
            LogMatchedTagValues();
            SaveData();  // Save matched data to JSON
        }
        InvokeRepeating("UpdateTagValues", 2.0f, 2.0f);
    }

    void Update()
    {
        if (!isTimerRunning && !isTimerCompleted)
        {
            StartTimer();
        }

        if (isTimerCompleted)
        {
            isTimerCompleted = false;
        }
    }

    private async Task InitializeClient()
    {
        try
        {
            application = new ApplicationInstance
            {
                ApplicationName = "UnityOPCUAClient",
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = "Opc.Ua.Client"
            };

            ApplicationConfiguration config = new ApplicationConfiguration
            {
                ApplicationName = "UnityOPCUAClient",
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    AutoAcceptUntrustedCertificates = true,
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = "Directory",
                        StorePath = "OPCUACerts",
                        SubjectName = application.ApplicationName
                    }
                },
                ClientConfiguration = new ClientConfiguration
                {
                    DefaultSessionTimeout = 60000
                }
            };

            await config.Validate(ApplicationType.Client);

            config.CertificateValidator.CertificateValidation += (s, e) =>
            {
                e.Accept = true;
            };

            application.ApplicationConfiguration = config;

            EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(endpointUrl, false);
            EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(config);
            ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

            session = await Session.Create(config, endpoint, false, "UnityOPCUAClient", 120000, null, null);

            Debug.Log("Connected to OPC UA server.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connection error: {ex.Message}");
        }
    }

    private void BrowseDevicesInChannel(NodeId channelNodeId)
    {
        if (session == null || !session.Connected)
        {
            Debug.LogError("Session is not connected.");
            return;
        }

        try
        {
            BrowseDescription nodeToBrowse = new BrowseDescription
            {
                NodeId = channelNodeId,
                BrowseDirection = BrowseDirection.Forward,
                IncludeSubtypes = true,
                NodeClassMask = (uint)NodeClass.Object,
                ResultMask = (uint)BrowseResultMask.All
            };

            BrowseResultCollection results;
            DiagnosticInfoCollection diagnosticInfos;

            session.Browse(null, null, 0, new BrowseDescriptionCollection { nodeToBrowse }, out results, out diagnosticInfos);

            foreach (var result in results)
            {
                foreach (var reference in result.References)
                {
                    string deviceName = reference.DisplayName.Text;
                    Debug.Log($"Device Found - NodeId: {reference.NodeId} | DisplayName: {deviceName}");

                    NodeId deviceNodeId = ExpandedNodeId.ToNodeId(reference.NodeId, session.NamespaceUris);
                    BrowseTagsInDevice(deviceNodeId, deviceName);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Browse error: {ex.Message}");
        }
    }

    private void BrowseTagsInDevice(NodeId deviceNodeId, string deviceName)
    { if (session == null || !session.Connected)
        {
            Debug.LogError("Session is not connected.");
            return;
        }

        try
        {
            BrowseDescription nodeToBrowse = new BrowseDescription
            {
                NodeId = deviceNodeId,
                BrowseDirection = BrowseDirection.Forward,
                IncludeSubtypes = true,
                NodeClassMask = (uint)NodeClass.Variable,
                ResultMask = (uint)BrowseResultMask.All
            };

            BrowseResultCollection results;
            DiagnosticInfoCollection diagnosticInfos;
            session.Browse(null, null, 0, new BrowseDescriptionCollection { nodeToBrowse }, out results, out diagnosticInfos);

            foreach (var result in results)
            {
                foreach (var reference in result.References)
                {
                    string tagName = reference.DisplayName.Text;
                    GameObject gameObject = GameObject.Find(tagName); // Find GameObject by tag name

                    if (gameObject != null)
                    {
                        NodeId nodeIdToRead = ExpandedNodeId.ToNodeId(reference.NodeId, session.NamespaceUris);
                        DataValue value = session.ReadValue(nodeIdToRead);
                        float tagValue = Convert.ToSingle(value.Value); // Tag value as float

                        Debug.Log($"Tag Value for {tagName}: {tagValue}");

                        // Define min and max values dynamically or statically
                        float minValue = 0f;    // Minimum threshold
                        float maxValue = 100f;  // Maximum threshold

                        // Compare the value and set the color based on proximity to min or max
                        SetObjectColorAndScale(gameObject, tagValue);

                        // Logging for warnings
                        if (tagValue < minValue)
                        {
                            Debug.Log($"Uyarı: {tagName} value ({tagValue}) is below the minimum threshold of {minValue}");
                        }
                        else if (tagValue > maxValue)
                        {
                            Debug.Log($"Uyarı: {tagName} value ({tagValue}) exceeds the maximum threshold of {maxValue}");
                        }
                        else
                        {
                            Debug.Log($"Stabil: {tagName} value ({tagValue}) is within the acceptable range.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"No GameObject found with the name: {tagName}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Browse error: {ex.Message}");
        }
    }

    private void LogMatchedTagValues()
    {
        Debug.Log("Matched Tag Values:");
        foreach (var value in matchedTagValues)
        {
            Debug.Log(value.objectName);
        }
    }

    private void SaveData()
    {
        if (jsonHandler != null)
        {
            jsonHandler.SaveObjectDataToJson(matchedTagValues);
        }
        else
        {
            Debug.LogError("JsonHandler not found!");
        }
    }
    void UpdateTagValues()
    {
        // Browse the devices and check tag values continuously
        NodeId channelNodeId = new NodeId("Channel1", 2); // Channel1's NodeId
        BrowseDevicesInChannel(channelNodeId);
    }

    private void OnApplicationQuit()
    {
        if (session != null && session.Connected)
        {
            session.Close();
            session.Dispose();
        }
    }
    private void SetObjectColorAndScale(GameObject obj, float value)
    {
        // Objenin `ObjectData` componentini alarak min ve max değerlerini kullanıyoruz
        ObjectData objectData = obj.GetComponent<ObjectData>();
        if (objectData == null)
        {
            Debug.LogWarning($"ObjectData bulunamadı: {obj.name}. Varsayılan değerler kullanılacak.");
            return;
        }

        float minValue = objectData.minValue;  // Objenin minimum değeri
        float maxValue = objectData.maxValue;  // Objenin maksimum değeri

        // Define the minimum and maximum colors (green to red)
        Color minColor = Color.green;
        Color maxColor = Color.red;

        // Calculate the normalized value between 0 and 1 based on the min/max range
        float normalizedValue = Mathf.InverseLerp(minValue, maxValue, value);

        // Interpolate the color between green and red based on the normalized value
        Color newColor = Color.Lerp(minColor, maxColor, normalizedValue);

        // Belirli bir alt objenin Renderer'ını bulmak
        Transform warningSystem = obj.transform.Find("PivotWarningLevelIndıcator").Find("WarningSystem");

        if (warningSystem != null)
        {
            Renderer objectRenderer = warningSystem.GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                // Alt objenin rengini değiştir
                Debug.Log($"{warningSystem.name} rengini {newColor} olarak değiştirdik.");
                objectRenderer.material.color = newColor;
            }
            else
            {
                Debug.LogWarning($"Renderer bulunamadı: {warningSystem.name}");
            }
        }
        else
        {
            Debug.LogWarning("Alt obje 'PivotWarningLevelIndıcator' bulunamadı.");
        }

        // Adjust the scale based on the value (between a minimum and maximum scale)
        Vector3 minScale = new Vector3(1f, 0f, 1f); // Minimum scale
        Vector3 maxScale = new Vector3(1f, 1f, 1f); // Maximum scale

        // Interpolate the scale based on the normalized value
        Vector3 newScale = Vector3.Lerp(minScale, maxScale, normalizedValue);
        Transform warningIndicator = obj.transform.Find("PivotWarningLevelIndıcator");
        // Alt objenin ölçeğini ayarla
        if (warningIndicator != null)
        {
            warningIndicator.localScale = newScale;
        }
    }


    private async void StartTimer()
    {
        isTimerRunning = true;

 

        await Task.Delay(3000);

        

        isTimerRunning = false;
        isTimerCompleted = true;
    }
}