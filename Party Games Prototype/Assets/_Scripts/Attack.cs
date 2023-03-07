using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private PlayerController playerController;
    private PlayerInputController playerInputController;

    [Header("Power")]
    [SerializeField] private float maxPower = 10f;
    private float currentPower = 0;

    [Header("Attack")]
    public Transform attackPoint;
    [SerializeField] private float attackPower;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCost = 5f;
    [SerializeField] [Range(0, 0.5f)] private float attackTime = 0.1f;
    private float attackTimeCounter;

    private bool isAttacking = false;
    private float elapsedTime;

    private void Awake()
    {
        playerInputController = GetComponent<PlayerInputController>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (currentPower < maxPower)
        {
            IncreasePower(Time.deltaTime);
        }

        if (playerInputController.attackIsPressed && currentPower >= attackCost)
        {
            isAttacking = true;

            attackTimeCounter = attackTime;

            playerController.playerRB.velocity = Vector2.zero;
            playerController.playerRB.gravityScale = 0;
            //playerController.enabled = false;

            MeleeAttack();
            DecreasePower(attackCost);

            playerController.animator.SetTrigger("isAttacking");
        }

        if (attackTimeCounter > 0)
        {
            attackTimeCounter -= Time.deltaTime;
        }
        else
        {
            isAttacking = false;
            //playerController.enabled = true;
            elapsedTime += Time.deltaTime;
            //playerController.playerRB.gravityScale = Mathf.Lerp(playerController.playerRB.gravityScale, playerController.gravityScale, elapsedTime / 2);
        }

        UpdatePowerBar();

        playerInputController.attackIsPressed = false;
    }

    private void MeleeAttack()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerController.enemyLayer);

        if (hitEnemy)
        {
            PlayerController enemyController = hitEnemy.GetComponent<PlayerController>();

            Vector2 dir = hitEnemy.transform.position - transform.position;

            enemyController.playerRB.AddForce(attackPower * dir, ForceMode2D.Impulse);
        }
    }

    public void IncreasePower(float amount)
    {
        currentPower = (currentPower + amount > maxPower) ? maxPower : currentPower + amount;
    }

    public void DecreasePower(float amount)
    {
        currentPower = (currentPower - amount <= 0) ? 0 : currentPower - amount;
    }

    private void UpdatePowerBar() => UIManager.Instance.UpdatePowerBar(playerController.player, currentPower / maxPower);
}
