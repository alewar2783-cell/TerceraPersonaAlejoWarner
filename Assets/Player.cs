using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;



[RequireComponent(typeof(Rigidbody))]
public class Personaje : MonoBehaviour
{
    [Tooltip("Fuerza")]
    [SerializeField] private float Speed = 5f;
    [Tooltip("Salto")]
    [SerializeField] private float jumpForce = 10f;
    [Tooltip("Radio Check Piso")]
    [SerializeField] private float groundCheckRadius = 0.3f;
    [Tooltip("Capa Piso")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask climbWall;
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject winPanel;

    private int totalCollectibles = 0;
    private int score = 0;
    private Rigidbody playerRigidbody;
    public GameObject Objeto;
    public GameObject[] collectibles;
    bool IsGrounded = false;    


    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        Time.timeScale = 1f;
        collectibles = GameObject.FindGameObjectsWithTag("Collectible");
        foreach (GameObject collectible in collectibles)
        {
            if (collectible != null)
            {
                totalCollectibles += collectible.GetComponent<Collectable>().scoreValue;
            }
        }
        winPanel.SetActive(false);
        UpdateUI();
    }


    private void FixedUpdate()
    {
        HandleMovement();
    }

    [SerializeField] private Transform cameraTransform; // C�mara a seguir

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // Direcci�n relativa a la c�mara
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = cameraForward * vertical + cameraRight * horizontal;
            moveDirection.Normalize();

            // Movimiento con velocidad directa
            Vector3 velocity = moveDirection * Speed;
            velocity.y = playerRigidbody.linearVelocity.y; // Manten� la velocidad vertical (para salto)
            playerRigidbody.linearVelocity = velocity;

            // (Opcional) Rotar el personaje hacia la direcci�n del movimiento
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        else
        {
            // Si no hay input, mantenemos la velocidad vertical pero detenemos horizontal
            Vector3 velocity = new Vector3(0, playerRigidbody.linearVelocity.y, 0);
            playerRigidbody.linearVelocity = velocity;
        }
    }



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            IsGrounded = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            CollectItem(other);
        }
        if (other.CompareTag("PowerUp"))
        {
            PowerUpCollected(other);
        }
    }
    private void PowerUpCollected(Collider other)
    {
        float newJumpForce = other.GetComponent<PowerUp>().jumpForce;
        float powerUpDuration = other.GetComponent<PowerUp>().duration;

        jumpForce = newJumpForce;

        Destroy(other.gameObject);

        Invoke("PowerUpEnd", powerUpDuration);

    }

    private void PowerUpEnd()
    {
        jumpForce = 5f;
    }

    private void CollectItem(Collider other)
    {
        Collectable collectedItem = other.GetComponent<Collectable>();
        score += collectedItem.scoreValue;
        UpdateUI();
        Destroy(other.gameObject);

        if (score >= totalCollectibles)
        {
            Win();
        }
    }

    private void Win()
    {
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void UpdateUI()
    {
        scoreText.text = "Puntos: " + score.ToString() + " / " + totalCollectibles.ToString();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(1);
    }

    private void Update()
    {
        HandleJump();
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded)
        {
            playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            IsGrounded = false; // Evita dobles saltos
        }
    }
}
