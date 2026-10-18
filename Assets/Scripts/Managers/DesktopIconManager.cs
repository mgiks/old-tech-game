using System.Collections;
using UnityEngine;

public class DesktopIconManager : MonoBehaviour
{
    [SerializeField] private GameObject icon;
    [SerializeField] private Virus virus;
    [SerializeField] private float padding = 0.01f;
    [SerializeField] private float coolDownDurationInSeconds = 1f;
    [SerializeField] private uint spawnCountPerWave = 6;
    [SerializeField] private float haloDiameterInVirusHeights = 2f;
    [SerializeField] private int maxSpawnAttempts = 30;

    private Camera cam;
    private readonly float distanceFromCamera = 10f;
    private bool spawnedFirstWave = false;

    void Start()
    {
        cam = Camera.main;
        StartCoroutine(SpawnDesktopIconsRoutine());
    }

    private IEnumerator SpawnDesktopIconsRoutine()
    {
        while (true)
        {
            if (spawnedFirstWave)
            {
                yield return new WaitForSeconds(coolDownDurationInSeconds);
            }
            else
            {
                spawnedFirstWave = true;
            }

            SpawnDesktopIcons(spawnCountPerWave);
        }
    }

    [ContextMenu("Spawn 6 Desktop Icons")]
    public void Spawn6DesktopIcons()
    {
        SpawnDesktopIcons(6);
    }

    public void SpawnDesktopIcons(uint count)
    {
        for (int i = 0; i < count; i++)
        {
            if (TryGetPositionOutsideHalo(out Vector2 coords))
            {
                Instantiate(icon, new Vector3(coords.x, coords.y, 0f), Quaternion.identity);
            }
        }
    }

    private bool TryGetPositionOutsideHalo(out Vector2 position)
    {
        Bounds virusBounds = virus.GetVisualBounds();
        Vector2 haloCenter = virusBounds.center;
        float haloRadius = virusBounds.size.y * haloDiameterInVirusHeights * 0.5f;

        Vector2 iconHalfExtents = GetIconHalfExtents();
        float minDistance = haloRadius + iconHalfExtents.magnitude;

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, distanceFromCamera));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, distanceFromCamera));
        float minX = bottomLeft.x + iconHalfExtents.x + padding;
        float maxX = topRight.x - iconHalfExtents.x - padding;
        float minY = bottomLeft.y + iconHalfExtents.y + padding;
        float maxY = topRight.y - iconHalfExtents.y - padding;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector2 candidate = new(Random.Range(minX, maxX), Random.Range(minY, maxY));
            if ((candidate - haloCenter).sqrMagnitude >= minDistance * minDistance)
            {
                position = candidate;
                return true;
            }
        }

        position = default;
        return false;
    }

    private Vector2 GetIconHalfExtents()
    {
        Vector3 size = icon.GetComponent<SpriteRenderer>().sprite.bounds.size;
        Vector3 scale = icon.transform.localScale;
        return new Vector2(size.x * Mathf.Abs(scale.x) * 0.5f, size.y * Mathf.Abs(scale.y) * 0.5f);
    }
}