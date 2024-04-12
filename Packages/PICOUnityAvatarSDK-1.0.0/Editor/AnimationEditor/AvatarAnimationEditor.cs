using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
using Pico.Avatar;
using UnityEngine.Networking;
using UnityEngine.Rendering;

namespace Pico
{
    namespace Avatar
    {
        public partial class AvatarAnimationEditor : EditorWindow
        {
            bool _started = false;
            Camera _camera;
            RuntimeBehaviour _runtimeBehaviour;
            SerializedObject _runtimeBehaviourObject;
            const string _editorScenePath = "Packages/org.byted.avatar.sdk/Editor/AnimationEditor/EditorResources/AvatarAnimationEditor.unity";
            const string _sceneName = "AvatarAnimationEditor";
            const string _avatarAppPrefabPath = "Packages/org.byted.avatar.sdk/Editor/AnimationEditor/EditorResources/AvatarAnimationEditor_PicoAvatarApp.prefab";
            const int _avatarLayer = 8;
            const int _jointLayer = 9;
            PicoAvatarApp _app = null;
            bool _avatarEnvReady = false;
            const string _userId = "8388ef7f-a01c-461b-83e4-763329e20612";
   
            string _bodyId = "1411856801264283648";
            string _faceId = "1411889538511646720";    
            string _topId = "1413305607298191360";
            string _bottomId = "1411925330923597824";
            string _shoesId = "1411925731026644992";
            string _hairId = "1411911742553042944";
            string _boneId = "1353565285274218496";
            string _eyebrowId = "1411898620169662464";

            string _clothId = "";


            const string _avatarSpec1 = "{\"info\":{\"sex\":\"female\",\"background\":{\"color\":[255,255,0],\"image\":\"https://dfsedffe.png\"}},\"graph\":{\"type\":\"PicoAvatar\",\"label\":\"generalassetgraphforpicoavatar\",\"directed\":true,\"nodes\":{\"1\":{\"label\":" +
                "\"Body\",\"metadata\":{\"uuid\":\"{Replace_1}\",\"category\":\"Body\",\"tag\":\"Body_dev\",\"incompatibleTags\":[\"hair_longBangs\"],\"pins\":{\"root\":{\"type\":\"transform\",\"entityName\":\"root\"}},\"colors\":{\"circleColor\":[1,1,1,1]},\"textures\":{\"signature\":\"fsegisjf\"},\"blendshapes\":{\"mouthOpen\":0.8},\"boneDisplacements\":{\"nose\":0.3},\"animations\":{\"gesture1\":\"fsegfsi\"}}},\"2\":{\"label\":" +
                "\"Head\",\"metadata\":{\"uuid\":\"{Replace_0}\",\"category\":\"Head\",\"tag\":\"Head_dev\",\"incompatibleTags\":[\"hair_longBangs\"],\"pins\":{\"root\":{\"type\":\"transform\",\"entityName\":\"root\"}},\"colors\":{\"circleColor\":[1,1,1,1]},\"textures\":{\"signature\":\"fsegisjf\"},\"blendshapes\":{\"mouthOpen\":0.8},\"boneDisplacements\":{\"nose\":0.3},\"animations\":{\"gesture1\":\"fsegfsi\"}}},\"3\":{\"label\":" +
                "\"Cloth\",\"metadata\":{\"uuid\":\"{Replace_3}\",\"category\":\"Cloth\",\"tag\":\"Cloth_dev\",\"incompatibleTags\":[\"hair_longBangs\"],\"pins\":{\"root\":{\"type\":\"transform\",\"entityName\":\"root\"}},\"colors\":{\"circleColor\":[1,1,1,1]},\"textures\":{\"signature\":\"fsegisjf\"},\"blendshapes\":{\"mouthOpen\":0.8},\"boneDisplacements\":{\"nose\":0.3},\"animations\":{\"gesture1\":\"fsegfsi\"}}},\"4\":{\"label\":" +
                "\"Hair\",\"metadata\":{\"uuid\":\"{Replace_2}\",\"category\":\"Hair\",\"tag\":\"Hair_dev\",\"incompatibleTags\":[\"hair_longBangs\"],\"pins\":{\"root\":{\"type\":\"transform\",\"entityName\":\"root\"}},\"colors\":{\"UserDefine_BaseColor\":[255,0,0]},\"textures\":{\"signature\":\"fsegisjf\"},\"blendshapes\":{\"mouthOpen\":0.8},\"boneDisplacements\":{\"nose\":0.3},\"animations\":{\"gesture1\":\"fsegfsi\"}}},\"5\":{\"label\":" +
                "\"Skeleton\",\"metadata\":{\"uuid\":\"{Replace_4}\",\"tag\":\"Skeleton\",\"category\":\"Skeleton\",\"incompatibleTags\":[\"hair_longBangs\"],\"pins\":{\"root\":{\"type\":\"transform\",\"entityName\":\"root\"}},\"colors\":{\"circleColor\":[1,1,1,1]},\"textures\":{\"signature\":\"fsegisjf\"},\"blendshapes\":{\"mouthOpen\":0.8},\"boneDisplacements\":{\"nose\":0.3}}}}}}";

  

