using System.Collections.Generic;
using UnityEngine;

namespace Pico.Avatar.Sample
{
    public class ActionAvatarMirror : MonoBehaviour
    {
        #region Public Properties
        public ActionAvatar localAvatar;
        #endregion


        #region Private Fields

        private bool enablePlaceHolder = false;

        private PicoAvatar target = null;

        private PicoAvatarPlaceholderRef _placeholderRef = null;

        private Transform entityHolderTrans = null;

        private Transform[] _avatarLods = new Transform[(int)AvatarLodLevel.Count];

        Dictionary<int, MeshRenderer> _mirrorRendererMap = new Dictionary<int, MeshRenderer>();
        private Dictionary<PicoAvatarRenderMesh, SkinnedMeshRenderer> _mirrorSkinnedRendererMap = new Dictionary<PicoAvatarRenderMesh, SkinnedMeshRenderer>();
        private AvatarSkeleton _mirroredAvatarSkeleton = null;

        private AvatarLodLevel currentLod = AvatarLodLevel.Invisible;

        // Keep track of target avatar id, so we know that if avatar id has been changed.
        private string targetAvatarId;

        //copy target criticalJoints
        private JointType[] criticalJoints;
        private GameObject[] criticalJointObjects;
        private XForm[] criticalJointXForms;

        // time to push away.
        public float mirrorPushAwayTime = 0.5f;
        public float minDistToMirror = -100f;
        public float maxDistToMirror = 100f;
        public float desiredDistToMirror = 2.35f;
        public Vector3 disiredMyDirection = new Vector3(.0f, 0.0f, 1.0f);
        // for debug purpose
        public bool applyBlendShape = true;

        private bool _isMirrorMovingAway = false;
        private Vector3 _mirrorTargetPosition = Vector3.zero;
        private Vector3 _mirrorFromPosition = Vector3.zero;
        private float _mirrorMoveAlpha = 0.0f;

        #endregion

        void InitBindLocalAvatar(ActionAvatar bindAvatar = null)
        {
            if (bindAvatar != null)
            {
                localAvatar = bindAvatar;
                enablePlaceHolder = localAvatar.enablePlaceHolder;
                return;
            }
        }

        public void StartAvatar(ActionAvatar bindAvatar = null)
        {
            if (localAvatar != null)
            {
                OnDispose();
            }
            InitBindLocalAvatar(bindAvatar);

            PicoAvatarManager instance = PicoAvatarManager.instance;
            if (instance != null)
                instance.OnAvatarSpecificationUpdated += OnAvatarLoaded;
        }
        void OnDispose()
        {
            PicoAvatarManager instance = PicoAvatarManager.instance;

            if (instance != null)
                instance.OnAvatarSpecificationUpdated -= OnAvatarLoaded;

            target = null;
            targetAvatarId = "";
            ClearLOD();
            ClearCriticalJoint();

        }
        void OnDestroy()
        {
            OnDispose();
        }

        private void OnAvatarLoaded(PicoAvatar avatar, int errorCode, string msg)
        {
            if (errorCode != 0)
                return;

            //if (target == null && avatar != null && avatar.capabilities.isLocalAvatar && avatar.userId != "111111111111111111") 
            if (target == null && avatar != null && avatar.userId != "111111111111111111" && avatar.userId != "1611774997722476544")
            {
                target = avatar;
                //
                target.entity.OnAvatarLodReady.AddListener(InitializeMirror);
                //
                if (target.entity.isAnyLodReady)
                {
                    InitializeMirror();
                }
            }
        }
        [ContextMenu("InitializeMirror")]
        public void InitializeMirror()
        {
            if (target != null)
            {
                Initialized(target);
            }
        }
        public void Initialized(PicoAvatar target_)
        {
            target = target_;

            BuildTransforms();
        }

        public void OnChangeCloth()
        {
            target = null;
            targetAvatarId = "";
            ClearLOD();
        }

        private void BuildTransforms()
        {
            if (entityHolderTrans == null)
            {
                var go = new GameObject("AvatarEntityHolder");

                entityHolderTrans = go.transform;
                entityHolderTrans.SetParent(this.transform);
                entityHolderTrans.localPosition = Vector3.zero;
                entityHolderTrans.localRotation = Quaternion.identity;
                entityHolderTrans.localScale = Vector3.one;
                entityHolderTrans.gameObject.layer = gameObject.layer;
            }

            if (target)
                this.criticalJoints = target.criticalJoints;
        }

