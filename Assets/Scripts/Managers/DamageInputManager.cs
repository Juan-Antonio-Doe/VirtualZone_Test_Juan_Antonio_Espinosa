using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageInputManager : MonoBehaviour {
	
	[field: Header("__Damage Input Settings__")]
	[field: SerializeField, TagSelector] private string targetColliderTag { get; set; } = "Enemy";

    [field: SerializeField] private Camera mainCamera { get; set; }
	[field: SerializeField] private InputField damageInputField { get; set; }


    void Update() {
#if UNITY_EDITOR || UNITY_STANDALONE
        // Right-click to damage the NPC under the mouse cursor
        if (Input.GetMouseButtonDown(1)) {
            TryDamageAtScreenPoint(Input.mousePosition);
        }

        // Left-click instantiates a bullet
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        // Touch input to damage the NPC under the touch point
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
            TryDamageAtScreenPoint(Input.GetTouch(0).position);
        }
#endif
    }

    private void TryDamageAtScreenPoint(Vector2 screenPoint) {
        Ray ray = mainCamera.ScreenPointToRay(screenPoint);

        if (Physics.Raycast(ray, out RaycastHit hit)) {
            if (hit.collider != null && hit.collider.CompareTag(targetColliderTag)) {
                NPC npc = hit.collider.GetComponentInParent<NPC>();
                
                if (npc != null) {
                    HealthController health = npc.GetComponent<HealthController>();
                    
                    if (health != null) {
                        health.TakeDamage(damageInputField);
                    }
                }
            }
        }
    }
}