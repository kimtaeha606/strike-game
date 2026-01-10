// DifficultyDirector.cs
// å��: score -> GloveSpec(���� �۷��� Ÿ��/�Ķ����) "����"�� �Ѵ�.
// ����: Instantiate, ��ġ ����(��ġ), ���� ����, ���� ����

using System;
using UnityEngine;

public enum GloveType
{
    Normal,
    Far,
    Small
}

[Serializable]
public struct GloveSpec
{
    public GloveType type;

    // ����/��ġ �ʿ��� ����� �Ķ����
    public float gapX;     // ���� �۷�������� �⺻ �Ÿ�
    public float scale;    // �۷��� ������(1=�⺻)

    // (����) �۷��� �̵� ���̵� �Ķ����
    public float moveAmpX;
    public float moveAmpY;
    public float moveSpeed;

    public GloveSpec(
        GloveType type,
        float gapX,
        float scale,
        float moveAmpX = 0f,
        float moveAmpY = 0f,
        float moveSpeed = 0f)
    {
        this.type = type;
        this.gapX = gapX;
        this.scale = scale;
        this.moveAmpX = moveAmpX;
        this.moveAmpY = moveAmpY;
        this.moveSpeed = moveSpeed;
    }
}

public sealed class DifficultyDirector : MonoBehaviour
{
    [Serializable]
    public sealed class TierRule
    {
        [Header("Score Range")]
        [Min(0)] public int minScore = 0;   // inclusive
        [Min(0)] public int maxScore = 9;   // inclusive (maxScore < 0 �̸� ����)

        [Header("Weights (Relative)")]
        [Min(0)] public int normalWeight = 100;
        [Min(0)] public int farWeight = 0;
        [Min(0)] public int smallWeight = 0;

        [Header("Type Presets")]
        public float normalGapX = 8f;
        public float normalScale = 1f;

        public float farGapX = 11f;
        public float farScale = 1f;

        public float smallGapX = 8f;
        public float smallScale = 0.8f;

        [Header("Optional Move (applied to ALL types in this tier)")]
        public float moveAmpX = 0f;
        public float moveAmpY = 0f;
        public float moveSpeed = 0f;

        public bool Contains(int score)
        {
            if (score < minScore) return false;
            if (maxScore < 0) return true; // infinite
            return score <= maxScore;
        }
    }

    [Header("Tier Rules (ordered by score)")]
    [SerializeField] private TierRule[] tiers;

    [Header("Fallback if tiers empty")]
    [SerializeField] private float fallbackGapX = 8f;
    [SerializeField] private float fallbackScale = 1f;

    /// <summary>
    /// score�� �Է����� �޾� ���� �۷��� ������ ������ ��ȯ�Ѵ�.
    /// </summary>
    public GloveSpec GetNextSpec(int score)
    {
        TierRule tier = FindTier(score);

        if (tier == null)
        {
            return new GloveSpec(GloveType.Normal, fallbackGapX, fallbackScale);
        }

        GloveType picked = PickWeightedType(
            tier.normalWeight,
            tier.farWeight,
            tier.smallWeight);

        // Ÿ�Ժ� preset ���� + (����) �̵� �Ķ���ʹ� tier �������� ����
        switch (picked)
        {
            case GloveType.Far:
                return new GloveSpec(
                    GloveType.Far,
                    tier.farGapX,
                    tier.farScale,
                    tier.moveAmpX,
                    tier.moveAmpY,
                    tier.moveSpeed);

            case GloveType.Small:
                return new GloveSpec(
                    GloveType.Small,
                    tier.smallGapX,
                    tier.smallScale,
                    tier.moveAmpX,
                    tier.moveAmpY,
                    tier.moveSpeed);

            default:
                return new GloveSpec(
                    GloveType.Normal,
                    tier.normalGapX,
                    tier.normalScale,
                    tier.moveAmpX,
                    tier.moveAmpY,
                    tier.moveSpeed);
        }
    }

    private TierRule FindTier(int score)
    {
        if (tiers == null || tiers.Length == 0) return null;

        for (int i = 0; i < tiers.Length; i++)
        {
            if (tiers[i] != null && tiers[i].Contains(score))
                return tiers[i];
        }

        // ��Ī ������ ������ Ƽ� fallback���� ���(���ϸ� null ��ȯ���� �ٲ㵵 ��)
        return tiers[tiers.Length - 1];
    }

    private static GloveType PickWeightedType(int normalW, int farW, int smallW)
    {
        int total = Mathf.Max(0, normalW) + Mathf.Max(0, farW) + Mathf.Max(0, smallW);
        if (total <= 0)
            return GloveType.Normal;

        int roll = UnityEngine.Random.Range(0, total); // 0..total-1
        int acc = 0;

        acc += Mathf.Max(0, normalW);
        if (roll < acc) return GloveType.Normal;

        acc += Mathf.Max(0, farW);
        if (roll < acc) return GloveType.Far;

        return GloveType.Small;
    }
}
