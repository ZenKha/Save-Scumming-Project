using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject missVFX;
    [SerializeField] private GameObject floatingText;

    private Rigidbody rb;

    private int _damage;
    private int _pierce;
    private int _attackRange;
    private int _accuracy;

    private Vector2 startPos;
    private Vector2 lastSpotVisited;

    private void Start()
    {
        Physics.IgnoreCollision(GetComponent<Collider>(), transform.root.GetComponent<Collider>(), true);
        rb = GetComponent<Rigidbody>();

        var unitInfo = transform.root.GetComponent<UnitInfo>();
        _damage = unitInfo.Damage;
        _pierce = unitInfo.Pierce;
        _attackRange = unitInfo.AttackRange;
        _accuracy = unitInfo.Accuracy;

        startPos = GridManager.instance.WorldPositionToGrid(transform.position);
        lastSpotVisited = startPos;

        rb.velocity = transform.forward * speed;
    }

    void Update()
    {
        var distance = Vector2.Distance(GridManager.instance.WorldPositionToGrid(transform.position), startPos);

        if (distance >= _attackRange)
        {
            ProjectileMiss();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.CompareTag("Wall"))
        {
            _pierce--;
            ProjectileMiss();
            return;
        }

        var coords = GridManager.instance.WorldPositionToGrid(transform.position);
        
        if (other.CompareTag("Obstacle"))
        {
            // Calculate accuracy change
            int distance = Mathf.FloorToInt(Vector2.Distance(coords, lastSpotVisited));
            _accuracy -= distance * 10;

            _pierce--;
            _accuracy -= 20;

            lastSpotVisited = coords;
            ProjectileMiss();
            return;
        }
        
        if (other.CompareTag("Character"))
        {
            // Damage Text
            var pos = transform.position + new Vector3(0, 2, 0);
            var text = Instantiate(floatingText, pos, Quaternion.identity);
            text.transform.eulerAngles = new Vector3(45, -135, 0);

            // Calculate accuracy change
            int distance = Mathf.FloorToInt(Vector2.Distance(coords, lastSpotVisited));
            _accuracy -= distance * 10;
            lastSpotVisited = coords;

            if (Random.Range(0, 100) < _accuracy)
            {
                _pierce--;

                int damage = other.transform.GetComponent<UnitInfo>().TakeDamage(_damage);
                text.GetComponent<TMP_Text>().text = damage.ToString();
                Debug.Log("Dealt " + damage + " damage to character in block (" + coords.x + "," + coords.y + ") with " + _accuracy + "% accuracy");

                ProjectileHit();
            }
            else
            {
                text.GetComponent<TMP_Text>().text = "Miss";
                Debug.Log("Attack missed to character in block (" + coords.x + "," + coords.y + ") with " + _accuracy + "% accuracy");

                ProjectileMiss();
            }
        }
    }

    private void ProjectileHit()
    {
        Instantiate(hitVFX, transform.position - (transform.forward * 0.25f), Quaternion.identity);
        if (_pierce <= 0) Destroy(gameObject);
    }

    private void ProjectileMiss()
    {
        Instantiate(missVFX, transform.position - (transform.forward * 0.25f), Quaternion.identity);
        if (_pierce <= 0) Destroy(gameObject);
    }
}
