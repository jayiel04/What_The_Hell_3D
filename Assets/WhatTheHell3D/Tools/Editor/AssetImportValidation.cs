using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Validación de la Fase 2: comprueba que los modelos GLTF/GLB/FBX se importaron,
/// que los personajes tienen mallas y que las animaciones del caballero existen.
/// </summary>
public static class AssetImportValidation
{
    private const string ArtRoot = "Assets/WhatTheHell3D/Art/Source";

    [MenuItem("WhatTheHell3D/Validación/Validar importación de assets")]
    public static void ValidateFromMenu()
    {
        int failures = ValidateAll();
        Debug.Log($"[AssetValidation] Validación terminada con {failures} fallos.");
    }

    /// <summary>Devuelve el número de fallos.</summary>
    public static int ValidateAll()
    {
        int failures = 0;
        string[] extensions = { ".gltf", ".glb", ".fbx" };
        foreach (string extension in extensions)
        {
            foreach (string path in Directory.GetFiles(ArtRoot, "*" + extension, SearchOption.AllDirectories))
            {
                string assetPath = path.Replace('\\', '/');
                Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (asset == null)
                {
                    Debug.LogError($"[AssetValidation] No importado: {assetPath}");
                    failures++;
                }
            }
        }

        failures += ValidateCharacter("characters/player/Knight_Male.gltf");
        failures += ValidateCharacter("characters/enemies/goblin/Goblin_Male.gltf");
        failures += ValidateCharacter("characters/enemies/goblin/Goblin_Female.gltf");
        failures += ValidateCharacter("characters/enemies/zombie/Zombie_Male.gltf");
        failures += ValidateCharacter("characters/enemies/zombie/Zombie_Female.gltf");
        failures += ValidateCharacter("characters/enemies/witch/Witch.gltf");

        string[] animationFiles =
        {
            "animations/player/Dying.fbx",
            "animations/player/Sprinting_Forward_Roll.fbx",
            "animations/player/Stable_Sword_Inward_Slash.fbx",
            "animations/player/Standing_Melee_Attack_Horizontal.fbx",
            "animations/player/retargeted/Knight_Dying.glb",
            "animations/player/retargeted/Knight_Sprinting_Forward_Roll.glb",
            "animations/player/retargeted/Knight_Stable_Sword_Inward_Slash.glb",
            "animations/player/retargeted/Knight_Standing_Melee_Attack_Horizontal.glb"
        };

        foreach (string relative in animationFiles)
        {
            string assetPath = $"{ArtRoot}/{relative}";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            int clips = 0;
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip && !asset.name.StartsWith("__preview__"))
                {
                    clips++;
                }
            }

            if (!File.Exists(assetPath.Replace('/', Path.DirectorySeparatorChar)))
            {
                Debug.LogError($"[AssetValidation] Falta animación: {assetPath}");
                failures++;
            }
            else if (clips == 0)
            {
                Debug.LogWarning($"[AssetValidation] Sin clips detectados en {assetPath}.");
            }
            else
            {
                Debug.Log($"[AssetValidation] {relative}: {clips} clip(s).");
            }
        }

        if (failures == 0)
        {
            Debug.Log("[AssetValidation] Todos los modelos y animaciones se importaron correctamente.");
        }

        return failures;
    }

    private static int ValidateCharacter(string relative)
    {
        string assetPath = $"{ArtRoot}/{relative}";
        Object main = AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (main == null)
        {
            Debug.LogError($"[AssetValidation] Personaje no importado: {assetPath}");
            return 1;
        }

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        int meshes = 0;
        int bones = 0;
        foreach (Object asset in assets)
        {
            if (asset is Mesh)
            {
                meshes++;
            }
            else if (asset is GameObject go && go.GetComponentInChildren<SkinnedMeshRenderer>() != null)
            {
                bones++;
            }
        }

        Debug.Log($"[AssetValidation] {relative}: meshes={meshes}, rigs={bones}");
        return meshes == 0 ? 1 : 0;
    }
}
