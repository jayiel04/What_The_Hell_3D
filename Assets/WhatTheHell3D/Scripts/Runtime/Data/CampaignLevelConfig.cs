using System;
using System.Collections.Generic;
using UnityEngine;

public enum CampaignEnemyKind
{
    Goblin,
    Zombie,
    Witch
}

public enum CampaignHazardKind
{
    Spikes,
    Saw,
    Lava
}

public enum CampaignPickupKind
{
    Coin,
    Heart,
    Key
}

[CreateAssetMenu(fileName = "CampaignLevelConfig", menuName = "WhatTheHell3D/Campaign Level Config")]
public sealed class CampaignLevelConfig : ScriptableObject
{
    [Header("Identity")]
    public int levelId = 1;
    public string title = string.Empty;
    public string objective = string.Empty;
    public string artTheme = string.Empty;

    [Header("Player")]
    public Vector3 playerStart = Vector3.zero;
    public float corridorHalfWidth = 12f;

    [Header("Environment")]
    public Color backgroundColor = Color.white;
    public Color ambientLightColor = Color.white;
    public float fogDensity;
    public float keyLightEnergy = 1f;
    public float fillLightEnergy = 0.35f;
    public Color fillLightColor = Color.white;

    [Header("Camera")]
    public float cameraDistance = 8f;
    public float cameraMaximumDistance = 10f;
    public float cameraHeight = 3f;
    public float cameraShoulderOffset;
    public float cameraLookAheadDistance = 1f;

    [Header("Layout")]
    public List<CampaignVolumePlacement> lava = new List<CampaignVolumePlacement>();
    public List<CampaignPlatformPlacement> platforms = new List<CampaignPlatformPlacement>();
    public List<CampaignForestIslandPlacement> forestIslandBases = new List<CampaignForestIslandPlacement>();
    public List<CampaignPlatformPlacement> bridges = new List<CampaignPlatformPlacement>();
    public List<CampaignMovingPlatformPlacement> movingPlatforms = new List<CampaignMovingPlatformPlacement>();
    public List<CampaignPlatformPlacement> fallingPlatforms = new List<CampaignPlatformPlacement>();
    public List<CampaignStairPlacement> stairs = new List<CampaignStairPlacement>();
    public List<CampaignHazardPlacement> hazards = new List<CampaignHazardPlacement>();
    public List<CampaignPickupLinePlacement> pickupLines = new List<CampaignPickupLinePlacement>();
    public List<CampaignPickupArcPlacement> pickupArcs = new List<CampaignPickupArcPlacement>();
    public List<CampaignBonusCachePlacement> bonusCaches = new List<CampaignBonusCachePlacement>();
    public List<CampaignPickupPlacement> pickups = new List<CampaignPickupPlacement>();
    public List<CampaignCheckpointPlacement> checkpoints = new List<CampaignCheckpointPlacement>();
    public List<CampaignEnemyPlacement> enemies = new List<CampaignEnemyPlacement>();
    public Vector3 goalPosition = Vector3.zero;
}

[Serializable]
public sealed class CampaignVolumePlacement
{
    public Vector3 position;
    public Vector3 size = Vector3.one;
}

[Serializable]
public sealed class CampaignPlatformPlacement
{
    public Vector3 position;
    public Vector3 size = Vector3.one;
    public Color color = Color.white;
}

[Serializable]
public sealed class CampaignForestIslandPlacement
{
    public Vector3 position;
    public Vector3 size = Vector3.one;
    public int variant;
}

[Serializable]
public sealed class CampaignMovingPlatformPlacement
{
    public Vector3 position;
    public Vector3 size = Vector3.one;
    public Vector3 travel;
    public float duration = 2f;
}

[Serializable]
public sealed class CampaignStairPlacement
{
    public Vector3 start;
    public int count;
    public float step;
}

[Serializable]
public sealed class CampaignHazardPlacement
{
    public Vector3 position;
    public Vector3 size = Vector3.one;
    public CampaignHazardKind kind;
}

[Serializable]
public sealed class CampaignPickupLinePlacement
{
    public Vector3 start;
    public Vector3 offset;
    public int count;
    public CampaignPickupKind kind;
}

[Serializable]
public sealed class CampaignPickupArcPlacement
{
    public Vector3 center;
    public float width;
    public int count;
    public CampaignPickupKind kind;
}

[Serializable]
public sealed class CampaignBonusCachePlacement
{
    public Vector3 position;
    public string theme = string.Empty;
}

[Serializable]
public sealed class CampaignPickupPlacement
{
    public Vector3 position;
    public CampaignPickupKind kind;
}

[Serializable]
public sealed class CampaignCheckpointPlacement
{
    public Vector3 position;
    public int index;
}

[Serializable]
public sealed class CampaignEnemyPlacement
{
    public CampaignEnemyKind kind;
    public Vector3 position;
    public float patrolDistance;
}
