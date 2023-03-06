using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private PlayerController playerController;
    private PlayerInputController playerInputController;
    private Animator animator;

    [SerializeField] private float maxPower = 100f;
    private float currentPower = 0;

    public Transform attackPoint;
    [SerializeField] private float attackPower;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCost = 50f;

    private void Awake()
    {
        playerInputController = GetComponent<PlayerInputController>();
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Debug.Log(currentPower);

        if (currentPower < maxPower)
        {
            IncreasePower(Time.deltaTime);
            Debug.Log(currentPower);
        }

        if (playerInputController.attackIsPressed && currentPower >= attackCost)
        {
            Debug.Log(playerInputController.attackIsPressed);
            playerController.playerRB.velocity = Vector2.zero;
            MeleeAttack();
            DecreasePower(attackCost);
        }

        UpdatePowerBar();
        UpdateAnimator();

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
        currentPower = (currentPower - amount < 0) ? 0 : currentPower - amount;
    }

    private void UpdateAnimator()
    {
        animator.SetBool("isAttacking", playerInputController.attackIsPressed);
    }

    private void UpdatePowerBar() => UIManager.Instance.UpdatePowerBar(playerController.player, currentPower);
}
