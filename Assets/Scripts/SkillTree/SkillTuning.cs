public static class SkillTuning
{
    public const int LevelsPerBranch = 3;

    public const float ConfidentialChancePerLevel = 0.012f;
    public const int ConfidentialMultiplier = 25;

    public const float InstinctDistanceWeight = 0.35f;

    public const float MagneticBaseRadius = 0.7f;
    public const float MagneticRadiusPerLevel = 0.4f;
    public const float MagneticPullSpeed = 4.2f;

    public const int CursorHoldBase = 6;
    public const int WideGripPerLevel = 2;

    public const float VacuumRadius = 3.2f;
    public const float VacuumPullSpeed = 6f;
    public const float VacuumStillDelay = 0.3f;
    public const float VacuumStillSlack = 0.15f;

    public const int OvertimeMbStep = 100;
    public const float OvertimeSecondsPerLevel = 0.2f;

    public const int SabotageEveryPcs = 3;
    public const float SabotageSecondsPerLevel = 2f;

    public static float MagneticRadius(float bonus) => MagneticBaseRadius + bonus;

    public static int CursorHold(float bonus) => CursorHoldBase + (int)bonus;
}
