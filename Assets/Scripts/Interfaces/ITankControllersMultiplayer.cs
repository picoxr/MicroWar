using System;
using UnityEngine;

namespace MicroWar.Multiplayer 
{
    public interface ITankShooting
    {
        event Action<float> OnTankShooting;
        public void SimulateFire(float launchForce);
    }

    public interface ITankHealth
    {
        event Action OnTankDeath;
        event Action<float> OnTakeDamage;
        event Action OnMaxUpHealth;
        event Action<bool> OnActivatingShield;
        void OnDeath();
        bool IsDead();
        void SetCurrentHealth(float startingHealth , float currentHealth);
        void ClientSetCurrentHealth(float startingHealth , float currentHealth);
        float GetStartingHealth();

        //TODO:Review below with Ilyas
        public void SetShieldPrefab(GameObject prefab);
        public void TakeDamage(float amount);
        public void AddShield();
        public void SetupShield(bool isActivate);
        public void MaxUpHealth();

        public float GetCurrentHealth();
    }

    public interface ITankMovement
    {
        void DisableControl();
        void EnableControl();
    }

}