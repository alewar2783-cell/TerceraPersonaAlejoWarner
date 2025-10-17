using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Personaje : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveForce = 15f;
    [SerializeField] private float maxSpeed = 15f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField, Tooltip("Punto de chequeo en la base del personaje")]
    private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform cameraTransform;

    [Header("Coleccionables y PowerUps")]
    [SerializeField] private float powerUpJumpBoost = 10f;
    [SerializeField] private float powerUpDuration = 5f;
    [SerializeField] private float respawnTime = 30f;
    [SerializeField] private int scorePerCollectible = 1;

    private Rigidbody rb;
    private float originalJumpForce;
    private int score;
    private Coroutine activePowerUp;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalJumpForce = jumpForce;

        if (groundCheckPoint == null)
        {
            Transform t = transform.Find("GroundCheck");
            if (t != null) groundCheckPoint = t;
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void Update()
    {
        HandleJumpInput();
    }

    // === MOVIMIENTO ===
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = cameraForward * vertical + cameraRight * horizontal;
            rb.AddForce(moveDirection * moveForce);

            // Limitar velocidad
            Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (flatVelocity.magnitude > maxSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
            }

            // Rotar personaje hacia dirección del movimiento
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    // === SALTO ===
    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log("Salto ejecutado");
        }
    }

    private bool IsGrounded()
    {
        Vector3 checkPos = groundCheckPoint ? groundCheckPoint.position : transform.position;
        return Physics.CheckSphere(checkPos, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
    }

    // === COLISIONES ===
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            score += scorePerCollectible;
            Debug.Log($"Coleccionable recogido. Puntuación actual: {score}");
            StartCoroutine(RespawnObject(other.gameObject, respawnTime));
        }
        else if (other.CompareTag("PowerUp"))
        {
            ApplyPowerUp(powerUpJumpBoost, powerUpDuration);
            Debug.Log("PowerUp activado: salto mejorado temporalmente");
            StartCoroutine(RespawnObject(other.gameObject, respawnTime));
        }
    }

    // === POWER UP ===
    private void ApplyPowerUp(float newJumpForce, float duration)
    {
        if (activePowerUp != null) StopCoroutine(activePowerUp);
        activePowerUp = StartCoroutine(PowerUpCoroutine(newJumpForce, duration));
    }

    private IEnumerator PowerUpCoroutine(float newJumpForce, float duration)
    {
        jumpForce = newJumpForce;
        yield return new WaitForSeconds(duration);
        jumpForce = originalJumpForce;
        activePowerUp = null;
        Debug.Log("PowerUp finalizado. Salto restaurado.");
    }

    // === RESPAWN ===
    private IEnumerator RespawnObject(GameObject obj, float delay)
    {
        obj.SetActive(false);
        yield return new WaitForSeconds(delay);
        obj.SetActive(true);
    }

    // === DEBUG VISUAL ===
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = groundCheckPoint ? groundCheckPoint.position : transform.position;
        Gizmos.DrawWireSphere(pos, groundCheckRadius);
    }
}
