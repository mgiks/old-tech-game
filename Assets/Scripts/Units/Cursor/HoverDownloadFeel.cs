using UnityEngine;

[CreateAssetMenu(menuName = "Old Tech/Hover Download Feel", fileName = "Hover Download Feel")]
public class HoverDownloadFeel : ScriptableObject
{
    public const string MetricName = "DownloadCharge";

    [TextArea(4, 8)]
    [SerializeField] private string metric =
        "DownloadCharge — число от 0 до 1.\n" +
        "0 = курсор только что встал на файл.\n" +
        "1 = секунды наведения достигли downloadSeconds, файл улетает к вирусу.\n" +
        "Покачивание растёт линейно от charge. Тряска = shakeDegreesAtOne * charge^shakeCurve.\n" +
        "Ближе к 1 тряска чуть слабеет: от shakeNearDoneStart до 1 множитель идёт к shakeNearDoneScale.\n" +
        "DownloadProgress живёт на каждом файле и не сбрасывается, когда курсор уходит.";

    [Header("DownloadCharge")]
    [Tooltip("Секунды наведения, за которые DownloadCharge проходит путь 0 → 1.")]
    [Min(0.05f)] public float downloadSeconds = 3f;

    [Header("Покачивание — угол Z, градусы")]
    [Tooltip("Амплитуда лёгкого покачивания при DownloadCharge = 0.")]
    public float swayDegreesAtZero = 2.5f;
    [Tooltip("Амплитуда покачивания при DownloadCharge = 1. Между 0 и 1 — линейно.")]
    public float swayDegreesAtOne = 8f;
    [Tooltip("Сколько полных покачиваний влево-вправо в секунду.")]
    public float swayHertz = 1.8f;

    [Header("Тряска — растёт как charge ^ shakeCurve")]
    [Tooltip("Амплитуда тряски в градусах при DownloadCharge = 1. При 0 тряски нет.")]
    public float shakeDegreesAtOne = 11f;
    [Tooltip("Частота тряски, качаний в секунду.")]
    public float shakeHertz = 14f;
    [Tooltip("Степень. 1 = тряска растёт ровно с charge. 2 и выше = тихо в начале, сильно у финиша.")]
    [Min(0.1f)] public float shakeCurve = 2.2f;
    [Tooltip("Сдвиг влево-вправо в мировых единицах при DownloadCharge = 1. Тоже умножается на charge^shakeCurve.")]
    public float slideAtOne = 0.035f;
    [Tooltip("С какого DownloadCharge тряска начинает слабеть.")]
    [Range(0.5f, 0.98f)] public float shakeNearDoneStart = 0.78f;
    [Tooltip("Множитель тряски при DownloadCharge = 1. 1 = как раньше, 0.72 = чуть слабее у финиша.")]
    [Range(0.3f, 1f)] public float shakeNearDoneScale = 0.72f;

    [Header("Полёт к вирусу")]
    [Tooltip("Сколько секунд файл летит от стола до вируса.")]
    [Min(0.05f)] public float flySeconds = 0.42f;
    [Tooltip("Боковой выгиб траектории в мировых единицах. 0 = прямая.")]
    public float flyArc = 0.18f;
    [Tooltip("Короткая вспышка на 100%, потом файл летит к вирусу.")]
    [Min(0.05f)] public float flashSeconds = 0.18f;
    [Tooltip("Тёплый цвет заливки. Альфа — сила подсветки.")]
    public Color downloadHighlight = new(1f, 0.7f, 0.22f, 0.85f);

    [Header("Busy-курсор")]
    [Tooltip("Кадры Windows Busy (frame_001 … frame_018). Пусто — подгрузка из Assets/Art/Cursors/busy_cusror.")]
    public Sprite[] busyFrames;
    [Tooltip("Длительность одного кадра. 0.033333 ≈ 30 FPS из animation.json.")]
    [Min(0.001f)] public float busyFrameSeconds = 0.033333f;
    [Tooltip("Сдвиг кружка от острия курсора в пикселях экрана монитора. X вправо, Y вверх.")]
    public Vector2 busyOffsetPixels = new(18f, 12f);
    [Tooltip("Диаметр кружка в пикселях экрана монитора.")]
    [Min(4f)] public float busyDiameterPixels = 28f;

    public struct Pose
    {
        public float charge;
        public float swayDegrees;
        public float shakeDegrees;
        public float slideUnits;
        public float angleDegrees;
    }

    public Pose Evaluate(float charge, float time)
    {
        charge = Mathf.Clamp01(charge);
        float shakeWeight = Mathf.Pow(charge, shakeCurve);
        float late = Mathf.SmoothStep(shakeNearDoneStart, 1f, charge);
        shakeWeight *= Mathf.Lerp(1f, shakeNearDoneScale, late);
        float swayAmp = Mathf.Lerp(swayDegreesAtZero, swayDegreesAtOne, charge);
        float sway = Mathf.Sin(time * swayHertz * Mathf.PI * 2f) * swayAmp;
        float shake = Mathf.Sin(time * shakeHertz * Mathf.PI * 2f) * shakeDegreesAtOne * shakeWeight;
        float slide = Mathf.Sin(time * shakeHertz * Mathf.PI * 2.6f) * slideAtOne * shakeWeight;

        return new Pose
        {
            charge = charge,
            swayDegrees = sway,
            shakeDegrees = shake,
            slideUnits = slide,
            angleDegrees = sway + shake
        };
    }
}
