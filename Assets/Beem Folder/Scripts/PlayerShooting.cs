using System.Collections;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("=== Weapon Stats ===")]
    [Tooltip("เปิดเพื่อยิงรัว (Auto) / ปิดเพื่อกดคลิกทีละนัด (Semi-Auto)")]
    public bool isAutomatic = true;
    [Tooltip("ความเร็วในการยิง (วินาทีต่อนัด)")]
    public float fireRate = 0.15f;
    [Tooltip("ระยะยิงไกลสุดของปืน")]
    public float weaponRange = 100f;

    [Header("=== Ammunition ===")]
    [Tooltip("จำนวนกระสุนสูงสุดต่อแม็กกาซีน")]
    public int maxAmmo = 30;
    private int currentAmmo;
    [Tooltip("เวลาที่ใช้ในการรีโหลด (วินาที)")]
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    [Header("=== Accuracy & Spread ===")]
    [Tooltip("ความบานของเป้า (ค่า 0 คือตรงเป๊ะ 100%)")]
    public float spreadAmount = 0.5f;

    [Header("=== References ===")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    [Tooltip("ลาก Camera หลักมาใส่ตรงนี้ จะได้ไม่ต้องค้นหาทุกครั้งที่ยิง")]
    public Camera mainCam;

    [Header("=== Effects (Optional) ===")]
    public ParticleSystem muzzleFlash; // แสงปลายปืน
    public AudioSource audioSource;    // ตัวกำเนิดเสียง
    public AudioClip shootSound;       // เสียงยิง

    private float nextFireTime;

    void Start()
    {
        // เริ่มเกมมาเติมกระสุนให้เต็ม
        currentAmmo = maxAmmo;

        // ถ้าลืมใส่กล้อง ให้มันหาออโต้
        if (mainCam == null) mainCam = Camera.main;
    }

    void Update()
    {
        // ถ้าร่างนี้ไม่ได้ถูกใช้งาน (ไม่ได้สิงอยู่) ให้ข้ามไปเลย
        if (!this.enabled) return;

        // ถ้ากำลังรีโหลดอยู่ จะยิงไม่ได้
        if (isReloading) return;

        // กด R เพื่อรีโหลดแบบ Manual
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        // เช็คการคลิกเมาส์ตามโหมดปืน (ยิงค้าง Auto หรือ กดทีละครั้ง Semi-Auto)
        bool isShooting = isAutomatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        if (isShooting && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
            else
            {
                // กระสุนหมด ให้บังคับรีโหลดอัตโนมัติ
                StartCoroutine(Reload());
            }
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("กำลังรีโหลดกระสุน...");

        // ตรงนี้สามารถสั่งเล่น Animation รีโหลด หรือเสียงรีโหลดได้

        // รอเวลาตามที่ตั้งค่าไว้
        yield return new WaitForSeconds(reloadTime);

        // เติมกระสุนและปลดล็อกการยิง
        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("รีโหลดเสร็จสิ้น!");
    }

    void Shoot()
    {
        // ลดกระสุน
        currentAmmo--;

        // เล่น Effect (ถ้ามีการใส่ไว้)
        if (muzzleFlash != null) muzzleFlash.Play();
        if (audioSource != null && shootSound != null) audioSource.PlayOneShot(shootSound);

        // คำนวณเป้าหมายจากหน้าจอ
        if (mainCam == null) mainCam = Camera.main;
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, weaponRange))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(weaponRange);
        }

        // --- ระบบเป้าบาน (Spread) ---
        // สุ่มเพิ่มตำแหน่ง X, Y, Z นิดหน่อยจากจุดเป้าหมายตรงกลาง
        float xSpread = Random.Range(-spreadAmount, spreadAmount);
        float ySpread = Random.Range(-spreadAmount, spreadAmount);
        float zSpread = Random.Range(-spreadAmount, spreadAmount);

        // ถ้าค่า spreadAmount เป็น 0 กระสุนจะพุ่งเข้ากลางเป้าเป๊ะๆ 
        targetPoint += new Vector3(xSpread, ySpread, zSpread);

        // คำนวณทิศทาง
        Vector3 direction = targetPoint - firePoint.position;

        // --- [แก้ไขใหม่] สร้างกระสุนและเก็บค่าไว้ในตัวแปร ---
        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));

        // ดึงสคริปต์ Bullet ของกระสุนนัดนี้มา
        Bullet bulletScript = newBullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            // ส่ง Tag ของคนที่ยิง (เช่น "Player" หรือ "Enemy") ไปบอกกระสุน
            // เพื่อให้กระสุนรู้ว่าต้องไม่ทำดาเมจใส่ Tag นี้นั่นเอง
            bulletScript.ignoreTag = this.gameObject.tag;
        }
    }
}