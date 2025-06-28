using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthController : MonoBehaviour {

    [field: Header("__Health Settings__")]
    [field: SerializeField, Range(0, 999)] private int maxHealth { get; set; } = 999;
    public int MaxHealth { get => maxHealth; }
    [field: SerializeField, ReadOnlyField] private int health { get; set; } = 100;
    public int Health { get => health; }
    [field: SerializeField] public bool isDead { get; private set; } = false;

    

    [field: Header("Events")]
    [field: SerializeField] private UnityEvent onTakeDamage { get; set; }
    [field: SerializeField] private UnityEvent onHeal { get; set; }
    [field: SerializeField] private UnityEvent onDeath { get; set; }
    [field: SerializeField] private UnityEvent onResurrect { get; set; }

    void Awake() {
        health = maxHealth;
    }

    #region Damage Methods

    public void TakeDamage(InputField damageInput) {
        if (damageInput != null) {
            if (string.IsNullOrEmpty(damageInput.text)) {
                Debug.LogWarning("Damage input field is empty");
                return;
            }
        }
        else {
            Debug.LogWarning("Damage input field is not assigned");
            return;
        }


        if (int.TryParse(damageInput.text, out int damage)) {
            if (damage < 0) {
                Debug.LogWarning("Damage cannot be negative");
                return;
            }

            TakeDamage(damage);
        } else {
            Debug.LogWarning("Invalid damage input");
        }
    }

    public void TakeDamage(int damage) {
        if (health > 0) {
            health -= damage;

            if (health <= 0) {
                Die();
                return;
            }

            onTakeDamage?.Invoke();
            //DisplayHealthOnClock();   // Called though onTakeDamage event
            Debug.Log($"<color=red>Took {damage} damage</color>, current health: {health}");
        }
    }

    #endregion

    #region Heal Methods

    public void Heal(InputField healInput) {
        if (healInput != null) {
            if (string.IsNullOrEmpty(healInput.text)) {
                Debug.LogWarning("Heal input field is empty");
                return;
            }
        }
        else {
            Debug.LogWarning("Heal input field is not assigned");
            return;
        }

        if (int.TryParse(healInput.text, out int healAmount)) {
            if (healAmount < 0) {
                Debug.LogWarning("Heal amount cannot be negative");
                return;
            }
            Heal(healAmount);
        } else {
            Debug.LogWarning("Invalid heal input");
        }
    }

    public void Heal(int amount) {
        if (health < maxHealth) {
            if (isDead) {
                Resurrect(amount);
                return;
            }

            health += amount;
            
            if (health > maxHealth) {
                health = maxHealth;
            }

            onHeal?.Invoke();
            //DisplayHealthOnClock();   // Called though onHeal event

            Debug.Log($"<color=green>Healed {amount} health</color>, current health: {health}");
        }
    }

    #endregion

    #region Death Methods

    void Resurrect(int amount) {
        isDead = false;
        health += amount;
        if (health > maxHealth) {
            health = maxHealth;
        }

        onResurrect?.Invoke();
        //DisplayHealthOnClock();   // Called though onResurrect event
        Debug.Log("<color=green>Resurrected</color>, current health: " + health);
    }

    void Die() {
        isDead = true;
        health = 0;

        onDeath?.Invoke();
        //DisplayHealthOnClock();   // Called though onDeath event
        Debug.Log("<color=red>Character is dead</color>");
    }

    #endregion

}