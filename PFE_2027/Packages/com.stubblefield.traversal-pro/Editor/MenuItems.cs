using Unity.Cinemachine;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace TraversalPro.Editor
{
    internal static class MenuItems
    {
        const string packagePath = "Packages/com.stubblefield.traversal-pro";
        
        [MenuItem("GameObject/Traversal Pro/First Person Player")]
        public static void CreateFirstPersonPlayer()
        {
            SetupCamera();
            GameObject parent = Selection.activeGameObject;
            GameObject instance = InstantiatePrefab("Prefabs/First Person Player", parent);
            Selection.activeObject = instance;
        }
        
        [MenuItem("GameObject/Traversal Pro/Third Person Player")]
        public static void CreateThirdPersonPlayer()
        {
            SetupCamera();
            GameObject parent = Selection.activeGameObject;
            GameObject instance = InstantiatePrefab("Prefabs/Third Person Player", parent);
            Material[] materials = 
            {
                LoadMaterial($"Art/UnityRobot/Materials/UnityRobotBody{CurrentRenderPipeline()}"),
                LoadMaterial($"Art/UnityRobot/Materials/UnityRobotArms{CurrentRenderPipeline()}"),
                LoadMaterial($"Art/UnityRobot/Materials/UnityRobotLegs{CurrentRenderPipeline()}"),
            };
            ApplyMaterials(instance, materials);
            Selection.activeObject = instance;
        }
        
        [MenuItem("GameObject/Traversal Pro/Character")]
        public static void CreateCharacter()
        {
            GameObject parent = Selection.activeGameObject;
            GameObject instance = InstantiatePrefab("Prefabs/Character", parent);
            Material[] materials = 
            {
                LoadMaterial($"Art/UnityRobot/Materials/UnityRobotBody{CurrentRenderPipeline()}_blue"),
                LoadMaterial($"Art/UnityRobot/Materials/UnityRobotArms{CurrentRenderPipeline()}_blue"),
                LoadMaterial($"Art/UnityRobot/Materials/UnityRobotLegs{CurrentRenderPipeline()}_blue"),
            };
            ApplyMaterials(instance, materials);
            Selection.activeObject = instance;
        }

        static void SetupCamera()
        {
            Camera camera = Camera.main;
            if (!camera)
            {
                camera = Object.FindFirstObjectByType<Camera>();
            }
            if (camera)
            {
                CinemachineBrain brain = camera.GetComponent<CinemachineBrain>();
                if (!brain)
                {
                    brain = camera.gameObject.AddComponent<CinemachineBrain>();
                }
            }
            else
            {
                Debug.LogWarning($"No camera was found in the scene.");
            }
        }

        static GameObject InstantiatePrefab(string localPathWithoutExtension, GameObject parent)
        {
            GameObject prefab = LoadPrefab(localPathWithoutExtension);
            GameObject instance = Object.Instantiate(prefab);
            instance.name = prefab.name;
            if (!parent) instance.transform.position = GetSpawnPoint();
            GameObjectUtility.SetParentAndAlign(instance, parent);
            Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
            return instance;
        }

        static Vector3 GetSpawnPoint()
        {
            return SceneView.lastActiveSceneView ? SceneView.lastActiveSceneView.pivot : default;
        }

        static GameObject LoadPrefab(string localPathWithoutExtension)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{packagePath}/{localPathWithoutExtension}.prefab");
        }
        
        static Material LoadMaterial(string localPathWithoutExtension)
        {
            return AssetDatabase.LoadAssetAtPath<Material>($"{packagePath}/{localPathWithoutExtension}.mat");
        }
        
        static void ApplyMaterials(GameObject root, Material[] materials)
        {
            Renderer rend = root.GetComponentInChildren<Renderer>();
            rend.sharedMaterials = materials;
        }
        
        static RenderPipeline CurrentRenderPipeline()
        {
            if (GraphicsSettings.currentRenderPipeline 
                && GraphicsSettings.currentRenderPipeline.defaultShader.name.Contains("HDRP"))
            {
                return RenderPipeline.HDRP;
            }
            if (GraphicsSettings.currentRenderPipeline
                && GraphicsSettings.currentRenderPipeline.defaultShader.name.Contains("Universal"))
            {
                return RenderPipeline.URP;
            }
            return RenderPipeline.BiRP;
        }

        enum RenderPipeline
        {
            BiRP = 0,
            HDRP = 1,
            URP = 2,
        }
    }
}