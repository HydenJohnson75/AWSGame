using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using TMPro;
using System.Net.Http;
using Unity.Services.Vivox;

public class RelayScript : MonoBehaviour
{

    [SerializeField]
    private Button playButton;
    [SerializeField]
    private TMP_InputField joinCodeInputField;

    private AWSConstants awsConstants = new AWSConstants();

    private void Awake()
    {
        playButton.onClick.AddListener(() =>
        {
            JoinRelay(joinCodeInputField.text);
        });
    }

    // Start is called before the first frame update
    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("SignedIn " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        string _server, _domain, _issuer, _key;

        _server = "https://mtu1xp-mad.vivox.com";
        _domain = "mtu1xp.vivox.com";
        _issuer = "57724-awsga-80135-udash";
        _key = "a1baf3USpuFFQSv6zB64SDJ0aTnN006d";

        InitializationOptions options = new InitializationOptions();
        options.SetVivoxCredentials(_server, _domain, _issuer, _key);

        await VivoxService.Instance.InitializeAsync();

#if UNITY_SERVER
        CreateRelay();
#endif
    }


    private async void CreateRelay()
    {
        try 
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("Join Code is: " + joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort) allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );

            UpdateJoinCode(joinCode);

            NetworkManager.Singleton.StartServer();
        }
        catch(RelayServiceException e)
        {
            Debug.Log("RelayServiceException: " + e.Message);
        }
        
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                ) ;

            NetworkManager.Singleton.StartClient();
        } 
        catch(RelayServiceException e) 
        { 
            Debug.Log("RelayServiceException: " + e.Message);
        }
    }

    private async void UpdateJoinCode(string joinCode)
    {
        using (var client = new HttpClient())
        {
            string json = "{\"Join_Code\": \"" + joinCode + "\"}";
            StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PutAsync(awsConstants.GetUpdateJoinCodeAPI(), content);
            Debug.Log("Response: " + response.StatusCode);

            string responseBody = await response.Content.ReadAsStringAsync();

            Debug.Log("Response Body: " + responseBody);
        }

    }
        // Update is called once per frame
    void Update()
    {
        
    }
}
