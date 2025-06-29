using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour {

    [field: Header("Projectile Settings")]
    [field: SerializeField] private Rigidbody rb { get; set; }
    [field: SerializeField] private float speed { get; set; } = 5f;
    [field: SerializeField] private float lifeTime { get; set; } = 5f;
    [field: SerializeField] public string poolTag { get; private set; } = "Mouse_Projectile";

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] public string targetTag { get; set; }
    [field: SerializeField, ReadOnlyField] public ObjectPool myPool { get; set; }
    [field: SerializeField, ReadOnlyField] public DamageInputManager damageInputManager { get; set; }


    void OnEnable() {
        //Para que funcione, hay que asegurarse de que el proyectil esta rotado para que mire hacia la direccion en la que queremos dispararlo
        rb.velocity = transform.forward * speed;

        StartCoroutine(DisableAfterTimeCo());

        //Debug.Log("Projectil launched");
    }

    IEnumerator DisableAfterTimeCo() {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision other) {
        //Debug.Log($"Projectil hit {other.name}");
        if (other.gameObject.CompareTag(targetTag)) {
            // In a larger project, this would not be optimal. GetComponents in general are expensive.
            damageInputManager.DamageTarget(other.gameObject.GetComponentInParent<NPC>().healthController);
            
            gameObject.SetActive(false);
        }
    }

    void OnDisable() {
        if (!gameObject.scene.isLoaded) return; // Avoid errors when the scene is unloaded (e.g., exiting play mode)

        rb.velocity = Vector3.zero;

        myPool.ReturnToPool(poolTag, gameObject);
    }
}