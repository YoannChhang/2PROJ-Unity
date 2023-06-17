using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Cinemachine;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class GameCameraMover : MonoBehaviour
{

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] private TMP_Text CameraModeText;
    [SerializeField] private GameObject Interface;

    [SerializeField] private GameObject VisibleMap;



    //Logic Variables

    private float targetFieldOfView = 6f;
    private float MaxFieldOfView = 13f;
    private float MinFieldOfView = 2f;
    private float zoomSpeed = 2f;



    private void Start()
    {
        SetScreenDefault();

    }


    void Update()
    {


        if (!EventSystem.current.IsPointerOverGameObject())
        {
            CameraEdgeScrolling();
        }
        CameraZoomScrolling();


    }

    private void CameraEdgeScrolling()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        int edgeScrollSize = 30;


        if (Input.mousePosition.x < edgeScrollSize && transform.position.x > -20)
        {
            inputDir.x = -1f ;
        }
        if (Input.mousePosition.y < edgeScrollSize && transform.position.y > -24)
        {
            inputDir.y = -1f;
        }

        if (Input.mousePosition.x > Screen.width - edgeScrollSize && transform.position.x < 20)
        {

            inputDir.x = +1f;
        }
        if (Input.mousePosition.y > Screen.height - edgeScrollSize && transform.position.y < -4)
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

 

    private Vector3Int minTilemapPosition;
    private Vector3Int maxTilemapPosition;


    public void SetScreenDefault()
    {


        Camera mainCamera = Camera.main;
        float cameraSize = mainCamera.orthographicSize;
        float aspectRatio = (float)Screen.width / Screen.height;

        float cameraWidth = cameraSize * 2.0f * aspectRatio;
        float cameraHeight = cameraSize * 2.0f;



        Tilemap[] tilemaps = VisibleMap.GetComponentsInChildren<Tilemap>();

        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.cellBounds.min.x < minTilemapPosition.x)
            {
                minTilemapPosition.x = tilemap.cellBounds.min.x;
            }
            if (tilemap.cellBounds.min.y < minTilemapPosition.y)
            {
                minTilemapPosition.y = tilemap.cellBounds.min.y;
            }

            if (tilemap.cellBounds.max.x > maxTilemapPosition.x)
            {
                maxTilemapPosition.x = tilemap.cellBounds.max.x;
            }
            if (tilemap.cellBounds.max.y > maxTilemapPosition.y)
            {
                maxTilemapPosition.y = tilemap.cellBounds.max.y;
            }
        }

        int gridWidth = maxTilemapPosition.x - minTilemapPosition.x + 1;
        int gridHeight = maxTilemapPosition.y - minTilemapPosition.y + 1;


        transform.position = new Vector3(
            0,
            -((gridHeight / 2) - (cameraHeight / 2))
            , -20);



    }
}
