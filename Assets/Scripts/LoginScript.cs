using UnityEngine;
using UnityEngine.UI;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections;
using System.Net.Http;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using TMPro;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using UnityEditor.PackageManager;
using System.Net;
using Mono.Cecil;

public class LoginScript : MonoBehaviour
{
    public TMP_InputField logInUsernameInput;
    public TMP_InputField logInPasswordInput;
    public TMP_InputField signUpUsernameInput;
    public TMP_InputField signUpPasswordInput;
    public TMP_InputField emailInput;
    public TMP_InputField codeInput;


    public TMP_Text loginStatusText;
    public TMP_Text signUpStatusText;

    public Button signUpSwitchButton;
    public Button signUpButton;
    public Button loginButton;
    public Button contiuneButton;
    public Button startGameButton;
    public Button authButton;


    public GameObject loginPage;
    public GameObject signUpPage;
    public GameObject confirmPage;
    public GameObject gameStartPage;
    public GameObject authPage;

    string authUsername;

    private AWSConstants awsConstants = new AWSConstants();
    private string awsAccessKeyId = "AKIAW3MEBS7JMZI3C2UX";
    private string awsSecretAccessKey = "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";
    private string awsRegion = "us-east-1";
    private bool signedUp = false;
    private bool authenticated = false;

    private IAmazonCognitoIdentityProvider cognitoService;

    // Start is called before the first frame update
    void Start()
    {
        loginButton.onClick.AddListener(Login);
        signUpSwitchButton.onClick.AddListener(SwitchToSignUp);
        signUpButton.onClick.AddListener(_SignUp);
        startGameButton.onClick.AddListener(StartGame);
        authButton.onClick.AddListener(Authenticate);
        cognitoService = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.USEast1);
    }

    async void Login()
    {
        string username = logInUsernameInput.text.ToString();
        string password = logInPasswordInput.text.ToString();
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            loginStatusText.text = "Username and password are required.";
            return;
        }

        HttpClient client = new HttpClient();
        string json = "{\"username\": \"" + username + "\", \"password\": \"" + password + "\"}";
        StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(awsConstants.GetAuthAPI(), content);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.Log("Response body: " + responseBody);

            if (responseBody == "true")
            {
                loginPage.SetActive(false);
                gameStartPage.SetActive(true);
            }
            else
            {
                loginStatusText.text = "Login failed.";
            }
        }
        else
        {
            loginStatusText.text = "Login failed: " + response.StatusCode;
        }
    }

    private void SwitchToSignUp()
    {
        loginPage.SetActive(false);
        signUpPage.SetActive(true);
    }

    private void SwitchToConfirm()
    {
        signUpPage.SetActive(false);
        confirmPage.SetActive(true);
    }

    private void SwitchToLogin()
    {
        confirmPage.SetActive(false);
        signUpPage.SetActive(false);
        loginPage.SetActive(true);
    }

    private void SwitchToSignUpFromContiune()
    {
        confirmPage.SetActive(false);
        signUpPage.SetActive(true);
    }

    private void StartGame()
    {
        SceneManager.LoadScene("GameMenuScene");
    }


    [System.Serializable]
    public class SignUpSendData
    {

        public string Username;
        public string Password;
        public string ClientId;
        public List<SignUpAttribute> UserAttributes;
    }

    [System.Serializable]
    public class SignUpAttribute
    {
        public string Name;
        public string Value;
    }

    async void _SignUp()
    {
        string username = signUpUsernameInput.text.ToString();
        string password = signUpPasswordInput.text.ToString();
        string email = emailInput.text.ToString();

        signedUp = await SignUpAsync(awsConstants.GetCognitoClientId(), username, password, email);
    }

    async Task<bool> SignUpAsync(string clientId, string userName, string password, String email)
    {
        var userAttrs = new AttributeType
        {
            Name = "email",
            Value = email,
        };

        var userAttrsUName = new AttributeType
        {
            Name = "custom:username",
            Value = userName,
        };

        var userAttrsPassword = new AttributeType
        {
            Name = "custom:password",
            Value = password,
        };


        var userAttrsList = new List<AttributeType>();
        userAttrsList.Add(userAttrs);
        userAttrsList.Add(userAttrsUName);
        userAttrsList.Add(userAttrsPassword);

        var signUpRequest = new SignUpRequest
        {
            UserAttributes = userAttrsList,
            Username = userName,
            ClientId = clientId,
            Password = password,
        };

        var response = await cognitoService.SignUpAsync(signUpRequest);

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            authUsername = userName;
            signUpPage.SetActive(false);
            authPage.SetActive(true);
            Debug.Log("User: "+ userName + " was created");
            return true;
        }

        return false;
    }

    async void Authenticate()
    {
        string authCode = codeInput.text.ToString();    

        authenticated = await ConfirmSignupAsync(awsConstants.GetCognitoClientId(), authCode, authUsername);
    }

    public async Task<bool> ConfirmSignupAsync(string clientId, string code, string userName)
    {
        var signUpRequest = new ConfirmSignUpRequest
        {
            ClientId = clientId,
            ConfirmationCode = code,
            Username = userName,
        };

        var response = await cognitoService.ConfirmSignUpAsync(signUpRequest);

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            authPage.SetActive(false);
            loginPage.SetActive(true);
            Debug.Log("User: " + userName + " was confirmed");
            return true;
        }

        return false;
    }

}

