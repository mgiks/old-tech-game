public static class SkillPurchase
{
    private static bool busy;

    public static long NextCost()
    {
        return EconomyMath.NextPurchaseCost(GameProgress.TotalPurchases);
    }

    public static PurchaseDenial Evaluate(SkillDef skill)
    {
        return UnlockEval.Evaluate(
            skill,
            GameProgress.GetSkillLevel,
            GameProgress.TotalPurchases,
            GameProgress.MB,
            NextCost());
    }

    public static PurchaseDenial Evaluate(string skillId)
    {
        return Evaluate(SkillBalanceTable.Find(skillId));
    }

    public static bool TryBuy(string skillId)
    {
        if (busy || string.IsNullOrEmpty(skillId))
        {
            return false;
        }

        busy = true;
        try
        {
            SkillDef skill = SkillBalanceTable.Find(skillId);
            if (!Evaluate(skill).CanBuy)
            {
                return false;
            }

            int nextLevel = GameProgress.GetSkillLevel(skillId) + 1;
            long cost = NextCost();
            if (!GameProgress.TrySpendMB(cost))
            {
                return false;
            }

            if (!GameProgress.CommitSkillLevel(skillId, nextLevel))
            {
                GameProgress.RefundMB(cost);
                return false;
            }

            return GameProgress.GetSkillLevel(skillId) == nextLevel;
        }
        finally
        {
            busy = false;
        }
    }

    public static string DenialText(PurchaseDenial denial)
    {
        switch (denial.Block)
        {
            case PurchaseBlock.MissingCurrency:
                return Loc.F("deny.no_mb_cost", NumberFormat.Mb(NextCost()));
            case PurchaseBlock.RequiredSkill:
                string name = denial.SkillId != null && Loc.TryGet($"skill.{denial.SkillId}.name", out string value)
                    ? value
                    : denial.SkillId;
                return Loc.F("deny.need_skill", name, denial.RequiredLevel);
            case PurchaseBlock.TotalThreshold:
                return Loc.F("deny.need_purchases", denial.RequiredPurchases, GameProgress.TotalPurchases);
            case PurchaseBlock.MaxLevel:
                return Loc.T("deny.max");
            default:
                return Loc.T("deny.locked");
        }
    }
}