            const string _avatarSpec2 = "{\"info\": {\"sex\": \"female\", \"continent\": \"AS\", \"background\": {\"image\": \"https://dfsedffe.png\", \"end_color\": [255, 168, 131], \"start_color\": [228, 140, 177]}, \"avatar_type\": \"preset\"}, \"graph\": {\"type\": \"PicoAvatar\", \"label\": \"general asset graph for pico avatar\", \"nodes\": " +
                "{\"1\": {\"label\": \"Body_Female\", \"metadata\": {\"tag\": \"Body_dev\", \"pins\": {\"root\": {\"type\": \"transform\", \"entityName\": \"root\"}}, \"uuid\": \"{Replace_0}\", \"colors\": {\"circleColor\": [1, 1, 1, 1]}, \"category\": \"Body_Female\", \"textures\": {\"signature\": \"fsegisjf\"}, \"animations\": {\"gesture1\": \"fsegfsi\"}, \"blendshapes\": {\"mouthOpen\": 0.8}, \"incompatibleTags\": [\"hair_longBangs\"], \"boneDisplacements\": {\"nose\": 0.3}}}, " +
                "\"2\": {\"label\": \"FacialFeature_Female\", \"metadata\": {\"tag\": \"Head_dev\", \"pins\": {\"root\": {\"type\": \"transform\", \"entityName\": \"root\"}}, \"uuid\": \"{Replace_1}\", \"colors\": {\"circleColor\": [1, 1, 1, 1]}, \"category\": \"FacialFeature_Female\", \"textures\": {\"signature\": \"fsegisjf\"}, \"animations\": {\"gesture1\": \"fsegfsi\"}, \"blendshapes\": {\"mouthOpen\": 0.8}, \"incompatibleTags\": [\"hair_longBangs\"], \"boneDisplacements\": {\"nose\": 0.3}}}, " +
                "\"3\": {\"label\": \"Top_Female\", \"metadata\": {\"tag\": \"Cloth_dev\", \"pins\": {\"root\": {\"type\": \"transform\", \"entityName\": \"root\"}}, \"uuid\": \"{Replace_2}\", \"colors\": {\"circleColor\": [1, 1, 1, 1]}, \"category\": \"Top_Female\", \"textures\": {\"signature\": \"fsegisjf\"}, \"animations\": {\"gesture1\": \"fsegfsi\"}, \"blendshapes\": {\"mouthOpen\": 0.8}, \"incompatibleTags\": [\"hair_longBangs\"], \"boneDisplacements\": {\"nose\": 0.3}}}, " +
                "\"4\": {\"label\": \"Bottom_Female\", \"metadata\": {\"tag\": \"Cloth_dev\", \"pins\": {\"root\": {\"type\": \"transform\", \"entityName\": \"root\"}}, \"uuid\": \"{Replace_3}\", \"colors\": {\"circleColor\": [1, 1, 1, 1]}, \"category\": \"Bottom_Female\", \"textures\": {\"signature\": \"fsegisjf\"}, \"animations\": {\"gesture1\": \"fsegfsi\"}, \"blendshapes\": {\"mouthOpen\": 0.8}, \"incompatibleTags\": [\"hair_longBangs\"], \"boneDisplacements\": {\"nose\": 0.3}}}, " +
                "\"5\": {\"label\": \"Shoes_Female\", \"metadata\": {\"tag\": \"Cloth_dev\", \"pins\": {\"root\": {\"type\": \"transform\", \"entityName\": \"root\"}}, \"uuid\": \"{Replace_4}\", \"colors\": {\"circleColor\": [1, 1, 1, 1]}, \"category\": \"Shoes_Female\", \"textures\": {\"signature\": \"fsegisjf\"}, \"animations\": {\"gesture1\": \"fsegfsi\"}, \"blendshapes\": {\"mouthOpen\": 0.8}, \"incompatibleTags\": [\"hair_longBangs\"], \"boneDisplacements\": {\"nose\": 0.3}}}, " +
                "\"6\": {\"label\": \"HairStyle_Female\", \"metadata\": {\"tag\": \"Hair_dev\", \"pins\": {\"root\": {\"type\": \"transform\", \"entityName\": \"root\"}}, \"uuid\": \"{Replace_5}\", \"colors\": {\"UserDefine_BaseColor\": [30, 30, 30]}, \"category\": \"HairStyle_Female\", \"textures\": {\"signature\": \"fsegisjf\"}, \"animations\": {\"gesture1\": \"fsegfsi\"}, \"blendshapes\": {\"mouthOpen\": 0.8}, \"incompatibleTags\": [\"hair_longBangs\"], \"boneDisplacements\": {\"nose\": 0.3}}}, " +
                "\"7\": {\"label\": \"Skeleton_Female\", \"metadata\": {\"tag\": \"Skeleton_Female\", \"pins\": {\"root\": {\"type\": \"transform\", \"entityName\": \"root\"}}, \"uuid\": \"{Replace_6}\", \"colors\": {\"circleColor\": [1, 1, 1, 1]}, \"category\": \"Skeleton\", \"textures\": {\"signature\": \"fsegisjf\"}, \"blendshapes\": {\"mouthOpen\": 0.8}, \"incompatibleTags\": [\"hair_longBangs\"], \"boneDisplacements\": {\"nose\": 0.3}}}, " +
                "\"8\": {\"label\": \"Eyebrow_Female\", \"metadata\": {\"tag\": \"Head_dev\", \"pins\": {\"root\": {\"type\": \"transform\", \"entityName\": \"root\"}}, \"uuid\": \"{Replace_7}\", \"colors\": {\"circleColor\": [1, 1, 1, 1]}, \"category\": \"Eyebrow_Female\", \"textures\": {\"signature\": \"fsegisjf\"}, \"animations\": {\"gesture1\": \"fsegfsi\"}, \"blendshapes\": {\"mouthOpen\": 0.8}, \"incompatibleTags\": [\"hair_longBangs\"], \"boneDisplacements\": {\"nose\": 0.3}}}}, \"directed\": true, \"avatar_style\": \"PicoAvatar2\"}}";


