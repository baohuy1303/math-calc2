using UnityEngine;
using UnityEngine.UI;
public class Cup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text scoreText;
    [Header("Detection Settings")]
    [Tooltip("Drag the BoxCollider2D (the trigger zone) here")]
    [SerializeField] private Collider2D scoreTriggerCollider;
    private int score = 0;
    public GameObject particleSystem;
    void Start()
    {
        UpdateScoreText();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Check if the object colliding is our projectile
        NumericalProjectile proj = collision.GetComponent<NumericalProjectile>();
        if (proj == null) return;
        Instantiate(particleSystem, transform.position, Quaternion.identity);
        // 2. Check if the collision happened specifically with our designated score trigger
        // This ignores your Polygon Collider or any other colliders on this object.
        if (scoreTriggerCollider != null && !collision.IsTouching(scoreTriggerCollider))
        {
            return;
        }
        // Score!
        score++;
        UpdateScoreText();
        Respawn();
        
        Destroy(collision.gameObject);
        
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private void Respawn()
    {
        Vector3 newPos = transform.position;
        newPos.x = Random.Range(0f, 4.7f);
        newPos.y = Random.Range(-2f, 2f);
        transform.position = newPos;
    }
}