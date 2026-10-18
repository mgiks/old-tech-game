using System.Collections;
using UnityEngine;

public class DesktopIconManager : MonoBehaviour
{
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject virus;
    [SerializeField] private float padding = 0.01f;
    [SerializeField] private float coolDownDurationInSeconds = 1f;
    [SerializeField] private uint spawnCountPerWave = 6;

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
            BoxCollider2D virusBoxCollider = virus.GetComponent<BoxCollider2D>();
            Vector2 virusCenter = virusBoxCollider.transform.TransformPoint(virusBoxCollider.offset);

            float virusWidth = virusBoxCollider.size.x;
            float virusHeight = virusBoxCollider.size.y;

            float virusXLeft = virusCenter.x - virusWidth;
            float virusXRight = virusCenter.x + virusWidth;
            float virusYBottom = virusCenter.y - virusHeight;
            float virusYTop = virusCenter.y + virusHeight;

            Vector2 virusXLeftCoor = new(virusXLeft, virusCenter.y);
            Vector2 virusXRightCoor = new(virusXRight, virusCenter.y);
            Vector2 virusYTopCoor = new(virusCenter.x, virusYTop);
            Vector2 virusYBottomCoor = new(virusCenter.x, virusYBottom);

            virusXLeftCoor = cam.WorldToViewportPoint(virusXLeftCoor);
            virusXRightCoor = cam.WorldToViewportPoint(virusXRightCoor);
            virusYTopCoor = cam.WorldToViewportPoint(virusYTopCoor);
            virusYBottomCoor = cam.WorldToViewportPoint(virusYBottomCoor);

            float randomX = RandomRange(padding, 1f - padding, virusXLeftCoor.x, virusXRightCoor.x);
            float randomY = RandomRange(padding, 1f - padding, virusYBottomCoor.y, virusYTopCoor.y);

            Vector3 randomPosition = new(randomX, randomY, distanceFromCamera);
            Vector3 worldRandomPosition = cam.ViewportToWorldPoint(randomPosition);

            Instantiate(icon, worldRandomPosition, Quaternion.identity);
        }
    }

    private float RandomRange(float rangeToIncludeMin, float rangeToIncludeMax, float rangeNotToIncludeMin, float rangeNotToIncludeMax)
    {
        float randLeftSideVal = Random.Range(rangeToIncludeMin, rangeNotToIncludeMin);
        float randRightSideVal = Random.Range(rangeNotToIncludeMax, rangeToIncludeMax);

        float randVal = Random.Range(randLeftSideVal, randRightSideVal);
        return randVal;
    }
}