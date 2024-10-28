using NineSolsAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EyeOfCthulhu : MonoBehaviour
{
    [SerializeField] private GameObject targetPlayer;
    [SerializeField] private FloatingHealthBar healthBar;

    [SerializeField] private int damage = 15;
    [SerializeField] private int defense = 12;
    [SerializeField] private float lifeMax = 2800;
    [SerializeField] private float currentLife;

    [SerializeField] private float speed = 150f;
    [SerializeField] private float attackRange = 150f;


    [SerializeField] private float cooldownTime = 1.5f; // Time to wait before moving toward the player
    [SerializeField] private float attackCooldownTime = 1f; // Time to wait before moving toward the player
    [SerializeField] private float lastMoveTime; // Tracks when the boss last moved
    [SerializeField] private float lastAttackMoveTime; // Tracks when the last attack move was made

    [SerializeField] private bool invincible = false; // Tracks when the last attack move was made

    private Rigidbody2D rb; // Rigidbody2D reference

    private void Awake()
    {
        healthBar = GetComponentInChildren<FloatingHealthBar>();
    }

    private void Start()
    {
        currentLife = lifeMax;

        rb = GetComponent<Rigidbody2D>();

        lastMoveTime = Time.time; // Initialize the last move time
        lastAttackMoveTime = Time.time; // Initialize the last attack move time
    }

    private void Update()
    {
        AI();
    }

    public void AI()
    {
        // Find the closest player if not already targeted
        if (targetPlayer == null)
        {
            TargetClosestPlayer();
        }

        // Calculate the direction towards the player
        Vector2 direction = (targetPlayer.transform.position - transform.position).normalized;

        // Calculate the current distance between the boss and the player
        float currentDistance = Vector2.Distance(transform.position, targetPlayer.transform.position);

        // Check if enough time has passed to move toward the player
        if (Time.time >= lastMoveTime + cooldownTime)
        {
            // Move toward the player if further away than attackRange
            if (currentDistance > attackRange)
            {
                rb.velocity = direction * speed; // Directly set the velocity toward the player
            }


            // Update the last move time
            lastMoveTime = Time.time;
        }

        if (currentDistance <= attackRange)
        {
            if (Time.time >= lastAttackMoveTime + attackCooldownTime) // Check if 1 second has passed
            {
                rb.velocity = direction * speed; // Directly set the velocity toward the player
                lastAttackMoveTime = Time.time; // Update the last attack move time
                AttackPlayer();
            }
        }
    }

    void TargetClosestPlayer()
    {
        float closestDistance = Mathf.Infinity;

        // Implement logic to find the closest player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); // Assuming player has the "Player" tag
        foreach (GameObject player in players)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                targetPlayer = player; // Set the closest player as the target
            }
        }
    }

    void AttackPlayer()
    {
        // Implement attack logic here (melee, ranged attacks, etc.)
    }

    void TakeDamage(float damageAmount)
    {
        currentLife -= damageAmount;
        healthBar.UpdateHealthBar(currentLife, lifeMax);
    }

    void Die()
    {
        Destroy(gameObject);
    }

    IEnumerator DelayedAction()
    {
        // Wait for 0.5 seconds
        yield return new WaitForSeconds(0.5f);
        invincible = false;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        ToastManager.Toast(col.gameObject.name);
        if (Player.i.CurrentState.name == "RollState")
            return;

        if (col.gameObject.name == "FooInit")
        {
            invincible = true;
            // Start the coroutine for the delayed action
            StartCoroutine(DelayedAction());
        }

        if (col.gameObject.name == "Effect_ToClose_Noise" && !invincible)
        {
            Player.i.health.currentValue -= 10;
            if (Player.i.health.currentValue <= 0)
            {
                Player.i.AllFull();
                SceneManager.LoadScene("testScript");
            }
        }

        //if (col.gameObject.name == "AttackFront")
        //{
        //    ToastManager.Toast("AttackFront");
        //    TakeDamage(200);
        //}

        TakeDamage(col.gameObject.GetComponent<EffectDealer>().FinalValue);

        if (currentLife <= 0)
            Die();
    }
}
