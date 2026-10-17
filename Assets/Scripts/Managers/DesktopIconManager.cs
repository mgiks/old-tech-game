using System.Collections;
using UnityEngine;

public class DesktopIconManager : MonoBehaviour
{
    [SerializeField] private GameObject icon;
    [SerializeField] private float padding = 0.01f;
    [SerializeField] private float distanceFromCamera = 10f;
    [SerializeField] private float coolDownDurationInSeconds = 1f;
    [SerializeField] private uint spawnCountPerWave = 6;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        StartCoroutine(SpawnDesktopIconsRoutine());
    }

    public void SpawnDesktopIcon(uint count)
    {
        for (int i = 0; i < count; i++)
        {
            float randomX = Random.Range(padding, 1f - padding);
            float randomY = Random.Range(padding, 1f - padding);

            Vector3 randomPosition = new(randomX, randomY, distanceFromCamera);
            Vector3 worldRandomPosition = cam.ViewportToWorldPoint(randomPosition);

            Instantiate(icon, worldRandomPosition, Quaternion.identity);
        }
    }

    private IEnumerator SpawnDesktopIconsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(coolDownDurationInSeconds);
            SpawnDesktopIcon(spawnCountPerWave);
        }
    }
}