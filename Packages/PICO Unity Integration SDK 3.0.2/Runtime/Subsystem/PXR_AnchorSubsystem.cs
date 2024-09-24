#if AR_FOUNDATION
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.PXR
{
    public class PXR_AnchorSubsystem : XRAnchorSubsystem
    {
        internal const string k_SubsystemId = "PXR_AnchorSubsystem";

        class PXR_AnchorProvider : Provider
        {
            private Dictionary<TrackableId, ulong> trackableIdToHandleMap;
            private Dictionary<ulong, XRAnchor> handleToXRAnchorMap;
            private HashSet<ulong> managedAnchorHandles;
            private bool isInit = false;

            public override void Start()
            {
                StartSpatialAnchorProvider();
            }

            private async void StartSpatialAnchorProvider()
            {
                var result = await PXR_MixedReality.StartSenseDataProvider(PxrSenseDataProviderType.SpatialAnchor);
                if (result == PxrResult.SUCCESS)
                {
                    if (!isInit)
                    {
                        trackableIdToHandleMap = new Dictionary<TrackableId, ulong>();
                        handleToXRAnchorMap = new Dictionary<ulong, XRAnchor>();
                        managedAnchorHandles = new HashSet<ulong>();
                        isInit = true;
                    }
                }
                else
                {
                    Debug.LogError("Spatial Anchor Provider Start Failed:" + result);
                }
            }

            public override void Stop()
            {
                var result = PXR_MixedReality.StopSenseDataProvider(PxrSenseDataProviderType.SpatialAnchor);
                if (result == PxrResult.SUCCESS)
                {

                }
                else
                {
                    Debug.LogError("Spatial Anchor Provider Stop Failed:" + result);
                }
            }

            public override void Destroy()
            {
                
            }

            public override TrackableChanges<XRAnchor> GetChanges(XRAnchor defaultAnchor, Allocator allocator)
            {
                var result = PXR_MixedReality.QuerySpatialAnchorAsync();
                if (result.Result.result == PxrResult.SUCCESS)
                {
                    var addedHandles = new List<ulong>();
                    var presentHandles = new List<ulong>();
                    var leftOverHandles = new HashSet<ulong>(managedAnchorHandles);
                    foreach (var handle in result.Result.anchorHandleList)
                    {
                        if (managedAnchorHandles.Contains(handle))
                        {
                            presentHandles.Add(handle);
                            leftOverHandles.Remove(handle);
                        }
                        else
                        {
                            addedHandles.Add(handle);
                        }
                    }

                    var addedAnchors = new NativeArray<XRAnchor>(addedHandles.Count, Allocator.Temp);
                    int addedIndex = 0;
                    foreach (var handle in addedHandles)
                    {
                        var anchor = handleToXRAnchorMap[handle];
                        addedAnchors[addedIndex++] = anchor;
                        managedAnchorHandles.Add(handle);
                    }

                    var removeIds = new NativeArray<TrackableId>(leftOverHandles.Count, Allocator.Temp);
                    int removeIndex = 0;
                    foreach (var handle in leftOverHandles)
                    {
                        var anchor = handleToXRAnchorMap[handle];
                        removeIds[removeIndex++] = anchor.trackableId;
                        trackableIdToHandleMap.Remove(anchor.trackableId);
                        handleToXRAnchorMap.Remove(handle);
                    }

                    var changedAnchors = new NativeArray<XRAnchor>(presentHandles.Count, Allocator.Temp);
                    int changedIndex = 0;
                    foreach (var handle in presentHandles)
                    {
                        var anchor = handleToXRAnchorMap[handle];
                        PXR_MixedReality.LocateAnchor(handle, out var position, out var rotation);
                        var newAnchor = new XRAnchor(anchor.trackableId,new Pose(position,rotation),TrackingState.Tracking,new IntPtr((long)handle));
                        handleToXRAnchorMap[handle] = newAnchor;
                        changedAnchors[changedIndex++] = newAnchor;
                    }

                    var trackableChanges = TrackableChanges<XRAnchor>.CopyFrom(addedAnchors,changedAnchors,removeIds,Allocator.Persistent);

                    return trackableChanges;
                }
                else
                {
                    return new TrackableChanges<XRAnchor>();
                }
            }

            public override bool TryAddAnchor(Pose pose, out XRAnchor anchor)
            {
                var result = PXR_MixedReality.CreateSpatialAnchorAsync(pose.position, pose.rotation);
                if (result.Result.result == PxrResult.SUCCESS)
                {
                    var result1 = PXR_MixedReality.PersistSpatialAnchorAsync(result.Result.anchorHandle);
                    if (result1.Result == PxrResult.SUCCESS)
                    {
                        var trackabledId = new TrackableId(result.Result.uuid.ToString());
                        var nativePtr = new IntPtr((long)result.Result.anchorHandle);
                        anchor = new XRAnchor(trackabledId, pose, TrackingState.Tracking, nativePtr);
                        trackableIdToHandleMap[trackabledId] = result.Result.anchorHandle;
                        handleToXRAnchorMap[result.Result.anchorHandle] = anchor;
                        return true;
                    }
                    else
                    {
                        anchor = XRAnchor.defaultValue;
                        return false;
                    }
                }
                else
                {
                    anchor = XRAnchor.defaultValue;
                    return false;
                }
            }

            public override bool TryRemoveAnchor(TrackableId anchorId)
            {
                var anchorHandle = trackableIdToHandleMap[anchorId];
                var result = PXR_MixedReality.DestroyAnchor(anchorHandle);
                if (result == PxrResult.SUCCESS)
                {
                    var result1 = PXR_MixedReality.UnPersistSpatialAnchorAsync(anchorHandle);
                    if (result1.Result == PxrResult.SUCCESS)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            var cInfo = new XRAnchorSubsystemDescriptor.Cinfo()
            {
                id = k_SubsystemId,
                providerType = typeof(PXR_AnchorProvider),
                subsystemTypeOverride = typeof(PXR_AnchorSubsystem),
                supportsTrackableAttachments = false
            };
            XRAnchorSubsystemDescriptor.Create(cInfo);
        }
    }
}
#endif