            private const string _officialMale =
                "{\"info\":{\"sex\":\"male\",\"status\":\"Online\",\"tag_list\":[\"Common\"],\"continent\":\"EU\",\"background\":{\"image\":\"https://dfsedffe.png\",\"end_color\":[133,182,255],\"start_color\":[148,111,253]},\"avatar_type\":\"preset\"},\"avatar\":{\"body\":{\"version\":1,\"perParams\":[],\"technique\":\"Pico2-Bone\",\"floatIdParams\":[]},\"head\":{\"version\":1,\"perParams\":[],\"technique\":\"Pico2-BS\",\"floatIdParams\":[]},\"skin\":{\"color\":\"\",\"white\":0,\"softening\":0},\"assetPins\":[],\"nextWearTimeStamp\":25},\"avatarStyle\":\"PicoAvatar3\"}";

            private const string _officialFemale =
                "{\"info\":{\"sex\":\"female\",\"status\":\"Online\",\"tag_list\":[\"Common\"],\"continent\":\"EU\",\"background\":{\"image\":\"https://dfsedffe.png\",\"end_color\":[133,182,255],\"start_color\":[148,111,253]},\"avatar_type\":\"preset\"},\"avatar\":{\"body\":{\"version\":1,\"perParams\":[],\"technique\":\"Pico2-Bone\",\"floatIdParams\":[]},\"head\":{\"version\":1,\"perParams\":[],\"technique\":\"Pico2-BS\",\"floatIdParams\":[]},\"skin\":{\"color\":\"\",\"white\":0,\"softening\":0},\"assetPins\":[],\"nextWearTimeStamp\":25},\"avatarStyle\":\"PicoAvatar3\"}";
                
            
            