        private void ClearLOD()
        {
            foreach (var d in _avatarLods)
            {
                d?.gameObject.SetActive(false);
                GameObject.DestroyImmediate(d?.gameObject);
            }
            _avatarLods = new Transform[(int)AvatarLodLevel.Count];

            _mirrorSkinnedRendererMap.Clear();
            if (_mirroredAvatarSkeleton != null)
            {
                _mirroredAvatarSkeleton.Release();
                _mirroredAvatarSkeleton = null;
            }
        }

        private void Update()
        {
            //
            BuildTransforms();

            // init a place holder avatar in mirror.
            if ((target == null || target.entity == null) &&
                (enablePlaceHolder && PicoAvatarApp.instance.extraSettings.enableBuiltinAvatarPlaceHolder) &&
                _placeholderRef == null &&
                PicoAvatarManager.canLoadAvatar &&
                PicoAvatarManager.instance.builtinPlaceHolderLocalAvatar != null)
            {
                _placeholderRef = new PicoAvatarPlaceholderRef("mirror", entityHolderTrans, PicoAvatarManager.instance.builtinPlaceHolderLocalAvatar, true);
            }


            if (target?.entity == null)
            {
                return;
            }

            if (_placeholderRef != null)
            {
                _placeholderRef.Release();
                _placeholderRef = null;
            }

            if (targetAvatarId != target.avatarId)
            {
                ClearLOD();
            }
            targetAvatarId = target.avatarId;

            var lodLevel = target.entity.currentLodLevel;
            Transform src = target.entity.transform;
            //
            entityHolderTrans.localPosition = src.localPosition;
            entityHolderTrans.localRotation = src.localRotation;
            entityHolderTrans.localScale = src.localScale;

            // check state.
            if (target.entity.bodyAnimController != null)
            {
                if (!_isMirrorMovingAway)
                {
                    //
                    if (maxDistToMirror < desiredDistToMirror + minDistToMirror)
                    {
                        maxDistToMirror = desiredDistToMirror + minDistToMirror;
                    }

                    //var headPos = target.entity.bodyAnimController.GetJointWorldXForm((uint)JointType.Head).position;
                    var srcHeadPos = target.entity.transform.position;
                    srcHeadPos = target.entity.bodyAnimController.GetJointWorldXForm(JointType.Head).position;
                    var mirrorHeadPos = entityHolderTrans.parent.position + src.localRotation * entityHolderTrans.localPosition;

                    //headPos = target.entity.transform.position;
                    // hack: currently only z axis is valid.
                    var dist = new Vector3(0, 0, mirrorHeadPos.z - srcHeadPos.z);

                    // if moved too much, move back.
                    if (minDistToMirror > 0 && (Vector3.Dot(dist, disiredMyDirection) < 0.0f || dist.sqrMagnitude < minDistToMirror * minDistToMirror))
                    {
                        _isMirrorMovingAway = true;
                        _mirrorMoveAlpha = 0.0f;
                        _mirrorFromPosition = entityHolderTrans.parent.position;
                        _mirrorTargetPosition = _mirrorFromPosition + this.disiredMyDirection * desiredDistToMirror;
                        // hack: currently only z axis is valid.
                        _mirrorTargetPosition.x = _mirrorFromPosition.x;
                        _mirrorTargetPosition.y = _mirrorFromPosition.y;

                    }// if too farawary
                    else if (dist.sqrMagnitude > this.maxDistToMirror * this.maxDistToMirror)
                    {
                        _isMirrorMovingAway = true;
                        _mirrorMoveAlpha = 0.0f;
                        _mirrorFromPosition = entityHolderTrans.parent.position;
                        _mirrorTargetPosition = _mirrorFromPosition - this.disiredMyDirection * desiredDistToMirror;
                        // hack: currently only z axis is valid.
                        _mirrorTargetPosition.x = _mirrorFromPosition.x;
                        _mirrorTargetPosition.y = _mirrorFromPosition.y;
                    }
                }
                //
                if (_isMirrorMovingAway)
                {
                    // animate to target position.
                    _mirrorMoveAlpha += (1.0f / (mirrorPushAwayTime + 0.0001f)) * Time.deltaTime;
                    if (_mirrorMoveAlpha > 1.0f)
                    {
                        _isMirrorMovingAway = false;
                        _mirrorMoveAlpha = 1.0f;
                    }

                    float alpha = Mathf.Sin(_mirrorMoveAlpha * Mathf.PI * 0.5f);

                    // update mirro position.
                    var newPos = Vector3.Lerp(
                        _mirrorFromPosition, _mirrorTargetPosition, alpha);
                    entityHolderTrans.parent.position = newPos;

                }
            }

            if ((uint)lodLevel < (uint)_avatarLods.Length)
            {
                if (_avatarLods[(int)lodLevel] == null || (_avatarLods[(int)lodLevel].GetComponentInChildren<MeshRenderer>(true) == null &&
                    _avatarLods[(int)lodLevel].GetComponentInChildren<SkinnedMeshRenderer>(true) == null))
                {
                    AvatarLod source = target.entity.GetCurrentAvatarLod();
                    if (source.isPrimitivesReady)
                    {
                        LoadLod(source, lodLevel);
                    }
                }

            }
            
            if(_mirroredAvatarSkeleton != null)
            {
                _mirroredAvatarSkeleton.UpdateUnityTransformsFromNative();

                if (applyBlendShape)
                {
                    foreach (var item in _mirrorSkinnedRendererMap)
                    {
                        var weights = item.Key.blendshapeWeights;
                        var meshrenderer = item.Value;
                        var meshName = meshrenderer.name;

                        if (weights != null && meshrenderer.sharedMesh)
                        {
                            for (int i = 0; i < weights.Length; ++i)
                            {
                                meshrenderer.SetBlendShapeWeight(i, weights[i]);
                            }
                        }
                    }
                }
            }

            // lod could be negative, which doesn't make sense as array index.
            if (lodLevel >= 0 && _avatarLods[(int)lodLevel])
            {
                if (!_avatarLods[(int)lodLevel].gameObject.activeSelf)
                {
                    AvatarLod source = target.entity.GetCurrentAvatarLod();
                    //
                    if (source.isPrimitivesReady)
                    {
                        _avatarLods[(int)lodLevel]?.gameObject.SetActive(true);

                        if (lodLevel != currentLod)
                        {
                            if ((uint)currentLod < (uint)_avatarLods.Length)
                            {
                                _avatarLods[(int)currentLod]?.gameObject.SetActive(false);
                            }
                            //
                            currentLod = lodLevel;
                            //
                        }
                    }
                }
            }

            UpdateJointTrans();
        }

