using MicroWar;
using UnityEngine;
using Unity.Netcode;
public class PlayerPickableObjectNetwork : NetworkBehaviour
{
    public AudioClip GenericPowerUpSFX;

    private void Start()
    {
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if(!IsHost)
        SoundManager.Instance.PlayPowerUpSFX(GenericPowerUpSFX);
    }
}