            IEnumerator _loadAvatarCoroutine = null;
            PicoAvatar _avatar = null;
            AvatarBodyAnimController _bodyAnimController = null;
            AvatarAnimationLayer _animationLayer = null;
            float _rotation = 0;
            readonly string[] _jointNames = {
                "Hips", "Spine1_skin", "Spine2_skin", "Chest", "Neck_skin", "Head",
                "Upleg_L_skin", "Leg_L_skin", "Foot_L", "Toes_L",
                "Upleg_R_skin", "Leg_R_skin", "Foot_R", "Toes_R",
                "Shoulder_L_skin", "Arm_L_skin", "ForeArm_L_skin", "Hand_L", "MiddleFinger1_L",
                "Shoulder_R_skin", "Arm_R_skin", "ForeArm_R_skin", "Hand_R", "MiddleFinger1_R",
            };
            readonly int[] _jointParents = {
                -1, 0, 1, 2, 3, 4,
                0, 6, 7, 8,
                0, 10, 11, 12,
                3, 14, 15, 16, 17,
                3, 19, 20, 21, 22,
            };
            bool _drawJoints = false;
            Camera _jointCamera = null;
            Mesh _sphere = null;
            Mesh _cone = null;
            Material _jointMaterial = null;

            void OnStart()
            {
                Debug.Log("AvatarAnimationEditor OnStart");

                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
                RenderPipelineManager.endCameraRendering += OnEndCameraRendering;

                Start();
            }

            void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
            {
                if (camera == _jointCamera)
                {
                    GL.wireframe = true;
                }
            }

            void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
            {
                if (camera == _jointCamera)
                {
                    GL.wireframe = false;
                }
            }

            void OnPlayModeStateChanged(PlayModeStateChange mode)
            {
                if (mode == PlayModeStateChange.ExitingPlayMode)
                {
                    Close();
                }
            }

            void OnDestroy()
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
                RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;

                if (Application.isPlaying)
                {
                    EditorApplication.ExitPlaymode();
                }
            }

            class RuntimeBehaviour : MonoBehaviour
            {
                public AvatarAnimationEditor owner;
                public Transform target;
                public AnimationClip[] clips;

                void Update()
                {
                    owner.Update();
                }
            }

