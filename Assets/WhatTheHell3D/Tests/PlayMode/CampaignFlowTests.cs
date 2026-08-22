using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WhatTheHell3D.Tests
{
    public sealed class CampaignFlowTests
    {
        private const string Level01 = "Assets/WhatTheHell3D/Scenes/CampaignLevel01.unity";

        [UnityTest]
        public IEnumerator Level01_CargaConJugadorCamaraHudYAudio()
        {
            yield return LoadScene(Level01);

            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            CameraController camera = Object.FindFirstObjectByType<CameraController>();
            CampaignHudController hud = Object.FindFirstObjectByType<CampaignHudController>();
            PauseController pause = Object.FindFirstObjectByType<PauseController>();
            CampaignAudioDirector audio = Object.FindFirstObjectByType<CampaignAudioDirector>();
            CampaignLevelRuntime runtime = Object.FindFirstObjectByType<CampaignLevelRuntime>();

            Assert.IsNotNull(player, "El nivel debe contener un jugador.");
            Assert.IsNotNull(camera, "El nivel debe contener una cámara de aventura.");
            Assert.IsNotNull(hud, "El nivel debe contener un HUD.");
            Assert.IsNotNull(pause, "El nivel debe contener pausa.");
            Assert.IsNotNull(audio, "El nivel debe contener director de audio.");
            Assert.IsNotNull(runtime, "El nivel debe contener el coordinador de campaña.");

            // El runtime configura todo en Start; esperar un frame adicional.
            yield return null;
            Assert.IsTrue(player.Health != null && player.Health.IsAlive, "El jugador debe estar vivo tras iniciar.");
            Assert.IsNotNull(runtime.PlayerTransform, "La referencia del jugador debe estar cableada en el runtime.");
            Assert.IsNotNull(hud.healthFill, "El HUD debe tener barra de salud UGUI.");
            Assert.IsNotNull(Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>(),
                "Debe existir un EventSystem para la UI.");
        }

        [UnityTest]
        public IEnumerator Level01_JugadorMuestraMallaDelCaballero()
        {
            yield return LoadScene(Level01);

            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            Assert.IsNotNull(player, "El nivel debe contener un jugador.");

            // Esperar a que el modelo glTF se cargue de forma asíncrona (6 modelos concurrentes pueden tardar más en headless).
            SkinnedMeshRenderer skin = null;
            for (int i = 0; i < 300; i++)
            {
                Transform model = player.transform.Find("KnightModel");
                if (model != null)
                {
                    skin = model.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (skin != null) break;
                }
                yield return null;
            }

            Assert.IsNotNull(skin,
                "El jugador debe mostrar la malla del caballero (Knight_Male.gltf) cargada desde StreamingAssets.");
            Assert.IsTrue(skin.sharedMesh != null && skin.sharedMesh.vertexCount > 0,
                "La malla del caballero debe tener vértices.");
        }

        [UnityTest]
        public IEnumerator Enemigos_MuestranMallaSegunKind()
        {
            yield return LoadScene(Level01);
            EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            Assert.IsTrue(enemies.Length > 0, "El nivel debe contener al menos un enemigo para verificar su malla.");

            foreach (EnemyController enemy in enemies)
            {
                SkinnedMeshRenderer skin = null;
                for (int i = 0; i < 300; i++)
                {
                    Transform model = enemy.transform.Find($"{enemy.kind}Model");
                    if (model != null)
                    {
                        skin = model.GetComponentInChildren<SkinnedMeshRenderer>();
                        if (skin != null) break;
                    }

                    // También buscar cualquier KnightModel-like si se usó nombre genérico.
                    if (skin == null)
                    {
                        skin = enemy.GetComponentInChildren<SkinnedMeshRenderer>();
                        if (skin != null && skin.transform != enemy.transform.Find("EnemyVisual")) break;
                        skin = null;
                    }

                    yield return null;
                }

                Assert.IsNotNull(skin,
                    $"El enemigo {enemy.kind} en {enemy.transform.position} debe mostrar su malla glTF ({enemy.kind}).");
                Assert.IsTrue(skin.sharedMesh != null && skin.sharedMesh.vertexCount > 0,
                    $"La malla de {enemy.kind} debe tener vértices.");
                Assert.IsFalse(skin.gameObject.name == "EnemyVisual",
                    "La malla validada debe ser la del modelo glTF, no la cápsula de respaldo.");
            }
        }

        [UnityTest]
        public IEnumerator Jugador_RecibeDanyoYMuereYReaparece()
        {
            yield return LoadScene(Level01);
            yield return null;

            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            HealthComponent health = player.Health;
            int before = health.CurrentHealth;
            health.TakeDamage(new DamageInfo(20, player.transform.position + Vector3.forward, null));
            Assert.Less(health.CurrentHealth, before, "El daño debe reducir la salud del jugador.");

            health.TakeDamage(new DamageInfo(health.CurrentHealth + 10, Vector3.zero, null));
            Assert.IsFalse(health.IsAlive, "El jugador debe morir con daño letal.");

            // Esperar la reaparición (1.1 s + margen).
            float timeout = Time.realtimeSinceStartup + 3f;
            while (!health.IsAlive && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.IsTrue(health.IsAlive, "El jugador debe reaparecer automáticamente.");
        }

        [UnityTest]
        public IEnumerator Enemigo_MuereAlRecibirDanyoLetal()
        {
            yield return LoadScene(Level01);
            yield return null;

            EnemyController enemy = Object.FindFirstObjectByType<EnemyController>();
            if (enemy == null)
            {
                Assert.Ignore("El nivel no contiene enemigos.");
                yield break;
            }

            HealthComponent health = enemy.Health;
            Assert.IsTrue(health.IsAlive);
            health.TakeDamage(new DamageInfo(99999, enemy.transform.position + Vector3.back, null));
            Assert.IsFalse(health.IsAlive, "El enemigo debe morir con daño letal.");
        }

        [UnityTest]
        public IEnumerator Bruja_LanzaProyectilDuranteAtaque()
        {
            yield return LoadScene("Assets/WhatTheHell3D/Scenes/CampaignLevel02.unity");
            yield return null;

            EnemyController witch = null;
            foreach (EnemyController enemy in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
            {
                if (enemy.kind == CampaignEnemyKind.Witch)
                {
                    witch = enemy;
                    break;
                }
            }

            if (witch == null)
            {
                Assert.Ignore("El nivel no contiene brujas.");
                yield break;
            }

            // Forzar estado de ataque inmediato y proyectil visible.
            witch.windUpTime = 0.05f;
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            float detectionBefore = witch.detectionRange;
            witch.detectionRange = 1000f;
            float distance = Vector3.Distance(witch.transform.position, playerObject.transform.position);
            witch.attackRange = Mathf.Max(witch.attackRange, distance + 1f);

            float timeout = Time.time + 3f;
            bool projectileSeen = false;
            while (Time.time < timeout && !projectileSeen)
            {
                foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                {
                    if (go.CompareTag("Projectile") && go.scene.IsValid())
                    {
                        projectileSeen = true;
                        break;
                    }
                }

                yield return null;
            }

            witch.detectionRange = detectionBefore;
            Assert.IsTrue(projectileSeen, "La bruja debería generar un proyectil al atacar.");
        }

        [UnityTest]
        public IEnumerator Pickup_IncrementaProgresoYCambiaEstado()
        {
            yield return LoadScene(Level01);
            yield return null;

            CampaignRuntimeState state = CampaignRuntimeState.Ensure(null);
            state.BeginLevel(LoadConfig(1));
            int collectedBefore = state.Progress.collected;

            PickupRuntime pickup = null;
            foreach (PickupRuntime candidate in Object.FindObjectsByType<PickupRuntime>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (candidate.Kind == CampaignPickupKind.Coin)
                {
                    pickup = candidate;
                    break;
                }
            }

            if (pickup == null)
            {
                Assert.Ignore("El nivel no contiene monedas.");
                yield break;
            }

            pickup.Interact(GameObject.FindGameObjectWithTag("Player"));
            Assert.IsTrue(pickup.IsCollected, "El pickup debe marcarse como recogido.");
            Assert.Greater(state.Progress.collected, collectedBefore, "El progreso debe incrementar al recoger monedas.");
        }

        [UnityTest]
        public IEnumerator Checkpoint_ActualizaPosicionDeReaparicion()
        {
            yield return LoadScene(Level01);
            yield return null;

            CampaignRuntimeState state = CampaignRuntimeState.Ensure(null);
            state.BeginLevel(LoadConfig(1));
            state.Progress.checkpointIndex = -1;

            CheckpointRuntime checkpoint = Object.FindFirstObjectByType<CheckpointRuntime>();
            if (checkpoint == null)
            {
                Assert.Ignore("El nivel no contiene checkpoints.");
                yield break;
            }

            // Teletransportar al jugador dentro del trigger del checkpoint.
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = checkpoint.respawnPosition + Vector3.up * 0.4f;
            CharacterController controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.position = checkpoint.respawnPosition + Vector3.up * 0.4f;
            controller.enabled = true;

            float timeout = Time.time + 2f;
            while (Time.time < timeout && state.Progress.checkpointIndex < checkpoint.index)
            {
                yield return new WaitForSeconds(0.1f);
                player.transform.position = checkpoint.respawnPosition + Vector3.up * 0.4f;
            }

            Assert.AreEqual(checkpoint.index, state.Progress.checkpointIndex, "El checkpoint debe registrarse en el progreso.");
            Assert.AreEqual(checkpoint.respawnPosition, state.GetRespawnPosition(Vector3.zero), "La posición de reaparición debe coincidir con el checkpoint.");
        }

        [UnityTest]
        public IEnumerator Meta_SinLlaveNoCompleta_ConLlaveAvanza()
        {
            yield return LoadScene(Level01);
            yield return null;

            CampaignRuntimeState state = CampaignRuntimeState.Ensure(null);
            CampaignLevelConfig config = LoadConfig(1);
            state.BeginLevel(config);

            GoalRuntime goal = Object.FindFirstObjectByType<GoalRuntime>();
            Assert.IsNotNull(goal, "El nivel debe contener una meta.");

            state.Progress.keyCollected = false;
            Assert.IsFalse(goal.CanFinish, "Sin llave la meta no debe permitir completar el nivel.");

            state.Progress.keyCollected = true;
            Assert.IsTrue(goal.CanFinish, "Con llave la meta debe permitir completar el nivel.");
        }

        [UnityTest]
        public IEnumerator Guardado_JsonSobreviveCicloCompleto()
        {
            JsonCampaignProgressStore store = new JsonCampaignProgressStore();
            store.Delete();

            CampaignProgressData original = CampaignProgressData.CreateNew(Level01);
            original.currentLevelId = 2;
            original.collected = 7;
            original.keyCollected = true;
            original.checkpointIndex = 3;
            original.checkpointPosition = new Vector3(11f, 2f, -4f);
            store.Save(original);

            CampaignProgressData loaded = store.Load();
            Assert.IsNotNull(loaded, "El guardado debe poder recargarse.");
            Assert.AreEqual(original.currentLevelId, loaded.currentLevelId);
            Assert.AreEqual(original.collected, loaded.collected);
            Assert.AreEqual(original.keyCollected, loaded.keyCollected);
            Assert.AreEqual(original.checkpointIndex, loaded.checkpointIndex);
            Assert.AreEqual(original.checkpointPosition, loaded.checkpointPosition);

            store.Delete();
            Assert.IsNull(store.Load(), "Tras borrar no debe haber guardado.");
            yield break;
        }


        [UnityTest]
        public IEnumerator Niveles_02Y03_CarganConContenidoCompleto()
        {
            foreach ((string scenePath, string configPath) in new[]
            {
                ("Assets/WhatTheHell3D/Scenes/CampaignLevel02.unity", "Assets/WhatTheHell3D/Data/CampaignLevel02_Mines.asset"),
                ("Assets/WhatTheHell3D/Scenes/CampaignLevel03.unity", "Assets/WhatTheHell3D/Data/CampaignLevel03_Castle.asset")
            })
            {
                yield return LoadScene(scenePath);
                yield return null;

#if UNITY_EDITOR
                CampaignLevelConfig config = UnityEditor.AssetDatabase.LoadAssetAtPath<CampaignLevelConfig>(configPath);
#else
                CampaignLevelConfig config = Resources.Load<CampaignLevelConfig>(configPath);
#endif
                Assert.IsNotNull(config, $"Config ausente para {scenePath}.");

                PlayerController player = Object.FindFirstObjectByType<PlayerController>();
                EnemyController enemy = Object.FindFirstObjectByType<EnemyController>();
                PickupRuntime pickup = Object.FindFirstObjectByType<PickupRuntime>();
                CheckpointRuntime checkpoint = Object.FindFirstObjectByType<CheckpointRuntime>();
                GoalRuntime goal = Object.FindFirstObjectByType<GoalRuntime>();

                Assert.IsNotNull(player, $"{scenePath} debe tener jugador.");
                Assert.IsNotNull(enemy, $"{scenePath} debe tener enemigos.");
                Assert.IsNotNull(pickup, $"{scenePath} debe tener pickups.");
                Assert.IsNotNull(checkpoint, $"{scenePath} debe tener checkpoints.");
                Assert.IsNotNull(goal, $"{scenePath} debe tener meta.");
                Assert.IsTrue(player.Health.IsAlive, $"{scenePath}: el jugador debe iniciar vivo.");
            }
        }

        private static IEnumerator LoadScene(string path)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(path);
            while (!operation.isDone)
            {
                yield return null;
            }

            // Dos frames para dejar ejecutar Start/Awake de todos los controladores.
            yield return null;
            yield return null;
        }

        private static CampaignLevelConfig LoadConfig(int levelId)
        {
            string path = levelId switch
            {
                2 => "Assets/WhatTheHell3D/Data/CampaignLevel02_Mines.asset",
                3 => "Assets/WhatTheHell3D/Data/CampaignLevel03_Castle.asset",
                _ => "Assets/WhatTheHell3D/Data/CampaignLevel01_Forest.asset"
            };
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<CampaignLevelConfig>(path);
#else
            return Resources.Load<CampaignLevelConfig>(path);
#endif
        }
    }
}
