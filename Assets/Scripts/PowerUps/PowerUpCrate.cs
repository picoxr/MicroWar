using MicroWar.Multiplayer;
using System;
using UnityEngine;

namespace MicroWar 
{
    public enum CrateType
    {
        None = 0,
        Health,
        Shield
    }

    public class PowerUpCrate : MonoBehaviour
    {
        private const string boxExplodeAnimTrigger = "TriggerBoxExplode";
        private static int boxExplodeAnimHash = -1;

        public Action OnConsumed;
        public Action OnPowerUpAnimFinished;
        public Action OnAllAnimFinished;

        private CrateType crateType = CrateType.None;

        public GameObject crateObject;
        public GameObject healthObject;
        public GameObject shieldObject;
        private HittableObject hittableCrate;

        public AudioClip interactionSFX;
        public Animator crateAnimator;

        private GameObject activeObject;

        private Transform powerUpTarget;

        private Color originalColor = Color.white;

        private MeshRenderer crateMeshRenderer;

        private bool isInitialized = false;

        //Multiplayer
        public Action OnHitSuccessfully;
        public Action OnAllAnimFinishMultiplayer;
        [HideInInspector] public PowerUpCrateNetwork powerUpCrateNetwork;

        private void Awake()
        {
            DeactivateAllTypes();

            if (boxExplodeAnimHash == -1)
            {
                boxExplodeAnimHash = Animator.StringToHash(boxExplodeAnimTrigger);
            }
        }

        private void Start()
        {
            //Multiplayer
            powerUpCrateNetwork = GetComponent<PowerUpCrateNetwork>();
            hittableCrate = crateObject.GetComponent<HittableObject>(); 
            hittableCrate.OnInteracted += OnCrateInteracted;
            
            crateMeshRenderer = crateObject.GetComponent<MeshRenderer>();
            
            originalColor = crateMeshRenderer.material.color;

            isInitialized = true;
        }

        private void DeactivatePowerUpObject()
        {
            activeObject.SetActive(false);
            OnAllAnimFinished?.Invoke();
            OnAllAnimFinishMultiplayer?.Invoke();
        }

        private void DeactivateAllTypes()
        {
            healthObject.SetActive(false);
            shieldObject.SetActive(false);
        }

        public void SetPowerUpType(CrateType powerUpType)
        {
            this.crateType = powerUpType;
            DeactivateAllTypes();
            ResetToDefaults();//Reset Crate
            crateObject.SetActive(true);
            switch (powerUpType)
            {
                case CrateType.None:
                    activeObject = null;
                    break;
                case CrateType.Health:
                    activeObject = healthObject;
                    healthObject.SetActive(true);
                    break;
                case CrateType.Shield:
                    activeObject = shieldObject;
                    shieldObject.SetActive(true);
                    break;
            }
        }

        public void OnCrateInteracted(bool isSuccessful)
        {
            if (isSuccessful)
            {
                OnSuccessfulHit();
                OnHitSuccessfully?.Invoke();
            }
            else 
            {
                OnFailedHit();
            }
        }

        public void OnCrateExplosionAnimFinished()
        {
            hittableCrate.EnableInteraction();
            crateObject.SetActive(false);
            DeactivatePowerUpObject();
        }

        private void ConsumePowerUp()
        {
            if (powerUpTarget != null)
            {
                ITankHealth health = powerUpTarget.gameObject.GetComponent<ITankHealth>();
                switch (crateType)
                {
                    case CrateType.Health:
                        health.MaxUpHealth();
                        break;
                    case CrateType.Shield:
                        health.AddShield();
                        break;
                }
            }
        }

        private void OnSuccessfulHit()
        {
            hittableCrate.DisableInteraction();
            crateAnimator.SetTrigger(boxExplodeAnimHash);
            crateMeshRenderer.material.color = Color.green;
            SoundManager.Instance.PlayGenericSFX(interactionSFX);
            ConsumePowerUp(); //Consume the power-up immediately when collision happens. To consume after the animation ends move this to OnCrateExplosionAnimFinished.
        }

        private void OnFailedHit()
        {
            crateMeshRenderer.material.color = Color.red;
            //TODO Failed Hit SFX
            //TODO Failed Hit Animation
        }

        public void ResetToDefaults()
        {
            if (isInitialized)
            {
                crateAnimator.ResetTrigger(boxExplodeAnimHash);
                crateMeshRenderer.material.color = originalColor;
            }
        }

        public CrateType GetPowerUpType()
        {
            return this.crateType;
        }

        internal void SetPowerUpTargetTransform(Transform powerUpTargetTransform)
        {
            this.powerUpTarget = powerUpTargetTransform;
        }

        public void BreakCrate()
        {
            if (crateObject.activeInHierarchy)
            {
                hittableCrate.SimulateOnTriggerEnter();
            }
        }

        public void SimulateHit()
        {
            OnSuccessfulHit();
        }

        private void OnDestroy()
        {
            hittableCrate.OnInteracted -= OnCrateInteracted;
        }
    }
}
