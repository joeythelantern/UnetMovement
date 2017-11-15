using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using PlayerManager;

public class NetworkPlayer : NetworkMessageHandler
{
    [Header("Player Properties")]
    public string playerID;

    [Header("Ship Movement Properties")]
    public bool canSendNetworkMovement;
    public float speed;
    public float networkSendRate = 5;
    public float timeBetweenMovementStart;
    public float timeBetweenMovementEnd;

    [Header("Camera Movement Properties")]
    public float distance = 15.0f;
    public float xSpeed = 60.0f;
    public float ySpeed = 120.0f;
    private float cameraX = 0;
    private float cameraY = 0;

    [Header("Lerping Properties")]
    public bool isLerpingPosition;
    public bool isLerpingRotation;
    public Vector3 realPosition;
    public Quaternion realRotation;
    public Vector3 lastRealPosition;
    public Quaternion lastRealRotation;
    public float timeStartedLerping;
    public float timeToLerp;

    private void Start()
    {
        playerID = "player" + GetComponent<NetworkIdentity>().netId.ToString();
        transform.name = playerID;
        Manager.Instance.AddPlayerToConnectedPlayers(playerID, gameObject);

        if (isLocalPlayer)
        {
            Manager.Instance.SetLocalPlayerID(playerID);

            Camera.main.transform.position = transform.position + new Vector3(0, 0, -20);
            Camera.main.transform.rotation = Quaternion.Euler(0, 0, 0);

            canSendNetworkMovement = false;
            RegisterNetworkMessages();
        }
        else
        {
            isLerpingPosition = false;
            isLerpingRotation = false;

            realPosition = transform.position;
            realRotation = transform.rotation;
        }
    }

    private void RegisterNetworkMessages()
    {
        NetworkManager.singleton.client.RegisterHandler(movement_msg, OnReceiveMovementMessage);
    }

    private void OnReceiveMovementMessage(NetworkMessage _message)
    {
        PlayerMovementMessage _msg = _message.ReadMessage<PlayerMovementMessage>();

        if (_msg.objectTransformName != transform.name)
        {
            Manager.Instance.ConnectedPlayers[_msg.objectTransformName].GetComponent<NetworkPlayer>().ReceiveMovementMessage(_msg.objectPosition, _msg.objectRotation, _msg.time);
        }
    }

    public void ReceiveMovementMessage(Vector3 _position, Quaternion _rotation, float _timeToLerp)
    {
        lastRealPosition = realPosition;
        lastRealRotation = realRotation;
        realPosition = _position;
        realRotation = _rotation;
        timeToLerp = _timeToLerp;

        if(realPosition != transform.position)
        {
            isLerpingPosition = true;
        }

        if(realRotation.eulerAngles != transform.rotation.eulerAngles)
        {
            isLerpingRotation = true;
        }

        timeStartedLerping = Time.time;
    }

    private void Update()
    {
        if(isLocalPlayer)
        {
            UpdateCameraMovement();
            UpdatePlayerMovement();
        }
        else
        {
            return;
        }
    }

    private void UpdateCameraMovement()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            cameraX += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            cameraY -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }

        Quaternion rotation = Quaternion.Euler(cameraY, cameraX, 0);

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + transform.position;

        Camera.main.transform.rotation = rotation;
        Camera.main.transform.position = position;
    }

    private void UpdatePlayerMovement()
    {
        var y = Input.GetAxis("Horizontal") * Time.deltaTime * 20;
        var x = Input.GetAxis("Vertical") * Time.deltaTime * 20;
        var z = Input.GetAxis("Rotational") * Time.deltaTime * 20;

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (speed < 10)
                speed++;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if(speed > -5)
                speed--;
        }

        if (speed != 0)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        if (y != 0 || x != 0 || z != 0)
        {
            transform.Rotate(x, y, -z);
        }

        if (!canSendNetworkMovement)
        {
            canSendNetworkMovement = true;
            StartCoroutine(StartNetworkSendCooldown());
        }
    }

    private IEnumerator StartNetworkSendCooldown()
    {
        timeBetweenMovementStart = Time.time;
        yield return new WaitForSeconds((1 / networkSendRate));
        SendNetworkMovement();
    }

    private void SendNetworkMovement()
    {
        timeBetweenMovementEnd = Time.time;
        SendMovementMessage(playerID, transform.position, transform.rotation, (timeBetweenMovementEnd - timeBetweenMovementStart));
        canSendNetworkMovement = false;
    }

    public void SendMovementMessage(string _playerID, Vector3 _position, Quaternion _rotation, float _timeTolerp)
    {
        PlayerMovementMessage _msg = new PlayerMovementMessage()
        {
            objectPosition = _position,
            objectRotation = _rotation,
            objectTransformName = _playerID,
            time = _timeTolerp
        };

        NetworkManager.singleton.client.Send(movement_msg, _msg);
    }

    private void FixedUpdate()
    {
        if(!isLocalPlayer)
        {
            NetworkLerp();
        }
    }

    private void NetworkLerp()
    {
        if(isLerpingPosition)
        {
            float lerpPercentage = (Time.time - timeStartedLerping) / timeToLerp;

            transform.position = Vector3.Lerp(lastRealPosition, realPosition, lerpPercentage);
        }

        if(isLerpingRotation)
        {
            float lerpPercentage = (Time.time - timeStartedLerping) / timeToLerp;

            transform.rotation = Quaternion.Lerp(lastRealRotation, realRotation, lerpPercentage);
        }
    }
}