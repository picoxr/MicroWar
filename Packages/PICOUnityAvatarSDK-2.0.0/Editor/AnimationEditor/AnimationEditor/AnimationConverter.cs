#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		public class AnimationConverter
		{
			const int ClipCurveTypeCount = 11;
			const int BlendShapeCurveIndex = 10;

			class ClipCurve
			{
				public AnimationCurve[] curves;
				public string blendShapeName;
				public string path;
			}

			public static string[] ConvertClipsToAnimaz(Transform target, AnimationClip[] clips, string[] names,
				string outDir)
			{
				List<string> clipNames = new List<string>();
				for (int i = 0; i < clips.Length; ++i)
				{
					var clip = clips[i];
					if (clip == null)
					{
						continue;
					}

					string name = clip.name;
					if (names != null)
					{
						name = names[i];
					}

					float frameRate = clip.frameRate;
					float length = clip.length;
					WrapMode wrapMode = clip.wrapMode;

					var curveMap = new Dictionary<string, ClipCurve>();
					var curveBindings = AnimationUtility.GetCurveBindings(clip);
					for (int j = 0; j < curveBindings.Length; ++j)
					{
						var binding = curveBindings[j];
						string path = binding.path;
						string propertyName = binding.propertyName;

						bool ignoreProperty = false;
						int curveTypeIndex = -1;
						string amazPropertyName = propertyName;
						switch (propertyName)
						{
							case "m_LocalPosition.x":
								curveTypeIndex = 0;
								amazPropertyName = "m_localMatrix";
								break;
							case "m_LocalPosition.y":
								curveTypeIndex = 1;
								amazPropertyName = "m_localMatrix";
								break;
							case "m_LocalPosition.z":
								curveTypeIndex = 2;
								amazPropertyName = "m_localMatrix";
								break;

							case "m_LocalRotation.x":
								curveTypeIndex = 3;
								amazPropertyName = "m_localMatrix";
								break;
							case "m_LocalRotation.y":
								curveTypeIndex = 4;
								amazPropertyName = "m_localMatrix";
								break;
							case "m_LocalRotation.z":
								curveTypeIndex = 5;
								amazPropertyName = "m_localMatrix";
								break;
							case "m_LocalRotation.w":
								curveTypeIndex = 6;
								amazPropertyName = "m_localMatrix";
								break;

							case "m_LocalScale.x":
								curveTypeIndex = 7;
								amazPropertyName = "m_localMatrix";
								break;
							case "m_LocalScale.y":
								curveTypeIndex = 8;
								amazPropertyName = "m_localMatrix";
								break;
							case "m_LocalScale.z":
								curveTypeIndex = 9;
								amazPropertyName = "m_localMatrix";
								break;

							default:
								if (propertyName.StartsWith("blendShape."))
								{
									curveTypeIndex = BlendShapeCurveIndex;
								}
								else
								{
									ignoreProperty = true;
								}

								break;
						}

						if (ignoreProperty)
						{
							Debug.LogWarning("ignore Property:" + propertyName);
							continue;
						}

						//Transform curveTarget = target.Find(path);
						var targetName = path.Substring(path.LastIndexOf('/') + 1);
						Transform curveTarget = FindTransformByName(target.gameObject, targetName);
						if (curveTarget == null && curveTypeIndex != BlendShapeCurveIndex)
						{
							Debug.LogError("can not find curve target: " + path);
							continue;
						}

						ClipCurve clipCurve;
						if (!curveMap.TryGetValue(path + amazPropertyName, out clipCurve))
						{
							clipCurve = new ClipCurve();
							clipCurve.curves = new AnimationCurve[ClipCurveTypeCount];
							clipCurve.path = path;
							curveMap.Add(path + amazPropertyName, clipCurve);
						}

						clipCurve.curves[curveTypeIndex] = AnimationUtility.GetEditorCurve(clip, binding);
						if (curveTypeIndex == BlendShapeCurveIndex)
						{
							int find = propertyName.LastIndexOf('.');
							if (find >= 0)
							{
								clipCurve.blendShapeName = propertyName.Substring(find + 1);
							}
							else
							{
								clipCurve.blendShapeName = propertyName;
							}
						}
					}

					foreach (var item in curveMap)
					{
						var curves = item.Value;
						bool transformCurves = (curves.curves[BlendShapeCurveIndex] == null);

						List<float> times = new List<float>();
						for (int j = 0; j < curves.curves.Length; ++j)
						{
							var curve = curves.curves[j];
							if (curve == null)
							{
								continue;
							}

							var keys = curve.keys;
							for (int k = 0; k < keys.Length; ++k)
							{
								float t = keys[k].time;
								int find = times.FindIndex((float v) => { return Mathf.Abs(t - v) < 0.0001f; });
								if (find < 0)
								{
									times.Add(t);
								}
							}
						}

						times.Sort();

						if (transformCurves)
						{
							for (int j = 0; j < curves.curves.Length; ++j)
							{
								if (j == BlendShapeCurveIndex)
								{
									continue;
								}

								var curve = curves.curves[j];
								if (curve == null)
								{
									//Transform curveTarget = target.Find(curves.path);
									var targetName = curves.path.Substring(curves.path.LastIndexOf('/') + 1);
									Transform curveTarget = FindTransformByName(target.gameObject, targetName);

									curve = new AnimationCurve();
									for (int k = 0; k < times.Count; ++k)
									{
										float v = 0;
										switch (j)
										{
											case 0:
												v = curveTarget.localPosition.x;
												break;
											case 1:
												v = curveTarget.localPosition.y;
												break;
											case 2:
												v = curveTarget.localPosition.z;
												break;

											case 3:
												v = curveTarget.localRotation.x;
												break;
											case 4:
												v = curveTarget.localRotation.y;
												break;
											case 5:
												v = curveTarget.localRotation.z;
												break;
											case 6:
												v = curveTarget.localRotation.w;
												break;

											case 7:
												v = curveTarget.localScale.x;
												break;
											case 8:
												v = curveTarget.localScale.y;
												break;
											case 9:
												v = curveTarget.localScale.z;
												break;
										}

										curve.AddKey(times[k], v);
									}

									curves.curves[j] = curve;
								}
								else if (curve.length != times.Count)
								{
									var keys = curve.keys;
									List<float> keyTimes = new List<float>();
									for (int k = 0; k < keys.Length; ++k)
									{
										keyTimes.Add(keys[k].time);
									}

									for (int k = 0; k < times.Count; ++k)
									{
										float t = times[k];
										int find = keyTimes.FindIndex((float v) =>
										{
											return Mathf.Abs(t - v) < 0.0001f;
										});
										if (find < 0)
										{
											curve.AddKey(t, curve.Evaluate(t));
										}
									}
								}
							}
						}
					}

					foreach (var item in curveMap)
					{
						var curves = item.Value;
						bool transformCurves = (curves.curves[BlendShapeCurveIndex] == null);
						int firstCurve = transformCurves ? 0 : BlendShapeCurveIndex;
						float[] times = new float[curves.curves[firstCurve].length];
						for (int j = 0; j < times.Length; ++j)
						{
							times[j] = curves.curves[firstCurve].keys[j].time;
						}

						for (int j = 0; j < curves.curves.Length; ++j)
						{
							var curve = curves.curves[j];
							if (transformCurves)
							{
								if (j >= 0 && j < BlendShapeCurveIndex)
								{
									Debug.Assert(curve != null);
								}
								else
								{
									Debug.Assert(curve == null);
								}
							}
							else
							{
								if (j == BlendShapeCurveIndex)
								{
									Debug.Assert(curve != null);
								}
								else
								{
									Debug.Assert(curve == null);
								}
							}

							if (curve != null)
							{
								var keys = curve.keys;
								for (int k = 0; k < keys.Length; ++k)
								{
									Debug.Assert(Mathf.Abs(times[k] - keys[k].time) < 0.0001f);
								}
							}
						}
					}

					AmazingLoader.AmazingAnimaz animaz = new AmazingLoader.AmazingAnimaz(name, curveMap.Count);
					animaz.duration = length;
					animaz.endTime = length;

					List<AmazingLoader.AmazingAnimazTrack> tracks = new List<AmazingLoader.AmazingAnimazTrack>();
					foreach (var item in curveMap)
					{
						//var targetName = item.Key;
						var curves = item.Value;
						var targetName = curves.path;
						bool transformCurves = (curves.curves[BlendShapeCurveIndex] == null);
						int firstCurve = transformCurves ? 0 : BlendShapeCurveIndex;

						{
							int find = targetName.LastIndexOf('/');
							if (find >= 0)
							{
								targetName = targetName.Substring(find + 1);
							}
						}

						var track = new AmazingLoader.AmazingAnimazTrack(curves.curves[firstCurve].length, targetName,
							transformCurves, curves.blendShapeName);

						//Debug.LogError(targetName);
						//Debug.LogError(curves.blendShapeName);
						//Debug.LogError("times");

						float[] times = new float[curves.curves[firstCurve].length];
						for (int j = 0; j < times.Length; ++j)
						{
							times[j] = curves.curves[firstCurve].keys[j].time;
							//Debug.LogErrorFormat("{0:0.000}", times[j]);
						}

						track.SetTimes(times);

						//Debug.LogError("values");

						for (int j = 0; j < times.Length; ++j)
						{
							if (transformCurves)
							{
								{
									float x = curves.curves[0].keys[j].value * -1;
									float y = curves.curves[1].keys[j].value;
									float z = curves.curves[2].keys[j].value;
									track.SetPosition(j, new Vector3(x, y, z));
								}
								{
									float x = curves.curves[3].keys[j].value;
									float y = curves.curves[4].keys[j].value * -1;
									float z = curves.curves[5].keys[j].value * -1;
									float w = curves.curves[6].keys[j].value;
									track.SetRotation(j, new Quaternion(x, y, z, w));
								}
								{
									float x = curves.curves[7].keys[j].value;
									float y = curves.curves[8].keys[j].value;
									float z = curves.curves[9].keys[j].value;
									track.SetScale(j, new Vector3(x, y, z));
								}
							}
							else
							{
								float weight = curves.curves[BlendShapeCurveIndex].keys[j].value / 100;
								//Debug.LogErrorFormat("{0:0.000}", weight);
								track.SetBlendShapeWeight(j, weight);
							}
						}

						animaz.SetTrack(tracks.Count, ref track);
						tracks.Add(track);
					}

					string animazName = name + ".animaz";
					if (names == null)
					{
						animazName = target.name + "." + animazName;
					}

					AmazingLoader.SaveAnimaz(outDir, animazName, ref animaz);
					clipNames.Add(name);

					for (int j = 0; j < tracks.Count; ++j)
					{
						var track = tracks[j];
						track.Destroy();
					}

					animaz.Destroy();
				}

				return clipNames.ToArray();
			}

			public static Transform FindTransformByName(GameObject skeletonRoot, string name)
			{
				var allNodes = skeletonRoot.GetComponentsInChildren<Transform>();
				for (int i = 0; i < allNodes.Length; ++i)
				{
					var node = allNodes[i].gameObject;
					if (node.name == name)
					{
						return node.transform;
					}
				}

				return null;
			}
		}
	}
}
#endif