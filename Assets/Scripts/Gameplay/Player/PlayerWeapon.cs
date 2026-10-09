using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>Fires lasers from the ship's muzzle while the fire button is held.</summary>
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] Projectile laserPrefab;
        [SerializeField] Transform muzzle;
        [SerializeField, Min(0.02f)] float fireInterval = 0.2f;

        PlayerInputReader input;
        float nextShotTime;

        void Awake() => input = GetComponent<PlayerInputReader>();

        void Update()
        {
            if (!input.IsFiring || Time.time < nextShotTime) return;

            nextShotTime = Time.time + fireInterval;
            Instantiate(laserPrefab, muzzle.position, muzzle.rotation);
            AudioDirector.Play(SoundEffect.LaserShot);
        }
    }
}
