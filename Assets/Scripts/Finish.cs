using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    private bool shake;
    private float _timer;
    [SerializeField] private GameObject collectablePrefab;
    private void Update()
    {
        if(!shake) return;
        _timer += Time.deltaTime;
        transform.rotation = Quaternion.AngleAxis(Random.Range(1, 10f), Vector3.up) * transform.rotation;
        if (_timer >= 2f)
        {
            shake = false;
            StartCoroutine(SpawnCollectables());
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.TryGetComponent(out PlayerController player))
        {
            shake = true;
            player.LevelCompleted();
        }
    }

    private IEnumerator SpawnCollectables()
    {
        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForSeconds(0.01f);
            Instantiate(collectablePrefab, transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
