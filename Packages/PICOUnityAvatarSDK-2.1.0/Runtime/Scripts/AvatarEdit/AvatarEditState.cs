namespace Pico
{
	namespace Avatar
	{
		#region AvatarEdit enums

		// avatar shaping param kind.
		public enum AvatarShapingParamKind
		{
			Custom = -1, // 
			BlendShape = 0, // 0 - 1 float value.
			CurveLineWithBlendShape = 1, // curve line with blend shape.
			ControlVertexShape = 2, // region selection and move.
		}

		// face profile shaping method type.
		public enum AvatarFaceProfileShapingMethod
		{
			None = 0,
			Bone = 1,
			BlendShape = 2,
		}

		// face facial feature shaping method type.
		public enum AvatarFaceFeatureShapingMethod
		{
			None = 0,
			Bone = 1,
			BlendShape = 2,
		}

		// body shaping method type.
		public enum AvatarBodyProfileShapingMethod
		{
			None = 0,
			Bone = 1,
		}

		// accessory asset type
		public enum AvatarAccessoryAssetType
		{
			None = -1, // placeholder
			Glasses = 0, // glasses
			Mask = 1, // face mask
			ClearGlasses = 2 // clear glasses, maybe needs special treatment
		}

		// asset type used to easily select special type of asset object from asset list.
		public enum AvatarAssetType
		{
			Suit = 0, // suit asset.
			AvatarEntity = 1, // baked avatar entity asset.
			Makeups = 2, // texture or material asset.
			CustomPrefab = 3, // custom prefab asset.
			Skeleton = 10, // skeleton asset.
			Clothes = 11, // general clothes asset
			Shoe = 12, // shoe and special clothes.
			Hair = 13, // hair
			Nail = 14, // hand finger nail.
			StringData = 15, // use extData to carry a data chunk
			Glove = 16, // glove.
			Accessory = 17 // accessory. cloth accessories, normally use mesh socket
		}

		#endregion

		// A proxy to help transfering api message between upper-level editor and native-level editor.
		public class AvatarEditState
		{
			#region Public Types

			// Current status.
			public enum Status
			{
				None = 0,
				Entering = 1,
				Working = 2,
			}

			#endregion


			#region Public Fields/Properties

			// current status
			public Status status
			{
				get => _status;
			}

			// current aspect.
			public uint curEditAspect
			{
				get => _curEditAspect;
			}

			// whether is loading asset.
			public bool isChangingAsset { get; private set; } = false;

			#endregion


			#region Public Methods

			/// <summary>
			/// which avatar to edit.
			/// </summary>
			/// <param name="avatar">avatar</param>
			public AvatarEditState(PicoAvatar avatar)
			{
				this._avatar = avatar;
				_rmiObject = new NativeCall_AvatarEditState(this, avatar.nativeAvatarId);
				_rmiObject.Retain();
			}

			/// <summary>
			/// Destroy the object. Invoked from AvatarManager.
			/// </summary>
			public void Destroy()
			{
				if (_rmiObject != null)
				{
					if (_status == Status.Working)
					{
						ExitState();
					}

					//
					_rmiObject.Release();
					_rmiObject = null;
				}

				_assetChangedCallback = null;
			}

			/// <summary>
			/// Enter edit state.
			/// </summary>
			/// <param name="editAspect"></param>
			/// <param name="finishCallback"></param>
			public void EnterState(uint editAspect, System.Action<NativeResult> finishCallback)
			{
				if (_status == Status.Entering || _status == Status.Working)
				{
					if (finishCallback != null)
					{
						finishCallback(NativeResult.InvalidState);
					}

					return;
				}

				//
				_targetEditAspect = editAspect;
				// keep track finish callback.
				_enterStateFinishCallback = finishCallback;
				//
				_status = Status.Entering;
				//
				_rmiObject.EnterState((uint)editAspect);
			}

			/// <summary>
			/// set configable items for edit state,currently only
			/// {"blendDuration": some_float_number} supported 
			/// </summary>
			/// <param name="editConfig"></param>
			public void SetEditConfig(string editConfig)
			{
				_rmiObject.SetEditConfig(editConfig);
			}

			/// <summary>
			/// Exit edit state.
			/// </summary>
			public void ExitState()
			{
				if (_status == Status.Working)
				{
					_status = Status.None;
					_rmiObject.ExitState();
				}
			}

			/// <summary>
			/// Sets DIY parameters for asset pin with asset id.
			/// For avatar non-asset parameters besides shaping parameters can use fake asset id to identify.
			/// </summary>
			/// <param name="assetId">assetId</param>
			/// <param name="diyParamsText">DIY parameters.</param>
			/// <param name="paramGroup">which part of DIY parameters need be updated. can be empty</param>
			/// <param name="operationId">operationId a self-increased id to identify this operation.
			/// if larger than 0, will record the operation which can be revoked later</param>
			/// <returns></returns>
			public string SetAssetPinDIYParams(string assetId, string diyParamsText, string paramGroup,
				uint operationId = 0)
			{
				if (_status == Status.Working)
				{
					var resultCode = _rmiObject.SetAssetPinDIYParams(assetId, diyParamsText, paramGroup, operationId);
					return resultCode;
				}

				return null;
			}

			/// <summary>
			/// Sets shaping parameter value.
			/// </summary>
			/// <param name="paramGroup">group id</param>
			/// <param name="paramId">paramId id</param>
			/// <param name="paramKind">paramKind</param>
			/// <param name="paramVal">parameter value whose type is decided by paramKind.</param>
			/// <param name="operationId">operationId a self-increased id to identify this operation. if larger than 0,
			/// will record the operation which can be revoked later.</param>
			/// <param name="forceUpdate">forceUpdate if true, force update immediately </param>
			/// <returns></returns>
			public string SetShapingParam(uint paramGroup, uint paramId, uint paramKind, string paramVal,
				uint operationId = 0, bool forceUpdate = false)
			{
				if (_status == Status.Working)
				{
					return _rmiObject.SetShapingParam(paramGroup, paramId, paramKind, paramVal, operationId,
						forceUpdate);
				}
				else
				{
					return "{}";
				}
			}

			/// <summary>
			/// Gets shaping parameter value.
			/// </summary>
			/// <param name="paramGroup">group id</param>
			/// <param name="paramId">paramId id</param>
			/// <param name="paramKind">paramKind</param>
			/// <returns>parameter value whose type is decided by paramKind</returns>
			public string GetShapingParam(uint paramGroup, uint paramId, uint paramKind)
			{
				if (_status == Status.Working)
				{
					return _rmiObject.GetShapingParam(paramGroup, paramId, paramKind);
				}

				return "{}";
			}

			/// <summary>
			/// Force Update the shaping parameters
			/// </summary>
			public void ForceUpdate()
			{
				if (_status == Status.Working)
				{
					_rmiObject.ForceUpdate();
				}
			}

			/// <summary>
			/// Sets shaping preset parameters.
			/// </summary>
			/// <param name="presetTypeName">presetTypeName preset type name</param>
			/// <param name="presetParamsJsonText">presetParamsJsonText json text for preset parameters</param>
			/// <param name="operationId">operationId a self-increased id to identify this operation. if larger than 0,
			/// will record the operation which can be revoked later/param>
			public void SetShapingPreset(string presetTypeName, string presetParamsJsonText, uint operationId = 0)
			{
				if (_status == Status.Working)
				{
					_rmiObject.SetShapingPreset(presetTypeName, presetParamsJsonText, operationId);
				}
			}

			/// <summary>
			/// Gets shaping parameter value
			/// </summary>
			/// <param name="presetTypeName">presetTypeName preset type name</param>
			/// <returns>json text for preset parameters</returns>
			public string GetShapingPreset(string presetTypeName)
			{
				if (_status == Status.Working)
				{
					return _rmiObject.GetShapingPreset(presetTypeName);
				}

				return "{}";
			}

			/// <summary>
			/// Preview parameter with unedited value
			/// </summary>
			/// <param name="paramGroup">group id</param>
			/// <param name="paramId">paramId id</param>
			public void StartCompareShapingParam(uint paramGroup, uint paramId = 0)
			{
				if (_status == Status.Working)
				{
					_rmiObject.StartCompareShapingParam(paramGroup, paramId);
				}
			}

			/// <summary>
			/// Gets shaping parameter value
			/// </summary>
			public void EndCompareShapingParam()
			{
				if (_status == Status.Working)
				{
					_rmiObject.EndCompareShapingParam();
				}
			}

			/// <summary>
			/// Gets all shaping parameter config
			/// </summary>
			/// <returns></returns>
			public string GetAllShapingParamConfig()
			{
				if (_rmiObject != null)
				{
					return _rmiObject.GetAllShapingParamConfig();
				}

				return "{}";
			}

			/// <summary>
			/// Gets latest specification text from native part
			/// </summary>
			/// <returns></returns>
			public string GetUpdatedAvatarSpecText()
			{
				if (_rmiObject != null)
				{
					return _rmiObject.GetUpdatedAvatarSpecText();
				}

				return "{}";
			}

			/// <summary>
			/// Make ui needed asset pin proto text.
			/// </summary>
			/// <param name="avatarSpecAssetPinProtoText"></param>
			/// <returns></returns>
			public string MakeUIAssetPinProtoText(string avatarSpecAssetPinProtoText)
			{
				if (_rmiObject != null)
				{
					return _rmiObject.MakeUIAssetPinProtoText(avatarSpecAssetPinProtoText);
				}

				return "{}";
			}

			#endregion


			#region Change Asset

			/// <summary>
			/// Put on asset
			/// </summary>
			/// <param name="userId">userId</param>
			/// <param name="assetId">assetId</param>
			/// <param name="assetType">assetType</param>
			/// <param name="diyParamsText">diyParamsText</param>
			/// <param name="controlParamsJsonText">json text for control params</param>
			/// <param name="operationId">operationId a self-increased id to identify this operation. if larger than 0,
			/// will record the operation which can be revoked later</param>
			/// <param name="callback"></param>
			public void PutOnAsset(string userId, string assetId, uint assetType, string diyParamsText,
				string controlParamsJsonText, uint operationId
				, System.Action<NativeResult, string> callback = null)
			{
				if (isChangingAsset)
				{
					if (callback != null)
					{
						callback(NativeResult.InvalidState, "Last changing asset on has not been finished!");
					}

					return;
				}

				//
				this.isChangingAsset = true;
				//
				PutOnAssetRequest.DoRequest(userId, assetId, assetType, diyParamsText, controlParamsJsonText,
					operationId, (errorCode, errorDesc) =>
					{
						this.isChangingAsset = false;
						//
						if (_assetChangedCallback != null)
						{
							_assetChangedCallback(errorCode);
						}
						if (callback != null)
						{
							callback(errorCode, errorDesc);
						}
					});
			}

			/// <summary>
			/// Undo commands.
			/// </summary>
			/// <param name="operationId">operationId Revoke command operation until the ocommand with the id revoked. if is zero, only revoke one command.</param>
			/// <param name="callback"></param>
			public void UndoCommands(uint operationId, System.Action<NativeResult, string> callback = null)
			{
				if (this.isChangingAsset)
				{
					if (callback != null)
					{
						callback(NativeResult.InvalidState, "Last changing asset on has not been finished!");
					}

					return;
				}

				//
				this.isChangingAsset = true;
				//
				UndoCommandsRequest.DoRequest(operationId, (errorCode, errorDesc) =>
				{
					this.isChangingAsset = false;
					//
					if (_assetChangedCallback != null)
					{
						_assetChangedCallback(errorCode);
					}
					if (callback != null)
					{
						callback(errorCode, errorDesc);
					}
				});
			}

			/// <summary>
			/// Redo commands
			/// </summary>
			/// <param name="operationId">operationId Revoke command operation until the ocommand with the id revoked.
			/// if is zero, only revoke one command.</param>
			/// <param name="callback"></param>
			public void RedoCommands(uint operationId, System.Action<NativeResult, string> callback = null)
			{
				if (this.isChangingAsset)
				{
					if (callback != null)
					{
						callback(NativeResult.InvalidState, "Next changing asset on has not been finished!");
					}

					return;
				}

				//
				this.isChangingAsset = true;
				//
				RedoCommandsRequest.DoRequest(operationId, (errorCode, errorDesc) =>
				{
					this.isChangingAsset = false;
					//
					if (_assetChangedCallback != null)
					{
						_assetChangedCallback(errorCode);
					}
					if (callback != null)
					{
						callback(errorCode, errorDesc);
					}
				});
			}

			public void SetAssetChangedCallback(System.Action<NativeResult> assetChangedCallback)
			{
				_assetChangedCallback = assetChangedCallback;
			}

			#endregion


			#region Private Fields/Properties

			//
			PicoAvatar _avatar = null;

			// rmi object.
			private NativeCall_AvatarEditState _rmiObject = null;

			// status
			private Status _status = Status.None;

			//
			private uint _targetEditAspect = (uint)0xfffffff;

			private uint _curEditAspect = (uint)0xfffffff;

			//
			private System.Action<NativeResult> _enterStateFinishCallback = null;

			private System.Action<string> _avatarSpecificationChangedCallback = null;
			
			private System.Action<NativeResult> _assetChangedCallback = null;
			//

			#endregion


			#region Private/Friend Methods

			// OnEnterState.
			internal void Notify_EnterState(NativeResult result)
			{
				if (_status == Status.Entering)
				{
					_status = Status.Working;
					//
					_curEditAspect = _targetEditAspect;
				}

				//
				if (_enterStateFinishCallback != null)
				{
					_enterStateFinishCallback(result);
				}
			}

			// Notification from native part that specification text changed.
			internal void Notify_AvatrSpecChanged(string avatarSpecText)
			{
				if (_avatarSpecificationChangedCallback != null)
				{
					_avatarSpecificationChangedCallback(avatarSpecText);
				}
			}

			#endregion
		}
	}
}