            void Start()
            {
                _camera = new GameObject("camera").AddComponent<Camera>();
                _camera.tag = "MainCamera";
                _camera.transform.localPosition = new Vector3(0, 1, 2);
                _camera.transform.localRotation = Quaternion.Euler(4, 180, 0);
                _camera.cullingMask = 1 << _avatarLayer;
                _camera.depth = 0;

                var desc = new RenderTextureDescriptor(2048, 2048, RenderTextureFormat.ARGBHalf, 32, 0);
                desc.msaaSamples = 4;
                _renderTexture = new RenderTexture(desc);
                _camera.targetTexture = _renderTexture;

                var light1 = new GameObject("light1").AddComponent<Light>();
                light1.transform.localRotation = Quaternion.Euler(30, 200, 0);
                light1.type = LightType.Directional;
                var light2 = new GameObject("light2").AddComponent<Light>();
                light2.transform.localRotation = Quaternion.Euler(200, -120, 0);
                light2.type = LightType.Directional;

                _runtimeBehaviour = _camera.gameObject.AddComponent<RuntimeBehaviour>();
                _runtimeBehaviour.owner = this;
                _runtimeBehaviour.StartCoroutine(InitAvatarEnv());
                _runtimeBehaviourObject = new SerializedObject(_runtimeBehaviour);
            }

            void Update()
            {
                if (_drawJoints)
                {
                    DrawJoints();
                }
            }

            void DrawJoints()
            {
                if (_avatar == null)
                {
                    return;
                }

                if (_cone == null || _sphere == null)
                {
                    GameObject handleGo = (GameObject) EditorGUIUtility.Load("SceneView/HandlesGO.fbx");
                    if (handleGo)
                    {
                        foreach (Transform t in handleGo.transform)
                        {
                            if (t.name == "Cone")
                            {
                                _cone = t.GetComponent<MeshFilter>().sharedMesh;
                            }
                            else if (t.name == "Sphere")
                            {
                                _sphere = t.GetComponent<MeshFilter>().sharedMesh;
                            }
                        }
                    }
                }

                if (_cone == null || _sphere == null)
                {
                    return;
                }

                if (_jointCamera == null)
                {
                    _jointCamera = new GameObject("jointCamera").AddComponent<Camera>();
                    _jointCamera.transform.localPosition = _camera.transform.position;
                    _jointCamera.transform.localRotation = _camera.transform.rotation;
                    _jointCamera.cullingMask = 1 << _jointLayer;
                    _jointCamera.targetTexture = _renderTexture;
                    _jointCamera.depth = 1;
                    _jointCamera.clearFlags = CameraClearFlags.Nothing;
                }

                if (_jointMaterial == null)
                {
                    _jointMaterial = new Material(Shader.Find("Hidden/AvatarAnimationEditor/Joint"));
                    _jointMaterial.color = Color.white;
                }
            }

            IEnumerator InitAvatarEnv()
            {
                // create app
                var avatarApp = AssetDatabase.LoadMainAssetAtPath(_avatarAppPrefabPath) as GameObject;
                if (avatarApp == null)
                {
                    Debug.LogError("Avatar App Prefab load failed: " + _avatarAppPrefabPath);
                    yield break;
                }
                avatarApp = Instantiate(avatarApp);
                _app = avatarApp.GetComponent<PicoAvatarApp>();
                _app.appSettings.serverType = ServerType.ProductionEnv;
                _app.accessType = AccessType.ThirdApp;
                _app.extraSettings.avatarSceneLayer = _avatarLayer;
                _app.lodSettings.forceLodLevel = AvatarLodLevel.Lod0;
                
                // wait app init
                while (!PicoAvatarApp.isWorking)
                {
                    yield return null;
                }
                
                if (string.IsNullOrEmpty(_app.loginSettings.accessToken))
                {
                    
                    UnityWebRequest webRequest = UnityWebRequest.Get(NetEnvHelper.GetFullRequestUrl(NetEnvHelper.SampleTokenApi, ""));
                    webRequest.timeout = 10; // 设置超时时间为10秒
                    webRequest.SetRequestHeader("Content-Type", "application/json"); 
                    yield return webRequest.SendWebRequest();
                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log("Get Token Failed! Reason:" + webRequest.error);
                        yield break;
                    }
                    string responseText = webRequest.downloadHandler.text;
                    var token = JsonConvert.DeserializeObject<JObject>(responseText)?.Value<string>("data");
                    _app.loginSettings.accessToken = token;
                    _app.StartAvatarManager();
                }
                else
                {
                    _app.StartAvatarManager();
                }
                
