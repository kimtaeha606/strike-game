// DifficultyDirector.cs
// 책임: score -> GloveSpec(다음 글러브 타입/파라미터) "결정"만 한다.
// 금지: Instantiate, 위치 랜덤(배치), 점수 변경, 게임 진행

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

    // 스폰/배치 쪽에서 사용할 파라미터
    public float gapX;     // 다음 글러브까지의 기본 거리
    public float scale;    // 글러브 스케일(1=기본)

    // (선택) 글러브 이동 난이도 파라미터
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
        [Min(0)] public int maxScore = 9;   // inclusive (maxScore < 0 이면 무한)

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
    /// score를 입력으로 받아 다음 글러브 스펙을 결정해 반환한다.
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

        // 타입별 preset 적용 + (선택) 이동 파라미터는 tier 공통으로 얹음
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

        // 매칭 없으면 마지막 티어를 fallback으로 사용(원하면 null 반환으로 바꿔도 됨)
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
