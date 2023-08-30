using System.Collections.Generic;
using UnityEngine;
using System;
using MicroWar.Multiplayer;
public class TurretController : MonoBehaviour
{
    public GameObject ammoPrefab;
    public float successiveFireDelay = 0.4f; //TODO: Utilise this and implement a queue mechanism for firing.

    public Action<List<Transform>> OnFireMultiple;    //Multiplayer

    private List<Transform> targetTransforms = null;

    public Transform[] launcherTransforms;

    public AudioSource turretAudioSource;
    public AudioClip[] launchSFX;

    public float launchForce;

    private bool isTriggered = false;

    private int nextLauncherIndex = 0;

    private void FixedUpdate()
    {
        if (isTriggered)
        {
            isTriggered = false;

            if (targetTransforms == null || targetTransforms.Count == 0) return;

            PerformFire();
        }
    }

    private void PerformFire() 
    {
        List<Transform>.Enumerator targets = targetTransforms.GetEnumerator();
        while (targets.MoveNext())
        {
            Rigidbody missileRigidBody = Instantiate(ammoPrefab, GetNextLauncherTransform()).GetComponent<Rigidbody>();
            HomingMissile homingMissile = missileRigidBody.GetComponent<HomingMissile>();

            //TODO: If target is not active or destroyed attack a random area
            homingMissile.SetTarget(targets.Current);

            missileRigidBody.transform.localPosition = Vector3.zero;
            missileRigidBody.transform.localScale *= 0.5f;
            missileRigidBody.AddForce(missileRigidBody.transform.forward * launchForce, ForceMode.Impulse);
            PlayLaunchSFX();
        }
    }

    private Transform GetNextLauncherTransform()
    {
        Transform transform = launcherTransforms[nextLauncherIndex];
        nextLauncherIndex = nextLauncherIndex++ % launcherTransforms.Length;
        return transform;
    }

    private void PlayLaunchSFX()
    {
        if (launchSFX == null) return;

        int effectCount = launchSFX.Length;

        turretAudioSource.PlayOneShot(launchSFX[UnityEngine.Random.Range(0, effectCount)]); //TODO: Implement a SoundManager and tidy up SFX handling
    }

    public void Fire(Transform targetTransform)
    {
        FireMultiple(new List<Transform> { targetTransform });
    }

    public void FireMultiple(List<Transform> targetTransforms)
    {
        OnFireMultiple?.Invoke(targetTransforms);
        this.targetTransforms = targetTransforms;
        isTriggered = true;
    }

    public void SimulateFire(ulong[] targetIds)
    {
        var targetTrans =  MultiplayerBehaviour.Instance.GetNetworkPlayerReference(targetIds);
        List<Transform>.Enumerator targets = targetTrans.GetEnumerator();
        while (targets.MoveNext())
        {
            Rigidbody missileRigidBody = Instantiate(ammoPrefab, GetNextLauncherTransform()).GetComponent<Rigidbody>();
            HomingMissile homingMissile = missileRigidBody.GetComponent<HomingMissile>();

            //TODO: If target is not active or destroyed attack a random area
            homingMissile.SetTarget(targets.Current);

            missileRigidBody.transform.localPosition = Vector3.zero;
            missileRigidBody.transform.localScale *= 0.5f;
            missileRigidBody.AddForce(missileRigidBody.transform.forward * launchForce, ForceMode.Impulse);
            PlayLaunchSFX();
        }
    }

}
