using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class GunShoot : MonoBehaviour
{
    [SerializeField] private HumanoidLandInput input;
    [SerializeField] private HumanoidLandController player;
    [SerializeField] private float weaponDamage = 15;
    public float fireRate = 0.25f; // Number in seconds which controls how often the player can fire
    public float weaponRange = 20f; // Distance in Unity units over which the player can fire

    public Transform gunEnd;
    public ParticleSystem muzzleFlash;
    public ParticleSystem cartridgeEjection;

    public GameObject metalHitEffect;
    public GameObject sandHitEffect;
    public GameObject stoneHitEffect;
    public GameObject waterLeakEffect;
    public GameObject waterLeakExtinguishEffect;
    public GameObject[] fleshHitEffects;
    public GameObject woodHitEffect;

    private float nextFire; // Float to store the time the player will be allowed to fire again, after firing
    private Animator anim;
    private GunAim gunAim;


    private void Update()
    {
        if (input.ShootIsPressed) Shoot();
    }

    private void Shoot()
    {
        if (Time.time > nextFire && player.playerIsAiming)
        {
            nextFire = Time.time + fireRate;
            // Quaternion hitRotation = Quaternion.identity;
            Transform targetHit = null;
            // Vector3 mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity))
            {
                // hitRotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);
                // mouseWorldPosition = hit.point;
                targetHit = hit.transform;
            }

            if (targetHit != null)
            {
                //Instantiate(pfContactParticle, mouseWorldPosition, hitRotation);
                HandleHit(hit);
                Instantiate(muzzleFlash, gunEnd.position, gunEnd.rotation);
            }
        }
    }

    private void HandleHit(RaycastHit hit)
    {
        if (hit.collider != null)
        {
            int layer = hit.collider.transform.gameObject.layer;
            switch (layer)
            {
                default:
                    SpawnDecal(hit, stoneHitEffect);
                    break;
                case 6:
                    SpawnDecal(hit, metalHitEffect);
                    break;
            }

            if (hit.collider.gameObject.CompareTag("Zombie"))
                hit.collider.gameObject.GetComponent<ZombieController>().TakeDamage(weaponDamage);
        }

        void SpawnDecal(RaycastHit hit, GameObject prefab)
        {
            var spawnedDecal = GameObject.Instantiate(prefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }
}