        private void UpdateJointTrans()
        {
            if (target == null)
                return;
            var entityAvatar = target.entity;
            if (this.criticalJoints != null && this.criticalJoints.Length > 0 && entityAvatar.bodyAnimController != null)
            {
                var length = criticalJoints.Length;
                if (criticalJointXForms == null)
                {
                    criticalJointXForms = new XForm[length];
                    criticalJointObjects = new GameObject[length];
                    for (var i = 0; i < length; ++i)
                    {
                        criticalJointObjects[i] = new GameObject(criticalJoints[i].ToString());
                        var curTran = criticalJointObjects[i].transform;
                        curTran.parent = this.entityHolderTrans;
                        curTran.localScale = Vector3.one;
                    }
                }
                entityAvatar.bodyAnimController.GetJointXForms(criticalJoints, ref criticalJointXForms);
                for (var i = 0; i < length; ++i)
                {
                    criticalJointObjects[i].transform.localPosition = criticalJointXForms[i].position;
                    criticalJointObjects[i].transform.localRotation = criticalJointXForms[i].orientation;
                }
            }
        }
        private void ClearCriticalJoint()
        {
            criticalJointXForms = null;
            if (criticalJointObjects != null)
            {
                foreach(var item in criticalJointObjects)
                {
                    GameObject.DestroyImmediate(item);
                }
            }
            criticalJointObjects = null;
        }