                while (!PicoAvatarManager.instance.isReady)
                {
                    yield return null;
                }

                _avatarEnvReady = true;

                Debug.Log("AvatarAnimationEditor InitAvatarEnv Done");

                yield return null;
            }

            IEnumerator LoadAvatar()
            {
                // load avatar
                var capability = new AvatarCapabilities();
                capability.manifestationType = AvatarManifestationType.Full;
                capability.controlSourceType = ControlSourceType.OtherPlayer;
                capability.bodyCulling = true;

                //pico avatar 1
                //string avatarSpec = _avatarSpec1.Replace("{Replace_0}", _faceId);
                //avatarSpec = avatarSpec.Replace("{Replace_1}", _bodyId);
                //avatarSpec = avatarSpec.Replace("{Replace_2}", _hairId);
                //avatarSpec = avatarSpec.Replace("{Replace_3}", _clothId);
                //avatarSpec = avatarSpec.Replace("{Replace_4}", _boneId);

                //pico avatar 2
                string avatarSpec = _avatarSpec2.Replace("{Replace_0}", _bodyId);
                avatarSpec = avatarSpec.Replace("{Replace_1}", _faceId);
                avatarSpec = avatarSpec.Replace("{Replace_2}", _topId);
                avatarSpec = avatarSpec.Replace("{Replace_3}", _bottomId);
                avatarSpec = avatarSpec.Replace("{Replace_4}", _shoesId);
                avatarSpec = avatarSpec.Replace("{Replace_5}", _hairId);
                avatarSpec = avatarSpec.Replace("{Replace_6}", _boneId);
                avatarSpec = avatarSpec.Replace("{Replace_7}", _eyebrowId);

                PicoAvatar avatar = PicoAvatarManager.instance.LoadAvatar(new AvatarLoadContext(_userId, null, avatarSpec, capability));
                if (avatar == null)
                {
                    Debug.LogError("AvatarAnimationEditor LoadAvatar failed");
                    yield break;
                }

                Transform avatarTransform = avatar.transform;
                avatarTransform.localPosition = Vector3.zero;
                avatarTransform.localRotation = Quaternion.Euler(0, 180, 0);
                avatarTransform.localScale = Vector3.one;

                while (!avatar.entity.isAnyLodReady)
                {
                    yield return null;
                }

                Debug.Log("AvatarAnimationEditor LoadAvatar Done");

                string animations = "";
                while (string.IsNullOrEmpty(animations))
                {
                    animations = avatar.GetAnimationNames();
                    yield return null;
                }

                Debug.Log("AvatarAnimationEditor GetAnimationNames:" + animations);

                _animations = ParseAnimations(animations);
                if (_animations != null && _playAnimation)
                {
                    if (_animations.Count > 0)
                    {
                        if (_playAnimationIndex >= _animations.Count)
                        {
                            _playAnimationIndex = 0;
                        }
                        avatar.PlayAnimation(_animations[_playAnimationIndex]);
                    }
                }

                avatar.OnLoadAnimationsExternComplete += OnLoadAnimationsExternComplete;

                //avatar.AddFirstEntityReadyCallback((avatarEntity) =>
                //{

                //    //
                //    _bodyAnimController = avatarEntity?.bodyAnimController;
                //    if (_bodyAnimController == null)
                //    {
                //        return;
                //    }
                //    _animationLayer = _bodyAnimController.CreateAnimationLayerByName("AnimationPreview");
                //    Pico.Avatar.AvatarMask mask = new Pico.Avatar.AvatarMask();
                //    mask.SetJointPositionEnable(JointType.Hips, true);
                //    _animationLayer.SetAvatarMask(mask);
    
                //});

                _loadAvatarCoroutine = null;
                _avatar = avatar;
                _rotation = 0;

                yield return null;
            }
            
