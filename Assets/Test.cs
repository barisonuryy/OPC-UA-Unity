using System;
using System.Collections;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using UnityEngine;

public class Test : MonoBehaviour
{
    private ApplicationInstance application;
    private Session session;

    // OPC UA Sunucusu URL'si (KEPServerEX sunucusu için)
    private string serverUrl = "opc.tcp://localhost:49320/UA/KEPServerEX";  // KEPServerEX için varsayılan URL, IP ve port ayarlamalarınızı yapın

    // Almak istediğiniz tag (Node) ID'si
    private string nodeId = "ns=2;s=Channel1.Device1.Tag1"; // KEPServerEX'deki tag node ID'si, sizin yapılandırmanıza göre değiştirin

    IEnumerator Start()
    {
        Debug.Log("OPC UA istemcisi başlatılıyor...");

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        // OPC UA istemcisi başlatma (asenkron görev tamamlanana kadar bekle)
        yield return InitializeOpcUaClient().AsCoroutine();
        Debug.Log("OPC UA istemcisi başlatıldı.");

        // Tag değerini alma
        ReadTagValue(nodeId);
#else
        Debug.LogError("OPC UA is not supported on this platform.");
#endif
    }

    private async System.Threading.Tasks.Task InitializeOpcUaClient()
    {
        
        try
        {
            Debug.Log("Initializing OPC UA client...");

            // Uygulama örneğini oluştur
            application = new ApplicationInstance
            {
                ApplicationName = "UnityOpcUaClient",
                ApplicationType = ApplicationType.Client
            };

            // Yapılandırmayı dosyadan yükle
            ApplicationConfiguration config = await application.LoadApplicationConfiguration("C:\\Users\\baris\\My project (1)\\.Config.xml", false);
            if (config == null)
            {
                throw new Exception("Configuration file could not be loaded.");
            }
            Debug.Log("Configuration file loaded successfully.");

            // Sertifika doğrulama işlemi atlandı
            Debug.Log("Skipping certificate validation...");

            // Endpoint'leri keşfetme
            Debug.Log("Discovering endpoints...");
            EndpointDescriptionCollection endpoints = DiscoverEndpoints(serverUrl);

            // Check if any endpoints were found
            if (endpoints == null || endpoints.Count == 0)
            {
                Debug.LogError("No endpoints found.");
                return;
            }

            // Güvenliksiz endpoint bulamazsak güvenlikli bir endpoint'i seçelim
            EndpointDescription selectedEndpoint = null;
            foreach (var endpoint in endpoints)
            {
                if (endpoint.SecurityMode == MessageSecurityMode.None) // Öncelikle None mod arıyoruz
                {
                    selectedEndpoint = endpoint;
                    break;
                }
                
            }

            // Eğer güvenliksiz endpoint yoksa, güvenlikli endpoint'lerden birini seçelim
            if (selectedEndpoint == null)
            {
                Debug.LogWarning("No security-free endpoint found, selecting a secure endpoint instead.");
                selectedEndpoint = endpoints[0]; // İlk bulunan güvenlikli endpoint'i seçiyoruz
            }

            Debug.Log("Using endpoint: " + selectedEndpoint.EndpointUrl);

            EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(config);
            ConfiguredEndpoint configuredEndpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

            // OPC UA session oluşturma
            session = await Session.Create(config, configuredEndpoint, false, "", 60000, null, null);
            Debug.Log("Connected to OPC UA server successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error initializing OPC UA client: " + ex.Message);
        }
    }

    private EndpointDescriptionCollection DiscoverEndpoints(string discoveryUrl)
    {
        // Endpoint'leri keşfetmek için DiscoveryClient kullanılır
        var endpointCollection = new EndpointDescriptionCollection();

        try
        {
            // discoveryUrl'yi Uri'ye çeviriyoruz
            Uri uri = new Uri(discoveryUrl);

            using (var client = DiscoveryClient.Create(uri))
            {
                endpointCollection = client.GetEndpoints(null);

                if (endpointCollection == null || endpointCollection.Count == 0)
                {
                    Debug.LogError("No endpoints discovered.");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error discovering endpoints: " + ex.Message);
        }

        return endpointCollection;
    }

    private void ReadTagValue(string nodeId)
    {
        try
        {
            // session null kontrolü
            if (session == null || !session.Connected)
            {
                Debug.LogError("Session is not connected or null.");
                return;
            }

            // Node ID'sini oluşturma
            NodeId tagNodeId = new NodeId(nodeId);

            // Tag değerini okuma
            DataValue value = session.ReadValue(tagNodeId);
            Debug.Log($"Tag değeri: {value.Value}, Durum: {value.StatusCode}");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error reading tag value: " + ex.Message);
        }
    }

    private void OnApplicationQuit()
    {
        // Oturumu kapatma
        if (session != null && session.Connected)
        {
            session.Close();
            session.Dispose();
        }
    }
}

// Asenkron görevleri coroutine ile uyumlu hale getirmek için eklendi
public static class TaskExtensions
{
    public static IEnumerator AsCoroutine(this System.Threading.Tasks.Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }
        if (task.Exception != null)
        {
            throw task.Exception;
        }
    }
}