        private void LoadLod(AvatarLod source, AvatarLodLevel lodLevel)
        {
            if (source == null || (uint)lodLevel >= (uint)AvatarLodLevel.Count)
                return;

            GameObject go = null;
            if (_avatarLods[(int)lodLevel])
            {
                go = _avatarLods[(int)lodLevel].gameObject;
            }
            else
            {
                go = new GameObject(string.Format("Lod{0}", lodLevel));
            }
            go.hideFlags = HideFlags.DontSave;
            go.layer = gameObject.layer;

            // inactive in default 
            go.SetActive(false);

            // add game object to entity.
            Transform lodTransform = go.transform;
            lodTransform.parent = entityHolderTrans ? entityHolderTrans : this.transform;
            lodTransform.localPosition = Vector3.zero;
            lodTransform.localRotation = Quaternion.identity;
            lodTransform.localScale = Vector3.one;

            _avatarLods[(int)lodLevel] = lodTransform;

            // create skeleton
            if (source.avatarSkeleton != null && _mirroredAvatarSkeleton == null)
            {
                _mirroredAvatarSkeleton = source.avatarSkeleton.CloneAsRef();
                _mirroredAvatarSkeleton.Retain();
                //
                _mirroredAvatarSkeleton.CreateTransforms(lodTransform);
                // update first.
                _mirroredAvatarSkeleton.UpdateUnityTransformsFromNative();
            }

            var renderMeshes = source.transform.GetComponentsInChildren<PicoPrimitiveRenderMesh>(true);

            for (int i = 0; i < renderMeshes.Length; ++i)
            {
                var sourceRenderMesh = renderMeshes[i];
                var renderGo = new GameObject(sourceRenderMesh.name);
                var renderTrans = renderGo.transform;

                renderTrans.parent = lodTransform;
                renderTrans.localPosition = sourceRenderMesh.transform.localPosition;
                renderTrans.localRotation = sourceRenderMesh.transform.localRotation;
                renderTrans.localScale = sourceRenderMesh.transform.localScale;

                renderTrans.gameObject.SetActive(sourceRenderMesh.transform.gameObject.activeSelf);
                
                if (renderTrans.gameObject.activeSelf == false 
                    && (sourceRenderMesh.primitive.nodeTypes & (uint)AvatarNodeTypes.Head) == 1)
                {
                    renderTrans.gameObject.SetActive(true);
                }

                var renderer = sourceRenderMesh.meshRenderer;
                var skinMeshRenderer = sourceRenderMesh.skinMeshRenderer;
                // get additive skeleton.
                var avatarSkeleton = _mirroredAvatarSkeleton.GetAdditiveSkeleton(sourceRenderMesh.avatarSkeleton.nativeHandle);
                if (avatarSkeleton == null)
                {
                    avatarSkeleton = _mirroredAvatarSkeleton;
                }

                if (renderer != null)
                {
                    var newRenderer = renderGo.AddComponent<MeshRenderer>();
                    newRenderer.sharedMaterials = renderer.sharedMaterials;

                    var filter = sourceRenderMesh.GetComponent<MeshFilter>();
                    var newFilter = renderGo.AddComponent<MeshFilter>();
                    newFilter.sharedMesh = filter?.sharedMesh;

                    // add a empty MaterialPropertyBlock to make renderer incompatible with the SRP Batcher
                    newRenderer.SetPropertyBlock(new MaterialPropertyBlock());

                    _mirrorRendererMap.Add(sourceRenderMesh.GetHashCode(), newRenderer);

                    var newParentTrans = _mirroredAvatarSkeleton.GetTransform((int)Pico.Avatar.Utility.AddNameToIDNameTable(renderer.transform.parent.name));
                    renderGo.transform.parent = newParentTrans;
                }
                else if (skinMeshRenderer != null && avatarSkeleton != null && sourceRenderMesh.avatarMeshBuffer != null)
                {
                    var newRenderer = renderGo.AddComponent<SkinnedMeshRenderer>();
                    newRenderer.sharedMaterials = skinMeshRenderer.sharedMaterials;

                    newRenderer.sharedMesh = skinMeshRenderer.sharedMesh;
                    //newRenderer.localBounds = skinMeshRenderer.localBounds;
                    //newRenderer.bounds = skinMeshRenderer.bounds;

                    //
                    {
                        var boneNameHahes = sourceRenderMesh.avatarMeshBuffer.boneNameHashes;
                        var bones = new Transform[boneNameHahes.Length];
                        //
                        for (int b = 0; b < boneNameHahes.Length; ++b)
                        {
                            var trans = avatarSkeleton.GetTransform(boneNameHahes[b]);
                            if (trans == null)
                            {
                                AvatarEnv.Log(DebugLogMask.GeneralError, "transform for a bone not found!");
                            }
                            bones[b] = trans;
                        }

                        newRenderer.rootBone = bones[0];
                        newRenderer.bones = bones;
                    }

                    _mirrorSkinnedRendererMap.Add(sourceRenderMesh, newRenderer);
                    //
                }

                // copy transform.
                {
                    renderGo.transform.localPosition = sourceRenderMesh.transform.localPosition;
                    renderGo.transform.localScale = sourceRenderMesh.transform.localScale;
                    renderGo.transform.localRotation = sourceRenderMesh.transform.localRotation;
                }

                sourceRenderMesh.onMaterialsUpdate -= OnMaterialsUpdate;
                sourceRenderMesh.onMaterialsUpdate += OnMaterialsUpdate;

                sourceRenderMesh.onMeshUpdate -= OnMeshUpdate;
                sourceRenderMesh.onMeshUpdate += OnMeshUpdate;
            }

            var mergedRenderMeshs = source.transform.GetComponentsInChildren<PicoAvatarMergedRenderMesh>(true);
            foreach (var mergedRenderMesh in mergedRenderMeshs)
            {
                var renderGo = new GameObject(mergedRenderMesh.name + "_Mirror");
                var renderTrans = renderGo.transform;

                var nameHash = Utility.AddNameToIDNameTable(mergedRenderMesh.transform.parent.name);
                var parentMirrorTrans = _mirroredAvatarSkeleton.GetTransform((int)nameHash);

                renderTrans.parent = parentMirrorTrans == null ? lodTransform : parentMirrorTrans;
                renderTrans.localPosition = mergedRenderMesh.transform.localPosition;
                renderTrans.localRotation = mergedRenderMesh.transform.localRotation;
                renderTrans.localScale = mergedRenderMesh.transform.localScale;
                renderTrans.gameObject.SetActive(mergedRenderMesh.transform.gameObject.activeSelf);

                // var avatarSkeleton = mirroredAvatarLodSkeleton.GetAdditiveSkeleton(mergedRenderMesh.avatarSke)
                var skinnedMeshRenderer = mergedRenderMesh.skinnedMeshRenderer;
                if (skinnedMeshRenderer != null)
                {
                    var selfRenderer = renderGo.AddComponent<SkinnedMeshRenderer>();
                    selfRenderer.sharedMaterial = mergedRenderMesh.skinnedMeshRenderer.sharedMaterial;
                    selfRenderer.sharedMesh = mergedRenderMesh.meshBuffer.mesh;
                    selfRenderer.localBounds = mergedRenderMesh.meshBuffer.mesh.bounds;

                    {
                        var boneNameHashes = mergedRenderMesh.meshBuffer.boneNameHashes;
                        var bones = new Transform[boneNameHashes.Length];
                        for (int i = 0; i < boneNameHashes.Length; i++)
                        {
                            var trans = _mirroredAvatarSkeleton.GetTransform(boneNameHashes[i]);
                            if (trans == null)
                            {
                                AvatarEnv.Log(DebugLogMask.GeneralError, "transform for a bone not found!");
                            }
                            bones[i] = trans;
                        }

                        selfRenderer.rootBone = _mirroredAvatarSkeleton.rootTransform;
                        selfRenderer.bones = bones;
                    }
                }
            }
        }


