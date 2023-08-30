using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using MicroWar.Platform;

namespace MicroWar.Platform.Netcode
{
    public class PICOTransport : NetworkTransport
    {
        public override ulong ServerClientId => 0;
        private ulong serverClientID; //TODO: how to implement this
        private PlatformController_Network networkController;


        public override void DisconnectLocalClient()
        {
            networkController.OnReceivePICOPacket -= InvokeTransportEvent;
            Debug.Log("[PICO transport]: Disconnect local client");
            //throw new NotImplementedException();
        }

        public override void DisconnectRemoteClient(ulong clientId)
        {
            //throw new NotImplementedException();
        }

        public override ulong GetCurrentRtt(ulong clientId)
        {
            return 0;
        }

        public override void Initialize(NetworkManager networkManager = null)
        {
            networkController = PlatformServiceManager.Instance.GetController<PlatformController_Network>();
            networkController.OnReceivePICOPacket += InvokeTransportEvent; // Listen event from Network controller
            //networkController.OnChangeHost+= OnChangeHost; // Listen event from Network controller
            Debug.Log("PICO Transport initialize called!");
            //throw new NotImplementedException();
        }

        private void OnChangeHost(ulong hostUID)
        {
            //TODO: ID references.
            serverClientID = hostUID;
            Debug.Log($"[{nameof(PICOTransport)}]: HostID: {serverClientID}");
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Tell network controller to pull event every frame.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="payload"></param>
        /// <param name="receiveTime"></param>
        /// <returns></returns>
        public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
        {
            networkController.PullEvent();
            clientId = default;
            payload = default;
            receiveTime = Time.realtimeSinceStartup;
            return NetworkEvent.Nothing;
        }

        private void InvokeTransportEvent(NetworkEvent networkEvent, ulong userId = 0, ArraySegment<byte> payload = default)
        {
            switch (networkEvent)
            {
                case NetworkEvent.Data:
                    InvokeOnTransportEvent(networkEvent, userId, payload, Time.realtimeSinceStartup);
                    break;
                case NetworkEvent.Connect:
                    InvokeOnTransportEvent(networkEvent, userId, payload, Time.realtimeSinceStartup);
                    break;
                case NetworkEvent.Disconnect:
                    InvokeOnTransportEvent(networkEvent, userId, payload, Time.realtimeSinceStartup);
                    serverClientID = default;
                    break;
                case NetworkEvent.TransportFailure:
                    break;
                case NetworkEvent.Nothing:
                    break;
                default:
                    break;
            }
        }

        public override void Send(ulong clientUID, ArraySegment<byte> payload, NetworkDelivery networkDelivery)
        {
            var payloadDataArray = payload.ToArray();
            networkController.SendPacket2UID(clientUID,payloadDataArray);
            //throw new NotImplementedException();
        }

        public override void Shutdown()
        {
            Debug.LogWarning($"Shutdown");
            //throw new NotImplementedException();
        }

        public override bool StartClient()
        {
            Debug.LogWarning($"Start Client");
            DebugUtils.Log(nameof(PICOTransport), "Start Client!");
            var myUID = (ulong)PlatformServiceManager.Instance.Me.ID.GetHashCode();
            //Connect to server
            InvokeTransportEvent(NetworkEvent.Connect, myUID);
            return true;
            //throw new NotImplementedException();
        }

        public override bool StartServer()
        {
            Debug.LogWarning($"Start Server");
            return true;
            //throw new NotImplementedException();
        }
    }

}