            IEnumerator LoadOfficialAvatar(AvatarSexType ast)
            {
                // load avatar
                var capability = new AvatarCapabilities();
                capability.manifestationType = AvatarManifestationType.Full;
                capability.controlSourceType = ControlSourceType.OtherPlayer;
                capability.bodyCulling = true;
                string avatarSpec = "";

                if (ast == AvatarSexType.Female)
                    avatarSpec = _officialFemale;
                else if (ast == AvatarSexType.Male)
                    avatarSpec = _officialMale;
                else
                    yield break;
                    

                PicoAvatar avatar = PicoAvatarManager.instance.LoadAvatar(new AvatarLoadContext(_userId, null, avatarSpec, capability));
                if (avatar == null)
                {
                    Debug.LogError("AvatarAnimationEditor LoadAvatar failed");
                    yield break;
                }

                Transform avatarTransform = avatar.transform;
                avatarTransform.localPosition = Vector3.zero;
                avatarTransform.localScale = Vector3.one;

                while (!avatar.entity.isAnyLodReady)
                {
                    yield return null;
                }

                Debug.Log("AvatarAnimationEditor LoadAvatar Done");

                string animations = "";
                while (string.IsNullOrEmpty(animations))
                {
                    animations = avatar.GetAnimationNames();
                    yield return null;
                }

                Debug.Log("AvatarAnimationEditor GetAnimationNames:" + animations);

                _animations = ParseAnimations(animations);
                if (_animations != null && _playAnimation)
                {
                    if (_animations.Count > 0)
                    {
                        if (_playAnimationIndex >= _animations.Count)
                        {
                            _playAnimationIndex = 0;
                        }
                        //avatar.PlayAnimation(_animations[_playAnimationIndex]);
                        PlayAnimationByName(_animations[_playAnimationIndex]);
                    }
                }

                avatar.OnLoadAnimationsExternComplete += OnLoadAnimationsExternComplete;

                //avatar.AddFirstEntityReadyCallback((avatarEntity) =>
                //{

                //    //
                //    _bodyAnimController = avatarEntity?.bodyAnimController;
                //    if (_bodyAnimController == null)
                //    {
                //        return;
                //    }
                //    _animationLayer = _bodyAnimController.CreateAnimationLayerByName("AnimationPreview");
                //    Pico.Avatar.AvatarMask mask = new Pico.Avatar.AvatarMask();
                //    mask.SetJointPositionEnable(JointType.Hips, true);
                //    _animationLayer.SetAvatarMask(mask);
    
                //});

                _loadAvatarCoroutine = null;
                _avatar = avatar;
                _rotation = 0;

                yield return null;
            }

            void UnloadAvatar()
            {
                _animationsExternToLoad = null;
                if (_loadAnimationsExternCoroutine != null)
                {
                    _runtimeBehaviour.StopCoroutine(_loadAnimationsExternCoroutine);
                    _loadAnimationsExternCoroutine = null;
                }

                PicoAvatarManager.instance.UnloadAllAvatars();
                _avatar = null;
                _animations = null;
            }

            public void PlayAnimationByName(string name)
            {
                var avatar = PicoAvatarManager.instance.GetAvatar(_userId);
                if (avatar != null)
                {
                    AvatarAnimationLayer layer =
                        avatar.entity.bodyAnimController.GetAnimationLayerByName("AnimationPreview");
                    if (layer == null)
                        layer = avatar.entity.bodyAnimController.CreateAnimationLayerByName("AnimationPreview");
                    
                    layer.PlayAnimationClip(name, 0, 1, 0);
                    /*for (int i = 0; i < (int)IKEffectorType.Count; ++i)
                    {
                        avatar.entity.bodyAnimController.bipedIKController.SetIKEnable((IKEffectorType)i, false);
                    }*/
                }
            }

            public void StopAnimation()
            {
                var avatar = PicoAvatarManager.instance.GetAvatar(_userId);
                if (avatar != null)
                {
                    AvatarAnimationLayer layer =
                        avatar.entity.bodyAnimController.GetAnimationLayerByName("AnimationPreview");
                    if (layer != null)
                    {
                        layer.StopAnimation(0);
                    }
                }
            }
            
        }
    }
}
