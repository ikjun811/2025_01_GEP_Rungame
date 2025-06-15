using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxManager : MonoBehaviour
{
    private Transform cameraTransform;

    void Start()
    {
        CameraControl gameCameraControl = FindObjectOfType<CameraControl>();

        if (gameCameraControl != null)
        {
            // 2. 그 오브젝트의 Transform을 카메라 Transform으로 사용합니다.
            cameraTransform = gameCameraControl.transform;
            Debug.Log("ParallaxManager: CameraControl이 붙어있는 게임 카메라를 찾았습니다.");
            Debug.Log($"ParallaxManager: 참조된 카메라 이름은 '{cameraTransform.gameObject.name}' 입니다.");
        }
        else
        {
            Debug.LogError("ParallaxManager: CameraControl 스크립트를 가진 카메라를 찾을 수 없습니다! 문제가 발생할 수 있습니다.");
            // 비상용으로 Camera.main을 다시 시도
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
                Debug.LogWarning("ParallaxManager: 대신 Camera.main을 사용합니다.");
            }
            else
            {
                Debug.LogError("ParallaxManager: 어떤 카메라도 찾지 못했습니다!");
                return;
            }
        }

        // 3. 자신의 모든 자식에 있는 ParallaxController들을 찾습니다.
        ParallaxController[] controllers = GetComponentsInChildren<ParallaxController>();

        // 4. 찾은 모든 컨트롤러에게 카메라 정보를 전달(초기화)합니다.
        foreach (ParallaxController controller in controllers)
        {
            controller.Initialize(cameraTransform);
        }
        Debug.Log($"{controllers.Length}개의 ParallaxController를 초기화했습니다.");
    }
}
