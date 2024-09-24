using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// AvatarBunchGroup holds a group of AvatarBunch which render many avatars that using same avatar model by gpu instance.
		/// </summary>
		public class AvatarBunchGroup
		{
			/// <summary>
			/// Gets a new avatar bunch with an avatar id. If not exit, a new AvatarBunch will be created.
			/// </summary>
			/// <param name="avatarId">The id of avatar</param>
			/// <returns>The AvatarBunch of avatar</returns>
			public AvatarBunch GetAvatarBunch(string avatarId)
			{
				AvatarBunch bunch = null;
				if (_avatarBunches.TryGetValue(avatarId, out bunch))
				{
					return bunch;
				}

				// create bunch group root transform.
				if (_bunchGroupTrans == null)
				{
					var go = new GameObject("_AvatarBunchGroup_");
					go.layer = (int)PicoAvatarApp.instance.extraSettings.avatarSceneLayer;
					_bunchGroupTrans = go.transform;
				}

				// create new bunch.
				{
					var go = new GameObject(String.Format("_AvatarBunch_{0}_", avatarId));
					go.transform.parent = _bunchGroupTrans;
					//
					bunch = go.AddComponent<AvatarBunch>();
				}

				_avatarBunches.Add(avatarId, bunch);
				//
				return bunch;
			}

			#region Private Fields

			// root transform to hold avatar bunch GameObjects.
			Transform _bunchGroupTrans = null;

			//
			Dictionary<string, AvatarBunch> _avatarBunches = new Dictionary<string, AvatarBunch>();

			#endregion


			#region Protected / Private Methods

			// schedule pre-update render data job work.
			internal void SchedulePreUpdateRenderData(NativeArray<JobHandle> jobHandles, ref int jobIndex)
			{
				foreach (var x in _avatarBunches)
				{
					x.Value.SchedulePreUpdateRenderData(jobHandles, ref jobIndex);
				}
			}

			// Update render data. invoked from AvatarManager to update all instances.
			internal void UpdateRenderData()
			{
				foreach (var x in _avatarBunches)
				{
					x.Value.UpdateRenderData();
				}
			}

			// Destroy the avatar bunches.
			internal void Destroy()
			{
				foreach (var x in _avatarBunches)
				{
					GameObject.Destroy(x.Value.gameObject);
				}

				_avatarBunches.Clear();

				//
				if (_bunchGroupTrans != null)
				{
					GameObject.DestroyImmediate(_bunchGroupTrans.gameObject);
					_bunchGroupTrans = null;
				}
			}

			#endregion
		}
	}
}