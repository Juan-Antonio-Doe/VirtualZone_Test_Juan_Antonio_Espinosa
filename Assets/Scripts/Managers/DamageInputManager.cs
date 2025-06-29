using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageInputManager : MonoBehaviour {
	
	[field: Header("__Damage Input Settings__")]
	[field: SerializeField, TagSelector] private string targetColliderTag { get; set; } = "Enemy";

    [field: SerializeField] private Camera mainCamera { get; set; }
	[field: SerializeField] private InputField damageInputField { get; set; }

    [field: Header("Bullet Settings")]
    [field: SerializeField, 
        Tooltip("Bullet spawn distance from the camera")] private float bulletSpawnDistance { get; set; } = 1.0f;
    [field: SerializeField] private ObjectPool objectPool { get; set; }
    [field: SerializeField] private Projectile projectilePrefab { get; set; }


    void Update() {
#if UNITY_EDITOR || UNITY_STANDALONE
        // Right-click to damage the NPC under the mouse cursor
        if (Input.GetMouseButtonDown(1)) {
            TryDamageAtScreenPoint(Input.mousePosition);
        }

        // Left-click instantiates a bullet
        if (Input.GetMouseButtonDown(0)) {
            TryDamageAtScreenPoint(Input.mousePosition, true);
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        // Touch input to damage the NPC under the touch point
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
            TryDamageAtScreenPoint(Input.GetTouch(0).position);
        }
#endif
    }

    private void TryDamageAtScreenPoint(Vector2 screenPoint, bool shoot=false) {
        Ray ray = mainCamera.ScreenPointToRay(screenPoint);

        if (Physics.Raycast(ray, out RaycastHit hit)) {
            if (hit.collider != null && hit.collider.CompareTag(targetColliderTag)) {
                if (shoot) {
                    // If shoot is true, instantiate a projectile
                    if (projectilePrefab != null && objectPool != null) {
                        InstantiateProjectile(hit);
                    }
                }
                else {
                    // If shoot is false, damage the NPC
                    NPC npc = hit.collider.GetComponentInParent<NPC>();

                    if (npc != null) {
                        DamageTarget(npc.healthController);
                    }
                }
            }
        }
    }

    void InstantiateProjectile(RaycastHit hit) {
        // Calculate spawn position in front of the camera
        Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * bulletSpawnDistance;

        // Calculate direction towards hit.point
        Vector3 direction = (hit.point - spawnPosition).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject bullet = objectPool.SpawnFromPool(
            projectilePrefab.poolTag,
            projectilePrefab.gameObject,
            spawnPosition,
            rotation
        );

        // Set the projectile properties
        if (bullet != null) {
            Projectile projectile = bullet.GetComponent<Projectile>();
            if (projectile != null) {
                projectile.myPool = objectPool;
                projectile.targetTag = targetColliderTag;
                projectile.damageInputManager = this;
            }
        }
    }

    public void DamageTarget(HealthController healthController) {
        if (healthController != null && damageInputField != null) {
            healthController.TakeDamage(damageInputField);
        }
    }
}