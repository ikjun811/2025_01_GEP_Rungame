using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [Tooltip("패럴랙스 효과의 강도. 0에 가까울수록 멀리 있는 것처럼 보입니다.")]
    [SerializeField, Range(0f, 1f)] private float parallaxFactor = 0.5f;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    // (수정) 로컬 좌표계에서의 크기를 저장합니다.
    private Vector3 localSpriteSize;

    private bool isInitialized = false;

    public void Initialize(Transform mainCameraTransform)
    {
        if (isInitialized) return;

        this.cameraTransform = mainCameraTransform;
        this.lastCameraPosition = cameraTransform.position;

        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            var localBounds = meshRenderer.GetComponent<MeshFilter>().mesh.bounds.size;
            localSpriteSize = new Vector3(
                meshRenderer.transform.localScale.x * localBounds.x,
                meshRenderer.transform.localScale.y * localBounds.y,
                meshRenderer.transform.localScale.z * localBounds.z
            );
        }
        else
        {
            Debug.LogError($"ParallaxController ({gameObject.name}): 자식 오브젝트에 MeshRenderer가 없습니다!");
            return;
        }

        isInitialized = true;
    }

    void LateUpdate()
    {
        if (!isInitialized) return;

        // 1. 카메라의 '월드' 이동량을 계산합니다.
        Vector3 cameraWorldMoveDelta = cameraTransform.position - lastCameraPosition;

        // 2. 패럴랙스 효과를 적용합니다.
        Vector3 parallaxMoveDelta = cameraWorldMoveDelta * (1.0f - parallaxFactor);

        // ==========================================================
        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ 축 매핑 수정 (핵심) ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // ==========================================================

        // 3. 이동량을 올바른 로컬 축에 적용합니다.
        //    - 카메라의 X축 이동(좌우 진행) -> 배경의 Z축 이동(앞뒤 깊이)
        //    - 카메라의 Y축 이동(상하) -> 배경의 Y축 이동(상하)
        //    - 카메라의 Z축 이동(플레이어 좌우이동) -> 배경의 X축 이동(좌우)
        Vector3 localMoveDelta = new Vector3(
            parallaxMoveDelta.z, // 카메라 Z -> 로컬 X
            parallaxMoveDelta.y, // 카메라 Y -> 로컬 Y
            parallaxMoveDelta.x  // 카메라 X -> 로컬 Z (게임 진행 방향)
        );

        // 4. 로컬 좌표 기준으로 위치를 이동시킵니다.
        //    Translate 함수는 자동으로 로컬 좌표 기준으로 이동합니다.
        transform.Translate(localMoveDelta, Space.Self);

        // ==========================================================
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ 축 매핑 수정 (핵심) ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
        // ==========================================================

        lastCameraPosition = cameraTransform.position;

        // 5. 무한 스크롤 로직 (이 부분은 더 단순하게 수정 가능)
        //    배경이 계속 이동하므로, 특정 경계를 넘어가면 위치를 리셋해야 합니다.
        //    로컬 Z축 위치를 확인합니다.
        if (Mathf.Abs(transform.localPosition.z) > localSpriteSize.z)
        {
            // 한 칸 뒤로(또는 앞으로) 이동하여 빈 공간을 채웁니다.
            // 어느 방향으로 리셋할지는 터널의 방향에 따라 달라집니다.
            // 예를 들어, Z값이 음수 방향으로 계속 커진다면
            if (transform.localPosition.z < 0)
                transform.localPosition += new Vector3(0, 0, localSpriteSize.z * 2);
            else
                transform.localPosition -= new Vector3(0, 0, localSpriteSize.z * 2);
        }

        // 상하, 좌우 무한 스크롤도 비슷한 방식으로 추가 가능
        if (Mathf.Abs(transform.localPosition.x) > localSpriteSize.x)
        {
            if (transform.localPosition.x < 0)
                transform.localPosition += new Vector3(localSpriteSize.x * 2, 0, 0);
            else
                transform.localPosition -= new Vector3(localSpriteSize.x * 2, 0, 0);
        }

        if (Mathf.Abs(transform.localPosition.y) > localSpriteSize.y)
        {
            if (transform.localPosition.y < 0)
                transform.localPosition += new Vector3(0, localSpriteSize.y * 2, 0);
            else
                transform.localPosition -= new Vector3(0, localSpriteSize.y * 2, 0);
        }
    }
}
