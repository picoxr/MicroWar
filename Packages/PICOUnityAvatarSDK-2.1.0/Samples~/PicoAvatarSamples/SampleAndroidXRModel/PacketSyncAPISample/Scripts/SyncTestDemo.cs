using UnityEngine;

namespace Pico.Avatar.Sample
{

    public class SyncTestDemo : MonoBehaviour
    {

        #region Public Fields/Properties

        public float playbackTimeDelay = 0.5F;
        [Tooltip("Time interval to record a packet.")]
        public float recordInterval = 0.2f;
        // source avatar that provide animation and position.
        public ActionAvatar srcAvatar = null;

        // destination avatar that consume playback animation and position.
        public ActionAvatar destAvatar = null;

        private MemoryView _packetMemoryView = null;
        private float _lastTimeRecordPacket = 0.0f;
        private float _serverTime = 0;
        private bool _newPacketRecorded = false;


        #endregion
        private void Start()
        {
            if (srcAvatar == null)
            {
                srcAvatar = GameObject.Find("LocalActionAvatar").GetComponent<ActionAvatar>();
            }
            if (destAvatar == null)
            {
                destAvatar = GameObject.Find("OtherActionAvatar").GetComponent<ActionAvatar>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            _serverTime += Time.deltaTime;
            if (!PicoAvatarManager.canLoadAvatar)
            {
                return;
            }

            if (srcAvatar == null || srcAvatar.Avatar == null || srcAvatar.Avatar.entity == null)
                return;
            if (destAvatar == null || destAvatar.Avatar == null || destAvatar.Avatar.entity == null)
                return;
            var srcEntity = srcAvatar.Avatar.entity;
            var destEntity = destAvatar.Avatar.entity;
            if (srcEntity.nativeEntityId == 0)
                return;
            if (destEntity.nativeEntityId == 0)
                return;

            UpdateRecordAnimation();
            UpdatePlaybackAnimation();
        }
        /**
       * Update playback.
       */
        private void UpdatePlaybackAnimation()
        {
            if (_packetMemoryView != null && destAvatar.Avatar.entity != null)
            {
                if (_newPacketRecorded == true)
                {
                    destAvatar.Avatar.entity.ApplyPacket(_packetMemoryView);
                    _newPacketRecorded = false;
                }
            }
            //
            PicoAvatarManager.instance.SyncNetSimulation(_serverTime - PicoAvatarApp.instance.netBodyPlaybackSettings.playbackInterval);

        }
        private void UpdateRecordAnimation()
        {
            //
            _packetMemoryView = null;

            if (srcAvatar.Avatar.entity.isAnyLodReady)
            {
                if (_serverTime - _lastTimeRecordPacket > PicoAvatarApp.instance.netBodyPlaybackSettings.recordInterval)
                {
                    _lastTimeRecordPacket = _serverTime;
                    //
                    _packetMemoryView = srcAvatar.Avatar.entity.GetFixedPacketMemoryView();
                    _newPacketRecorded = true;
                }
            }
        }
    }
}

