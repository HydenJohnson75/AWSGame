using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class AWSConstants
{
    private const string AuthAPI = "https://5knj5mqwub.execute-api.us-east-1.amazonaws.com/APIStaging//log-in";
    private const string SignUpAPI = "https://5knj5mqwub.execute-api.us-east-1.amazonaws.com/APIStaging/sign-up";
    private const string UpdateJoinCodeAPI = "https://bx944eb46j.execute-api.us-east-1.amazonaws.com/ServerStage/";
    private const string CognitoClientId = "2rgsccq0qu3sjdimq4c4u139hq";
    private const string CognitoURL = "https://multiplayergame.auth.us-east-1.amazoncognito.com";

    public AWSConstants()
    { 
    
    }

    public string GetAuthAPI()
    {
        return AuthAPI;
    }

    public string GetSignUpAPI()
    {
        return SignUpAPI;
    }

    public string GetUpdateJoinCodeAPI()
    {
        return UpdateJoinCodeAPI;
    }

    public string GetCognitoClientId()
    {
        return CognitoClientId;
    }

    public string GetCognitoURL()
    {
        return CognitoURL;
    }
}
