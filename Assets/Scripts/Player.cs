﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private PlayerMovement playerMovement;

    public float base_maxhealth = 100;
    public float health_perLevel = 20;
    public float Health { get { return health; } }
    public int Level { get { return level; } }
    public float attack1Dam = 5;
    public float attack2Dam = 7.5f;
    public float attack3Dam = 10;
    public float downwardAttackDam = 7.5f;
    public Text coinText;
    public Text healhtText;

    private float activeMaxHealth;
    private float health;
    private int level;
    private int coins;
    private string enemyWeaponTag = "EnemyWeapon";
    private string trapTag = "Trap";

    [HideInInspector] public bool isDead;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }
    void Start()
    {
        playerMovement.damageContainer.SetDamageCall(() => GetDamage());
        playerMovement.damageContainer.SetDoKnockbackCall(() => IsCurrnetAttackKnockingBack());
        Initialize();
        coins = 0;
        SetCoinText();
        SetHealthText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            print("DON'T CHEAT!!!!!!!!!!!!!!!");
            Revive();
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

    private void Die()
    {
        isDead = true;
        playerMovement.animator.SetBool("Dead", true);
    }

    private void Revive()
    {
        isDead = false;
        health = activeMaxHealth;
        playerMovement.animator.SetBool("Dead", false);
    }

    private void Initialize()
    {
        activeMaxHealth = base_maxhealth + level * health_perLevel;
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

    void SetCoinText()
    {
        coinText.text = "Coins: " + coins.ToString();
    }

    void SetHealthText()
    {
        healhtText.text = "Health: " + health.ToString("0") + activeMaxHealth.ToString("0");
    }
}
