using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Ship Movement Properties")]
    public float speed;

    [Header("Camera Movement Properties")]
    public float distance = 15.0f;
    public float xSpeed = 60.0f;
    public float ySpeed = 120.0f;
    private float cameraX = 0;
    private float cameraY = 0;

    private void Start()
    {
        if (isLocalPlayer)
        {
            Camera.main.transform.position = transform.position + new Vector3(0, 0, -20);
            Camera.main.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
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
    }
}
