using System;
using UnityEngine;

public class BossBehavior : MonoBehaviour
{
    public int aiStyle = 4;
    public int target = -1;
    public Vector3 position;
    public Vector3 velocity;
    public float rotation;
    public float timeLeft = 60f; // Example time left
    public int life;
    public int lifeMax = 100; // Example max life
    private float[] ai = new float[4];

    void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    private void HandleRotation()
    {
        float num106 = position.x - GetTargetPosition().x;
        float num107 = position.y - GetTargetPosition().y - 59f; // Adjust as needed
        float num108 = Mathf.Atan2(num107, num106) + Mathf.PI / 2;

        // Normalize rotation
        if (num108 < 0f) num108 += 2 * Mathf.PI;
        else if (num108 > 2 * Mathf.PI) num108 -= 2 * Mathf.PI;

        float num109 = GetRotationSpeed();

        // Update rotation based on calculated angle
        UpdateRotation(num108, num109);
    }

    private Vector3 GetTargetPosition()
    {
        return Player.i.Center;
    }

    private float GetRotationSpeed()
    {
        return (ai[0] == 0f) ? 0.02f : (ai[0] == 3f && ai[1] == 2f && ai[2] > 40f) ? 0.08f : 0.05f;
    }

    private void UpdateRotation(float targetAngle, float speed)
    {
        if (rotation < targetAngle)
        {
            rotation += (targetAngle - rotation) > Mathf.PI ? -speed : speed;
        }
        else
        {
            rotation -= (rotation - targetAngle) > Mathf.PI ? -speed : speed;
        }

        if (Mathf.Abs(rotation - targetAngle) < speed)
        {
            rotation = targetAngle;
        }

        // Normalize rotation
        if (rotation < 0f) rotation += 2 * Mathf.PI;
        else if (rotation > 2 * Mathf.PI) rotation -= 2 * Mathf.PI;
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = Player.i.transform.position; // Get the player's position
        float distanceToTarget = Vector3.Distance(position, targetPosition); // Calculate distance to player

        // Determine movement speed and direction
        float normalMoveSpeed = 1f; // Normal movement speed
        float sprintMoveSpeed = 3f; // Sprint movement speed
        Vector3 direction = (targetPosition - position).normalized; // Direction towards player

        // Update boss position based on distance to target
        if (distanceToTarget > 100f) // Engage when far away
        {
            position += direction * normalMoveSpeed; // Move towards the player
        }
        else if (distanceToTarget < 70f) // Close enough to sprint
        {
            position += direction * sprintMoveSpeed; // Sprint towards the player
        }
        else if (distanceToTarget < 40f) // Retreating behavior when very close
        {
            position -= direction * normalMoveSpeed; // Move away from the player
        }

        // Update position in the Transform
        transform.position = position;

        // Update rotation to face the target
        rotation = Mathf.Atan2(direction.y, direction.x);
    }

}
