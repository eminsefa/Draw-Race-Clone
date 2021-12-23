using UnityEngine;
using Random = UnityEngine.Random;

public class Collectable : MonoBehaviour
{
    private Rigidbody _rb;

    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider other)
    {
        var random = new Vector3(Random.Range(-2f, 2f), Random.Range(2f, 3f), Random.Range(-2f, 2f));
        _rb.AddForce(random*3f,ForceMode.Impulse);
        Destroy(gameObject,5f);
    }
}
