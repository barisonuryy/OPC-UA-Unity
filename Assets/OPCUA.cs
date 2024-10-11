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
    [SerializeField]
    GameObject[] pools;
    [SerializeField] float poolMaxHeight, poolMinHeight;
    [SerializeField] string[] tagNames;
    public List<TMP_Text> tagValueTextMeshProList;
    private List<ObjectMata> matchedTagValues = new List<ObjectMata>();
    private ApplicationInstance application;
    private Session session;
    private JsonHandler jsonHandler;  // Reference to JsonHandler script
    bool isConnected;
    private bool isTimerRunning = false;
    private bool isTimerCompleted = false;
    public GameObject[] chargeState,chargeTemp,chargeTemp2;

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
                NodeId = deviceNodeId,
                BrowseDirection = BrowseDirection.Forward,
                IncludeSubtypes = true,
                NodeClassMask = (uint)NodeClass.Variable,
                ResultMask = (uint)BrowseResultMask.All
            };

            BrowseResultCollection results;
            DiagnosticInfoCollection diagnosticInfos;
            session.Browse(null, null, 0, new BrowseDescriptionCollection { nodeToBrowse }, out results, out diagnosticInfos);

            int textMeshIndex = 0;  // TextMeshPro referansları için index

            foreach (var result in results)
            {
                foreach (var reference in result.References)
                {
                    string tagName = reference.DisplayName.Text;
                    GameObject gameObject = GameObject.Find(tagName);  // Find GameObject by tag name

                    if (gameObject != null && textMeshIndex < tagValueTextMeshProList.Count)
                    {
                        NodeId nodeIdToRead = ExpandedNodeId.ToNodeId(reference.NodeId, session.NamespaceUris);
                        DataValue value = session.ReadValue(nodeIdToRead);
                        float tagValue = Convert.ToSingle(value.Value);  // Tag value as float

                        Debug.Log($"Tag Value for {tagName}: {tagValue}");

                        // TextMeshPro bileşenine veriyi yazdır
                        if (tagName != "ChargeState")
                        {
                            tagValueTextMeshProList[textMeshIndex].text = $"{tagNames[textMeshIndex]}                 {tagValue}";
                            SetObjectColorAndScale(gameObject, tagValue);
                        }
                        else
                        {
                            Debug.Log("State Değer" + tagValue);
                            if (tagValue == 0)
                            {
                                chargeState[0].SetActive(false);
                                chargeState[1].SetActive(true);
                            }
                            else
                            {
                                chargeState[0].SetActive(true);
                                chargeState[1].SetActive(false);
                            }
                        }
                        if (tagName == "Capacity")
                        {

                        }
                  


                        // TextMeshPro index'ini artır
                        textMeshIndex++;
                    }
                    else
                    {
                        Debug.LogWarning($"No GameObject found with the name: {tagName} or no available TextMeshPro slot.");
                    }
                    if (tagName == "BatteryTemperature"&& gameObject != null)
                    {
                   
                        NodeId nodeIdToRead = ExpandedNodeId.ToNodeId(reference.NodeId, session.NamespaceUris);
                        DataValue value = session.ReadValue(nodeIdToRead);
                        float minVal = gameObject.GetComponent<ObjectData>().minValue;
                        float maxVal = gameObject.GetComponent<ObjectData>().maxValue;
                        float tagValue = Convert.ToSingle(value.Value);
                        if (tagValue < minVal)
                        {
                            chargeTemp[0].SetActive(false);
                            chargeTemp[1].SetActive(true);
                            chargeTemp2[0].SetActive(true);
                            chargeTemp2[1].SetActive(false);
                        }
                        else if (tagValue > maxVal)
                        {
                            chargeTemp[0].SetActive(true);
                            chargeTemp[1].SetActive(false);
                            chargeTemp2[0].SetActive(false);
                            chargeTemp2[1].SetActive(true);
                        }
                        else
                        {
                            chargeTemp[0].SetActive(false);
                            chargeTemp[1].SetActive(true);
                            chargeTemp2[0].SetActive(false);
                            chargeTemp2[1].SetActive(true);
                        }
                    
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

        // If the object is 'Capacity', move it along the Y-axis based on a specific ratio
        if (obj.name == "Capacity")
        {
            float normalizedMovement = value / maxValue; // Calculate the normalized movement along Y-axis

            // Define the proportion or ratio factor
            float movementFactor = 0.5f;  // This is the ratio (adjust to fit your needs)

            // Calculate the movement along Y-axis based on this ratio
            float minY = poolMinHeight; // Define the minimum Y position
            float maxY = poolMaxHeight; // Define the maximum Y position

            // Apply the ratio factor to the movement
            float proportionalMovement = Mathf.Lerp(minY, maxY, normalizedMovement * movementFactor);

            pools[0].transform.localPosition = new Vector3(pools[0].transform.localPosition.x, proportionalMovement, pools[0].transform.localPosition.z);
            pools[1].transform.localPosition = new Vector3(pools[1].transform.localPosition.x, proportionalMovement, pools[1].transform.localPosition.z);

            Debug.Log($"Capacity object moved to Y position: {proportionalMovement}");
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