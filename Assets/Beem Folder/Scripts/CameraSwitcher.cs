using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("=== Camera Toggle Settings ===")]
    public KeyCode switchKey = KeyCode.F;
    [Tooltip("ความเร็วในการบินสลับร่าง หรือสลับ FPS/TPS")]
    public float transitionSpeed = 12f;

    [Header("=== Positions (Local Space) ===")]
    public Vector3 fpsPosition = Vector3.zero;

    [Header("=== Marvel Rivals TPS Style ===")]
    public Vector3 tpsPosition = new Vector3(0.8f, -0.4f, -2.5f);

    [Header("=== Collision Settings ===")]
    public LayerMask obstacleLayers;

    private bool isTPS = false;

    void Start()
    {
        transform.localPosition = fpsPosition;
        if (obstacleLayers == 0) obstacleLayers = ~LayerMask.GetMask("Player", "Enemy", "Ignore Raycast");
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey)) isTPS = !isTPS;

        Vector3 targetLocalPos = fpsPosition;

        if (isTPS)
        {
            Vector3 pivotWorldPos = transform.parent.position;
            Vector3 desiredWorldPos = transform.parent.TransformPoint(tpsPosition);

            Vector3 direction = desiredWorldPos - pivotWorldPos;
            if (Physics.Raycast(pivotWorldPos, direction.normalized, out RaycastHit hit, direction.magnitude, obstacleLayers))
            {
                Vector3 safeWorldPos = hit.point + hit.normal * 0.15f;
                targetLocalPos = transform.parent.InverseTransformPoint(safeWorldPos);
            }
            else
            {
                targetLocalPos = tpsPosition;
            }
        }

        // ดึงตำแหน่งกล้องให้ลอยเข้า-ออก หรือลอยไปสิงร่างใหม่อย่างนุ่มนวล
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, Time.deltaTime * transitionSpeed);

        // [เพิ่มใหม่] ค่อยๆ หมุนกล้องให้ตรงเป้า 
        // เวลาสิงร่าง กล้องจะค่อยๆ สวิงหันหน้าไปทางเดียวกับที่ร่างใหม่กำลังมองอยู่แบบสวยงาม
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * transitionSpeed);
    }
}