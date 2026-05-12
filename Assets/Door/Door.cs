using UnityEngine;

public class Door : Interactable
{
    [Header("Behaviour")]
    [SerializeField] private Transform doorBody;
    [SerializeField] private float speed = 5f;

    [Header("Positions")]
    public Vector3 openedPosition;
    public Vector3 closedPosition;

    public bool Opened { get; private set; }

    private void Start()
    {
        if (doorBody == null) doorBody = transform;
        doorBody.localPosition = closedPosition;
    }

    private void Update()
    {
        // เช็คว่าอยู่ใกล้และกด E หรือไม่
        if (isInside && Input.GetKeyDown(KeyCode.E))
        {
            Opened = !Opened;
        }

        // สั่งให้ประตูลื่นไปหาตำแหน่งเป้าหมาย
        Vector3 targetPos = Opened ? openedPosition : closedPosition;
        doorBody.localPosition = Vector3.Lerp(doorBody.localPosition, targetPos, speed * Time.deltaTime);
    }
}