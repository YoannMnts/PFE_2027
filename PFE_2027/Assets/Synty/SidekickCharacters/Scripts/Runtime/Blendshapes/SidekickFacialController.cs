// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Synty.SidekickCharacters
{
    [ExecuteInEditMode]
    public class SidekickFacialController : MonoBehaviour
    {
        [Range(-1, 1)]
        public float _ebrlFrownBlendValue = 0f;
        [Range(-1, 1)]
        public float _ebrlInnerBlendValue = 0f;
        [Range(-1, 1)]
        public float _ebrlOuterBlendValue = 0f;
        [Range(-1, 1)]
        public float _ebrrFrownBlendValue = 0f;
        [Range(-1, 1)]
        public float _ebrrInnerBlendValue = 0f;
        [Range(-1, 1)]
        public float _ebrrOuterBlendValue = 0f;
        [Range(-1, 1)]
        public float _eyelUpDownBlendValue = 0f;
        [Range(-1, 1)]
        public float _eyelLeftRightBlendValue = 0f;
        [Range(0, 1)]
        public float _eyelBlinkBlendValue = 0f;
        [Range(0, 1)]
        public float _eyelSquintBlendValue = 0f;
        [Range(0, 1)]
        public float _eyelWideBlendValue = 0f;
        [Range(-1, 1)]
        public float _eyerUpDownBlendValue = 0f;
        [Range(-1, 1)]
        public float _eyerLeftRightBlendValue = 0f;
        [Range(0, 1)]
        public float _eyerBlinkBlendValue = 0f;
        [Range(0, 1)]
        public float _eyerSquintBlendValue = 0f;
        [Range(0, 1)]
        public float _eyerWideBlendValue = 0f;
        [Range(0, 1)]
        public float _noslSneerBlendValue = 0f;
        [Range(0, 1)]
        public float _nosrSneerBlendValue = 0f;
        [Range(0, 1)]
        public float _chklHollowPuffBlendValue = 0f;
        [Range(0, 1)]
        public float _chkrHollowPuffBlendValue = 0f;
        [Range(-1, 1)]
        public float _jawOpenCloseBlendValue = 0f;
        [Range(-1, 1)]
        public float _jawLeftRightBlendValue = 0f;
        [Range(-1, 1)]
        public float _jawBackForwardBlendValue = 0f;
        [Range(-1, 1)]
        public float _mthLeftRightBlendValue = 0f;
        [Range(0, 1)]
        public float _mthFunnelBlendValue = 0f;
        [Range(0, 1)]
        public float _mthPuckerBlendValue = 0f;
        [Range(0, 1)]
        public float _mthShrugUpperBlendValue = 0f;
        [Range(0, 1)]
        public float _mthShrugLowerBlendValue = 0f;
        [Range(0, 1)]
        public float _mthRollUpperBlendValue = 0f;
        [Range(0, 1)]
        public float _mthRollOutUpperBlendValue = 0f;
        [Range(0, 1)]
        public float _mthRollLowerBlendValue = 0f;
        [Range(0, 1)]
        public float _mthRollOutLowerBlendValue = 0f;
        [Range(0, 1)]
        public float _mthCloseBlendValue = 0f;
        [Range(0, 1)]
        public float _mthlFrownSmileBlendValue = 0f;
        [Range(0, 1)]
        public float _mthrFrownSmileBlendValue = 0f;
        [Range(0, 1)]
        public float _mthlPressStretchBlendValue = 0f;
        [Range(0, 1)]
        public float _mthrPressStretchBlendValue = 0f;
        [Range(0, 1)]
        public float _mthlUpperUpBlendValue = 0f;
        [Range(0, 1)]
        public float _mthrUpperUpBlendValue = 0f;
        [Range(0, 1)]
        public float _mthlLowerDownBlendValue = 0f;
        [Range(0, 1)]
        public float _mthrLowerDownBlendValue = 0f;
        [Range(-1, 1)]
        public float _tongDownUpBlendValue = 0f;
        [Range(-1, 1)]
        public float _tongInOutBlendValue = 0f;
        [Range(-1, 1)]
        public float _tongLowerRaiseBlendValue = 0f;
        [Range(-1, 1)]
        public float _tongTwistLeftRightBlendValue = 0f;
        [Range(-1, 1)]
        public float _tongCurlDownUpBlendValue = 0f;
        [Range(-1, 1)]
        public float _tongCurlLeftRightBlendValue = 0f;
        [Range(-1, 1)]
        public float _tongCurlSideDownUpBlendValue = 0f;

        // Change this string to set the default preset directory
        public string _presetDirectory = "Assets";
        public string _saveName;
        public List<string> _presetNames;
        public string _selectedPreset = "None";
        [NonSerialized]
        public Dictionary<string, string> _presetFileDictionary = new Dictionary<string, string>();

        [NonSerialized]
        private List<SkinnedMeshRenderer> _meshRenderers = new List<SkinnedMeshRenderer>();
        [NonSerialized]
        private Dictionary<SkinnedMeshRenderer, Dictionary<string, int>> _blendshapeIndices;
        [NonSerialized]
        private Transform _eyeLBone;
        [NonSerialized]
        private Transform _eyeRBone;

        private readonly Vector3 _eyeLOriginalRotation = new Vector3(0.024f, 270f, 90f);
        private readonly Vector3 _eyeROriginalRotation = new Vector3(0.024f, 270f, 90f);

        private readonly Vector3 _eyeLDown = new Vector3(13.5f, 270f, 90f);
        private readonly Vector3 _eyeLUp = new Vector3(-9.5f, 270f, 90f);
        private readonly Vector3 _eyeLLeft = new Vector3(0f, 270f, 68f);
        private readonly Vector3 _eyeLRight = new Vector3(0f, 270f, 105f);
        private readonly Vector3 _eyeRDown = new Vector3(13.5f, 270f, 90f);
        private readonly Vector3 _eyeRUp = new Vector3(-9.5f, 270f, 90f);
        private readonly Vector3 _eyeRLeft = new Vector3(0f, 270f, 105f);
        private readonly Vector3 _eyeRRight = new Vector3(0f, 270f, 68f);

        private readonly List<string> _variableNames = new List<string>
        {
            "ebrlFrownBlendValue",
            "ebrlInnerBlendValue",
            "ebrlOuterBlendValue",
            "ebrrFrownBlendValue",
            "ebrrInnerBlendValue",
            "ebrrOuterBlendValue",
            "eyelUpDownBlendValue",
            "eyelLeftRightBlendValue",
            "eyelBlinkBlendValue",
            "eyelSquintBlendValue",
            "eyelWideBlendValue",
            "eyerUpDownBlendValue",
            "eyerLeftRightBlendValue",
            "eyerBlinkBlendValue",
            "eyerSquintBlendValue",
            "eyerWideBlendValue",
            "noslSneerBlendValue",
            "nosrSneerBlendValue",
            "chklHollowPuffBlendValue",
            "chkrHollowPuffBlendValue",
            "jawOpenCloseBlendValue",
            "jawLeftRightBlendValue",
            "jawBackForwardBlendValue",
            "mthLeftRightBlendValue",
            "mthFunnelBlendValue",
            "mthPuckerBlendValue",
            "mthShrugUpperBlendValue",
            "mthShrugLowerBlendValue",
            "mthRollUpperBlendValue",
            "mthRollOutUpperBlendValue",
            "mthRollLowerBlendValue",
            "mthRollOutLowerBlendValue",
            "mthCloseBlendValue",
            "mthlFrownSmileBlendValue",
            "mthrFrownSmileBlendValue",
            "mthlPressStretchBlendValue",
            "mthrPressStretchBlendValue",
            "mthlUpperUpBlendValue",
            "mthrUpperUpBlendValue",
            "mthlLowerDownBlendValue",
            "mthrLowerDownBlendValue",
            "tongDownUpBlendValue",
            "tongInOutBlendValue",
            "tongLowerRaiseBlendValue",
            "tongTwistLeftRightBlendValue",
            "tongCurlDownUpBlendValue",
            "tongCurlLeftRightBlendValue",
            "tongCurlSideDownUpBlendValue"
        };

        public SidekickFacialController() {}

        private void OnEnable()
        {
            UpdateMeshReference();
            CacheBlendshapeIndices();
            ReadBlendshapeValues();
        }

        public void UpdateMeshReference()
        {
            _meshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
            _eyeLBone = gameObject.GetComponentsInChildren<Transform>().First(t => t.name == "eye_l");
            _eyeRBone = gameObject.GetComponentsInChildren<Transform>().First(t => t.name == "eye_r");
        }

        private void OnValidate()
        {
            ApplyBlendshapes();
        }

        public void CacheBlendshapeIndices()
        {
            _blendshapeIndices = new Dictionary<SkinnedMeshRenderer, Dictionary<string, int>>();
            if (_meshRenderers.Count < 1)
            {
                return;
            }

            foreach (SkinnedMeshRenderer meshRenderer in _meshRenderers)
            {
                if (meshRenderer == null || meshRenderer.sharedMesh == null)
                {
                    continue;
                }

                Dictionary<string, int> currentIndices = new Dictionary<string, int>();

                Mesh mesh = meshRenderer.sharedMesh;
                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    string blendName = mesh.GetBlendShapeName(i);
                    currentIndices[blendName] = i;
                }

                _blendshapeIndices[meshRenderer] = currentIndices;
            }
        }

        public void SetBlendValue(string blendName, float value)
        {
            foreach (SkinnedMeshRenderer meshRenderer in _meshRenderers)
            {
                if (_blendshapeIndices.TryGetValue(meshRenderer, out Dictionary<string, int> currentIndices))
                {
                    if (currentIndices.TryGetValue(blendName, out int index))
                    {
                        meshRenderer.SetBlendShapeWeight(index, Mathf.Clamp01(value) * 100f);
                    }
                }
            }
        }

        public void ApplyBlendshapes()
        {
            Vector3 eyeLeftRotation = _eyeLOriginalRotation;
            Vector3 eyeRightRotation = _eyeROriginalRotation;

            foreach (SkinnedMeshRenderer meshRenderer in _meshRenderers)
            {
                if (meshRenderer == null)
                {
                    continue;
                }

                if (_blendshapeIndices == null || _eyeLBone == null || _eyeRBone == null)
                {
                    UpdateMeshReference();
                    CacheBlendshapeIndices();
                }

                if (_blendshapeIndices != null && _blendshapeIndices.TryGetValue(meshRenderer, out Dictionary<string, int> currentIndices))
                {
                    foreach (KeyValuePair<string, int> pair in currentIndices)
                    {
                        string blendName = pair.Key;
                        int index = blendName.LastIndexOf('.');
                        string blendEnd = blendName.Substring(index, blendName.Length - index);
                        float weight = meshRenderer.GetBlendShapeWeight(pair.Value);
                        bool applyBlendChange = false;

                        #region BLEND_ACTIONS
                        switch (blendEnd)
                        {
                            case ".browFrownLeft":
                                weight = _ebrlFrownBlendValue < 0 ? -_ebrlFrownBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".browRaiseLeft":
                                weight = _ebrlFrownBlendValue > 0 ? _ebrlFrownBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".browInnerDownLeft":
                                weight = _ebrlInnerBlendValue < 0 ? -_ebrlInnerBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".browInnerUpLeft":
                                weight = _ebrlInnerBlendValue > 0 ? _ebrlInnerBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".browOuterDownLeft":
                                weight = _ebrlOuterBlendValue < 0 ? -_ebrlOuterBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".browOuterUpLeft":
                                weight = _ebrlOuterBlendValue > 0 ? _ebrlOuterBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".browFrownRight":
                                weight = _ebrrFrownBlendValue < 0 ? -_ebrrFrownBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".browRaiseRight":
                                weight = _ebrrFrownBlendValue > 0 ? _ebrrFrownBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".browInnerDownRight":
                                weight = _ebrrInnerBlendValue < 0 ? -_ebrrInnerBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".browInnerUpRight":
                                weight = _ebrrInnerBlendValue > 0 ? _ebrrInnerBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".browOuterDownRight":
                                weight = _ebrrOuterBlendValue < 0 ? -_ebrrOuterBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".browOuterUpRight":
                                weight = _ebrrOuterBlendValue > 0 ? _ebrrOuterBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".eyeLookDownLeft":
                                weight = _eyelUpDownBlendValue < 0 ? -_eyelUpDownBlendValue : 0;
                                if (_eyelUpDownBlendValue <= 0)
                                {
                                    eyeLeftRotation += Vector3.Lerp(_eyeLOriginalRotation, _eyeLDown, weight) - _eyeLOriginalRotation;
                                }
                                applyBlendChange = true;
                                break;
                            case ".eyeLookUpLeft":
                                weight = _eyelUpDownBlendValue > 0 ? _eyelUpDownBlendValue : 0;
                                if (_eyelUpDownBlendValue > 0)
                                {
                                    eyeLeftRotation += Vector3.Lerp(_eyeLOriginalRotation, _eyeLUp, weight) - _eyeLOriginalRotation;
                                }
                                applyBlendChange = true;
                                break;
                            case ".eyeLookOutLeft":
                                weight = _eyelLeftRightBlendValue < 0 ? -_eyelLeftRightBlendValue : 0;
                                if (_eyelLeftRightBlendValue <= 0)
                                {
                                    eyeLeftRotation += Vector3.Lerp(_eyeLOriginalRotation, _eyeLLeft, weight) - _eyeLOriginalRotation;
                                }
                                applyBlendChange = true;
                                break;
                            case ".eyeLookInLeft":
                                weight = _eyelLeftRightBlendValue > 0 ? _eyelLeftRightBlendValue : 0;
                                if (_eyelLeftRightBlendValue > 0)
                                {
                                    eyeLeftRotation += Vector3.Lerp(_eyeLOriginalRotation, _eyeLRight, weight) - _eyeLOriginalRotation;
                                }
                                applyBlendChange = true;
                                break;
                            case ".eyeBlinkUpperLeft":
                                weight = _eyelBlinkBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".eyeBlinkLowerLeft":
                                weight = _eyelBlinkBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".eyeSquintLeft":
                                weight = _eyelSquintBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".cheekSquintLeft":
                                weight = _eyelSquintBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".eyeWideUpperLeft":
                                weight = _eyelWideBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".eyeWideLowerLeft":
                                weight = _eyelWideBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".eyeLookDownRight":
                                weight = _eyerUpDownBlendValue < 0 ? -_eyerUpDownBlendValue : 0;
                                if (_eyerUpDownBlendValue <= 0)
                                {
                                    eyeRightRotation += Vector3.Lerp(_eyeROriginalRotation, _eyeRDown, weight) - _eyeROriginalRotation;
                                }
                                applyBlendChange = true;
                                break;
                            case ".eyeLookUpRight":
                                weight = _eyerUpDownBlendValue > 0 ? _eyerUpDownBlendValue : 0;
                                if (_eyerUpDownBlendValue > 0)
                                {
                                    eyeRightRotation += Vector3.Lerp(_eyeROriginalRotation, _eyeRUp, weight) - _eyeROriginalRotation;
                                }
                                applyBlendChange = true;
                                break;
                            case ".eyeLookOutRight":
                                weight = _eyerLeftRightBlendValue > 0 ? _eyerLeftRightBlendValue : 0;
                                if (_eyerLeftRightBlendValue > 0)
                                {
                                    eyeRightRotation += Vector3.Lerp(_eyeROriginalRotation, _eyeRLeft, weight) - _eyeROriginalRotation;
                                }
                                applyBlendChange = true;
                                break;
                            case ".eyeLookInRight":
                                weight = _eyerLeftRightBlendValue < 0 ? -_eyerLeftRightBlendValue : 0;
                                if (_eyerLeftRightBlendValue <= 0)
                                {
                                    eyeRightRotation += Vector3.Lerp(_eyeLOriginalRotation, _eyeRRight, weight) - _eyeROriginalRotation;
                                }
                                applyBlendChange = true;
                                break;
                            case ".eyeBlinkUpperRight":
                                weight = _eyerBlinkBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".eyeBlinkLowerRight":
                                weight = _eyerBlinkBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".eyeSquintRight":
                                weight = _eyerSquintBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".cheekSquintRight":
                                weight = _eyerSquintBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".eyeWideUpperRight":
                                weight = _eyerWideBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".eyeWideLowerRight":
                                weight = _eyerWideBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".noseSneerLeft":
                                weight = _noslSneerBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".noseSneerRight":
                                weight = _nosrSneerBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".cheekHollowLeft":
                                weight = _chklHollowPuffBlendValue < 0 ? -_chklHollowPuffBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".cheekPuffLeft":
                                weight = _chklHollowPuffBlendValue > 0 ? _chklHollowPuffBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".cheekHollowRight":
                                weight = _chkrHollowPuffBlendValue < 0 ? -_chkrHollowPuffBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".cheekPuffRight":
                                weight = _chkrHollowPuffBlendValue > 0 ? _chkrHollowPuffBlendValue : 0;;
                                applyBlendChange = true;
                                break;
                            case ".jawClose":
                                weight = _jawOpenCloseBlendValue < 0 ? -_jawOpenCloseBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".jawOpen":
                                weight = _jawOpenCloseBlendValue > 0 ? _jawOpenCloseBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".jawLeft":
                                weight = _jawLeftRightBlendValue < 0 ? -_jawLeftRightBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".jawRight":
                                weight = _jawLeftRightBlendValue > 0 ? _jawLeftRightBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".jawBackward":
                                weight = _jawBackForwardBlendValue < 0 ? -_jawBackForwardBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".jawForward":
                                weight = _jawBackForwardBlendValue > 0 ? _jawBackForwardBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".mouthLeft":
                                weight = _mthLeftRightBlendValue < 0 ? -_mthLeftRightBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".mouthRight":
                                weight = _mthLeftRightBlendValue > 0 ? _mthLeftRightBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".mouthFunnel":
                                weight = _mthFunnelBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".mouthPucker":
                                weight = _mthPuckerBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".mouthShrugUpper":
                                weight = _mthShrugUpperBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".mouthShrugLower":
                                weight = _mthShrugLowerBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".mouthRollUpper":
                                weight = _mthRollUpperBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".mouthRollOutUpper":
                                weight = _mthRollOutUpperBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".mouthRollLower":
                                weight = _mthRollLowerBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".mouthRollOutLower":
                                weight = _mthRollOutLowerBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".mouthClose":
                                weight = _mthCloseBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".mouthFrownLeft":
                                weight = _mthlFrownSmileBlendValue < 0 ? -_mthlFrownSmileBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".mouthSmileLeft":
                                weight = _mthlFrownSmileBlendValue > 0 ? _mthlFrownSmileBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".mouthPressLeft":
                                weight = _mthlPressStretchBlendValue < 0 ? -_mthlPressStretchBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".mouthStretchLeft":
                                weight = _mthlPressStretchBlendValue > 0 ? _mthlPressStretchBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".mouthUpperUpLeft":
                                weight = _mthlUpperUpBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".mouthLowerDownLeft":
                                weight = _mthlLowerDownBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".mouthFrownRight":
                                weight = _mthrFrownSmileBlendValue < 0 ? -_mthrFrownSmileBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".mouthSmileRight":
                                weight = _mthrFrownSmileBlendValue > 0 ? _mthrFrownSmileBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".mouthPressRight":
                                weight = _mthrPressStretchBlendValue < 0 ? -_mthrPressStretchBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".mouthStretchRight":
                                weight = _mthrPressStretchBlendValue > 0 ? _mthrPressStretchBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".mouthUpperUpRight":
                                weight = _mthrUpperUpBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".mouthLowerDownRight":
                                weight = _mthrLowerDownBlendValue;
                                applyBlendChange = true;
                                break;
                            case ".tongueDown":
                                weight = _tongDownUpBlendValue < 0 ? -_tongDownUpBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".tongueUp":
                                weight = _tongDownUpBlendValue > 0 ? _tongDownUpBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".tongueIn":
                                weight = _tongInOutBlendValue < 0 ? -_tongInOutBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".tongueOut":
                                weight = _tongInOutBlendValue > 0 ? _tongInOutBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".tongueLower":
                                weight = _tongLowerRaiseBlendValue < 0 ? -_tongLowerRaiseBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".tongueRaise":
                                weight = _tongLowerRaiseBlendValue > 0 ? _tongLowerRaiseBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".tongueTwistLeft":
                                weight = _tongTwistLeftRightBlendValue < 0 ? -_tongTwistLeftRightBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".tongueTwistRight":
                                weight = _tongTwistLeftRightBlendValue > 0 ? _tongTwistLeftRightBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".tongueCurlDown":
                                weight = _tongCurlDownUpBlendValue < 0 ? -_tongCurlDownUpBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".tongueCurlUp":
                                weight = _tongCurlDownUpBlendValue > 0 ? _tongCurlDownUpBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".tongueCurlLeft":
                                weight = _tongCurlLeftRightBlendValue < 0 ? -_tongCurlLeftRightBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".tongueCurlRight":
                                weight = _tongCurlLeftRightBlendValue > 0 ? _tongCurlLeftRightBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".tongueSideCurlDown":
                                weight = _tongCurlSideDownUpBlendValue < 0 ? -_tongCurlSideDownUpBlendValue : 0;
                                applyBlendChange = true;
                                break;
                            case ".tongueSideCurlUp":
                                weight = _tongCurlSideDownUpBlendValue > 0 ? _tongCurlSideDownUpBlendValue : 0;
                                applyBlendChange = true;
                                break;
                        }
                        #endregion

                        if (applyBlendChange)
                        {
                            SetBlendValue(blendName, weight);
                        }
                    }
                }
            }

            if (_eyeLBone != null)
            {
                _eyeLBone.localRotation = Quaternion.Euler(eyeLeftRotation);
            }

            if (_eyeRBone != null)
            {
                _eyeRBone.localRotation = Quaternion.Euler(eyeRightRotation);
            }
        }

        public void ReadBlendshapeValues()
        {
            ResetAllSliders();

            foreach (SkinnedMeshRenderer meshRenderer in _meshRenderers)
            {
                if (meshRenderer == null)
                {
                    continue;
                }

                if (_blendshapeIndices == null || _eyeLBone == null || _eyeRBone == null)
                {
                    UpdateMeshReference();
                    CacheBlendshapeIndices();
                }

                if (_blendshapeIndices != null && _blendshapeIndices.TryGetValue(meshRenderer, out Dictionary<string, int> currentIndices))
                {
                    foreach (KeyValuePair<string, int> pair in currentIndices)
                    {
                        string blendName = pair.Key;
                        int index = blendName.LastIndexOf('.');
                        string blendEnd = blendName.Substring(index, blendName.Length - index);
                        float weight = meshRenderer.GetBlendShapeWeight(pair.Value) / 100f;

                        #region BLEND_ACTIONS

                        switch (blendEnd)
                        {
                            case ".browFrownLeft":
                                if (weight > 0)
                                {
                                    if (_ebrlFrownBlendValue != 0)
                                    {
                                        float newWeight = (_ebrlFrownBlendValue + -weight) / 2f;
                                        _ebrlFrownBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _ebrlFrownBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".browRaiseLeft":
                                if (weight > 0)
                                {
                                    if (_ebrlFrownBlendValue != 0)
                                    {
                                        float newWeight = (_ebrlFrownBlendValue + weight) / 2f;
                                        _ebrlFrownBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _ebrlFrownBlendValue = weight;
                                    }
                                }

                                break;
                            case ".browInnerDownLeft":
                                if (weight > 0)
                                {
                                    if (_ebrlInnerBlendValue != 0)
                                    {
                                        float newWeight = (_ebrlInnerBlendValue + -weight) / 2f;
                                        _ebrlInnerBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _ebrlInnerBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".browInnerUpLeft":
                                if (weight > 0)
                                {
                                    if (_ebrlInnerBlendValue != 0)
                                    {
                                        float newWeight = (_ebrlInnerBlendValue + weight) / 2f;
                                        _ebrlInnerBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _ebrlInnerBlendValue = weight;
                                    }
                                }

                                break;
                            case ".browOuterDownLeft":
                                if (weight > 0)
                                {
                                    if (_ebrlOuterBlendValue != 0)
                                    {
                                        float newWeight = (_ebrlOuterBlendValue + -weight) / 2f;
                                        _ebrlOuterBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _ebrlOuterBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".browOuterUpLeft":
                                if (weight > 0)
                                {
                                    if (_ebrlOuterBlendValue != 0)
                                    {
                                        float newWeight = (_ebrlOuterBlendValue + weight) / 2f;
                                        _ebrlOuterBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _ebrlOuterBlendValue = weight;
                                    }
                                }

                                break;
                            case ".browFrownRight":
                                if (weight > 0)
                                {
                                    if (_ebrrFrownBlendValue != 0)
                                    {
                                        float newWeight = (_ebrrFrownBlendValue + -weight) / 2f;
                                        _ebrrFrownBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _ebrrFrownBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".browRaiseRight":
                                if (weight > 0)
                                {
                                    if (_ebrrFrownBlendValue != 0)
                                    {
                                        float newWeight = (_ebrrFrownBlendValue + weight) / 2f;
                                        _ebrrFrownBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _ebrrFrownBlendValue = weight;
                                    }
                                }

                                break;
                            case ".browInnerDownRight":
                                if (weight > 0)
                                {
                                    if (_ebrrInnerBlendValue != 0)
                                    {
                                        float newWeight = (_ebrrInnerBlendValue + -weight) / 2f;
                                        _ebrrInnerBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _ebrrInnerBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".browInnerUpRight":
                                if (weight > 0)
                                {
                                    if (_ebrrInnerBlendValue != 0)
                                    {
                                        float newWeight = (_ebrrInnerBlendValue + weight) / 2f;
                                        _ebrrInnerBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _ebrrInnerBlendValue = weight;
                                    }
                                }

                                break;
                            case ".browOuterDownRight":
                                if (weight > 0)
                                {
                                    if (_ebrrOuterBlendValue != 0)
                                    {
                                        float newWeight = (_ebrrOuterBlendValue + -weight) / 2f;
                                        _ebrrOuterBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _ebrrOuterBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".browOuterUpRight":
                                if (weight > 0)
                                {
                                    if (_ebrrOuterBlendValue != 0)
                                    {
                                        float newWeight = (_ebrrOuterBlendValue + weight) / 2f;
                                        _ebrrOuterBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _ebrrOuterBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeLookDownLeft":
                                if (weight > 0)
                                {
                                    if (_eyelUpDownBlendValue != 0)
                                    {
                                        float newWeight = (_eyelUpDownBlendValue + -weight) / 2f;
                                        _eyelUpDownBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyelUpDownBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".eyeLookUpLeft":
                                if (weight > 0)
                                {
                                    if (_eyelUpDownBlendValue != 0)
                                    {
                                        float newWeight = (_eyelUpDownBlendValue + weight) / 2f;
                                        _eyelUpDownBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyelUpDownBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeLookOutLeft":
                                if (weight > 0)
                                {
                                    if (_eyelLeftRightBlendValue != 0)
                                    {
                                        float newWeight = (_eyelLeftRightBlendValue + -weight) / 2f;
                                        _eyelLeftRightBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyelLeftRightBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".eyeLookInLeft":
                                if (weight > 0)
                                {
                                    if (_eyelLeftRightBlendValue != 0)
                                    {
                                        float newWeight = (_eyelLeftRightBlendValue + weight) / 2f;
                                        _eyelLeftRightBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyelLeftRightBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeBlinkUpperLeft":
                                if (weight > 0)
                                {
                                    if (_eyelBlinkBlendValue != 0)
                                    {
                                        float newWeight = (_eyelBlinkBlendValue + weight) / 2f;
                                        _eyelBlinkBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyelBlinkBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeBlinkLowerLeft":
                                if (weight > 0)
                                {
                                    if (_eyelBlinkBlendValue != 0)
                                    {
                                        float newWeight = (_eyelBlinkBlendValue + weight) / 2f;
                                        _eyelBlinkBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyelBlinkBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeSquintLeft":
                                if (weight > 0)
                                {
                                    if (_eyelSquintBlendValue != 0)
                                    {
                                        float newWeight = (_eyelSquintBlendValue + weight) / 2f;
                                        _eyelSquintBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyelSquintBlendValue = weight;
                                    }
                                }

                                break;
                            case ".cheekSquintLeft":
                                if (weight > 0)
                                {
                                    if (_eyelSquintBlendValue != 0)
                                    {
                                        float newWeight = (_eyelSquintBlendValue + weight) / 2f;
                                        _eyelSquintBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyelSquintBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeWideUpperLeft":
                                if (weight > 0)
                                {
                                    if (_eyelWideBlendValue != 0)
                                    {
                                        float newWeight = (_eyelWideBlendValue + weight) / 2f;
                                        _eyelWideBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyelWideBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeWideLowerLeft":
                                if (weight > 0)
                                {
                                    if (_eyelWideBlendValue != 0)
                                    {
                                        float newWeight = (_eyelWideBlendValue + weight) / 2f;
                                        _eyelWideBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyelWideBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeLookDownRight":
                                if (weight > 0)
                                {
                                    if (_eyerUpDownBlendValue != 0)
                                    {
                                        float newWeight = (_eyerUpDownBlendValue + -weight) / 2f;
                                        _eyerUpDownBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyerUpDownBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".eyeLookUpRight":
                                if (weight > 0)
                                {
                                    if (_eyerUpDownBlendValue != 0)
                                    {
                                        float newWeight = (_eyerUpDownBlendValue + weight) / 2f;
                                        _eyerUpDownBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyerUpDownBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeLookOutRight":
                                if (weight > 0)
                                {
                                    if (_eyerLeftRightBlendValue != 0)
                                    {
                                        float newWeight = (_eyerLeftRightBlendValue + weight) / 2f;
                                        _eyerLeftRightBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyerLeftRightBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeLookInRight":
                                if (weight > 0)
                                {
                                    if (_eyerLeftRightBlendValue != 0)
                                    {
                                        float newWeight = (_eyerLeftRightBlendValue + -weight) / 2f;
                                        _eyerLeftRightBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyerLeftRightBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".eyeBlinkUpperRight":
                                if (weight > 0)
                                {
                                    if (_eyerBlinkBlendValue != 0)
                                    {
                                        float newWeight = (_eyerBlinkBlendValue + weight) / 2f;
                                        _eyerBlinkBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyerBlinkBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeBlinkLowerRight":
                                if (weight > 0)
                                {
                                    if (_eyerBlinkBlendValue != 0)
                                    {
                                        float newWeight = (_eyerBlinkBlendValue + weight) / 2f;
                                        _eyerBlinkBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyerBlinkBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeSquintRight":
                                if (weight > 0)
                                {
                                    if (_eyerSquintBlendValue != 0)
                                    {
                                        float newWeight = (_eyerSquintBlendValue + weight) / 2f;
                                        _eyerSquintBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyerSquintBlendValue = weight;
                                    }
                                }

                                break;
                            case ".cheekSquintRight":
                                if (weight > 0)
                                {
                                    if (_eyerSquintBlendValue != 0)
                                    {
                                        float newWeight = (_eyerSquintBlendValue + weight) / 2f;
                                        _eyerSquintBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyerSquintBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeWideUpperRight":
                                if (weight > 0)
                                {
                                    if (_eyerWideBlendValue != 0)
                                    {
                                        float newWeight = (_eyerWideBlendValue + weight) / 2f;
                                        _eyerWideBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyerWideBlendValue = weight;
                                    }
                                }

                                break;
                            case ".eyeWideLowerRight":
                                if (weight > 0)
                                {
                                    if (_eyerWideBlendValue != 0)
                                    {
                                        float newWeight = (_eyerWideBlendValue + weight) / 2f;
                                        _eyerWideBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _eyerWideBlendValue = weight;
                                    }
                                }

                                break;
                            case ".noseSneerLeft":
                                if (weight > 0)
                                {
                                    if (_noslSneerBlendValue != 0)
                                    {
                                        float newWeight = (_noslSneerBlendValue + weight) / 2f;
                                        _noslSneerBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _noslSneerBlendValue = weight;
                                    }
                                }

                                break;
                            case ".noseSneerRight":
                                if (weight > 0)
                                {
                                    if (_nosrSneerBlendValue != 0)
                                    {
                                        float newWeight = (_nosrSneerBlendValue + weight) / 2f;
                                        _nosrSneerBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _nosrSneerBlendValue = weight;
                                    }
                                }

                                break;
                            case ".cheekHollowLeft":
                                if (weight > 0)
                                {
                                    if (_chklHollowPuffBlendValue != 0)
                                    {
                                        float newWeight = (_chklHollowPuffBlendValue + -weight) / 2f;
                                        _chklHollowPuffBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _chklHollowPuffBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".cheekPuffLeft":
                                if (weight > 0)
                                {
                                    if (_chklHollowPuffBlendValue != 0)
                                    {
                                        float newWeight = (_chklHollowPuffBlendValue + weight) / 2f;
                                        _chklHollowPuffBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _chklHollowPuffBlendValue = weight;
                                    }
                                }

                                break;
                            case ".cheekHollowRight":
                                if (weight > 0)
                                {
                                    if (_chkrHollowPuffBlendValue != 0)
                                    {
                                        float newWeight = (_chkrHollowPuffBlendValue + -weight) / 2f;
                                        _chkrHollowPuffBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _chkrHollowPuffBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".cheekPuffRight":
                                if (weight > 0)
                                {
                                    if (_chkrHollowPuffBlendValue != 0)
                                    {
                                        float newWeight = (_chkrHollowPuffBlendValue + weight) / 2f;
                                        _chkrHollowPuffBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _chkrHollowPuffBlendValue = weight;
                                    }
                                }

                                break;
                            case ".jawClose":
                                if (weight > 0)
                                {
                                    if (_jawOpenCloseBlendValue != 0)
                                    {
                                        float newWeight = (_jawOpenCloseBlendValue + -weight) / 2f;
                                        _jawOpenCloseBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _jawOpenCloseBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".jawOpen":
                                if (weight > 0)
                                {
                                    if (_jawOpenCloseBlendValue != 0)
                                    {
                                        float newWeight = (_jawOpenCloseBlendValue + weight) / 2f;
                                        _jawOpenCloseBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _jawOpenCloseBlendValue = weight;
                                    }
                                }

                                break;
                            case ".jawLeft":
                                if (weight > 0)
                                {
                                    if (_jawLeftRightBlendValue != 0)
                                    {
                                        float newWeight = (_jawLeftRightBlendValue + -weight) / 2f;
                                        _jawLeftRightBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _jawLeftRightBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".jawRight":
                                if (weight > 0)
                                {
                                    if (_jawLeftRightBlendValue != 0)
                                    {
                                        float newWeight = (_jawLeftRightBlendValue + weight) / 2f;
                                        _jawLeftRightBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _jawLeftRightBlendValue = weight;
                                    }
                                }

                                break;
                            case ".jawBackward":
                                if (weight > 0)
                                {
                                    if (_jawBackForwardBlendValue != 0)
                                    {
                                        float newWeight = (_jawBackForwardBlendValue + -weight) / 2f;
                                        _jawBackForwardBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _jawBackForwardBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".jawForward":
                                if (weight > 0)
                                {
                                    if (_jawBackForwardBlendValue != 0)
                                    {
                                        float newWeight = (_jawBackForwardBlendValue + weight) / 2f;
                                        _jawBackForwardBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _jawBackForwardBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthLeft":
                                if (weight > 0)
                                {
                                    if (_mthLeftRightBlendValue != 0)
                                    {
                                        float newWeight = (_mthLeftRightBlendValue + -weight) / 2f;
                                        _mthLeftRightBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthLeftRightBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".mouthRight":
                                if (weight > 0)
                                {
                                    if (_mthLeftRightBlendValue != 0)
                                    {
                                        float newWeight = (_mthLeftRightBlendValue + weight) / 2f;
                                        _mthLeftRightBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthLeftRightBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthFunnel":
                                if (weight > 0)
                                {
                                    if (_mthFunnelBlendValue != 0)
                                    {
                                        float newWeight = (_mthFunnelBlendValue + weight) / 2f;
                                        _mthFunnelBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthFunnelBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthPucker":
                                if (weight > 0)
                                {
                                    if (_mthPuckerBlendValue != 0)
                                    {
                                        float newWeight = (_mthPuckerBlendValue + weight) / 2f;
                                        _mthPuckerBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthPuckerBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthShrugUpper":
                                if (weight > 0)
                                {
                                    if (_mthShrugUpperBlendValue != 0)
                                    {
                                        float newWeight = (_mthShrugUpperBlendValue + weight) / 2f;
                                        _mthShrugUpperBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthShrugUpperBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthShrugLower":
                                if (weight > 0)
                                {
                                    if (_mthShrugLowerBlendValue != 0)
                                    {
                                        float newWeight = (_mthShrugLowerBlendValue + weight) / 2f;
                                        _mthShrugLowerBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthShrugLowerBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthRollUpper":
                                if (weight > 0)
                                {
                                    if (_mthRollUpperBlendValue != 0)
                                    {
                                        float newWeight = (_mthRollUpperBlendValue + weight) / 2f;
                                        _mthRollUpperBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthRollUpperBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthRollOutUpper":
                                if (weight > 0)
                                {
                                    if (_mthRollOutUpperBlendValue != 0)
                                    {
                                        float newWeight = (_mthRollOutUpperBlendValue + weight) / 2f;
                                        _mthRollOutUpperBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthRollOutUpperBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthRollLower":
                                if (weight > 0)
                                {
                                    if (_mthRollLowerBlendValue != 0)
                                    {
                                        float newWeight = (_mthRollLowerBlendValue + weight) / 2f;
                                        _mthRollLowerBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthRollLowerBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthRollOutLower":
                                if (weight > 0)
                                {
                                    if (_mthRollOutLowerBlendValue != 0)
                                    {
                                        float newWeight = (_mthRollOutLowerBlendValue + weight) / 2f;
                                        _mthRollOutLowerBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthRollOutLowerBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthClose":
                                if (weight > 0)
                                {
                                    if (_mthCloseBlendValue != 0)
                                    {
                                        float newWeight = (_mthCloseBlendValue + weight) / 2f;
                                        _mthCloseBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthCloseBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthFrownLeft":
                                if (weight > 0)
                                {
                                    if (_mthlFrownSmileBlendValue != 0)
                                    {
                                        float newWeight = (_mthlFrownSmileBlendValue + -weight) / 2f;
                                        _mthlFrownSmileBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthlFrownSmileBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".mouthSmileLeft":
                                if (weight > 0)
                                {
                                    if (_mthlFrownSmileBlendValue != 0)
                                    {
                                        float newWeight = (_mthlFrownSmileBlendValue + weight) / 2f;
                                        _mthlFrownSmileBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthlFrownSmileBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthPressLeft":
                                if (weight > 0)
                                {
                                    if (_mthlPressStretchBlendValue != 0)
                                    {
                                        float newWeight = (_mthlPressStretchBlendValue + -weight) / 2f;
                                        _mthlPressStretchBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthlPressStretchBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".mouthStretchLeft":
                                if (weight > 0)
                                {
                                    if (_mthlPressStretchBlendValue != 0)
                                    {
                                        float newWeight = (_mthlPressStretchBlendValue + weight) / 2f;
                                        _mthlPressStretchBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthlPressStretchBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthUpperUpLeft":
                                if (weight > 0)
                                {
                                    if (_mthlUpperUpBlendValue != 0)
                                    {
                                        float newWeight = (_mthlUpperUpBlendValue + weight) / 2f;
                                        _mthlUpperUpBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthlUpperUpBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthLowerDownLeft":
                                if (weight > 0)
                                {
                                    if (_mthlLowerDownBlendValue != 0)
                                    {
                                        float newWeight = (_mthlLowerDownBlendValue + weight) / 2f;
                                        _mthlLowerDownBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthlLowerDownBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthFrownRight":
                                if (weight > 0)
                                {
                                    if (_mthrFrownSmileBlendValue != 0)
                                    {
                                        float newWeight = (_mthrFrownSmileBlendValue + -weight) / 2f;
                                        _mthrFrownSmileBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthrFrownSmileBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".mouthSmileRight":
                                if (weight > 0)
                                {
                                    if (_mthrFrownSmileBlendValue != 0)
                                    {
                                        float newWeight = (_mthrFrownSmileBlendValue + weight) / 2f;
                                        _mthrFrownSmileBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthrFrownSmileBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthPressRight":
                                if (weight > 0)
                                {
                                    if (_mthrPressStretchBlendValue != 0)
                                    {
                                        float newWeight = (_mthrPressStretchBlendValue + -weight) / 2f;
                                        _mthrPressStretchBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthrPressStretchBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".mouthStretchRight":
                                if (weight > 0)
                                {
                                    if (_mthrPressStretchBlendValue != 0)
                                    {
                                        float newWeight = (_mthrPressStretchBlendValue + weight) / 2f;
                                        _mthrPressStretchBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthrPressStretchBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthUpperUpRight":
                                if (weight > 0)
                                {
                                    if (_mthrUpperUpBlendValue != 0)
                                    {
                                        float newWeight = (_mthrUpperUpBlendValue + weight) / 2f;
                                        _mthrUpperUpBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthrUpperUpBlendValue = weight;
                                    }
                                }

                                break;
                            case ".mouthLowerDownRight":
                                if (weight > 0)
                                {
                                    if (_mthrLowerDownBlendValue != 0)
                                    {
                                        float newWeight = (_mthrLowerDownBlendValue + weight) / 2f;
                                        _mthrLowerDownBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _mthrLowerDownBlendValue = weight;
                                    }
                                }

                                break;
                            case ".tongueDown":
                                if (weight > 0)
                                {
                                    if (_tongDownUpBlendValue != 0)
                                    {
                                        float newWeight = (_tongDownUpBlendValue + -weight) / 2f;
                                        _tongDownUpBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongDownUpBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".tongueUp":
                                if (weight > 0)
                                {
                                    if (_tongDownUpBlendValue != 0)
                                    {
                                        float newWeight = (_tongDownUpBlendValue + weight) / 2f;
                                        _tongDownUpBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongDownUpBlendValue = weight;
                                    }
                                }

                                break;
                            case ".tongueIn":
                                if (weight > 0)
                                {
                                    if (_tongInOutBlendValue != 0)
                                    {
                                        float newWeight = (_tongInOutBlendValue + -weight) / 2f;
                                        _tongInOutBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongInOutBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".tongueOut":
                                if (weight > 0)
                                {
                                    if (_tongInOutBlendValue != 0)
                                    {
                                        float newWeight = (_tongInOutBlendValue + weight) / 2f;
                                        _tongInOutBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongInOutBlendValue = weight;
                                    }
                                }

                                break;
                            case ".tongueLower":
                                if (weight > 0)
                                {
                                    if (_tongLowerRaiseBlendValue != 0)
                                    {
                                        float newWeight = (_tongLowerRaiseBlendValue + -weight) / 2f;
                                        _tongLowerRaiseBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongLowerRaiseBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".tongueRaise":
                                if (weight > 0)
                                {
                                    if (_tongLowerRaiseBlendValue != 0)
                                    {
                                        float newWeight = (_tongLowerRaiseBlendValue + weight) / 2f;
                                        _tongLowerRaiseBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongLowerRaiseBlendValue = weight;
                                    }
                                }

                                break;
                            case ".tongueTwistLeft":
                                if (weight > 0)
                                {
                                    if (_tongTwistLeftRightBlendValue != 0)
                                    {
                                        float newWeight = (_tongTwistLeftRightBlendValue + -weight) / 2f;
                                        _tongTwistLeftRightBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongTwistLeftRightBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".tongueTwistRight":
                                if (weight > 0)
                                {
                                    if (_tongTwistLeftRightBlendValue != 0)
                                    {
                                        float newWeight = (_tongTwistLeftRightBlendValue + weight) / 2f;
                                        _tongTwistLeftRightBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongTwistLeftRightBlendValue = weight;
                                    }
                                }

                                break;
                            case ".tongueCurlDown":
                                if (weight > 0)
                                {
                                    if (_tongCurlDownUpBlendValue != 0)
                                    {
                                        float newWeight = (_tongCurlDownUpBlendValue + -weight) / 2f;
                                        _tongCurlDownUpBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongCurlDownUpBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".tongueCurlUp":
                                if (weight > 0)
                                {
                                    if (_tongCurlDownUpBlendValue != 0)
                                    {
                                        float newWeight = (_tongCurlDownUpBlendValue + weight) / 2f;
                                        _tongCurlDownUpBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongCurlDownUpBlendValue = weight;
                                    }
                                }

                                break;
                            case ".tongueCurlLeft":
                                if (weight > 0)
                                {
                                    if (_tongCurlLeftRightBlendValue != 0)
                                    {
                                        float newWeight = (_tongCurlLeftRightBlendValue + -weight) / 2f;
                                        _tongCurlLeftRightBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongCurlLeftRightBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".tongueCurlRight":
                                if (weight > 0)
                                {
                                    if (_tongCurlLeftRightBlendValue != 0)
                                    {
                                        float newWeight = (_tongCurlLeftRightBlendValue + weight) / 2f;
                                        _tongCurlLeftRightBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongCurlLeftRightBlendValue = weight;
                                    }
                                }

                                break;
                            case ".tongueSideCurlDown":
                                if (weight > 0)
                                {
                                    if (_tongCurlSideDownUpBlendValue != 0)
                                    {
                                        float newWeight = (_tongCurlSideDownUpBlendValue + -weight) / 2f;
                                        _tongCurlSideDownUpBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongCurlSideDownUpBlendValue = -weight;
                                    }
                                }

                                break;
                            case ".tongueSideCurlUp":
                                if (weight > 0)
                                {
                                    if (_tongCurlSideDownUpBlendValue != 0)
                                    {
                                        float newWeight = (_tongCurlSideDownUpBlendValue + weight) / 2f;
                                        _tongCurlSideDownUpBlendValue = newWeight;
                                    }
                                    else
                                    {
                                        _tongCurlSideDownUpBlendValue = weight;
                                    }
                                }

                                break;
                        }

                        #endregion
                    }
                }
            }
        }

        public void SavePreset()
        {
            if (string.IsNullOrEmpty(_saveName))
            {
                EditorUtility.DisplayDialog("Unable to Save Preset", "No preset name was provided to save the preset to.", "Ok");
                return;
            }

            List<string> serializedData = SerializeValues();
            string outputFileName = Path.Combine(_presetDirectory, _saveName + ".fcp");
            File.WriteAllLines(outputFileName, serializedData);
        }

        public List<string> SerializeValues()
        {
            List<string> serialisedValues = new List<string>();
            foreach (string field in _variableNames)
            {
                switch (field)
                {
                    case "ebrlFrownBlendValue":
                        serialisedValues.Add(field + ":" + _ebrlFrownBlendValue);
                        break;
                    case "ebrlInnerBlendValue":
                        serialisedValues.Add(field + ":" + _ebrlInnerBlendValue);
                        break;
                    case "ebrlOuterBlendValue":
                        serialisedValues.Add(field + ":" + _ebrlOuterBlendValue);
                        break;
                    case "ebrrFrownBlendValue":
                        serialisedValues.Add(field + ":" + _ebrrFrownBlendValue);
                        break;
                    case "ebrrInnerBlendValue":
                        serialisedValues.Add(field + ":" + _ebrrInnerBlendValue);
                        break;
                    case "ebrrOuterBlendValue":
                        serialisedValues.Add(field + ":" + _ebrrOuterBlendValue);
                        break;
                    case "eyelUpDownBlendValue":
                        serialisedValues.Add(field + ":" + _eyelUpDownBlendValue);
                        break;
                    case "eyelLeftRightBlendValue":
                        serialisedValues.Add(field + ":" + _eyelLeftRightBlendValue);
                        break;
                    case "eyelBlinkBlendValue":
                        serialisedValues.Add(field + ":" + _eyelBlinkBlendValue);
                        break;
                    case "eyelSquintBlendValue":
                        serialisedValues.Add(field + ":" + _eyelSquintBlendValue);
                        break;
                    case "eyelWideBlendValue":
                        serialisedValues.Add(field + ":" + _eyelWideBlendValue);
                        break;
                    case "eyerUpDownBlendValue":
                        serialisedValues.Add(field + ":" + _eyerUpDownBlendValue);
                        break;
                    case "eyerLeftRightBlendValue":
                        serialisedValues.Add(field + ":" + _eyerLeftRightBlendValue);
                        break;
                    case "eyerBlinkBlendValue":
                        serialisedValues.Add(field + ":" + _eyerBlinkBlendValue);
                        break;
                    case "eyerSquintBlendValue":
                        serialisedValues.Add(field + ":" + _eyerSquintBlendValue);
                        break;
                    case "eyerWideBlendValue":
                        serialisedValues.Add(field + ":" + _eyerWideBlendValue);
                        break;
                    case "noslSneerBlendValue":
                        serialisedValues.Add(field + ":" + _noslSneerBlendValue);
                        break;
                    case "nosrSneerBlendValue":
                        serialisedValues.Add(field + ":" + _nosrSneerBlendValue);
                        break;
                    case "chklHollowPuffBlendValue":
                        serialisedValues.Add(field + ":" + _chklHollowPuffBlendValue);
                        break;
                    case "chkrHollowPuffBlendValue":
                        serialisedValues.Add(field + ":" + _chkrHollowPuffBlendValue);
                        break;
                    case "jawOpenCloseBlendValue":
                        serialisedValues.Add(field + ":" + _jawOpenCloseBlendValue);
                        break;
                    case "jawLeftRightBlendValue":
                        serialisedValues.Add(field + ":" + _jawLeftRightBlendValue);
                        break;
                    case "jawBackForwardBlendValue":
                        serialisedValues.Add(field + ":" + _jawBackForwardBlendValue);
                        break;
                    case "mthLeftRightBlendValue":
                        serialisedValues.Add(field + ":" + _mthLeftRightBlendValue);
                        break;
                    case "mthFunnelBlendValue":
                        serialisedValues.Add(field + ":" + _mthFunnelBlendValue);
                        break;
                    case "mthPuckerBlendValue":
                        serialisedValues.Add(field + ":" + _mthPuckerBlendValue);
                        break;
                    case "mthShrugUpperBlendValue":
                        serialisedValues.Add(field + ":" + _mthShrugUpperBlendValue);
                        break;
                    case "mthShrugLowerBlendValue":
                        serialisedValues.Add(field + ":" + _mthShrugLowerBlendValue);
                        break;
                    case "mthRollUpperBlendValue":
                        serialisedValues.Add(field + ":" + _mthRollUpperBlendValue);
                        break;
                    case "mthRollOutUpperBlendValue":
                        serialisedValues.Add(field + ":" + _mthRollOutUpperBlendValue);
                        break;
                    case "mthRollLowerBlendValue":
                        serialisedValues.Add(field + ":" + _mthRollLowerBlendValue);
                        break;
                    case "mthRollOutLowerBlendValue":
                        serialisedValues.Add(field + ":" + _mthRollOutLowerBlendValue);
                        break;
                    case "mthCloseBlendValue":
                        serialisedValues.Add(field + ":" + _mthCloseBlendValue);
                        break;
                    case "mthlFrownSmileBlendValue":
                        serialisedValues.Add(field + ":" + _mthlFrownSmileBlendValue);
                        break;
                    case "mthrFrownSmileBlendValue":
                        serialisedValues.Add(field + ":" + _mthrFrownSmileBlendValue);
                        break;
                    case "mthlPressStretchBlendValue":
                        serialisedValues.Add(field + ":" + _mthlPressStretchBlendValue);
                        break;
                    case "mthrPressStretchBlendValue":
                        serialisedValues.Add(field + ":" + _mthrPressStretchBlendValue);
                        break;
                    case "mthlUpperUpBlendValue":
                        serialisedValues.Add(field + ":" + _mthlUpperUpBlendValue);
                        break;
                    case "mthrUpperUpBlendValue":
                        serialisedValues.Add(field + ":" + _mthrUpperUpBlendValue);
                        break;
                    case "mthlLowerDownBlendValue":
                        serialisedValues.Add(field + ":" + _mthlLowerDownBlendValue);
                        break;
                    case "mthrLowerDownBlendValue":
                        serialisedValues.Add(field + ":" + _mthrLowerDownBlendValue);
                        break;
                    case "tongDownUpBlendValue":
                        serialisedValues.Add(field + ":" + _tongDownUpBlendValue);
                        break;
                    case "tongInOutBlendValue":
                        serialisedValues.Add(field + ":" + _tongInOutBlendValue);
                        break;
                    case "tongLowerRaiseBlendValue":
                        serialisedValues.Add(field + ":" + _tongLowerRaiseBlendValue);
                        break;
                    case "tongTwistLeftRightBlendValue":
                        serialisedValues.Add(field + ":" + _tongTwistLeftRightBlendValue);
                        break;
                    case "tongCurlDownUpBlendValue":
                        serialisedValues.Add(field + ":" + _tongCurlDownUpBlendValue);
                        break;
                    case "tongCurlLeftRightBlendValue":
                        serialisedValues.Add(field + ":" + _tongCurlLeftRightBlendValue);
                        break;
                    case "tongCurlSideDownUpBlendValue":
                        serialisedValues.Add(field + ":" + _tongCurlSideDownUpBlendValue);
                        break;
                }
            }

            return serialisedValues;
        }

        public void LoadPreset(string presetName)
        {
            if (presetName == "None")
            {
                return;
            }

            string fileLocation = _presetFileDictionary[presetName];
            List<string> serializedData = File.ReadAllLines(fileLocation).ToList();
            string line;
            foreach (string field in _variableNames)
            {
                switch (field)
                {
                    case "ebrlFrownBlendValue":
                        line = serializedData.Find(data => data.Contains("ebrlFrownBlendValue"));
                        _ebrlFrownBlendValue = GetValue(line);
                        break;
                    case "ebrlInnerBlendValue":
                        line = serializedData.Find(data => data.Contains("ebrlInnerBlendValue"));
                        _ebrlInnerBlendValue = GetValue(line);
                        break;
                    case "ebrlOuterBlendValue":
                        line = serializedData.Find(data => data.Contains("ebrlOuterBlendValue"));
                        _ebrlOuterBlendValue = GetValue(line);
                        break;
                    case "ebrrFrownBlendValue":
                        line = serializedData.Find(data => data.Contains("ebrrFrownBlendValue"));
                        _ebrrFrownBlendValue = GetValue(line);
                        break;
                    case "ebrrInnerBlendValue":
                        line = serializedData.Find(data => data.Contains("ebrrInnerBlendValue"));
                        _ebrrInnerBlendValue = GetValue(line);
                        break;
                    case "ebrrOuterBlendValue":
                        line = serializedData.Find(data => data.Contains("ebrrOuterBlendValue"));
                        _ebrrOuterBlendValue = GetValue(line);
                        break;
                    case "eyelUpDownBlendValue":
                        line = serializedData.Find(data => data.Contains("eyelUpDownBlendValue"));
                        _eyelUpDownBlendValue = GetValue(line);
                        break;
                    case "eyelLeftRightBlendValue":
                        line = serializedData.Find(data => data.Contains("eyelLeftRightBlendValue"));
                        _eyelLeftRightBlendValue = GetValue(line);
                        break;
                    case "eyelBlinkBlendValue":
                        line = serializedData.Find(data => data.Contains("eyelBlinkBlendValue"));
                        _eyelBlinkBlendValue = GetValue(line);
                        break;
                    case "eyelSquintBlendValue":
                        line = serializedData.Find(data => data.Contains("eyelSquintBlendValue"));
                        _eyelSquintBlendValue = GetValue(line);
                        break;
                    case "eyelWideBlendValue":
                        line = serializedData.Find(data => data.Contains("eyelWideBlendValue"));
                        _eyelWideBlendValue = GetValue(line);
                        break;
                    case "eyerUpDownBlendValue":
                        line = serializedData.Find(data => data.Contains("eyerUpDownBlendValue"));
                        _eyerUpDownBlendValue = GetValue(line);
                        break;
                    case "eyerLeftRightBlendValue":
                        line = serializedData.Find(data => data.Contains("eyerLeftRightBlendValue"));
                        _eyerLeftRightBlendValue = GetValue(line);
                        break;
                    case "eyerBlinkBlendValue":
                        line = serializedData.Find(data => data.Contains("eyerBlinkBlendValue"));
                        _eyerBlinkBlendValue = GetValue(line);
                        break;
                    case "eyerSquintBlendValue":
                        line = serializedData.Find(data => data.Contains("eyerSquintBlendValue"));
                        _eyerSquintBlendValue = GetValue(line);
                        break;
                    case "eyerWideBlendValue":
                        line = serializedData.Find(data => data.Contains("eyerWideBlendValue"));
                        _eyerWideBlendValue = GetValue(line);
                        break;
                    case "noslSneerBlendValue":
                        line = serializedData.Find(data => data.Contains("noslSneerBlendValue"));
                        _noslSneerBlendValue = GetValue(line);
                        break;
                    case "nosrSneerBlendValue":
                        line = serializedData.Find(data => data.Contains("nosrSneerBlendValue"));
                        _nosrSneerBlendValue = GetValue(line);
                        break;
                    case "chklHollowPuffBlendValue":
                        line = serializedData.Find(data => data.Contains("chklHollowPuffBlendValue"));
                        _chklHollowPuffBlendValue = GetValue(line);
                        break;
                    case "chkrHollowPuffBlendValue":
                        line = serializedData.Find(data => data.Contains("chkrHollowPuffBlendValue"));
                        _chkrHollowPuffBlendValue = GetValue(line);
                        break;
                    case "jawOpenCloseBlendValue":
                        line = serializedData.Find(data => data.Contains("jawOpenCloseBlendValue"));
                        _jawOpenCloseBlendValue = GetValue(line);
                        break;
                    case "jawLeftRightBlendValue":
                        line = serializedData.Find(data => data.Contains("jawLeftRightBlendValue"));
                        _jawLeftRightBlendValue = GetValue(line);
                        break;
                    case "jawBackForwardBlendValue":
                        line = serializedData.Find(data => data.Contains("jawBackForwardBlendValue"));
                        _jawBackForwardBlendValue = GetValue(line);
                        break;
                    case "mthLeftRightBlendValue":
                        line = serializedData.Find(data => data.Contains("mthLeftRightBlendValue"));
                        _mthLeftRightBlendValue = GetValue(line);
                        break;
                    case "mthFunnelBlendValue":
                        line = serializedData.Find(data => data.Contains("mthFunnelBlendValue"));
                        _mthFunnelBlendValue = GetValue(line);
                        break;
                    case "mthPuckerBlendValue":
                        line = serializedData.Find(data => data.Contains("mthPuckerBlendValue"));
                        _mthPuckerBlendValue = GetValue(line);
                        break;
                    case "mthShrugUpperBlendValue":
                        line = serializedData.Find(data => data.Contains("mthShrugUpperBlendValue"));
                        _mthShrugUpperBlendValue = GetValue(line);
                        break;
                    case "mthShrugLowerBlendValue":
                        line = serializedData.Find(data => data.Contains("mthShrugLowerBlendValue"));
                        _mthShrugLowerBlendValue = GetValue(line);
                        break;
                    case "mthRollUpperBlendValue":
                        line = serializedData.Find(data => data.Contains("mthRollUpperBlendValue"));
                        _mthRollUpperBlendValue = GetValue(line);
                        break;
                    case "mthRollOutUpperBlendValue":
                        line = serializedData.Find(data => data.Contains("mthRollOutUpperBlendValue"));
                        _mthRollOutUpperBlendValue = GetValue(line);
                        break;
                    case "mthRollLowerBlendValue":
                        line = serializedData.Find(data => data.Contains("mthRollLowerBlendValue"));
                        _mthRollLowerBlendValue = GetValue(line);
                        break;
                    case "mthRollOutLowerBlendValue":
                        line = serializedData.Find(data => data.Contains("mthRollOutLowerBlendValue"));
                        _mthRollOutLowerBlendValue = GetValue(line);
                        break;
                    case "mthCloseBlendValue":
                        line = serializedData.Find(data => data.Contains("mthCloseBlendValue"));
                        _mthCloseBlendValue = GetValue(line);
                        break;
                    case "mthlFrownSmileBlendValue":
                        line = serializedData.Find(data => data.Contains("mthlFrownSmileBlendValue"));
                        _mthlFrownSmileBlendValue = GetValue(line);
                        break;
                    case "mthrFrownSmileBlendValue":
                        line = serializedData.Find(data => data.Contains("mthrFrownSmileBlendValue"));
                        _mthrFrownSmileBlendValue = GetValue(line);
                        break;
                    case "mthlPressStretchBlendValue":
                        line = serializedData.Find(data => data.Contains("mthlPressStretchBlendValue"));
                        _mthlPressStretchBlendValue = GetValue(line);
                        break;
                    case "mthrPressStretchBlendValue":
                        line = serializedData.Find(data => data.Contains("mthrPressStretchBlendValue"));
                        _mthrPressStretchBlendValue = GetValue(line);
                        break;
                    case "mthlUpperUpBlendValue":
                        line = serializedData.Find(data => data.Contains("mthlUpperUpBlendValue"));
                        _mthlUpperUpBlendValue = GetValue(line);
                        break;
                    case "mthrUpperUpBlendValue":
                        line = serializedData.Find(data => data.Contains("mthrUpperUpBlendValue"));
                        _mthrUpperUpBlendValue = GetValue(line);
                        break;
                    case "mthlLowerDownBlendValue":
                        line = serializedData.Find(data => data.Contains("mthlLowerDownBlendValue"));
                        _mthlLowerDownBlendValue = GetValue(line);
                        break;
                    case "mthrLowerDownBlendValue":
                        line = serializedData.Find(data => data.Contains("mthrLowerDownBlendValue"));
                        _mthrLowerDownBlendValue = GetValue(line);
                        break;
                    case "tongDownUpBlendValue":
                        line = serializedData.Find(data => data.Contains("tongDownUpBlendValue"));
                        _tongDownUpBlendValue = GetValue(line);
                        break;
                    case "tongInOutBlendValue":
                        line = serializedData.Find(data => data.Contains("tongInOutBlendValue"));
                        _tongInOutBlendValue = GetValue(line);
                        break;
                    case "tongLowerRaiseBlendValue":
                        line = serializedData.Find(data => data.Contains("tongLowerRaiseBlendValue"));
                        _tongLowerRaiseBlendValue = GetValue(line);
                        break;
                    case "tongTwistLeftRightBlendValue":
                        line = serializedData.Find(data => data.Contains("tongTwistLeftRightBlendValue"));
                        _tongTwistLeftRightBlendValue = GetValue(line);
                        break;
                    case "tongCurlDownUpBlendValue":
                        line = serializedData.Find(data => data.Contains("tongCurlDownUpBlendValue"));
                        _tongCurlDownUpBlendValue = GetValue(line);
                        break;
                    case "tongCurlLeftRightBlendValue":
                        line = serializedData.Find(data => data.Contains("tongCurlLeftRightBlendValue"));
                        _tongCurlLeftRightBlendValue = GetValue(line);
                        break;
                    case "tongCurlSideDownUpBlendValue":
                        line = serializedData.Find(data => data.Contains("tongCurlSideDownUpBlendValue"));
                        _tongCurlSideDownUpBlendValue = GetValue(line);
                        break;
                }
            }

            ApplyBlendshapes();
        }

        public float GetValue(string line)
        {
            int index = line.IndexOf(":", StringComparison.Ordinal) + 1;
            if (float.TryParse(line.Substring(index, line.Length - index), out float value))
            {
                return value;
            }

            return 0f;
        }

        public void ResetAllSliders()
        {
            #region RESET_BLENDS
            foreach (string field in _variableNames)
            {
                switch (field)
                {
                    case "ebrlFrownBlendValue":
                        _ebrlFrownBlendValue = 0;
                        break;
                    case "ebrlInnerBlendValue":
                        _ebrlInnerBlendValue = 0;
                        break;
                    case "ebrlOuterBlendValue":
                        _ebrlOuterBlendValue = 0;
                        break;
                    case "ebrrFrownBlendValue":
                        _ebrrFrownBlendValue = 0;
                        break;
                    case "ebrrInnerBlendValue":
                        _ebrrInnerBlendValue = 0;
                        break;
                    case "ebrrOuterBlendValue":
                        _ebrrOuterBlendValue = 0;
                        break;
                    case "eyelUpDownBlendValue":
                        _eyelUpDownBlendValue = 0;
                        break;
                    case "eyelLeftRightBlendValue":
                        _eyelLeftRightBlendValue = 0;
                        break;
                    case "eyelBlinkBlendValue":
                        _eyelBlinkBlendValue = 0;
                        break;
                    case "eyelSquintBlendValue":
                        _eyelSquintBlendValue = 0;
                        break;
                    case "eyelWideBlendValue":
                        _eyelWideBlendValue = 0;
                        break;
                    case "eyerUpDownBlendValue":
                        _eyerUpDownBlendValue = 0;
                        break;
                    case "eyerLeftRightBlendValue":
                        _eyerLeftRightBlendValue = 0;
                        break;
                    case "eyerBlinkBlendValue":
                        _eyerBlinkBlendValue = 0;
                        break;
                    case "eyerSquintBlendValue":
                        _eyerSquintBlendValue = 0;
                        break;
                    case "eyerWideBlendValue":
                        _eyerWideBlendValue = 0;
                        break;
                    case "noslSneerBlendValue":
                        _noslSneerBlendValue = 0;
                        break;
                    case "nosrSneerBlendValue":
                        _nosrSneerBlendValue = 0;
                        break;
                    case "chklHollowPuffBlendValue":
                        _chklHollowPuffBlendValue = 0;
                        break;
                    case "chkrHollowPuffBlendValue":
                        _chkrHollowPuffBlendValue = 0;
                        break;
                    case "jawOpenCloseBlendValue":
                        _jawOpenCloseBlendValue = 0;
                        break;
                    case "jawLeftRightBlendValue":
                        _jawLeftRightBlendValue = 0;
                        break;
                    case "jawBackForwardBlendValue":
                        _jawBackForwardBlendValue = 0;
                        break;
                    case "mthLeftRightBlendValue":
                        _mthLeftRightBlendValue = 0;
                        break;
                    case "mthFunnelBlendValue":
                        _mthFunnelBlendValue = 0;
                        break;
                    case "mthPuckerBlendValue":
                        _mthPuckerBlendValue = 0;
                        break;
                    case "mthShrugUpperBlendValue":
                        _mthShrugUpperBlendValue = 0;
                        break;
                    case "mthShrugLowerBlendValue":
                        _mthShrugLowerBlendValue = 0;
                        break;
                    case "mthRollUpperBlendValue":
                        _mthRollUpperBlendValue = 0;
                        break;
                    case "mthRollOutUpperBlendValue":
                        _mthRollOutUpperBlendValue = 0;
                        break;
                    case "mthRollLowerBlendValue":
                        _mthRollLowerBlendValue = 0;
                        break;
                    case "mthRollOutLowerBlendValue":
                        _mthRollOutLowerBlendValue = 0;
                        break;
                    case "mthCloseBlendValue":
                        _mthCloseBlendValue = 0;
                        break;
                    case "mthlFrownSmileBlendValue":
                        _mthlFrownSmileBlendValue = 0;
                        break;
                    case "mthrFrownSmileBlendValue":
                        _mthrFrownSmileBlendValue = 0;
                        break;
                    case "mthlPressStretchBlendValue":
                        _mthlPressStretchBlendValue = 0;
                        break;
                    case "mthrPressStretchBlendValue":
                        _mthrPressStretchBlendValue = 0;
                        break;
                    case "mthlUpperUpBlendValue":
                        _mthlUpperUpBlendValue = 0;
                        break;
                    case "mthrUpperUpBlendValue":
                        _mthrUpperUpBlendValue = 0;
                        break;
                    case "mthlLowerDownBlendValue":
                        _mthlLowerDownBlendValue = 0;
                        break;
                    case "mthrLowerDownBlendValue":
                        _mthrLowerDownBlendValue = 0;
                        break;
                    case "tongDownUpBlendValue":
                        _tongDownUpBlendValue = 0;
                        break;
                    case "tongInOutBlendValue":
                        _tongInOutBlendValue = 0;
                        break;
                    case "tongLowerRaiseBlendValue":
                        _tongLowerRaiseBlendValue = 0;
                        break;
                    case "tongTwistLeftRightBlendValue":
                        _tongTwistLeftRightBlendValue = 0;
                        break;
                    case "tongCurlDownUpBlendValue":
                        _tongCurlDownUpBlendValue = 0;
                        break;
                    case "tongCurlLeftRightBlendValue":
                        _tongCurlLeftRightBlendValue = 0;
                        break;
                    case "tongCurlSideDownUpBlendValue":
                        _tongCurlSideDownUpBlendValue = 0;
                        break;
                }
            }
            #endregion
        }

        public void PopulatePresetList()
        {
            _presetNames = new List<string>();
            if (!string.IsNullOrEmpty(_presetDirectory))
            {
                List<string> files = Directory.GetFiles(_presetDirectory, "*.fcp", SearchOption.AllDirectories).ToList();
                foreach (string file in files)
                {
                    FileInfo info = new FileInfo(file);
                    string fileName = info.Name.Substring(0, info.Name.IndexOf(".fcp", StringComparison.Ordinal));
                    _presetNames.Add(fileName);
                    _presetFileDictionary.TryAdd(fileName, file);
                }
            }
        }
    }

    [CustomEditor(typeof(SidekickFacialController))]
    public class SidekickFacialControllerInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            SidekickFacialController controller = (SidekickFacialController) target;
            VisualElement root = new VisualElement();


            Image headerImage = new Image()
            {
                image = Resources.Load<Texture2D>("UI/T_SidekickTitleFacial"),
                style =
                {
                    height = 60,
                    width = Length.Percent(100),
                    backgroundColor = new Color(0.82f, 0.133f, 0.2f),
                    alignSelf = new StyleEnum<Align>(Align.Center),
                    alignContent = new StyleEnum<Align>(Align.Center)
                }
            };

            root.Add(headerImage);

            #region PRESETS
            Foldout presets = new Foldout()
            {
                text = "Presets",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                }
            };

            Label presetFolderTitle = new Label("Preset Directory");

            presets.Add(presetFolderTitle);

            VisualElement directoryLayout = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            TextField directoryField = new TextField()
            {
                style =
                {
                    width = Length.Percent(85),
                },
                isReadOnly = true,
                bindingPath = "_presetDirectory"
            };

            Button directoryPickerButton = new Button(
                delegate
                {
                    string directory = EditorUtility.OpenFolderPanel("Select Directory", "Assets", "");
                    if (!string.IsNullOrEmpty(directory) && directory.Contains("Assets"))
                    {
                        int index = directory.IndexOf("Assets", StringComparison.Ordinal);
                        directoryField.value = directory.Substring(index, directory.Length - index);
                    }
                }
            )
            {
                text = "Browse",
                style =
                {
                    width = Length.Percent(12),
                },
            };

            directoryLayout.Add(directoryPickerButton);
            directoryLayout.Add(directoryField);

            presets.Add(directoryLayout);

            Label loadPreset = new Label("Load Preset");
            presets.Add(loadPreset);

            VisualElement presetLayout = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            List<string> popupValues = new List<string>
            {
                "None"
            };

            controller.PopulatePresetList();
            popupValues.AddRange(controller._presetNames);

            PopupField<string> presetPopup = new PopupField<string>()
            {
                bindingPath = "_selectedPreset",
                choices = popupValues,
                value = popupValues[0]
            };

            Button prevButton = new Button(delegate { presetPopup.index--; })
            {
                text = "Prev",
            };

            Button nextButton = new Button(delegate { presetPopup.index++; })
            {
                text = "Next"
            };

            prevButton.SetEnabled(presetPopup.index > 0);
            nextButton.SetEnabled(presetPopup.index < presetPopup.choices.Count - 1);

            presetPopup.RegisterValueChangedCallback(
                evt =>
                {
                    controller.LoadPreset(evt.newValue);
                    prevButton.SetEnabled(presetPopup.index > 0);
                    nextButton.SetEnabled(presetPopup.index < presetPopup.choices.Count - 1);
                }
            );

            presetLayout.Add(prevButton);
            presetLayout.Add(nextButton);
            presetLayout.Add(presetPopup);

            presets.Add(presetLayout);

            Label savePresetLabel = new Label("Save Preset");
            presets.Add(savePresetLabel);

            VisualElement saveLayout = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            TextField saveNameField = new TextField()
            {
                style =
                {
                    width = Length.Percent(85),
                },
                bindingPath = "_saveName"
            };

            Button saveButton = new Button(
                delegate
                {
                    controller.SavePreset();
                }
            )
            {
                text = "Save",
                style =
                {
                    width = Length.Percent(12),
                },
            };

            saveLayout.Add(saveNameField);
            saveLayout.Add(saveButton);

            presets.Add(saveLayout);

            root.Add(presets);

            #endregion

            Foldout controls = new Foldout()
            {
                text = "Controls",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                }
            };

            Image linkImage = new Image()
            {
                image = EditorGUIUtility.IconContent("Linked").image,
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = 15,
                    height = 15
                }
            };

            Image unlinkImage = new Image()
            {
                image = EditorGUIUtility.IconContent("Unlinked").image,
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = 15,
                    height = 15
                }
            };

            #region EYEBROWS
            Foldout eyebrowFoldout = new Foldout
            {
                text = "EYEBROWS"
            };

            VisualElement labelRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label leftEyebrowLabel = new Label("LEFT")
            {
                style =
                {
                    flexGrow = 1,
                    marginLeft = 70,
                    alignSelf = new StyleEnum<Align>(Align.Center),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter)
                }
            };

            Label rightEyebrowLabel = new Label("RIGHT")
            {
                style =
                {
                    flexGrow = 1,
                    alignSelf = new StyleEnum<Align>(Align.Center),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter)
                }
            };

            labelRow.Add(leftEyebrowLabel);
            labelRow.Add(rightEyebrowLabel);

            VisualElement frownRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label frownLabel = new Label("Frown/Raise")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider ebrlFrownRaise = new Slider
            {
                bindingPath = "_ebrlFrownBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            Button mirrorFrown = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorFrown.Add(unlinkImage);

            Slider ebrrFrownRaise = new Slider
            {
                bindingPath = "_ebrrFrownBlendValue",
                // label = "Frown/Raise",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            VisualElement innerEBRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label innerEBLabel = new Label("Inner Down/Up")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider ebrlInnerDownUp = new Slider
            {
                bindingPath = "_ebrlInnerBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            Button mirrorInnerEB = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorInnerEB.Add(unlinkImage);

            Slider ebrrInnerDownUp = new Slider
            {
                bindingPath = "_ebrrInnerBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            VisualElement outerEBRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label outerEBLabel = new Label("Outer Down/Up")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider ebrlOuterDownUp = new Slider
            {
                bindingPath = "_ebrlOuterBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };


            Button mirrorOuterEB = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorOuterEB.Add(unlinkImage);

            Slider ebrrOuterDownUp = new Slider
            {
                bindingPath = "_ebrrOuterBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            eyebrowFoldout.Add(labelRow);
            frownRow.Add(frownLabel);
            frownRow.Add(ebrlFrownRaise);
            frownRow.Add(mirrorFrown);
            frownRow.Add(ebrrFrownRaise);
            eyebrowFoldout.Add(frownRow);
            innerEBRow.Add(innerEBLabel);
            innerEBRow.Add(ebrlInnerDownUp);
            innerEBRow.Add(mirrorInnerEB);
            innerEBRow.Add(ebrrInnerDownUp);
            eyebrowFoldout.Add(innerEBRow);
            outerEBRow.Add(outerEBLabel);
            outerEBRow.Add(ebrlOuterDownUp);
            outerEBRow.Add(mirrorOuterEB);
            outerEBRow.Add(ebrrOuterDownUp);
            eyebrowFoldout.Add(outerEBRow);

            controls.Add(eyebrowFoldout);
            #endregion

            #region EYES
            Foldout eyeFoldout = new Foldout
            {
                text = "EYES"
            };

            VisualElement eyeLabelRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label leftEyeLabel = new Label("LEFT")
            {
                style =
                {
                    flexGrow = 1,
                    marginLeft = 70,
                    alignSelf = new StyleEnum<Align>(Align.Center),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter)
                }
            };
            Label rightEyeLabel = new Label("RIGHT")
            {
                style =
                {
                    flexGrow = 1,
                    alignSelf = new StyleEnum<Align>(Align.Center),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter)
                }
            };

            VisualElement lookUpRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label lookUpLabel = new Label("Down/Up")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider eyelLookUpDown = new Slider
            {
                bindingPath = "_eyelUpDownBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            Button mirrorLookUp = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorLookUp.Add(unlinkImage);

            Slider eyerLookUpDown = new Slider
            {
                bindingPath = "_eyerUpDownBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            VisualElement lookLeftRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label lookLeftLabel = new Label("Left/Right")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider eyelLookLeftRight = new Slider
            {
                bindingPath = "_eyelLeftRightBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            Button mirrorLookLeft = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorLookLeft.Add(unlinkImage);

            Slider eyerLookLeftRight = new Slider
            {
                bindingPath = "_eyerLeftRightBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            VisualElement blinkRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label blinkLabel = new Label("Blink")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider eyelBlink = new Slider
            {
                bindingPath = "_eyelBlinkBlendValue",
                showInputField = true,
                lowValue = 0,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            Button mirrorBlink = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorBlink.Add(unlinkImage);

            Slider eyerBlink = new Slider
            {
                bindingPath = "_eyerBlinkBlendValue",
                showInputField = true,
                lowValue = 0,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            VisualElement squintRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label squintLabel = new Label("Squint")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider eyelSquint = new Slider
            {
                bindingPath = "_eyelSquintBlendValue",
                showInputField = true,
                lowValue = 0,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            Button mirrorSquint = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorSquint.Add(unlinkImage);

            Slider eyerSquint = new Slider
            {
                bindingPath = "_eyerSquintBlendValue",
                showInputField = true,
                lowValue = 0,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            VisualElement wideRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label wideLabel = new Label("Wide")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider eyelWide = new Slider
            {
                bindingPath = "_eyelWideBlendValue",
                showInputField = true,
                lowValue = 0,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            Button mirrorWide = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorWide.Add(unlinkImage);

            Slider eyerWide = new Slider
            {
                bindingPath = "_eyerWideBlendValue",
                showInputField = true,
                lowValue = 0,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            eyeLabelRow.Add(leftEyeLabel);
            eyeLabelRow.Add(rightEyeLabel);
            eyeFoldout.Add(eyeLabelRow);
            lookUpRow.Add(lookUpLabel);
            lookUpRow.Add(eyelLookUpDown);
            lookUpRow.Add(mirrorLookUp);
            lookUpRow.Add(eyerLookUpDown);
            eyeFoldout.Add(lookUpRow);
            lookLeftRow.Add(lookLeftLabel);
            lookLeftRow.Add(eyelLookLeftRight);
            lookLeftRow.Add(mirrorLookLeft);
            lookLeftRow.Add(eyerLookLeftRight);
            eyeFoldout.Add(lookLeftRow);
            blinkRow.Add(blinkLabel);
            blinkRow.Add(eyelBlink);
            blinkRow.Add(mirrorBlink);
            blinkRow.Add(eyerBlink);
            eyeFoldout.Add(blinkRow);
            squintRow.Add(squintLabel);
            squintRow.Add(eyelSquint);
            squintRow.Add(mirrorSquint);
            squintRow.Add(eyerSquint);
            eyeFoldout.Add(squintRow);
            wideRow.Add(wideLabel);
            wideRow.Add(eyelWide);
            wideRow.Add(mirrorWide);
            wideRow.Add(eyerWide);
            eyeFoldout.Add(wideRow);

            controls.Add(eyeFoldout);
            #endregion

            #region NOSE
            Foldout noseFoldout = new Foldout
            {
                text = "NOSE"
            };

            VisualElement sneerLabelRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label sneerLeftLabel = new Label("LEFT")
            {
                style =
                {
                    flexGrow = 1,
                    marginLeft = 70,
                    alignSelf = new StyleEnum<Align>(Align.Center),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter)
                }
            };
            Label sneerRightLabel = new Label("RIGHT")
            {
                style =
                {
                    flexGrow = 1,
                    alignSelf = new StyleEnum<Align>(Align.Center),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter)
                }
            };

            VisualElement sneerRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label sneerLabel = new Label("Sneer")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider noseSneerLeft = new Slider
            {
                bindingPath = "_noslSneerBlendValue",
                showInputField = true,
                lowValue = 0,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            Button mirrorSneer = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorSneer.Add(unlinkImage);

            Slider noseSneerRight = new Slider
            {
                bindingPath = "_nosrSneerBlendValue",
                showInputField = true,
                lowValue = 0,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            sneerLabelRow.Add(sneerLeftLabel);
            sneerLabelRow.Add(sneerRightLabel);
            noseFoldout.Add(sneerLabelRow);
            sneerRow.Add(sneerLabel);
            sneerRow.Add(noseSneerLeft);
            sneerRow.Add(mirrorSneer);
            sneerRow.Add(noseSneerRight);
            noseFoldout.Add(sneerRow);

            controls.Add(noseFoldout);
            #endregion

            #region CHEEKS
            Foldout cheeksFoldout = new Foldout
            {
                text = "CHEEKS"
            };

            VisualElement cheekLabelRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label cheekLeftLabel = new Label("LEFT")
            {
                style =
                {
                    flexGrow = 1,
                    marginLeft = 70,
                    alignSelf = new StyleEnum<Align>(Align.Center),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter)
                }
            };
            Label cheekRightLabel = new Label("RIGHT")
            {
                style =
                {
                    flexGrow = 1,
                    alignSelf = new StyleEnum<Align>(Align.Center),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter)
                }
            };

            VisualElement cheekRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label cheekLabel = new Label("Hollow/Puff")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider cheekHollowPuffLeft = new Slider
            {
                bindingPath = "_chklHollowPuffBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            Button mirrorCheek = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorCheek.Add(unlinkImage);

            Slider cheekHollowPuffRight = new Slider
            {
                bindingPath = "_chkrHollowPuffBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            cheekLabelRow.Add(cheekLeftLabel);
            cheekLabelRow.Add(cheekRightLabel);
            cheeksFoldout.Add(cheekLabelRow);
            cheekRow.Add(cheekLabel);
            cheekRow.Add(cheekHollowPuffLeft);
            cheekRow.Add(mirrorCheek);
            cheekRow.Add(cheekHollowPuffRight);
            cheeksFoldout.Add(cheekRow);

            controls.Add(cheeksFoldout);
            #endregion

            #region JAW
            Foldout jawFoldout = new Foldout
            {
                text = "JAW"
            };

            Slider jawCloseOpen = new Slider
            {
                bindingPath = "_jawOpenCloseBlendValue",
                label = "Close/Open",
                showInputField = true,
                lowValue = -1,
                highValue = 1
            };

            Slider jawLeftRight = new Slider
            {
                bindingPath = "_jawLeftRightBlendValue",
                label = "Left/Right",
                showInputField = true,
                lowValue = -1,
                highValue = 1
            };

            Slider jawBackwardForward = new Slider
            {
                bindingPath = "_jawBackForwardBlendValue",
                label = "Backward/Forward",
                showInputField = true,
                lowValue = -1,
                highValue = 1
            };

            jawFoldout.Add(jawCloseOpen);
            jawFoldout.Add(jawLeftRight);
            jawFoldout.Add(jawBackwardForward);

            controls.Add(jawFoldout);
            #endregion

            #region MOUTH
            Foldout mouthFoldout = new Foldout
            {
                text = "MOUTH"
            };

            Slider mouthLeftRight = new Slider
            {
                bindingPath = "_mthLeftRightBlendValue",
                label = "Left/Right",
                showInputField = true,
                lowValue = -1,
                highValue = 1
            };

            Slider mouthFunnel = new Slider
            {
                bindingPath = "_mthFunnelBlendValue",
                label = "Funnel",
                showInputField = true,
                lowValue = 0,
                highValue = 1
            };

            Slider mouthPucker = new Slider
            {
                bindingPath = "_mthPuckerBlendValue",
                label = "Pucker",
                showInputField = true,
                lowValue = 0,
                highValue = 1
            };

            Slider mouthShrugUpper = new Slider
            {
                bindingPath = "_mthShrugUpperBlendValue",
                label = "Shrug Upper",
                showInputField = true,
                lowValue = 0,
                highValue = 1
            };

            Slider mouthShrugLower = new Slider
            {
                bindingPath = "_mthShrugLowerBlendValue",
                label = "Shrug Lower",
                showInputField = true,
                lowValue = 0,
                highValue = 1
            };

            Slider mouthRollUpper = new Slider
            {
                bindingPath = "_mthRollUpperBlendValue",
                label = "Roll Upper",
                showInputField = true,
                lowValue = 0,
                highValue = 1
            };

            Slider mouthRollOutUpper = new Slider
            {
                bindingPath = "_mthRollOutUpperBlendValue",
                label = "Roll Out Upper",
                showInputField = true,
                lowValue = 0,
                highValue = 1
            };

            Slider mouthRollLower = new Slider
            {
                bindingPath = "_mthRollLowerBlendValue",
                label = "Roll Lower",
                showInputField = true,
                lowValue = 0,
                highValue = 1
            };

            Slider mouthRollOutLower = new Slider
            {
                bindingPath = "_mthRollOutLowerBlendValue",
                label = "Roll Out Lower",
                showInputField = true,
                lowValue = 0,
                highValue = 1
            };

            Slider mouthClose = new Slider
            {
                bindingPath = "_mthCloseBlendValue",
                label = "Close",
                showInputField = true,
                lowValue = 0,
                highValue = 1
            };

            VisualElement mouthLabelRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label mouthLeftLabel = new Label("LEFT")
            {
                style =
                {
                    flexGrow = 1,
                    marginLeft = 70,
                    alignSelf = new StyleEnum<Align>(Align.Center),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter)
                }
            };
            Label mouthRightLabel = new Label("RIGHT")
            {
                style =
                {
                    flexGrow = 1,
                    alignSelf = new StyleEnum<Align>(Align.Center),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter)
                }
            };

            VisualElement mouthSmileRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label mouthSmileLabel = new Label("Frown/Smile")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider mouthFrownSmileLeft = new Slider
            {
                bindingPath = "_mthlFrownSmileBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            Button mirrorMouthSmile = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorMouthSmile.Add(unlinkImage);

            Slider mouthFrownSmileRight = new Slider
            {
                bindingPath = "_mthrFrownSmileBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            VisualElement mouthPressRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label mouthPressLabel = new Label("Press/Stretch")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider mouthPressStretchLeft = new Slider
            {
                bindingPath = "_mthlPressStretchBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            Button mirrorPress = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorPress.Add(unlinkImage);

            Slider mouthPressStretchRight = new Slider
            {
                bindingPath = "_mthrPressStretchBlendValue",
                showInputField = true,
                lowValue = -1,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            VisualElement mouthUpperUpRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label mouthUpperUpLabel = new Label("Upper Up")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider mouthUpperUpLeft = new Slider
            {
                bindingPath = "_mthlUpperUpBlendValue",
                showInputField = true,
                lowValue = 0,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            Button mirrorUpperUp = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorUpperUp.Add(unlinkImage);

            Slider mouthUpperUpRight = new Slider
            {
                bindingPath = "_mthrUpperUpBlendValue",
                showInputField = true,
                lowValue = 0,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            VisualElement mouthLowerDownRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Label mouthLowerDownLabel = new Label("Lower Down")
            {
                style =
                {
                    width = 120,
                    marginLeft = 2
                }
            };

            Slider mouthLowerDownLeft = new Slider
            {
                bindingPath = "_mthlLowerDownBlendValue",
                showInputField = true,
                lowValue = 0,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };

            Button mirrorLowerDown = new Button()
            {
                style =
                {
                    width = 28,
                    height = 20,
                    marginLeft = Length.Percent(3),
                    marginRight = Length.Percent(3)
                }
            };

            mirrorLowerDown.Add(unlinkImage);

            Slider mouthLowerDownRight = new Slider
            {
                bindingPath = "_mthrLowerDownBlendValue",
                showInputField = true,
                lowValue = 0,
                highValue = 1,
                style =
                {
                    flexGrow = 1
                }
            };








            mouthFoldout.Add(mouthLeftRight);
            mouthFoldout.Add(mouthFunnel);
            mouthFoldout.Add(mouthPucker);
            mouthFoldout.Add(mouthShrugUpper);
            mouthFoldout.Add(mouthShrugLower);
            mouthFoldout.Add(mouthRollUpper);
            mouthFoldout.Add(mouthRollOutUpper);
            mouthFoldout.Add(mouthRollLower);
            mouthFoldout.Add(mouthRollOutLower);
            mouthFoldout.Add(mouthClose);

            mouthLabelRow.Add(mouthLeftLabel);
            mouthLabelRow.Add(mouthRightLabel);
            mouthFoldout.Add(mouthLabelRow);
            mouthSmileRow.Add(mouthSmileLabel);
            mouthSmileRow.Add(mouthFrownSmileLeft);
            mouthSmileRow.Add(mirrorMouthSmile);
            mouthSmileRow.Add(mouthFrownSmileRight);
            mouthFoldout.Add(mouthSmileRow);
            mouthPressRow.Add(mouthPressLabel);
            mouthPressRow.Add(mouthPressStretchLeft);
            mouthPressRow.Add(mirrorPress);
            mouthPressRow.Add(mouthPressStretchRight);
            mouthFoldout.Add(mouthPressRow);
            mouthUpperUpRow.Add(mouthUpperUpLabel);
            mouthUpperUpRow.Add(mouthUpperUpLeft);
            mouthUpperUpRow.Add(mirrorUpperUp);
            mouthUpperUpRow.Add(mouthUpperUpRight);
            mouthFoldout.Add(mouthUpperUpRow);
            mouthLowerDownRow.Add(mouthLowerDownLabel);
            mouthLowerDownRow.Add(mouthLowerDownLeft);
            mouthLowerDownRow.Add(mirrorLowerDown);
            mouthLowerDownRow.Add(mouthLowerDownRight);
            mouthFoldout.Add(mouthLowerDownRow);

            controls.Add(mouthFoldout);
            #endregion

            #region TONGUE
            Foldout tongueFoldout = new Foldout
            {
                text = "TONGUE"
            };

            Slider tongueDownUp = new Slider
            {
                bindingPath = "_tongDownUpBlendValue",
                label = "Down/Up",
                showInputField = true,
                lowValue = -1,
                highValue = 1
            };

            Slider tongueInOut = new Slider
            {
                bindingPath = "_tongInOutBlendValue",
                label = "In/Out",
                showInputField = true,
                lowValue = -1,
                highValue = 1
            };

            Slider tongueLowerRaise = new Slider
            {
                bindingPath = "_tongLowerRaiseBlendValue",
                label = "Lower/Raise",
                showInputField = true,
                lowValue = -1,
                highValue = 1
            };

            Slider tongueTwistLeftRight = new Slider
            {
                bindingPath = "_tongTwistLeftRightBlendValue",
                label = "Twist Left/Right",
                showInputField = true,
                lowValue = -1,
                highValue = 1
            };

            Slider tongueCurlDownUp = new Slider
            {
                bindingPath = "_tongCurlDownUpBlendValue",
                label = "Curl Down/Up",
                showInputField = true,
                lowValue = -1,
                highValue = 1
            };

            Slider tongueCurlLeftRight = new Slider
            {
                bindingPath = "_tongCurlLeftRightBlendValue",
                label = "Curl Left/Right",
                showInputField = true,
                lowValue = -1,
                highValue = 1
            };

            Slider tongueCurlSideDownUp = new Slider
            {
                bindingPath = "_tongCurlSideDownUpBlendValue",
                label = "Curl Side Down/Up",
                showInputField = true,
                lowValue = -1,
                highValue = 1
            };

            tongueFoldout.Add(tongueDownUp);
            tongueFoldout.Add(tongueInOut);
            tongueFoldout.Add(tongueLowerRaise);
            tongueFoldout.Add(tongueTwistLeftRight);
            tongueFoldout.Add(tongueCurlDownUp);
            tongueFoldout.Add(tongueCurlLeftRight);
            tongueFoldout.Add(tongueCurlSideDownUp);

            controls.Add(tongueFoldout);
            #endregion

            root.Add(controls);

            return root;
        }
    }
}
#endif
