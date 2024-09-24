using System.Collections.Generic;
using UnityEngine;

namespace Pico.Avatar.Sample
{
    internal class LocalAnimationFollowSync
    {
        /**
         * @brief Set lead avatar.
         */ 
        public void SetLeadAvatar(PicoAvatar leadAvatar)
        {
            _leadAvatar = new AvatarItem(leadAvatar);
        }

        /**
         * @brief add follow avatar.
         */ 
        public void AddFollowAvatar(PicoAvatar leadAvatar)
        {
            _followAvatars.Add(new AvatarItem(leadAvatar));
        }

        /**
         * @brief reset.
         */ 
        public void Reset()
        {
            _leadAvatar = null;
            _followAvatars.Clear();
        }

        /**
         * @brief Update syncher
         */ 
        public void Update()
        {
            if(_leadAvatar == null || !_leadAvatar.avatar.isAnyEntityReady)
            {
                return;
            }
            //
            var curLeadAvatarPackat = _leadAvatar.avatar.entity.GetFixedPacketMemoryView();
            //
            foreach(var x in _followAvatars)
            {
                x.Update(curLeadAvatarPackat);
            }
            //
            PicoAvatarManager.instance.SyncNetSimulation(Time.realtimeSinceStartup);
        }

        #region Private Fields

        private AvatarItem _leadAvatar;
        private List<AvatarItem> _followAvatars = new List<AvatarItem>();

        #endregion


        #region Private Type
        
        class AvatarItem
        {
            public AvatarItem(PicoAvatar avatar)
            {
                this.avatar = avatar;
                _lastPacketTime = Time.time + UnityEngine.Random.RandomRange(0.0f, 0.5f) * PicoAvatarApp.instance.netBodyPlaybackSettings.recordInterval;
            }

            public void Update(MemoryView leadAvatarPacket)
            {
                if (!avatar.isActiveAndEnabled && avatar.entity == null)
                {
                    return;
                }
                if (Time.time - _lastPacketTime >  PicoAvatarApp.instance.netBodyPlaybackSettings.recordInterval)
                {
                    _lastPacketTime = Time.time;
                    avatar.entity.ApplyPacket(leadAvatarPacket);
                }
            }

            public PicoAvatar avatar;
            private float _lastPacketTime = 0.0f;
        }
        #endregion
    }
}
