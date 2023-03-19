using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Cinemachine;
using TMPro;

public class GameCameraMover : MonoBehaviour
{

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] private TMP_Text CameraModeText;


    //Logic Variables
    public bool CameraMode = false;

    private float targetFieldOfView = 6f;
    private float MaxFieldOfView = 15f;
    private float MinFieldOfView = 2f;
    private float zoomSpeed = 2f;


    //timeouts
    private float timer = 0f;
    private float timeBetweenCommands = 1f;


    void Update()
    {
        timer += Time.deltaTime;


        //check inputs
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (timer >= timeBetweenCommands)
            {
                timer = 0f;

                CameraMode = !CameraMode;
                ToggleCameraMode();
            }

        }


        
        if (CameraMode)
        {
            CameraEdgeScrolling();
            CameraZoomScrolling();
        }
    }

    private void CameraEdgeScrolling()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        int edgeScrollSize = 50;

        if (Input.mousePosition.x < edgeScrollSize)
        {
            inputDir.x = -1f;
        }
        if (Input.mousePosition.y < edgeScrollSize)
        {
            inputDir.y = -1f;
        }

        if (Input.mousePosition.x > Screen.width - edgeScrollSize)
        {

            inputDir.x = +1f;
        }
        if (Input.mousePosition.y > Screen.height - edgeScrollSize)
        {
            inputDir.y = +1f;
        }
        //Vector3 moveDir = transform.forward * inputDir.y + transform.right * inputDir.x;

        float moveSpeed = 10f;
        transform.position += inputDir * moveSpeed * Time.deltaTime;
    }

    private void CameraZoomScrolling()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            targetFieldOfView -= 1;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            targetFieldOfView += 1;
        }

        targetFieldOfView = Mathf.Clamp(targetFieldOfView, MinFieldOfView, MaxFieldOfView);

        if (cinemachineVirtualCamera)
        {
            //cinemachineVirtualCamera.m_Lens.OrthographicSize = targetFieldOfView;
            cinemachineVirtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(cinemachineVirtualCamera.m_Lens.OrthographicSize, targetFieldOfView, Time.deltaTime * zoomSpeed);
        }
        
    }

    public void ToggleCameraMode()
    {
        //CameraMode UI
        CameraModeText.gameObject.SetActive(CameraMode);

    }
}
