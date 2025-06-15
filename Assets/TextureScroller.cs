using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureScroller : MonoBehaviour
{
    [Tooltip("기본 스크롤 속도에 곱해질 배율. 이 값을 조절하여 속도감을 조절합니다.")]
    public float speedMultiplier = 0.1f;

    [Tooltip("플레이어 속도가 0일 때의 최소 스크롤 속도 (터널이 멈추지 않도록)")]
    public Vector2 minScrollSpeed = new Vector2(0f, 0.5f);

    // --- 내부 변수 ---
    private Renderer objectRenderer;
    private Material materialInstance;
    private PlayerControl playerControl; // 플레이어 컨트롤러 참조

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError("TextureScroller: Renderer 컴포넌트를 찾을 수 없습니다!");
            enabled = false;
            return;
        }

        materialInstance = objectRenderer.material;

        // 플레이어 오브젝트를 찾아서 PlayerControl 컴포넌트 참조를 가져옵니다.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerControl = playerObject.GetComponent<PlayerControl>();
        }

        if (playerControl == null)
        {
            Debug.LogWarning("TextureScroller: PlayerControl을 찾지 못했습니다. 최소 속도로만 움직입니다.");
        }
    }

    void Update()
    {
        // 현재 플레이어 속도를 가져옵니다.
        float currentPlayerSpeed = 0f;
        if (playerControl != null)
        {
            // 플레이어 속도를 가져오는 방식은 PlayerControl의 구현에 따라 달라집니다.
            // 1. public 변수에 직접 접근
            currentPlayerSpeed = playerControl.current_speed;

            // 2. Getter 함수 사용 (더 권장)
            // currentPlayerSpeed = playerControl.GetCurrentSpeed();
        }

        // 플레이어 속도를 기반으로 최종 스크롤 속도를 계산합니다.
        // Y축으로 스크롤한다고 가정합니다.
        float scrollY = currentPlayerSpeed * speedMultiplier;

        // 최종 스크롤 속도 벡터를 만듭니다. 최소 속도보다 낮아지지 않도록 보장합니다.
        Vector2 currentScrollSpeed = new Vector2(
            minScrollSpeed.x,
            Mathf.Max(minScrollSpeed.y, scrollY) // 최소 속도와 계산된 속도 중 더 큰 값을 사용
        );

        // 시간에 따라 텍스처 오프셋을 변경합니다.
        Vector2 offset = currentScrollSpeed * Time.deltaTime;
        materialInstance.mainTextureOffset += offset;
    }
}
