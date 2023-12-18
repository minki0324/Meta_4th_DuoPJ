using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class BuildArea : MonoBehaviour
    {
        [SerializeField] private GameObject[] BuildAreas;
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float raycastDistance = 5f;

        private void Update()
        {
            MovePlaneWithMouse();
            DetectObstacle();
        }

        private void MovePlaneWithMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                transform.position = hit.point;
            }
        }

        private void DetectObstacle()
        {
            for (int i = 0; i < BuildAreas.Length; i++)
            {
                RaycastHit hit;

                // 레이어 마스크 설정을 추가하여 obstacleLayer 레이어만 감지하도록 함
                if (Physics.Raycast(BuildAreas[i].transform.position, -BuildAreas[i].transform.up, out hit, raycastDistance, obstacleLayer))
                {
                    BuildAreas[i].GetComponent<Renderer>().material = invalidMaterial;
                }
                else
                {
                    BuildAreas[i].GetComponent<Renderer>().material = validMaterial;
                }
            }
        }
    }