﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class Player : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerLevel level;

    public float base_maxhealth = 100;
    public float health_perLevel = 20;
    public float Health { get { return health; } }
    public int Level { get { return level.currentLevel; } }

    public List<int> Checkpoints;

    public float attack1Dam = 5;
    public float attack2Dam = 7.5f;
    public float attack3Dam = 10;
    public float downwardAttackDam = 7.5f;
    public Text coinText;
    public Text healthText;
    public int coins;

    private float activeMaxHealth;
    private float health;
    //private int level;
    private string enemyWeaponTag = "EnemyWeapon";
    private string trapTag = "Trap";
    private Action interactAction;
    private Vector2 respawnPos;

    [HideInInspector] public bool isDead;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        level = GetComponent<PlayerLevel>();
    }
    void Start()
    {
        playerMovement.damageContainer.SetDamageCall(() => GetDamage());
        playerMovement.damageContainer.SetDoKnockbackCall(() => IsCurrnetAttackKnockingBack());
        coins = 0;
        CalculateStats();
        SetCoinText();
        SetHealthText();
        level.SetLevelText();
    }

    void Update()
    {
        if (Input.GetButtonDown("Interact"))
        {
            if(interactAction != null)
                interactAction.Invoke();
        }
    }

    /// <summary>
    /// Hits the player
    /// </summary>
    /// <param name="damage">the amount of damage to deal</param>
    /// <param name="knockBackDirection">set to 1 if attack came from right of PC, -1 if from left, leave 0 if no knockback</param>
    public void GetHit(float damage, int knockBackDirection = 0)
    {
        health -= damage;
        print("PC Health - " + health);
        if (knockBackDirection == 2)
            playerMovement.KnockbackUp(damage);
        else if (knockBackDirection != 0)
            playerMovement.KnockBack(knockBackDirection == 1, damage);
        if (health <= 0)
            Die();
        SetHealthText();
    }

    public void SetInteractAction(Action action)
    {
        interactAction = action;
    }
    public void ClearInteractAction(Action action)
    {
        if (interactAction == action)
            interactAction = null;
    }

    private void Die()
    {
        isDead = true;
        playerMovement.animator.SetBool("Dead", true);
        StartCoroutine(ReviveAfterTime(2f));
    }
    private IEnumerator ReviveAfterTime(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        Revive();
    }

    private void Revive()
    {
        isDead = false;
        health = activeMaxHealth;
        playerMovement.animator.SetBool("Dead", false);
        transform.position = respawnPos;
        SetHealthText();
        SetCoinText();
    }
    public void SetRespawnPos(Vector2 respawnPos)
    {
        this.respawnPos = respawnPos;
    }

    private void CalculateStats()
    {
        //activeMaxHealth = base_maxhealth + level.currentLevel * health_perLevel;
        //health = activeMaxHealth;
        SetHealthByLevel();
    }

    public void SetHealthByLevel()
    {
        activeMaxHealth = base_maxhealth + level.currentLevel * health_perLevel;
        health = activeMaxHealth;
    }
    public float GetDamage()
    {
        if (playerMovement.isDownwardAttacking)
            return downwardAttackDam;
        switch (playerMovement.currentAttackNum)
        {
            case 1:
                return attack1Dam;
            case 2:
                return attack2Dam;
            case 3:
                return attack3Dam;
        }
        Debug.LogError("current attack number is nor recognized: " + playerMovement.currentAttackNum);
        return -1;
    }

    public void AddHealth(float healthAmount)
    {
        healthAmount = activeMaxHealth * healthAmount / 100;
        health += healthAmount;
        if (health >= activeMaxHealth)
            health = activeMaxHealth;
        SetHealthText();
    }

    public bool IsCurrnetAttackKnockingBack()
    {
        return playerMovement.currentAttackNum == 3;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyWeaponTag) && !isDead && !playerMovement.isInvulnerable)
            GetHit(other.transform.GetComponentInParent<DamageContainer>().GetDamage(),
                other.transform.position.x > transform.position.x ? 1 : -1);

        if (other.gameObject.CompareTag("Pick Up"))
        {
            Destroy(other.gameObject);
            coins++;
            SetCoinText();
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(trapTag) && !playerMovement.isInvulnerable && !isDead)
        {
            GetHit(collision.collider.GetComponent<Trap>().damagePercent * activeMaxHealth / 100, 2);
        }
    }

    public void SetCoinText()
    {
        coinText.text = "Coins: " + coins.ToString();
    }

    public void SetHealthText()
    {
        healthText.text = "Health: " + health.ToString() + "/" + activeMaxHealth.ToString();
    }
}
