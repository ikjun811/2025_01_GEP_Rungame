using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [Tooltip("�з����� ȿ���� ����. 0�� �������� �ָ� �ִ� ��ó�� ���Դϴ�.")]
    [SerializeField, Range(0f, 1f)] private float parallaxFactor = 0.5f;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    // (����) ���� ��ǥ�迡���� ũ�⸦ �����մϴ�.
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
            Debug.LogError($"ParallaxController ({gameObject.name}): �ڽ� ������Ʈ�� MeshRenderer�� �����ϴ�!");
            return;
        }

        isInitialized = true;
    }

    void LateUpdate()
    {
        if (!isInitialized) return;

        // 1. ī�޶��� '����' �̵����� ����մϴ�.
        Vector3 cameraWorldMoveDelta = cameraTransform.position - lastCameraPosition;

        // 2. �з����� ȿ���� �����մϴ�.
        Vector3 parallaxMoveDelta = cameraWorldMoveDelta * (1.0f - parallaxFactor);

        // ==========================================================
        // ������������������ �� ���� ���� (�ٽ�) ������������������
        // ==========================================================

        // 3. �̵����� �ùٸ� ���� �࿡ �����մϴ�.
        //    - ī�޶��� X�� �̵�(�¿� ����) -> ����� Z�� �̵�(�յ� ����)
        //    - ī�޶��� Y�� �̵�(����) -> ����� Y�� �̵�(����)
        //    - ī�޶��� Z�� �̵�(�÷��̾� �¿��̵�) -> ����� X�� �̵�(�¿�)
        Vector3 localMoveDelta = new Vector3(
            parallaxMoveDelta.z, // ī�޶� Z -> ���� X
            parallaxMoveDelta.y, // ī�޶� Y -> ���� Y
            parallaxMoveDelta.x  // ī�޶� X -> ���� Z (���� ���� ����)
        );

        // 4. ���� ��ǥ �������� ��ġ�� �̵���ŵ�ϴ�.
        //    Translate �Լ��� �ڵ����� ���� ��ǥ �������� �̵��մϴ�.
        transform.Translate(localMoveDelta, Space.Self);

        // ==========================================================
        // ������������������ �� ���� ���� (�ٽ�) ������������������
        // ==========================================================

        lastCameraPosition = cameraTransform.position;

        // 5. ���� ��ũ�� ���� (�� �κ��� �� �ܼ��ϰ� ���� ����)
        //    ����� ��� �̵��ϹǷ�, Ư�� ��踦 �Ѿ�� ��ġ�� �����ؾ� �մϴ�.
        //    ���� Z�� ��ġ�� Ȯ���մϴ�.
        if (Mathf.Abs(transform.localPosition.z) > localSpriteSize.z)
        {
            // �� ĭ �ڷ�(�Ǵ� ������) �̵��Ͽ� �� ������ ä��ϴ�.
            // ��� �������� ���������� �ͳ��� ���⿡ ���� �޶����ϴ�.
            // ���� ���, Z���� ���� �������� ��� Ŀ���ٸ�
            if (transform.localPosition.z < 0)
                transform.localPosition += new Vector3(0, 0, localSpriteSize.z * 2);
            else
                transform.localPosition -= new Vector3(0, 0, localSpriteSize.z * 2);
        }

        // ����, �¿� ���� ��ũ�ѵ� ����� ������� �߰� ����
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
