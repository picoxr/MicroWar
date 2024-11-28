using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Place holder avatar. Usually a special avatar.
		/// </summary>
		public class PicoPlaceholderAvatar : PicoAvatar
		{
			#region Public Properties

			// whether is for local avatar. Should only be set by PicoAvatarManager.
			public bool isForLocalAvatar
			{
				get => _isForLocalAvatar;
			}

			// Whether the placeholder is referenced currently.
			public bool isReferenced
			{
				get => _entityRefCount > 0;
			}

			#endregion


			#region Friend Methods

			//Retain reference for the place holder avatar.
			//@warning Invoke for RetainPlaceholderEntity MUST be paired with ReleasePlaceholderEntity
			//to keep reference count balanced.
			internal void RetainPlaceholderEntity(PicoAvatarPlaceholderRef placeHolderRef,
				System.Action<AvatarEntity> readyCallback)
			{
				_entityRefCount++;

				// if other avatar place holder is the first one, should be activated.
				if (_entityRefCount == 1)
				{
					if (!_isForLocalAvatar)
					{
						this.gameObject.SetActive(true);
					}
				}

				AddFirstEntityReadyCallback((avatar, entity) =>
				{
					// play animation.
					if (_entityRefCount == 1)
					{
						this.PlayAnimation("idlePlaceHolder");
					}

					//
					readyCallback(entity);

					// if not local avatar, hide ite.
					if (!_isForLocalAvatar)
					{
						// this.gameObject.SetActive(false);
					}
				});
			}


			//Release reference for the place holder avatar.

			internal void ReleasePlaceholderEntity(PicoAvatarPlaceholderRef placeHolderRef)
			{
				if (--_entityRefCount == 0 && this.curState != State.Dead)
				{
					if (_isForLocalAvatar)
					{
						// unload place holder avatar.
						PicoAvatarManager.instance.UnloadAvatar(this);
					}
					else
					{
						// stop calculate animation.
						this.StopAnimation(true);

						// hide other avatar place holder
						this.gameObject.SetActive(false);
					}
				}
			}


			//Sets whether is for local avatar. 
			//@remark Should only be set by PicoAvatarManager.

			internal void SetForLocalAvatar(bool forLocalAvatar)
			{
				_isForLocalAvatar = forLocalAvatar;
			}

			override internal void Notify_AvatarEntityLodReadyToShow(AvatarEntity avatarEntity,
				AvatarLodLevel curLodLevel)
			{
				//
				base.Notify_AvatarEntityLodReadyToShow(avatarEntity, curLodLevel);

				// hide placeholder  for non-local avatar.
				if (!_isForLocalAvatar && _entityRefCount == 0)
				{
					this.gameObject.SetActive(false);
				}
			}

			#endregion


			#region Private Fields

			// whether is for local avatar. Should only be set by PicoAvatarManager.
			private bool _isForLocalAvatar = false;

			// count of avatars that reference the place holder avatar.
			private int _entityRefCount = 0;

			#endregion
		}

		/// <summary>
		/// An instance for avatar place holder.
		/// </summary>
		/// @note
		/// Copy entity renderables from PicoPlaceholderAvatar.entity. Lod not considered.Once the object is released, can not be used any more.
		public class PicoAvatarPlaceholderRef
		{
			#region Public Properties

			// whether is for local avatar. 
			public bool isForLocalAvatar
			{
				get => _isForLocalAvatar;
			}

			// Query whether place holder entity copied here.
			public bool isReady
			{
				get => _copiedEntityTrans != null;
			}

			#endregion


			#region Public Methods

			/// <summary>
			/// Construct place holder reference.
			/// </summary>
			/// <param name="name">Name of placeholderref</param>
			/// <param name="parentTrans">Parent transform</param>
			/// <param name="placeHolderAvatar">PicoPlaceholderAvatar to ref</param>
			/// <param name="isMirror">Mirror avatar</param>
			public PicoAvatarPlaceholderRef(string name, Transform parentTrans, PicoPlaceholderAvatar placeHolderAvatar,
				bool isMirror = false)
			{
				if (parentTrans == null || placeHolderAvatar == null)
				{
					throw new System.ArgumentNullException("parent and placeHolderAvatar can not be null");
				}

				//
				_parentTrans = parentTrans;
				_name = name;
				_isForLocalAvatar = placeHolderAvatar.isForLocalAvatar;
				_weakPlaceHolderAvatar = new System.WeakReference<PicoPlaceholderAvatar>(placeHolderAvatar);
				_isMirror = isMirror;

				// retain place holder entity.
				placeHolderAvatar.RetainPlaceholderEntity(this, (entity) =>
				{
					// maybe parent has been released.
					if (_parentTrans != null && _copiedEntityTrans == null)
					{
						var srcTrans = entity.transform;

						// if(AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
						// {
						//     AvatarEnv.Log(DebugLogMask.AvatarLoad, string.Format("PicoAvatarPlaceholderRef.RetainPlaceholderEntity: pos: {0}, {1}, {2}, userId:{3}", srcTrans.localPosition.x, srcTrans.localPosition.y, srcTrans.localPosition.z, _parentTrans.gameObject.name));
						//     AvatarEnv.Log(DebugLogMask.AvatarLoad, string.Format("PicoAvatarPlaceholderRef.RetainPlaceholderEntity.parent: {0}, entity: {1}, children count {2}", entity.transform.parent.gameObject.name, entity.gameObject.name, entity.gameObject.transform.childCount));
						//     for (int i = 0; i < entity.gameObject.transform.childCount; i++)
						//     {
						//         AvatarEnv.Log(DebugLogMask.AvatarLoad, string.Format("PicoAvatarPlaceholderRef.RetainPlaceholderEntity: child {0} : name: {1}", i, entity.gameObject.transform.GetChild(i).name));
						//     }
						// }

						var copiedEntityGo = GameObject.Instantiate(entity.gameObject, _parentTrans);
						copiedEntityGo.name = "PlaceHolderEntity";
						_copiedEntityTrans = copiedEntityGo.transform;

						_copiedEntityTrans.localPosition = srcTrans.localPosition;
						_copiedEntityTrans.localRotation = srcTrans.localRotation;
						_copiedEntityTrans.localScale = Vector3.one;
						//

						// if is local avatar, should unload avatar.
						if (isForLocalAvatar && isMirror == false) //local Avatar.
						{
							copiedEntityGo.SetActive(true);
						}
						else
						{
							copiedEntityGo.SetActiveRecursively(true); //other avatar || local mirror avatar.
						}

						if (isForLocalAvatar == true) //mirror avatar || local avatar.
						{
							_copiedEntityTrans.localRotation = srcTrans.localRotation * Quaternion.Euler(0, 180, 0);
						}
					}
				});
			}


			/// <summary>
			/// Release the reference
			/// </summary>
			public void Release()
			{
				//
				_parentTrans = null;
				//
				if (_copiedEntityTrans != null)
				{
					UnityEngine.Object.Destroy(_copiedEntityTrans.gameObject);
					_copiedEntityTrans = null;
				}

				//
				if (_weakPlaceHolderAvatar != null)
				{
					if (_weakPlaceHolderAvatar.TryGetTarget(out PicoPlaceholderAvatar avatar))
					{
						avatar?.ReleasePlaceholderEntity(this);
					}

					_weakPlaceHolderAvatar = null;
				}
			}

			/// <summary>
			/// Place holder avatar referenced.
			/// </summary>
			/// <returns>Null if place holder avatar has been unloaded</returns>
			public PicoPlaceholderAvatar GetPlaceholderAvatar()
			{
				if (_weakPlaceHolderAvatar != null)
				{
					if (_weakPlaceHolderAvatar.TryGetTarget(out PicoPlaceholderAvatar avatar))
					{
						return avatar;
					}
				}

				return null;
			}

			// For PicoAvatarXXX transform.
			public void UpdateMovement()
			{
				if (isForLocalAvatar == true)
				{
					//Update From _weakPlaceHolderAvatar.
					if (_weakPlaceHolderAvatar != null)
					{
						if (_weakPlaceHolderAvatar.TryGetTarget(out PicoPlaceholderAvatar avatar)
						    && _copiedEntityTrans != null)
						{
							if (avatar != null && avatar.entity != null &&
							    avatar.curState != PicoAvatar.State.Dead)
							{
								var srcPlaceholderTrans = avatar.entity.transform;

								// AvatarEnv.Log(DebugLogMask.AvatarLoad, string.Format("PicoAvatarPlaceholderRef.Move: pos: {0}, {1}, {2}, userId:{3}", srcPlaceholderTrans.localPosition.x, srcPlaceholderTrans.localPosition.y, srcPlaceholderTrans.localPosition.z, _name));

								_copiedEntityTrans.localPosition = srcPlaceholderTrans.localPosition;
								_copiedEntityTrans.localRotation = srcPlaceholderTrans.localRotation;
							}
						}
					}
				}
			}

			#endregion


			#region Private Fields

			// whether is for local avatar. Copied from PicoPlaceholderAvatar.
			private bool _isForLocalAvatar = false;

			// whether is for mirror.
			private bool _isMirror = false;

			// whether to add coped entity renderable objects.
			private Transform _parentTrans;

			// debug name.
			private string _name;

			// copied entity transform
			private Transform _copiedEntityTrans;

			// place holder avatar referenced.
			private System.WeakReference<PicoPlaceholderAvatar> _weakPlaceHolderAvatar;

			#endregion
		}
	}
}