using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Pico
{
	namespace Avatar
	{
		public class AvatarAnimatorSceneInit : MonoBehaviour
		{
			private Camera _camera;
			const int _avatarLayer = 8;
			private GameObject _canvas;
			private GameObject _panel;
			private GameObject _eventSystem;
			private Font defaultFont;


			public AvatarAnimator avatarAnimator;
			public GameObject avatarApp;
			PicoAvatarApp _app = null;
			PicoAvatar avatar;
			bool _avatarEnvReady = false;
			IEnumerator _loadAvatarCoroutine = null;
			List<AvatarAnimationLayer> layerList = null;
			AvatarAnimationLayer lastLayer = null;


			private void Awake()
			{
				InitObj();
			}


			public void InitObj()
			{
				//camera
				_camera = new GameObject("camera").AddComponent<Camera>();
				_camera.tag = "MainCamera";
				_camera.transform.localPosition = new Vector3(0, 1, -2);
				_camera.transform.localRotation = Quaternion.Euler(4, 0, 0);
				_camera.cullingMask = 1 << _avatarLayer;
				_camera.depth = 0;

				//light
				var light1 = new GameObject("light1").AddComponent<Light>();
				light1.transform.rotation = Quaternion.Euler(30, 0, 0);
				light1.type = LightType.Directional;
				var light2 = new GameObject("light2").AddComponent<Light>();
				light2.transform.rotation = Quaternion.Euler(-20, 150, -180);
				light2.type = LightType.Directional;

				//canvas
				_canvas = new GameObject("Canvas");
				var canvasComp = _canvas.AddComponent<Canvas>();
				canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
				_canvas.layer = 5;
				CanvasScaler canvasScaler = _canvas.AddComponent<CanvasScaler>();
				canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				canvasScaler.referenceResolution = new Vector2(1920, 1080);
				canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
				canvasScaler.matchWidthOrHeight = 0.5f;
				_canvas.AddComponent<GraphicRaycaster>();

				//eventSystem
				_eventSystem = new GameObject("EventSystem");
				_eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

				//panel
				_panel = new GameObject("Panel");
				_panel.transform.SetParent(_canvas.transform);
				GridLayoutGroup grid = _panel.AddComponent<GridLayoutGroup>();
				grid.cellSize = new Vector2(250, 80);
				grid.spacing = new Vector2(20, 10);
				grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
				grid.startAxis = GridLayoutGroup.Axis.Vertical;
				grid.childAlignment = TextAnchor.UpperLeft;
				grid.constraint = GridLayoutGroup.Constraint.Flexible;
				RectTransform rectTrans = _panel.GetComponent<RectTransform>();
				SetRectTransformStretch(rectTrans);
				//init avatarApp
				StartCoroutine(InitAvatarEnv());
			}

			public Button NewButton(string text, UnityEngine.Events.UnityAction callback)
			{
				//button
				GameObject _button = new GameObject("Button");
				_button.transform.SetParent(_panel.transform);
				_button.transform.localScale = Vector3.one;

				Image imageComp = _button.AddComponent<Image>();

				Button buttonComp = _button.AddComponent<Button>();
				if (callback != null)
					buttonComp.onClick.AddListener(callback);

				GameObject _text = new GameObject("Text");
				_text.transform.SetParent(_button.transform);

				Text textComp = _text.AddComponent<Text>();
				SetRectTransformStretch(_text.GetComponent<RectTransform>());
				defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
				textComp.font = defaultFont;
				textComp.fontSize = 25;
				textComp.color = Color.black;
				textComp.alignment = TextAnchor.MiddleCenter;
				textComp.text = text;

				return buttonComp;
			}

			public void SetRectTransformStretch(RectTransform rectTransform)
			{
				rectTransform.anchoredPosition = new Vector2(0, 0);
				rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
				rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
				rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
				rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;

				rectTransform.localScale = Vector3.one;
			}

			IEnumerator InitAvatarEnv()
			{
				//wait to load avatarApp
				yield return new WaitForEndOfFrame();
				// create app
				if (avatarApp == null)
				{
					Debug.LogError("Avatar App Prefab load failed");
					yield break;
				}

				avatarApp = Instantiate(avatarApp);
				_app = avatarApp.GetComponent<PicoAvatarApp>();
				_app.loginSettings.accessToken =
					"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHBpcmVJbiI6IjE2NTQ0MDE5MDAiLCJzaWciOiJsaWdodHBhY2thZ2UiLCJ0b2tlblR5cGUiOiIwIiwidXNlcklEIjoiMTE1NDU2MzkyNTI2NzEzMjQxNiIsInZlcnNpb24iOiIwLjAuMSJ9.d8cvtJthkot6C0lWCzsMVzzrAH1jlklhzW1zCResAug";
				// _app.serverType = ServerType.ProductionEnv;
				_app.accessType = AccessType.ThirdApp;
				_app.extraSettings.avatarSceneLayer = _avatarLayer;
				_app.lodSettings.forceLodLevel = AvatarLodLevel.Lod0;

				// Utility.EnableSkin = true;

				// wait app init
				while (!PicoAvatarApp.isWorking)
				{
					yield return null;
				}

				// start manager
				/*if (!_app.appSettings.autoStartAvatarManager)
				{
				    _app.StartAvatarManager();
				}*/
				while (!PicoAvatarManager.instance.isReady)
				{
					yield return null;
				}

				_avatarEnvReady = true;

				if (_loadAvatarCoroutine == null)
				{
					// if (avatar)
					// {
					//     UnloadAvatar();
					// }
					//init avatar
					_loadAvatarCoroutine = LoadAvatar();
					StartCoroutine(_loadAvatarCoroutine);
				}

				Debug.Log("AvatarAnimatorEditor InitAvatarEnv Done");

				yield return null;
			}

			IEnumerator LoadAvatar()
			{
				// load avatar
				var capability = new AvatarCapabilities();
				capability.manifestationType = AvatarManifestationType.Full;
				// capability.isLocalAvatar = true;
				capability.controlSourceType = ControlSourceType.NPC;
				capability.bodyCulling = true;

				avatar = PicoAvatarManager.instance.LoadAvatar(new AvatarLoadContext(avatarAnimator.userId, "",
					AvatarAnimator.jsonSpecDict[avatarAnimator.jsonSpec.currentString], capability));
				if (avatar == null)
				{
					Debug.LogError("AvatarAnimationEditor LoadAvatar failed");
					yield break;
				}

				Transform avatarTransform = avatar.transform;
				avatarTransform.localPosition = Vector3.zero;
				avatarTransform.localRotation = Quaternion.Euler(0, 0, 0);
				avatarTransform.localScale = Vector3.one;

				while (!avatar.entity.isAnyLodReady)
				{
					yield return null;
				}

				Debug.Log("AvatarAnimationEditor LoadAvatar Done");

				for (int i = 0; i < avatarAnimator.animationPathRelativeAssets.Length; i++)
				{
					string path = System.IO.Path.Combine(Application.dataPath + "/",
						avatarAnimator.animationPathRelativeAssets[i]);

					try
					{
						Debug.Log("Extern animation path: " + path);
						avatar.LoadAllAnimationsExtern(path);
					}
					catch (System.IO.DirectoryNotFoundException e)
					{
						Debug.LogError(e.Message);
					}
				}

				AvatarBodyAnimController _bodyAnimController = avatar.entity.bodyAnimController;
				layerList = avatarAnimator.InitAvatarAnimator(_bodyAnimController);

				//Draw Button
				DrawAnimationButton();

				_loadAvatarCoroutine = null;
				// _rotation = 0;

				yield return null;
			}

			void UnloadAvatar()
			{
				PicoAvatarManager.instance.UnloadAllAvatars();
				avatar = null;
			}

			public void DrawAnimationButton()
			{
				for (int i = 0; i < layerList.Count; i++)
				{
					for (int j = 0; j < avatarAnimator.layers[i].nodes.Count; j++)
					{
						AvatarAnimationStateNode stateNode =
							avatarAnimator.layers[i].nodes[j] as AvatarAnimationStateNode;
						if (stateNode != null)
						{
							AvatarAnimationState state = layerList[i].GetAnimationStateByName(stateNode.stateName);
							if (state != null)
							{
								int tempi = i;
								NewButton(stateNode.stateName, () =>
								{
									// Debug.Log("index i = " + tempi + "  state.name = " + state.name);
									lastLayer?.StopAnimation(0);
									layerList[tempi].PlayAnimationState(state, 0);
									lastLayer = layerList[tempi];
								});
							}
						}
					}
				}
			}


			IEnumerator GetAnimNames()
			{
				Debug.Log("GetAnimationNames enter!!!!");
				string animations = "";
				while (string.IsNullOrEmpty(animations))
				{
					animations = avatar.GetAnimationNames();
					yield return null;
				}

				Debug.Log("AvatarAnimationEditor GetAnimationNames:" + animations);
			}


			List<string> ParseAnimations(string animations)
			{
				List<string> result = new List<string>();
				var tempList = JsonConvert.DeserializeObject<List<System.Object>>(animations);
				if (tempList != null)
					foreach (var item in tempList)
					{
						result.Add(item.ToString());
					}

				return result;
			}
		}
	}
}