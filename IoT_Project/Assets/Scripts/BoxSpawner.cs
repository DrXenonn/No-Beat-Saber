using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] Boxes;
    [SerializeField] private Transform[] SpawnPoints;
    [SerializeField] private float SpawnInterval = 1.5f;

    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= SpawnInterval)
        {
            SpawnObstacle();
            _timer = 0f;
        }
    }

    private void SpawnObstacle()
    {
        var point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        var randomBox = Boxes[Random.Range(0, Boxes.Length)];
        var box = Instantiate(randomBox);
        box.transform.position = point.position;
        box.transform.rotation = point.rotation;
    }
}