// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using Synty.SidekickCharacters.Blendshapes;
using Synty.SidekickCharacters.Database;
using Synty.SidekickCharacters.Database.DTO;
using Synty.SidekickCharacters.Enums;
using Synty.SidekickCharacters.SkinnedMesh;
using Synty.SidekickCharacters.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Synty.SidekickCharacters.API
{
    public class SidekickRuntime
    {
        private const string _BLEND_GENDER_NAME = "masculineFeminine";
        private const string _BLEND_MUSCLE_NAME = "defaultBuff";
        private const string _BLEND_SHAPE_HEAVY_NAME = "defaultHeavy";
        private const string _BLEND_SHAPE_SKINNY_NAME = "defaultSkinny";

        private const string _TEXTURE_COLOR_NAME = "ColorMap.png";
        private const string _TEXTURE_METALLIC_NAME = "MetallicMap.png";
        private const string _TEXTURE_SMOOTHNESS_NAME = "SmoothnessMap.png";
        private const string _TEXTURE_REFLECTION_NAME = "ReflectionMap.png";
        private const string _TEXTURE_EMISSION_NAME = "EmissionMap.png";
        private const string _TEXTURE_OPACITY_NAME = "OpacityMap.png";
        private const string _TEXTURE_PREFIX = "T_";

        private static readonly int _COLOR_MAP = Shader.PropertyToID("_ColorMap");
        private static readonly int _METALLIC_MAP = Shader.PropertyToID("_MetallicMap");
        private static readonly int _SMOOTHNESS_MAP = Shader.PropertyToID("_SmoothnessMap");
        private static readonly int _REFLECTION_MAP = Shader.PropertyToID("_ReflectionMap");
        private static readonly int _EMISSION_MAP = Shader.PropertyToID("_EmissionMap");
        private static readonly int _OPACITY_MAP = Shader.PropertyToID("_OpacityMap");

        private const string _BASE_MODEL_AVATAR_NAME = "SK_BaseModelAvatar";

        private DatabaseManager _dbManager;
        private GameObject _baseModel;
        private GameObject _resolvedBaseModel;
        private Avatar _resolvedAvatar;
        private Material _currentMaterial;
        private RuntimeAnimatorController _currentAnimationController;
        private List<Vector2> _currentUVList;
        private Dictionary<ColorPartType, List<Vector2>> _currentUVDictionary;
        private Dictionary<string, Vector3> _blendShapeRigMovement;
        private Dictionary<string, Quaternion> _blendShapeRigRotation;
        private Dictionary<CharacterPartType, Dictionary<string, string>> _partLibrary;
        private Dictionary<CharacterPartType, List<SidekickPart>> _allPartsLibrary;
        private Dictionary<string, List<string>> _partOutfitMap;
        private Dictionary<string, bool> _partOutfitToggleMap;
        private Dictionary<string, Dictionary<SidekickSpecies, Dictionary<CharacterPartType, List<string>>>> _filterPartDictionary;
        private Dictionary<CharacterPartType, Dictionary<string, SidekickPart>> _mappedPartDictionary;
        private Dictionary<CharacterPartType, List<string>> _mappedPartList;
        private Dictionary<SidekickSpecies, Dictionary<CharacterPartType, List<string>>> _mappedBasePartDictionary;
        private Dictionary<string, SidekickSpecies> _speciesDictionary;
        private Dictionary<string, List<SidekickPartPreset>> _mappedPresetFilterDictionary;
        private Dictionary<SidekickSpecies, List<SidekickPartPreset>> _mappedBasePresetDictionary;
        private int _partCount;
        private SidekickSpecies _currentSpecies;

        private float _bodyTypeBlendValue;
        private float _bodySizeSkinnyBlendValue;
        private float _bodySizeHeavyBlendValue;
        private float _musclesBlendValue;

        public DatabaseManager DBManager
        {
            get => _dbManager;
            set => _dbManager = value;
        }

        public GameObject BaseModel
        {
            get => _baseModel;
            set => _baseModel = value;
        }

        /// <summary>
        ///     The base model resolved by the most recent CreateCharacter call; falls back to the assigned base model.
        /// </summary>
        private GameObject EffectiveBaseModel => _resolvedBaseModel != null ? _resolvedBaseModel : _baseModel;

        /// <summary>
        ///     The avatar resolved by the most recent CreateCharacter call; falls back to the assigned base model's avatar.
        /// </summary>
        private Avatar EffectiveAvatar
        {
            get
            {
                if (_resolvedAvatar != null)
                {
                    return _resolvedAvatar;
                }

                Animator baseAnimator = _baseModel.GetComponentInChildren<Animator>();
                return baseAnimator != null ? baseAnimator.avatar : null;
            }
        }

        /// <summary>
        ///     When true, characters are always built on the assigned BaseModel and head-part avatar auto-detection is skipped.
        /// </summary>
        public bool ForceAssignedBaseModel { get; set; }

        public Material CurrentMaterial
        {
            get => _currentMaterial;
            set => _currentMaterial = value;
        }

        public RuntimeAnimatorController CurrentAnimationController
        {
            get => _currentAnimationController;
            set => _currentAnimationController = value;
        }

        public List<Vector2> CurrentUVList
        {
            get => _currentUVList;
            set => _currentUVList = value;
        }

        public Dictionary<ColorPartType, List<Vector2>> CurrentUVDictionary
        {
            get => _currentUVDictionary;
            set => _currentUVDictionary = value;
        }

        public Dictionary<CharacterPartType, Dictionary<string, string>> PartLibrary
        {
            get => _partLibrary;
            set => _partLibrary = value;
        }

        public int PartCount
        {
            get => _partCount;
            private set => _partCount = value;
        }

        public Dictionary<string, List<string>> PartOutfitMap
        {
            get => _partOutfitMap;
            set => _partOutfitMap = value;
        }

        public Dictionary<string, bool> PartOutfitToggleMap
        {
            get => _partOutfitToggleMap;
            set => _partOutfitToggleMap = value;
        }

        public float BodyTypeBlendValue
        {
            get => _bodyTypeBlendValue;
            set => _bodyTypeBlendValue = value;
        }

        public float BodySizeSkinnyBlendValue
        {
            get => _bodySizeSkinnyBlendValue;
            set => _bodySizeSkinnyBlendValue = value;
        }

        public float BodySizeHeavyBlendValue
        {
            get => _bodySizeHeavyBlendValue;
            set => _bodySizeHeavyBlendValue = value;
        }

        public float MusclesBlendValue
        {
            get => _musclesBlendValue;
            set => _musclesBlendValue = value;
        }

        public SidekickSpecies CurrentSpecies
        {
            get => _currentSpecies;
            set => _currentSpecies = value;
        }

        public Dictionary<string, Dictionary<SidekickSpecies, Dictionary<CharacterPartType, List<string>>>> FilterPartDictionary
        {
            get => _filterPartDictionary;
            private set => _filterPartDictionary = value;
        }

        public Dictionary<CharacterPartType, Dictionary<string, SidekickPart>> MappedPartDictionary
        {
            get => _mappedPartDictionary;
            private set => _mappedPartDictionary = value;
        }

        public Dictionary<SidekickSpecies, Dictionary<CharacterPartType, List<string>>> MappedBasePartDictionary
        {
            get => _mappedBasePartDictionary;
            private set => _mappedBasePartDictionary = value;
        }

        public Dictionary<CharacterPartType, List<string>> MappedPartList
        {
            get => _mappedPartList;
            private set => _mappedPartList = value;
        }

        public Dictionary<CharacterPartType, List<SidekickPart>> AllPartsLibrary
        {
            get => _allPartsLibrary;
            private set => _allPartsLibrary = value;
        }

        public Dictionary<string, List<SidekickPartPreset>> MappedPresetFilterDictionary
        {
            get => _mappedPresetFilterDictionary;
            private set => _mappedPresetFilterDictionary = value;
        }

        public Dictionary<SidekickSpecies, List<SidekickPartPreset>> MappedBasePresetDictionary
        {
            get => _mappedBasePresetDictionary;
            private set => _mappedBasePresetDictionary = value;
        }

        /// <summary>
        ///     Creates and instance of the SidekickRuntime with the given parameters.
        /// </summary>
        /// <param name="model">The base donor model to use. This is used to provide a base rig that parts can be added and removed from.</param>
        /// <param name="material">The base material that will be applied to all parts that are added or removed from the character.</param>
        /// <param name="animationController">The animation controller to apply to the created model.</param>
        /// <param name="dbManager">The Database Manager to use, if not provided a new connection will be created.</param>
        public SidekickRuntime(GameObject model, Material material, RuntimeAnimatorController animationController = null, DatabaseManager dbManager = null)
        {
            _dbManager = dbManager ?? new DatabaseManager();

            if (_dbManager.GetCurrentDbConnection() == null)
            {
                _dbManager.GetDbConnection(true);
            }

            _baseModel = model;
            _currentMaterial = material;
            _currentAnimationController = animationController;

            ResetUVData();
        }

        public static async Task PopulateToolData(SidekickRuntime runtime)
        {
            await runtime.LoadPartLibrary();
            await runtime.PopulatePresetLibrary();
        }

        /// <summary>
        ///     Takes all the parts selected in the window, and combines them into a single model in the scene.
        /// </summary>
        /// <param name="modelName">What to call the parent GameObject of the created character.</param>;
        /// <param name="toCombine">The list of SkinnedMeshes to combine to create the character.</param>
        /// <param name="combineMesh">
        ///     When true the character mesh will be combined into a single mesh. When false each part keeps its own mesh, but all
        ///     parts share a single merged skeleton hierarchy.
        /// </param>
        /// <param name="processBoneMovement">When true the bones will be moved to match the blend shape settings.</param>
        /// <param name="existingModel">
        ///     No longer used; a new model is always created. Kept for API compatibility — callers must use the returned object and
        ///     destroy any previous model themselves.
        /// </param>
        /// <param name="combineBodyBlendShapes">
        ///     When true the body blend shapes are kept on the created model. When false the current body blend shape values are
        ///     baked into the mesh and the blend shapes removed.
        /// </param>
        /// <param name="combineFacialBlendShapes">
        ///     When true the facial blend shapes are kept on the created model. When false the current facial blend shape values are
        ///     baked into the mesh and the blend shapes removed.
        /// </param>
        /// <returns>A new character object.</returns>
        public GameObject CreateCharacter(
            string modelName,
            List<SkinnedMeshRenderer> toCombine,
            bool combineMesh,
            bool processBoneMovement,
            GameObject existingModel = null,
            bool combineBodyBlendShapes = true,
            bool combineFacialBlendShapes = true
        )
        {
            PopulateUVDictionary(toCombine);

            _resolvedBaseModel = ResolveBaseModel(toCombine);

            GameObject newSpawn;

            if (combineMesh)
            {
                newSpawn = Combiner.CreateCombinedSkinnedMesh(toCombine, EffectiveBaseModel, _currentMaterial);
            }
            else
            {
                newSpawn = Combiner.CreateSeparateSkinnedMeshes(toCombine, EffectiveBaseModel, _currentMaterial);
            }

            newSpawn.name = modelName;

            Renderer renderer = newSpawn.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = _currentMaterial;
            }

            if (newSpawn.GetComponent<Animator>() == null)
            {
                Animator newModelAnimator = newSpawn.AddComponent<Animator>();
                newModelAnimator.avatar = EffectiveAvatar;
                newModelAnimator.Rebind();

                if (_currentAnimationController != null)
                {
                    newModelAnimator.runtimeAnimatorController = _currentAnimationController;
                }
            }

            UpdateBlendShapes(newSpawn);

            // Bake before any bone movement; the saved bindposes are only valid to re-apply while the skeleton is still in bindpose.
            BakeAndFilterBlendShapes(newSpawn, combineBodyBlendShapes, combineFacialBlendShapes);

            if (processBoneMovement)
            {
                ProcessRigMovementOnBlendShapeChange(SidekickBlendShapeRigMovement.GetAllForProcessing(_dbManager));
                ProcessBoneMovement(newSpawn);
            }

            return newSpawn;
        }

        /// <summary>
        ///     Creates the model but with all parts as separate meshes.
        /// </summary>
        /// <param name="parts">The parts to build into the character.</param>
        /// <param name="outputModelName">What to call the parent GameObject of the created character.</param>
        /// <returns>A new game object with all the part meshes and a single rig.</returns>
        [Obsolete("No longer used by the tool; use CreateCharacter, which routes through Combiner.CreateSeparateSkinnedMeshes.")]
        public GameObject CreateModelFromParts(
            List<SkinnedMeshRenderer> parts,
            string outputModelName,
            GameObject existingModel = null
        )
        {

            List<CharacterPartType> allTypes = Enum.GetValues(typeof(CharacterPartType)).Cast<CharacterPartType>().ToList();

            GameObject partsModel = existingModel == null ? new GameObject(outputModelName) : existingModel;

            Transform modelRootBone = _baseModel.GetComponentInChildren<SkinnedMeshRenderer>().rootBone;
            GameObject newRootBone;
            if (existingModel != null)
            {
                GameObject oldRootBone = existingModel.transform.Find("root").gameObject;
#if UNITY_EDITOR
                GameObject.DestroyImmediate(oldRootBone);
#else
                GameObject.Destroy(oldRootBone);
#endif
            }

            newRootBone = Object.Instantiate(modelRootBone.gameObject, partsModel.transform, true);
            newRootBone.name = modelRootBone.name;

            Hashtable boneNameMap = Combiner.CreateBoneNameMap(newRootBone);
            Transform[] bones = new Transform[boneNameMap.Count];
            if (existingModel != null)
            {
                boneNameMap.Values.CopyTo(bones, 0);
            }

            Transform[] additionalBones = Combiner.FindAdditionalBones(boneNameMap, new List<SkinnedMeshRenderer>(parts));
            if (additionalBones.Length > 0)
            {
                Combiner.JoinAdditionalBonesToBoneArray(bones, additionalBones, boneNameMap);
                // Need to redo the name map now that we have updated the bone array.
                boneNameMap = Combiner.CreateBoneNameMap(newRootBone);
            }

            for (int i = 0; i < parts.Count; i++)
            {
                SkinnedMeshRenderer part = parts[i];

                allTypes.Remove(ExtractPartType(part.name));

                if (existingModel != null && partsModel != null)
                {
                    SkinnedMeshRenderer existingPart = partsModel.GetComponentsInChildren<SkinnedMeshRenderer>()
                        .FirstOrDefault(go => go.name.Contains(ExtractPartTypeString(part.name)));

                    if (existingPart != null)
                    {
#if UNITY_EDITOR
                        GameObject.DestroyImmediate(existingPart.gameObject);
#else
                        GameObject.Destroy(existingModel.gameObject);
#endif
                    }
                }

                GameObject newPart = new GameObject(part.name);
                newPart.transform.parent = partsModel.transform;
                SkinnedMeshRenderer renderer = newPart.AddComponent<SkinnedMeshRenderer>();
                renderer.updateWhenOffscreen = true;
                Transform[] oldBones = part.bones;
                Transform[] newBones = new Transform[part.bones.Length];
                for (int j = 0; j < oldBones.Length; j++)
                {
                    newBones[j] = (Transform) boneNameMap[oldBones[j].name];
                }

                renderer.sharedMesh = MeshUtils.CopyMesh(part.sharedMesh);
                renderer.rootBone = (Transform) boneNameMap[part.rootBone.name];

                Combiner.MergeAndGetAllBlendShapeDataOfSkinnedMeshRenderers(
                    new[]
                    {
                        part
                    },
                    renderer.sharedMesh,
                    renderer
                );

                renderer.bones = newBones;
                renderer.sharedMaterial = _currentMaterial;
            }

            foreach (CharacterPartType type in allTypes)
            {
                SkinnedMeshRenderer existingPart = partsModel.GetComponentsInChildren<SkinnedMeshRenderer>()
                    .FirstOrDefault(go => go.name.Contains(CharacterPartTypeUtils.GetPartTypeString(type)));

                if (existingPart != null)
                {
#if UNITY_EDITOR
                    GameObject.DestroyImmediate(existingPart.gameObject);
#else
                    GameObject.Destroy(existingModel.gameObject);
#endif
                }
            }

            return partsModel;
        }

        /// <summary>
        ///     Populates the list of current UVs and UV part dictionary.
        /// </summary>
        public void PopulateUVDictionary(List<SkinnedMeshRenderer> usedParts)
        {
            ResetUVData();

            foreach (SkinnedMeshRenderer skinnedMesh in usedParts)
            {
                if (skinnedMesh == null || skinnedMesh.sharedMesh == null)
                {
                    continue;
                }

                // A combined scene mesh (single renderer named "mesh") carries no part name; its UVs still
                // contribute to the global list, just not to a per-part bucket.
                List<Vector2> partUVs = null;
                if (skinnedMesh.name.Count(c => c == '_') >= 2
                    && Enum.TryParse(ExtractPartType(skinnedMesh.name).ToString(), out ColorPartType type)
                    && _currentUVDictionary.ContainsKey(type))
                {
                    partUVs = _currentUVDictionary[type];
                }

                foreach (Vector2 uv in skinnedMesh.sharedMesh.uv)
                {
                    int scaledU = (int) Math.Floor(uv.x * 16);
                    int scaledV = (int) Math.Floor(uv.y * 16);

                    if (scaledU == 16)
                    {
                        scaledU = 15;
                    }

                    if (scaledV == 16)
                    {
                        scaledV = 15;
                    }

                    Vector2 scaledUV = new Vector2(scaledU, scaledV);
                    // For the global UV list, we don't want any duplicates on a global level
                    if (!_currentUVList.Contains(scaledUV))
                    {
                        _currentUVList.Add(scaledUV);
                    }

                    // For the part specific UV list we may have UVs that are in the global list already, we don't want to exclude these, so check
                    // them separately to the global list
                    if (partUVs != null && !partUVs.Contains(scaledUV))
                    {
                        partUVs.Add(scaledUV);
                    }
                }
            }
        }

        /// <summary>
        ///     Resets the current UV list and UV part dictionary to empty collections, with an entry for every part type.
        /// </summary>
        private void ResetUVData()
        {
            _currentUVList = new List<Vector2>();
            _currentUVDictionary = new Dictionary<ColorPartType, List<Vector2>>();

            foreach (ColorPartType type in Enum.GetValues(typeof(ColorPartType)))
            {
                _currentUVDictionary.Add(type, new List<Vector2>());
            }
        }

        /// <summary>
        ///     Updates the blend shape values of the combined model.
        /// </summary>
        public void UpdateBlendShapes(GameObject model)
        {
            if (model == null)
            {
                return;
            }

            List<SkinnedMeshRenderer> allMeshes = model.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
            foreach (SkinnedMeshRenderer skinnedMesh in allMeshes)
            {
                Mesh sharedMesh = skinnedMesh.sharedMesh;
                for (int i = 0; i < sharedMesh.blendShapeCount; i++)
                {
                    string blendName = sharedMesh.GetBlendShapeName(i);
                    if (blendName.Contains(_BLEND_GENDER_NAME))
                    {
                        skinnedMesh.SetBlendShapeWeight(i, (_bodyTypeBlendValue + 100) / 2);
                    }
                    else if (blendName.Contains(_BLEND_SHAPE_SKINNY_NAME))
                    {
                        skinnedMesh.SetBlendShapeWeight(i, _bodySizeSkinnyBlendValue);
                    }
                    else if (blendName.Contains(_BLEND_SHAPE_HEAVY_NAME))
                    {
                        skinnedMesh.SetBlendShapeWeight(i, _bodySizeHeavyBlendValue);
                    }
                    else if (blendName.Contains(_BLEND_MUSCLE_NAME))
                    {
                        skinnedMesh.SetBlendShapeWeight(i, (_musclesBlendValue + 100) / 2);
                    }
                }
            }
        }

        /// <summary>
        ///     Checks if the given blend shape name is one of the body shape blend shapes.
        /// </summary>
        /// <param name="blendShapeName">The blend shape name to check.</param>
        /// <returns>True if the blend shape is a body shape blend shape; otherwise false.</returns>
        public static bool IsBodyBlendShape(string blendShapeName)
        {
            return blendShapeName.Contains(_BLEND_GENDER_NAME)
                || blendShapeName.Contains(_BLEND_MUSCLE_NAME)
                || blendShapeName.Contains(_BLEND_SHAPE_HEAVY_NAME)
                || blendShapeName.Contains(_BLEND_SHAPE_SKINNY_NAME);
        }

        /// <summary>
        ///     Removes any blend shape groups that are not being kept from all meshes on the given model, baking their current
        ///     values into the mesh vertices first so the model keeps its current shape.
        /// </summary>
        /// <param name="model">The model to process.</param>
        /// <param name="keepBodyBlendShapes">When true the body blend shapes are kept on the meshes.</param>
        /// <param name="keepFacialBlendShapes">When true the facial blend shapes are kept on the meshes.</param>
        public static void BakeAndFilterBlendShapes(GameObject model, bool keepBodyBlendShapes, bool keepFacialBlendShapes)
        {
            if (keepBodyBlendShapes && keepFacialBlendShapes)
            {
                return;
            }

            foreach (SkinnedMeshRenderer renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (renderer.sharedMesh == null || renderer.sharedMesh.blendShapeCount == 0)
                {
                    continue;
                }

                Mesh bakedMesh = MeshUtils.CopyMesh(renderer.sharedMesh);
                // Copy bone weights and bindposes before baking so the mesh can be re-skinned after baking.
                BoneWeight[] boneWeights = bakedMesh.boneWeights;
                Matrix4x4[] bindposes = bakedMesh.bindposes;

                List<BlendShapeData> keptBlendShapes = BlendShapeUtils.GetBlendShapeData(
                    bakedMesh,
                    renderer,
                    blendShapeName => IsBodyBlendShape(blendShapeName) ? keepBodyBlendShapes : keepFacialBlendShapes,
                    0,
                    new List<BlendShapeData>()
                );

                // Zero the kept shapes so BakeMesh only bakes in the removed shapes; the kept shapes get their weights
                // re-applied by RestoreBlendShapeData, which would otherwise double-apply their deltas.
                foreach (BlendShapeData blendShapeData in keptBlendShapes)
                {
                    renderer.SetBlendShapeWeight(blendShapeData.blendShapeFrameIndex, 0f);
                }

                renderer.BakeMesh(bakedMesh);
                // Re-skin the new baked mesh.
                bakedMesh.boneWeights = boneWeights;
                bakedMesh.bindposes = bindposes;
                renderer.sharedMesh = bakedMesh;

                if (keptBlendShapes.Count > 0)
                {
                    BlendShapeUtils.RestoreBlendShapeData(keptBlendShapes, bakedMesh, renderer);
                }
            }
        }

        /// <summary>
        ///     Populates the internal library of parts based on the files in the project.
        /// </summary>
        public Dictionary<CharacterPartType, Dictionary<string, string>> PopulatePartLibrary()
        {
            _partLibrary = new Dictionary<CharacterPartType, Dictionary<string, string>>();
            _partOutfitMap = new Dictionary<string, List<string>>();
            _partOutfitToggleMap = new Dictionary<string, bool>();
            _partCount = 0;

            List<string> files = Directory.GetFiles("Assets", "SK_*_*_*_*_*.fbx", SearchOption.AllDirectories).ToList();

            foreach (CharacterPartType partType in Enum.GetValues(typeof(CharacterPartType)))
            {
                Dictionary<string, string> partLocationDictionary = new Dictionary<string, string>();

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    string partName = fileInfo.Name;
                    partName = partName.Substring(0, partName.IndexOf(".fbx", StringComparison.Ordinal));
                    CharacterPartType characterPartType = ExtractPartType(partName);
                    if (characterPartType > 0 && characterPartType == partType && !partLocationDictionary.ContainsKey(partName))
                    {
                        partLocationDictionary.Add(partName, file);
                        _partCount++;

                        // TODO: populate with actual outfit data when we have proper information about part outfits
                        string tempPartOutfit = GetOutfitNameFromPartName(partName);
                        List<string> partNameList;
                        if (_partOutfitMap.TryGetValue(tempPartOutfit, out List<string> value))
                        {
                            partNameList = value;
                            partNameList.Add(partName);
                            _partOutfitMap[tempPartOutfit] = partNameList;
                        }
                        else
                        {
                            partNameList = new List<string>();
                            partNameList.Add(partName);
                            _partOutfitMap.Add(tempPartOutfit, partNameList);
                            _partOutfitToggleMap.Add(tempPartOutfit, true);
                        }
                    }
                }

                _partLibrary.Add(partType, partLocationDictionary);
            }

            return _partLibrary;
        }

        /// <summary>
        ///     Populates the internal library of Presets into libraries based on filters and base parts.
        /// </summary>
        public Task PopulatePresetLibrary()
        {
            HashSet<SidekickPartPreset> uniqueList = new HashSet<SidekickPartPreset>();

            // Built as locals and published to the fields in one step at the end, so a concurrent
            // rebuild can never interleave with this one and double up entries.
            Dictionary<string, List<SidekickPartPreset>> mappedPresetFilterDictionary = new Dictionary<string, List<SidekickPartPreset>>();
            Dictionary<SidekickSpecies, List<SidekickPartPreset>> mappedBasePresetDictionary = new Dictionary<SidekickSpecies, List<SidekickPartPreset>>();

            foreach (SidekickPresetFilter filter in SidekickPresetFilter.GetAll(_dbManager))
            {
                List<SidekickPartPreset> presets = filter.GetAllPresetsForFilter(_dbManager, true, true);
                mappedPresetFilterDictionary[filter.Term] = presets;
                uniqueList.UnionWith(presets);
            }

            // Check for and add BASE only presets
            List<SidekickPartPreset> allPresets = SidekickPartPreset.GetAll(_dbManager);
            List<SidekickPartPreset> presetsNotInFilters = allPresets.Where(preset => !uniqueList.Contains(preset)).ToList();
            foreach (SidekickPartPreset preset in presetsNotInFilters)
            {
                if (preset.HasOnlyBasePartsAndAllAvailable(_dbManager))
                {
                    if (mappedBasePresetDictionary.TryGetValue(preset.Species, out List<SidekickPartPreset> mappedPresets))
                    {
                        mappedPresets.Add(preset);
                        mappedBasePresetDictionary[preset.Species] = mappedPresets;
                    }
                    else
                    {
                        List<SidekickPartPreset> presets = new List<SidekickPartPreset>
                        {
                            preset
                        };

                        mappedBasePresetDictionary.Add(preset.Species, presets);
                    }
                }
            }

            _mappedPresetFilterDictionary = mappedPresetFilterDictionary;
            _mappedBasePresetDictionary = mappedBasePresetDictionary;

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Populates the internal library of parts based on the files in the project.
        /// </summary>
        public Task LoadPartLibrary()
        {
            // Built as locals and published to the fields in one step at the end, so a concurrent
            // rebuild can never interleave with this one and double up entries or the part count.
            Dictionary<CharacterPartType, List<SidekickPart>> allPartsLibrary = new Dictionary<CharacterPartType, List<SidekickPart>>();
            Dictionary<CharacterPartType, List<string>> mappedPartList = new Dictionary<CharacterPartType, List<string>>();
            Dictionary<CharacterPartType, Dictionary<string, SidekickPart>> mappedPartDictionary = new Dictionary<CharacterPartType, Dictionary<string, SidekickPart>>();
            Dictionary<SidekickSpecies, Dictionary<CharacterPartType, List<string>>> mappedBasePartDictionary =
                new Dictionary<SidekickSpecies, Dictionary<CharacterPartType, List<string>>>();
            Dictionary<string, SidekickSpecies> speciesDictionary = new Dictionary<string, SidekickSpecies>();
            int partCount = 0;

#if UNITY_EDITOR
            // Editor-only: verify each part's FBX exists in the project. A player build has no Assets folder on disk and
            // loads part models from Resources instead, so this scan must not run at runtime.
            Dictionary<string, string> filesOnDisk = new Dictionary<string, string>();
            foreach (string file in Directory.GetFiles("Assets", "SK_*_*_*_*_*.fbx", SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(file);
                string partName = fileInfo.Name;
                partName = partName.Substring(0, partName.IndexOf(".fbx", StringComparison.Ordinal));
                filesOnDisk.TryAdd(partName, file);
            }
#endif

            foreach (CharacterPartType type in Enum.GetValues(typeof(CharacterPartType)))
            {
                allPartsLibrary[type] = new List<SidekickPart>();
                mappedPartDictionary[type] = new Dictionary<string, SidekickPart>();
                mappedPartList[type] = new List<string>();
            }

            SidekickSpecies unrestrictedSpecies = null;

            foreach (SidekickSpecies species in SidekickSpecies.GetAll(_dbManager, false))
            {
                speciesDictionary[species.Name] = species;
                mappedBasePartDictionary[species] = new Dictionary<CharacterPartType, List<string>>();

                if (species.Name == "Unrestricted")
                {
                    unrestrictedSpecies = species;
                }
            }

            List<SidekickPart> allParts = SidekickPart.GetAll(_dbManager);

            foreach (SidekickPart part in allParts)
            {
#if UNITY_EDITOR
                bool partAvailable = filesOnDisk.ContainsKey(part.Name);
#else
                // Player build: trust the shipped DB's file_exists flag (set at author time); no disk scan, no write.
                // Model loads are null-checked at use, so a part whose model is missing is simply skipped.
                bool partAvailable = part.FileExists;
#endif
                if (partAvailable)
                {
                    partCount++;

                    part.FileExists = true;

                    List<SidekickPart> parts = allPartsLibrary.TryGetValue(part.Type, out List<SidekickPart> value)
                        ? value
                        : new List<SidekickPart>();
                    parts.Add(part);
                    allPartsLibrary[part.Type] = parts;

                    Dictionary<string, SidekickPart> partMap = mappedPartDictionary[part.Type];
                    partMap[part.Name] = part;
                    mappedPartDictionary[part.Type] = partMap;

                    List<string> currentList = mappedPartList[part.Type];
                    currentList.Add(part.Name);
                    mappedPartList[part.Type] = currentList;

                    if (part.Name.Contains("_BASE_"))
                    {
                        if (!mappedBasePartDictionary.ContainsKey(part.Species))
                        {
                            continue;
                        }

                        Dictionary<CharacterPartType, List<string>> basePartMap = mappedBasePartDictionary[part.Species];
                        List<string> partList = basePartMap.TryGetValue(part.Type, out List<string> existingList) ? existingList : new List<string>();
                        partList.Add(part.Name);
                        basePartMap[part.Type] = partList;
                        mappedBasePartDictionary[part.Species] = basePartMap;

                        if (unrestrictedSpecies != null)
                        {
                            Dictionary<CharacterPartType, List<string>> unrestrictedBasePartMap = mappedBasePartDictionary[unrestrictedSpecies];
                            List<string> unrestrictedPartList = unrestrictedBasePartMap.TryGetValue(part.Type, out List<string> unrestrictedList) ? unrestrictedList : new List<string>();
                            unrestrictedPartList.Add(part.Name);
                            unrestrictedBasePartMap[part.Type] = unrestrictedPartList;
                            mappedBasePartDictionary[unrestrictedSpecies] = unrestrictedBasePartMap;
                        }
                    }

                }
                else
                {
                    part.FileExists = false;
                }

            }

#if UNITY_EDITOR
            // Persists the FileExists flag computed above; only meaningful in the editor.
            SidekickPart.UpdateAll(_dbManager, allParts);
#endif

            _allPartsLibrary = allPartsLibrary;
            _mappedPartList = mappedPartList;
            _mappedPartDictionary = mappedPartDictionary;
            _mappedBasePartDictionary = mappedBasePartDictionary;
            _speciesDictionary = speciesDictionary;
            _partCount = partCount;

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Gets the "outfit" name from the part name.
        ///     TODO: This will be replaced once parts and outfits have a proper relationship.
        /// </summary>
        /// <param name="partName">The part name to parse the "outfit" name from.</param>
        /// <returns>The "outfit" name.</returns>
        public string GetOutfitNameFromPartName(string partName)
        {
            if (string.IsNullOrEmpty(partName))
            {
                return "None";
            }

            return string.Join('_', partName.Substring(3).Split('_').Take(2));
        }

        /// <summary>
        ///     Determines the part type from the part name. This will work as long as the naming format is correct.
        /// </summary>
        /// <param name="partName">The name of the part.</param>
        /// <returns>The part type.</returns>
        public CharacterPartType ExtractPartType(string partName)
        {
            string partTypeString = ExtractPartTypeString(partName);
            string partIndexString = "0";
            if (partTypeString.Length > 2)
            {
                partIndexString = partTypeString.Substring(0, 2);
            }

            bool valueParsed = int.TryParse(partIndexString, out int index);
            return valueParsed ? (CharacterPartType) index : 0;
        }

        /// <summary>
        ///     Extracts the part type string from the file name.
        /// </summary>
        /// <param name="partName">The name of the part.</param>
        /// <returns>The part type string</returns>
        public string ExtractPartTypeString(string partName)
        {
            return partName.Split('_').Reverse().ElementAt(1);
        }

        /// <summary>
        ///     Finds the head part in the given meshes and returns its source model root.
        /// </summary>
        /// <param name="toCombine">The list of part meshes being combined.</param>
        /// <returns>The root GameObject of the head part's source model, or null if no head part exists.</returns>
        public GameObject GetHeadPartModel(List<SkinnedMeshRenderer> toCombine)
        {
            foreach (SkinnedMeshRenderer mesh in toCombine)
            {
                if (mesh == null || mesh.name.Count(c => c == '_') < 2)
                {
                    continue;
                }

                if (ExtractPartType(mesh.name) == CharacterPartType.Head)
                {
                    return mesh.transform.root.gameObject;
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the avatar associated with the given model asset. Checks for an Animator first ("Create From This Model"
        ///     imports), then falls back to the import settings ("Copy From Other Avatar" imports add no Animator component;
        ///     their avatar only exists as the importer's source avatar, which is editor-only data).
        /// </summary>
        /// <param name="model">The model asset to get the avatar for.</param>
        /// <returns>The avatar associated with the model, or null if it has none.</returns>
        private static Avatar GetModelAvatar(GameObject model)
        {
            Animator animator = model.GetComponentInChildren<Animator>();
            if (animator != null && animator.avatar != null)
            {
                return animator.avatar;
            }

#if UNITY_EDITOR
            UnityEditor.ModelImporter importer =
                UnityEditor.AssetImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(model)) as UnityEditor.ModelImporter;
            if (importer != null)
            {
                return importer.sourceAvatar;
            }
#endif

            return null;
        }

        /// <summary>
        ///     Resolves which base model to build a character on. If the head part's avatar is not the standard
        ///     SK_BaseModelAvatar (e.g. Alien variants, which have a unique avatar per variant), the head's source model
        ///     is used as the rig and avatar donor, unless <see cref="ForceAssignedBaseModel" /> is set.
        ///     Also stores the resolved avatar for use when the donor model carries no Animator of its own.
        /// </summary>
        /// <param name="toCombine">The list of part meshes being combined.</param>
        /// <returns>The model to use as the rig and avatar donor for the character.</returns>
        public GameObject ResolveBaseModel(List<SkinnedMeshRenderer> toCombine)
        {
            _resolvedAvatar = null;

            if (ForceAssignedBaseModel)
            {
                return _baseModel;
            }

            GameObject headModel = GetHeadPartModel(toCombine);
            if (headModel == null)
            {
                return _baseModel;
            }

            Avatar headAvatar = GetModelAvatar(headModel);
            if (headAvatar == null || headAvatar.name == _BASE_MODEL_AVATAR_NAME)
            {
                return _baseModel;
            }

            _resolvedAvatar = headAvatar;
            return headModel;
        }

        /// <summary>
        ///     Processes the movement of rig joints based on blend shape changes.
        /// </summary>
        public void ProcessRigMovementOnBlendShapeChange(Dictionary<CharacterPartType, Dictionary<BlendShapeType, SidekickBlendShapeRigMovement>> offsetLibrary)
        {
            Transform modelRootBone = EffectiveBaseModel.transform.Find("root");
            Hashtable boneNameMap = Combiner.CreateBoneNameMap(modelRootBone.gameObject);

            _blendShapeRigMovement = new Dictionary<string, Vector3>();
            _blendShapeRigRotation = new Dictionary<string, Quaternion>();

            foreach (KeyValuePair<CharacterPartType, string> entry in BlendshapeJointAdjustment.PART_TYPE_JOINT_MAP)
            {
                Transform bone = (Transform) boneNameMap[entry.Value];

                float feminineBlendValue = (_bodyTypeBlendValue + 100) / 2 / 100;

                Vector3 allMovement = bone.localPosition;
                Quaternion allRotation = bone.localRotation;

                if (offsetLibrary.TryGetValue(entry.Key, out Dictionary<BlendShapeType, SidekickBlendShapeRigMovement> blendOffsetLibrary))
                {
                    foreach (BlendShapeType blendType in Enum.GetValues(typeof(BlendShapeType)))
                    {
                        if (blendOffsetLibrary.TryGetValue(blendType, out SidekickBlendShapeRigMovement rigMovement))
                        {
                            if (rigMovement == null)
                            {
                                continue;
                            }

                            switch (blendType)
                            {
                                case BlendShapeType.Feminine:
                                    allMovement += rigMovement.GetBlendedOffsetValue(feminineBlendValue);
                                    allRotation *= rigMovement.GetBlendedRotationValue(feminineBlendValue);
                                    break;
                                case BlendShapeType.Heavy:
                                    allMovement += rigMovement.GetBlendedOffsetValue(_bodySizeHeavyBlendValue / 100);
                                    allRotation *= rigMovement.GetBlendedRotationValue(_bodySizeHeavyBlendValue / 100);
                                    break;
                                case BlendShapeType.Skinny:
                                    allMovement += rigMovement.GetBlendedOffsetValue(_bodySizeSkinnyBlendValue / 100);
                                    allRotation *= rigMovement.GetBlendedRotationValue(_bodySizeSkinnyBlendValue / 100);
                                    break;
                                case BlendShapeType.Bulk:
                                    allMovement += rigMovement.GetBlendedOffsetValue((_musclesBlendValue + 100) / 2 / 100);
                                    allRotation *= rigMovement.GetBlendedRotationValue((_musclesBlendValue + 100) / 2 / 100);
                                    break;
                            }
                        }
                    }
                }

                _blendShapeRigMovement[entry.Value] = allMovement;
                _blendShapeRigRotation[entry.Value] = allRotation;
            }
        }

        /// <summary>
        ///     Processes the movement of the rig with regards to the current blend shape settings.
        /// </summary>
        /// <param name="model">The model to process the movement on.</param>
        public void ProcessBoneMovement(GameObject model)
        {
            if (model == null)
            {
                return;
            }

            Transform modelRootBone = model.transform.Find("root");
            Hashtable boneNameMap = Combiner.CreateBoneNameMap(modelRootBone.gameObject);
            Combiner.ProcessBoneMovement(boneNameMap, _blendShapeRigMovement, _blendShapeRigRotation);
        }

        /// <summary>
        ///     Updates the texture on the given color row for the specified color type.
        /// </summary>
        /// <param name="colorType">The color type to update.</param>
        /// <param name="colorRow">The color row to get the updated color from.</param>
        public void UpdateColor(ColorType colorType, SidekickColorRow colorRow)
        {
            if (colorRow == null)
            {
                return;
            }

            if (_currentMaterial == null)
            {
                return;
            }

            switch (colorType)
            {
                case ColorType.Metallic:
                    Texture2D metallic = (Texture2D) _currentMaterial.GetTexture(_METALLIC_MAP);
                    UpdateTexture(metallic, colorRow.NiceMetallic, colorRow.ColorProperty.U, colorRow.ColorProperty.V);
                    _currentMaterial.SetTexture(_METALLIC_MAP, metallic);
                    break;
                case ColorType.Smoothness:
                    Texture2D smoothness = (Texture2D) _currentMaterial.GetTexture(_SMOOTHNESS_MAP);
                    UpdateTexture(smoothness, colorRow.NiceSmoothness, colorRow.ColorProperty.U, colorRow.ColorProperty.V);
                    _currentMaterial.SetTexture(_SMOOTHNESS_MAP, smoothness);
                    break;
                case ColorType.Reflection:
                    Texture2D reflection = (Texture2D) _currentMaterial.GetTexture(_REFLECTION_MAP);
                    UpdateTexture(reflection, colorRow.NiceReflection, colorRow.ColorProperty.U, colorRow.ColorProperty.V);
                    _currentMaterial.SetTexture(_REFLECTION_MAP, reflection);
                    break;
                case ColorType.Emission:
                    Texture2D emission = (Texture2D) _currentMaterial.GetTexture(_EMISSION_MAP);
                    UpdateTexture(emission, colorRow.NiceEmission, colorRow.ColorProperty.U, colorRow.ColorProperty.V);
                    _currentMaterial.SetTexture(_EMISSION_MAP, emission);
                    break;
                case ColorType.Opacity:
                    Texture2D opacity = (Texture2D) _currentMaterial.GetTexture(_OPACITY_MAP);
                    UpdateTexture(opacity, colorRow.NiceOpacity, colorRow.ColorProperty.U, colorRow.ColorProperty.V);
                    _currentMaterial.SetTexture(_OPACITY_MAP, opacity);
                    break;
                case ColorType.MainColor:
                default:
                    Texture2D color = (Texture2D) _currentMaterial.GetTexture(_COLOR_MAP);
                    UpdateTexture(color, colorRow.NiceColor, colorRow.ColorProperty.U, colorRow.ColorProperty.V);
                    _currentMaterial.SetTexture(_COLOR_MAP, color);
                    break;
            }
        }

        /// <summary>
        ///     Updates the color on the texture with the given new color.
        /// </summary>
        /// <param name="texture">The texture to update.</param>
        /// <param name="newColor">The color to assign to the texture.</param>
        /// <param name="u">The u positioning on the texture to update.</param>
        /// <param name="v">The v positioning on the texture to update.</param>
        public void UpdateTexture(Texture2D texture, Color newColor, int u, int v)
        {
            int scaledU = u * 2;
            int scaledV = v * 2;
            texture.SetPixel(scaledU, scaledV, newColor);
            texture.SetPixel(scaledU + 1, scaledV, newColor);
            texture.SetPixel(scaledU, scaledV + 1, newColor);
            texture.SetPixel(scaledU + 1, scaledV + 1, newColor);
            texture.Apply();
        }

    }
}
