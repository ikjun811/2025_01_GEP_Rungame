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
            // 2. �� ������Ʈ�� Transform�� ī�޶� Transform���� ����մϴ�.
            cameraTransform = gameCameraControl.transform;
            Debug.Log("ParallaxManager: CameraControl�� �پ��ִ� ���� ī�޶� ã�ҽ��ϴ�.");
            Debug.Log($"ParallaxManager: ������ ī�޶� �̸��� '{cameraTransform.gameObject.name}' �Դϴ�.");
        }
        else
        {
            Debug.LogError("ParallaxManager: CameraControl ��ũ��Ʈ�� ���� ī�޶� ã�� �� �����ϴ�! ������ �߻��� �� �ֽ��ϴ�.");
            // �������� Camera.main�� �ٽ� �õ�
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
                Debug.LogWarning("ParallaxManager: ��� Camera.main�� ����մϴ�.");
            }
            else
            {
                Debug.LogError("ParallaxManager: � ī�޶� ã�� ���߽��ϴ�!");
                return;
            }
        }

        // 3. �ڽ��� ��� �ڽĿ� �ִ� ParallaxController���� ã���ϴ�.
        ParallaxController[] controllers = GetComponentsInChildren<ParallaxController>();

        // 4. ã�� ��� ��Ʈ�ѷ����� ī�޶� ������ ����(�ʱ�ȭ)�մϴ�.
        foreach (ParallaxController controller in controllers)
        {
            controller.Initialize(cameraTransform);
        }
        Debug.Log($"{controllers.Length}���� ParallaxController�� �ʱ�ȭ�߽��ϴ�.");
    }
}
