using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    private bool joinedLobby = false;

    // Start is called before the first frame update
    async void Start()
    {
        if (VivoxService.Instance != null)
        {
            VivoxService.Instance.ParticipantAddedToChannel += Test;
            await VivoxService.Instance.JoinGroupChannelAsync("RandyChannel", ChatCapability.AudioOnly);
        }


    }

    private void Test(VivoxParticipant participant)
    {
        joinedLobby = true;
        Debug.Log(participant.ChannelName);
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner)
        {
            return;
        }

        Vector3 moveDir = new Vector3(0,0,0);

        if (Input.GetKey(KeyCode.W))
        {
            moveDir.z = +1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir.z = -1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDir.x = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir.x = +1f;
        }

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

    }
}