        public GameObject GetJointObject(JointType jointType)
        {
            if (criticalJointObjects == null || criticalJoints == null)
                return null;
            for (int i = 0; i < criticalJoints.Length; i++)
            {
                if (jointType == criticalJoints[i])
                {
                    if (criticalJointObjects.Length > i)
                        return criticalJointObjects[i];
                    break;
                }
            }
            return null;
        }

        void OnMaterialsUpdate(PicoAvatarRenderMesh renderMesh)
        {
            MeshRenderer meshRenderer;
            if (_mirrorRendererMap.TryGetValue(renderMesh.GetHashCode(), out meshRenderer))
            {
                meshRenderer.sharedMaterials = renderMesh.meshRenderer.sharedMaterials;
            }
            else
            {
                SkinnedMeshRenderer skinnedMeshRenderer;
                if (_mirrorSkinnedRendererMap.TryGetValue(renderMesh, out skinnedMeshRenderer))
                {
                    skinnedMeshRenderer.sharedMaterials = renderMesh.skinMeshRenderer.sharedMaterials;
                }
            }
        }
        void OnMeshUpdate(PicoAvatarRenderMesh renderMesh)
        {
            MeshRenderer meshRenderer;
            if (_mirrorRendererMap.TryGetValue(renderMesh.GetHashCode(), out meshRenderer))
            {
                if (renderMesh.meshRenderer != null)
                {
                    meshRenderer.GetComponent<MeshFilter>().sharedMesh = renderMesh.meshRenderer.GetComponent<MeshFilter>().sharedMesh;
                }
            }
            else
            {
                SkinnedMeshRenderer skinnedMeshRenderer;
                if (_mirrorSkinnedRendererMap.TryGetValue(renderMesh, out skinnedMeshRenderer))
                {
                    if (renderMesh.skinMeshRenderer != null)
                    {
                        skinnedMeshRenderer.sharedMesh = renderMesh.skinMeshRenderer.sharedMesh;
                    }
                }
            }
        }
    }
}
