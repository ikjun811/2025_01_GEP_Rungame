using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureScroller : MonoBehaviour
{
    [Tooltip("�⺻ ��ũ�� �ӵ��� ������ ����. �� ���� �����Ͽ� �ӵ����� �����մϴ�.")]
    public float speedMultiplier = 0.1f;

    [Tooltip("�÷��̾� �ӵ��� 0�� ���� �ּ� ��ũ�� �ӵ� (�ͳ��� ������ �ʵ���)")]
    public Vector2 minScrollSpeed = new Vector2(0f, 0.5f);

    // --- ���� ���� ---
    private Renderer objectRenderer;
    private Material materialInstance;
    private PlayerControl playerControl; // �÷��̾� ��Ʈ�ѷ� ����

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError("TextureScroller: Renderer ������Ʈ�� ã�� �� �����ϴ�!");
            enabled = false;
            return;
        }

        materialInstance = objectRenderer.material;

        // �÷��̾� ������Ʈ�� ã�Ƽ� PlayerControl ������Ʈ ������ �����ɴϴ�.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerControl = playerObject.GetComponent<PlayerControl>();
        }

        if (playerControl == null)
        {
            Debug.LogWarning("TextureScroller: PlayerControl�� ã�� ���߽��ϴ�. �ּ� �ӵ��θ� �����Դϴ�.");
        }
    }

    void Update()
    {
        // ���� �÷��̾� �ӵ��� �����ɴϴ�.
        float currentPlayerSpeed = 0f;
        if (playerControl != null)
        {
            // �÷��̾� �ӵ��� �������� ����� PlayerControl�� ������ ���� �޶����ϴ�.
            // 1. public ������ ���� ����
            currentPlayerSpeed = playerControl.current_speed;

            // 2. Getter �Լ� ��� (�� ����)
            // currentPlayerSpeed = playerControl.GetCurrentSpeed();
        }

        // �÷��̾� �ӵ��� ������� ���� ��ũ�� �ӵ��� ����մϴ�.
        // Y������ ��ũ���Ѵٰ� �����մϴ�.
        float scrollY = currentPlayerSpeed * speedMultiplier;

        // ���� ��ũ�� �ӵ� ���͸� ����ϴ�. �ּ� �ӵ����� �������� �ʵ��� �����մϴ�.
        Vector2 currentScrollSpeed = new Vector2(
            minScrollSpeed.x,
            Mathf.Max(minScrollSpeed.y, scrollY) // �ּ� �ӵ��� ���� �ӵ� �� �� ū ���� ���
        );

        // �ð��� ���� �ؽ�ó �������� �����մϴ�.
        Vector2 offset = currentScrollSpeed * Time.deltaTime;
        materialInstance.mainTextureOffset += offset;
    }
}
