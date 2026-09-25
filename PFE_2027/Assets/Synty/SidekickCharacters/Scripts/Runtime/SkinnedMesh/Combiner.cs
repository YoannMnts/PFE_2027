// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using Synty.SidekickCharacters.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synty.SidekickCharacters.SkinnedMesh
{
    /// <summary>
    ///     Combines a set of given SkinnedMeshRenderers into a single SkinnedMEshRenderer.
    /// </summary>
    public static class Combiner
    {
        /// <summary>
        ///     Merges meshes together, including maintaining blend shape data.
        /// </summary>
        /// <param name="skinnedMeshesToMerge">Meshes to merge.</param>
        /// <param name="finalMesh">The mesh to merge everything into.</param>
        /// <param name="finalSkinnedMeshRenderer">The SkinnedMeshRenderer to attach the combined mesh to.</param>
        public static void MergeAndGetAllBlendShapeDataOfSkinnedMeshRenderers(
            SkinnedMeshRenderer[] skinnedMeshesToMerge,
            Mesh finalMesh,
            SkinnedMeshRenderer finalSkinnedMeshRenderer
        )
        {
            List<BlendShapeData> allBlendShapeData = new List<BlendShapeData>();

            //Verify each skinned mesh renderer and get info about all blendshapes of all meshes
            int totalVerticesVerifiedAtHereForBlendShapes = 0;

            foreach (SkinnedMeshRenderer combine in skinnedMeshesToMerge)
            {
                // Skip any parts that have not been assigned
                if (combine == null)
                {
                    continue;
                }

                List<BlendShapeData> newData = BlendShapeUtils.GetBlendShapeData(
                    finalMesh,
                    combine,
                    Array.Empty<string>(),
                    totalVerticesVerifiedAtHereForBlendShapes,
                    allBlendShapeData
                );

                //Set vertices verified at here, after processing all blendshapes for this mesh
                totalVerticesVerifiedAtHereForBlendShapes += combine.sharedMesh.vertexCount;
            }

            BlendShapeUtils.RestoreBlendShapeData(allBlendShapeData, finalMesh, finalSkinnedMeshRenderer);
        }

        /// <summary>
        ///     Processes the given GameObject and combines the objects contained into combined meshes grouped by their material.
        ///     Then returns a new GameObject with the combined data.
        /// </summary>
        /// <param name="skinnedMeshesToCombine">All of the meshes to combine into a single model</param>
        /// <param name="baseModel">The base model that has the base rig and where the skinned meshes will be combined to.</param>
        /// <param name="baseMaterial">The base material to use for the combined model.</param>
        /// <returns>A new GameObject containing all the combined objects, grouped and combined by Material.</returns>
        public static GameObject CreateCombinedSkinnedMesh(
            List<SkinnedMeshRenderer> skinnedMeshesToCombine,
            GameObject baseModel,
            Material baseMaterial
        )
        {
            // Create the new base GameObject. This will store all the combined meshes.
            GameObject combinedModel = new GameObject("Prefab Character");
            GameObject combinedSkinnedMesh = new GameObject("mesh");
            combinedSkinnedMesh.transform.parent = combinedModel.transform;

            skinnedMeshesToCombine.Sort((a, b) => string.Compare(a.name, b.name));
            Material material = null;
            Mesh mesh = new Mesh();
            GameObject rootBone = BuildSkeleton(skinnedMeshesToCombine, baseModel, combinedModel.transform, out _);

            // Build the unique skeleton; one entry per unique bone name, ordered by rig hierarchy traversal.
            HashSet<string> usedBoneNames = new HashSet<string>();
            foreach (SkinnedMeshRenderer child in skinnedMeshesToCombine)
            {
                foreach (Transform bone in child.bones)
                {
                    usedBoneNames.Add(bone.name);
                }
            }

            List<Transform> uniqueBoneList = new List<Transform>();
            Dictionary<string, int> boneIndexByName = new Dictionary<string, int>();
            CollectUsedBonesDepthFirst(rootBone.transform, usedBoneNames, uniqueBoneList, boneIndexByName);

            List<CombineInstance> combineInstances = new List<CombineInstance>();
            List<BoneWeight> allBoneWeights = new List<BoneWeight>();

            // Iterate through the skinned meshes and process them into Material groupings, and remap their bone weights to the unique skeleton.
            foreach (SkinnedMeshRenderer child in skinnedMeshesToCombine)
            {
                material = child.sharedMaterial;

                mesh = MeshUtils.CopyMesh(child.sharedMesh);

                int[] indexRemap = new int[child.bones.Length];
                for (int i = 0; i < indexRemap.Length; i++)
                {
                    if (!boneIndexByName.TryGetValue(child.bones[i].name, out int uniqueIndex))
                    {
                        Debug.LogWarning($"Combiner: bone '{child.bones[i].name}' on part '{child.name}' not found in rig; remapping to root.");
                        uniqueIndex = 0;
                    }
                    indexRemap[i] = uniqueIndex;
                }
                // Collected separately rather than written back to the copied mesh, as CombineMeshes would offset the indices again.
                allBoneWeights.AddRange(RemapBoneWeights(mesh.boneWeights, indexRemap));

                Matrix4x4 transformMatrix = child.localToWorldMatrix;

                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = mesh;
                combineInstance.transform = transformMatrix;
                combineInstances.Add(combineInstance);
            }

            SkinnedMeshRenderer renderer = combinedSkinnedMesh.AddComponent<SkinnedMeshRenderer>();
            Transform[] uniqueBones = uniqueBoneList.ToArray();
            renderer.bones = uniqueBones;
            renderer.updateWhenOffscreen = true;
            Mesh newMesh = new Mesh();
            newMesh.CombineMeshes(combineInstances.ToArray(), true, true);
            newMesh.RecalculateBounds();
            newMesh.name = combinedModel.name;
            renderer.rootBone = rootBone.transform;

            // Part transforms are baked into the vertices by CombineMeshes, so each bindpose maps from
            // the combined renderer's space (identity, at the scene origin) into bone space.
            Matrix4x4 rendererLocalToWorld = combinedSkinnedMesh.transform.localToWorldMatrix;
            Matrix4x4[] uniqueBindPoses = new Matrix4x4[uniqueBones.Length];
            for (int i = 0; i < uniqueBones.Length; i++)
            {
                uniqueBindPoses[i] = uniqueBones[i].worldToLocalMatrix * rendererLocalToWorld;
            }

            if (allBoneWeights.Count != newMesh.vertexCount)
            {
                Debug.LogError($"Combiner: bone weight count {allBoneWeights.Count} != combined vertex count {newMesh.vertexCount}.");
            }

            // Assign the remapped weights before shrinking the bindposes so no intermediate state has out-of-range bone indices.
            newMesh.boneWeights = allBoneWeights.ToArray();
            newMesh.bindposes = uniqueBindPoses;

            renderer.sharedMesh = newMesh;
            renderer.enabled = true;
            renderer.sharedMaterial = baseMaterial == null ? material : baseMaterial;
            MergeAndGetAllBlendShapeDataOfSkinnedMeshRenderers(skinnedMeshesToCombine.ToArray(), renderer.sharedMesh, renderer);

            return combinedModel;
        }

        /// <summary>
        ///     Processes the given GameObject and creates a model where each part keeps its own mesh, but all parts share a single
        ///     merged skeleton hierarchy, built the same way as the combined mesh path.
        /// </summary>
        /// <param name="partsToAdd">All of the part meshes to add to the model.</param>
        /// <param name="baseModel">The base model that has the base rig that the parts will be bound to.</param>
        /// <param name="baseMaterial">The base material to use for the model.</param>
        /// <returns>A new GameObject containing a renderer per part, all sharing a single skeleton.</returns>
        public static GameObject CreateSeparateSkinnedMeshes(
            List<SkinnedMeshRenderer> partsToAdd,
            GameObject baseModel,
            Material baseMaterial
        )
        {
            GameObject partsModel = new GameObject("Prefab Character");

            partsToAdd.Sort((a, b) => string.Compare(a.name, b.name));
            GameObject rootBone = BuildSkeleton(partsToAdd, baseModel, partsModel.transform, out Hashtable boneNameMap);

            foreach (SkinnedMeshRenderer part in partsToAdd)
            {
                GameObject newPart = new GameObject(part.name);
                newPart.transform.parent = partsModel.transform;
                SkinnedMeshRenderer renderer = newPart.AddComponent<SkinnedMeshRenderer>();
                renderer.updateWhenOffscreen = true;

                // Remap the part's bones to the shared skeleton by name, keeping the part's own bone order so the copied
                // bone weights and bindposes stay valid without remapping.
                Transform[] oldBones = part.bones;
                Transform[] newBones = new Transform[oldBones.Length];
                for (int i = 0; i < oldBones.Length; i++)
                {
                    Transform newBone = (Transform) boneNameMap[oldBones[i].name];
                    if (newBone == null)
                    {
                        Debug.LogWarning($"Combiner: bone '{oldBones[i].name}' on part '{part.name}' not found in rig; remapping to root.");
                        newBone = rootBone.transform;
                    }
                    newBones[i] = newBone;
                }

                renderer.sharedMesh = MeshUtils.CopyMesh(part.sharedMesh);
                Transform newRootBone = (Transform) boneNameMap[part.rootBone.name];
                renderer.rootBone = newRootBone == null ? rootBone.transform : newRootBone;

                // CopyMesh does not copy blend shapes, so re-add them from the source part.
                MergeAndGetAllBlendShapeDataOfSkinnedMeshRenderers(
                    new[]
                    {
                        part
                    },
                    renderer.sharedMesh,
                    renderer
                );

                renderer.bones = newBones;
                renderer.sharedMaterial = baseMaterial == null ? part.sharedMaterial : baseMaterial;
            }

            return partsModel;
        }

        /// <summary>
        ///     Builds the shared skeleton for a new character model by instantiating the base model's rig and adding any extra
        ///     bones required by the given parts.
        /// </summary>
        /// <param name="parts">The part meshes that will be bound to the skeleton.</param>
        /// <param name="baseModel">The base model that has the base rig.</param>
        /// <param name="characterRoot">The transform of the new character model the skeleton will be parented to.</param>
        /// <param name="boneNameMap">The output map between bone names and the bones of the new skeleton.</param>
        /// <returns>The root bone GameObject of the new skeleton.</returns>
        private static GameObject BuildSkeleton(
            List<SkinnedMeshRenderer> parts,
            GameObject baseModel,
            Transform characterRoot,
            out Hashtable boneNameMap
        )
        {
            Transform modelRootBone = baseModel.GetComponentInChildren<SkinnedMeshRenderer>().rootBone;
            GameObject rootBone = GameObject.Instantiate(modelRootBone.gameObject, characterRoot, true);
            rootBone.name = modelRootBone.name;
            boneNameMap = CreateBoneNameMap(rootBone);
            Transform[] additionalBones = FindAdditionalBones(boneNameMap, new List<SkinnedMeshRenderer>(parts));
            if (additionalBones.Length > 0)
            {
                JoinAdditionalBonesToBoneArray(Array.Empty<Transform>(), additionalBones, boneNameMap);
                // Need to redo the name map now that we have updated the bone array.
                boneNameMap = CreateBoneNameMap(rootBone);
            }

            return rootBone;
        }

        /// <summary>
        ///     Collects the bones used by the combined parts in rig hierarchy (pre-order depth-first) order, one entry per unique bone name.
        ///     Traversal order and first-wins duplicate name handling match <see cref="CreateBoneNameMap" /> so each indexed
        ///     Transform is the same instance the bone name map resolves to.
        /// </summary>
        /// <param name="current">The current bone being processed.</param>
        /// <param name="usedBoneNames">The names of all bones used by the parts being combined.</param>
        /// <param name="orderedBones">The output list of unique bones, in hierarchy order.</param>
        /// <param name="boneIndexByName">The output map from bone name to its index in <paramref name="orderedBones" />.</param>
        private static void CollectUsedBonesDepthFirst(
            Transform current,
            HashSet<string> usedBoneNames,
            List<Transform> orderedBones,
            Dictionary<string, int> boneIndexByName
        )
        {
            if (usedBoneNames.Contains(current.name) && !boneIndexByName.ContainsKey(current.name))
            {
                boneIndexByName.Add(current.name, orderedBones.Count);
                orderedBones.Add(current);
            }

            // Recurse into all children regardless; a used bone can sit under an unused one.
            for (int i = 0; i < current.childCount; i++)
            {
                CollectUsedBonesDepthFirst(current.GetChild(i), usedBoneNames, orderedBones, boneIndexByName);
            }
        }

        /// <summary>
        ///     Remaps the bone indices of the given bone weights using the given index remap table.
        /// </summary>
        /// <param name="weights">The bone weights to remap.</param>
        /// <param name="indexRemap">A table mapping old bone indices to new bone indices.</param>
        /// <returns>A new array of bone weights with remapped bone indices.</returns>
        private static BoneWeight[] RemapBoneWeights(BoneWeight[] weights, int[] indexRemap)
        {
            BoneWeight[] remappedWeights = new BoneWeight[weights.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                BoneWeight weight = weights[i];
                weight.boneIndex0 = indexRemap[weight.boneIndex0];
                weight.boneIndex1 = indexRemap[weight.boneIndex1];
                weight.boneIndex2 = indexRemap[weight.boneIndex2];
                weight.boneIndex3 = indexRemap[weight.boneIndex3];
                remappedWeights[i] = weight;
            }
            return remappedWeights;
        }

        /// <summary>
        ///     Processes the movement of bones if required for the given movement dictionary.
        /// </summary>
        /// <param name="boneNameMap">The bone name map that has all the bones of the rig.</param>
        /// <param name="movementDictionary">The dictionary of bones to process the movement from.</param>
        /// <param name="rotationDictionary">The dictionary of bone rotations to process.</param>
        public static void ProcessBoneMovement(Hashtable boneNameMap, Dictionary<string, Vector3> movementDictionary, Dictionary<string, Quaternion> rotationDictionary)
        {
            Dictionary<string, Vector3> bonePositionDictionary = new Dictionary<string, Vector3>();
            Dictionary<string, Quaternion> boneRotationDictionary = new Dictionary<string, Quaternion>();
            Dictionary<string, Vector3> boneMovementDictionary = new Dictionary<string, Vector3>();
            foreach (Transform currentBone in boneNameMap.Values)
            {
                // Store bone positions from rig before processing joints.
                bonePositionDictionary.TryAdd(currentBone.name, currentBone.transform.localPosition);
                boneRotationDictionary.TryAdd(currentBone.name, currentBone.transform.localRotation);

                if (movementDictionary.ContainsKey(currentBone.name))
                {
                    float jointDistance = Vector3.Distance(bonePositionDictionary[currentBone.name], movementDictionary[currentBone.name]);
                    float rotationDistance = Quaternion.Angle(boneRotationDictionary[currentBone.name], rotationDictionary[currentBone.name]);

                    // If the bone in the new part is at a different location, move the actual bone to the same position.
                    if (jointDistance > 0.0001)
                    {
                        Vector3 rigMovement = movementDictionary[currentBone.name];
                        // If an existing joint movement exists, and is further from the standard joint position, use that instead.
                        if (boneMovementDictionary.TryGetValue(currentBone.name, out Vector3 existingMovement)
                            && Math.Abs(Vector3.Distance(bonePositionDictionary[currentBone.name], existingMovement)) > Math.Abs(jointDistance))
                        {
                            rigMovement = existingMovement;
                        }

                        currentBone.transform.localPosition = rigMovement;
                        boneMovementDictionary[currentBone.name] = rigMovement;
                    }

                    if (rotationDistance > 0.01)
                    {
                        Quaternion rigRotation = rotationDictionary[currentBone.name];
                        if (boneRotationDictionary.TryGetValue(currentBone.name, out Quaternion existingRotation)
                            && Math.Abs(Quaternion.Angle(boneRotationDictionary[currentBone.name], existingRotation)) > Math.Abs(rotationDistance))
                        {
                            rigRotation = existingRotation;
                        }

                        currentBone.transform.localRotation = rigRotation;
                        boneRotationDictionary[currentBone.name] = rigRotation;
                    }
                }
            }
        }

        /// <summary>
        ///     Creates a map between bones and their names.
        /// </summary>
        /// <param name="currentBone">The Current bone being mapped.</param>
        /// <returns>A hashmap between bone names and bones.</returns>
        public static Hashtable CreateBoneNameMap(GameObject currentBone)
        {
            Hashtable boneNameMap = new Hashtable();
            boneNameMap.Add(currentBone.name, currentBone.transform);

            for (int i = 0; i < currentBone.transform.childCount; i++)
            {
                Hashtable childBoneMap = CreateBoneNameMap(currentBone.transform.GetChild(i).gameObject);
                foreach (DictionaryEntry entry in childBoneMap)
                {
                    if (!boneNameMap.ContainsKey(entry.Key))
                    {
                        boneNameMap.Add(entry.Key, (Transform) entry.Value);
                    }
                }
            }
            return boneNameMap;
        }

        /// <summary>
        ///     Finds any bones in a given list of SkinnedMeshRenderers that aren't in the given bone map.
        /// </summary>
        /// <param name="boneMap">The bone map to check for the existence of bones.</param>
        /// <param name="meshes">The list of SkinnedMeshRenderers to check for additional bones.</param>
        /// <returns>An array of all additional bones.</returns>
        public static Transform[] FindAdditionalBones(Hashtable boneMap, List<SkinnedMeshRenderer> meshes)
        {
            List<Transform> newBones = new List<Transform>();
            foreach (SkinnedMeshRenderer mesh in meshes)
            {
                foreach (Transform bone in mesh.bones)
                {
                    if (!boneMap.ContainsKey(bone.name))
                    {
                        newBones.Add(bone);
                    }
                }
            }
            return newBones.ToArray();
        }

        /// <summary>
        ///     Adds additional bones to the current bone array.
        /// </summary>
        /// <param name="bones">The current bone array.</param>
        /// <param name="additionBones">The new bones to add.</param>
        /// <param name="boneMap">The current bone name map.</param>
        /// <returns>The new bone array.</returns>
        public static Transform[] JoinAdditionalBonesToBoneArray(Transform[] bones, Transform[] additionBones, Hashtable boneMap)
        {
            List<Transform> fullBones = new List<Transform>();
            fullBones.AddRange(bones);
            foreach (Transform bone in additionBones)
            {
                Transform newParent = (Transform) boneMap[bone.parent.name];

                if (newParent != null && !newParent.Find(bone.name))
                {
                    GameObject newBone = GameObject.Instantiate(bone.gameObject, newParent);
                    newBone.name = newBone.name.Replace("(Clone)", "");
                    fullBones.Add(newBone.transform);
                    if (!boneMap.ContainsKey(bone.name))
                    {
                        boneMap.Add(bone.name, newBone.transform);
                    }
                }
            }
            return fullBones.ToArray();
        }
    }
}
