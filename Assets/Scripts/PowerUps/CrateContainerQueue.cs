using System.Collections.Generic;
using UnityEngine;
using System;
namespace MicroWar
{
    public class CrateContainerQueue : MonoBehaviour
    {
        private struct CrateInfo
        {
            public CrateType Type;
            public Transform PowerUpTargetTransform;

            public CrateInfo(CrateType crateType, Transform powerUpTargetTransform)
            {
                Type = crateType;
                PowerUpTargetTransform = powerUpTargetTransform;
            }
        }

        public Transform Container;
        public GameObject BreakableCratePrefab;
        private PowerUpCrate activeCrate;

        private Queue<CrateInfo> crateQueue;
        private bool hasVisibleCrate = false;

        //Multiplayer
        public Action<CrateType> OnSetCrateType;
        public Action<PowerUpCrate, CrateType> OnInstantiateCrate;
        private void Awake()
        {
            crateQueue= new Queue<CrateInfo>();

        }

        private void Start()
        {
            Container.transform.localScale *= GameManager.Instance.EnvironmentManager.CurrentBattleGroundScaleFactor;
            GameManager.Instance.EnvironmentManager.OnBattlegroundScaled += OnBattlegroundScaled;
        }

        private void OnDestroy()
        {
            GameManager.Instance.EnvironmentManager.OnBattlegroundScaled -= OnBattlegroundScaled;
        }

        private void OnBattlegroundScaled(float scaleFactor)
        {
            Container.transform.localScale *= scaleFactor;
        }

        public void Enqueue(CrateType crateType, Transform targetTransform)
        {
            crateQueue.Enqueue(new CrateInfo(crateType, targetTransform));
            ProcessQueue();
        }

        public PowerUpCrate GetActiveCrate()
        {
            if (hasVisibleCrate)
            {
                return activeCrate;
            }

            return null;
        }

        public bool HasCrate()
        {
            return crateQueue.Count > 0;
        }

        private void ProcessQueue()
        {
            if (hasVisibleCrate || crateQueue.Count == 0)
            {
                return; 
            }

            CrateInfo crateInfo = crateQueue.Dequeue();

            if (activeCrate == null)
            {
                activeCrate = InstantiateCrate(crateInfo.Type, BreakableCratePrefab, Container, crateInfo.PowerUpTargetTransform);
                OnInstantiateCrate?.Invoke(activeCrate, crateInfo.Type);//Multiplayer host
            }
            else 
            {
                activeCrate.SetPowerUpType(crateInfo.Type);
                OnSetCrateType?.Invoke(crateInfo.Type);//Multiplayer host
            }

            activeCrate.gameObject.SetActive(true);
            hasVisibleCrate = true;
            activeCrate.OnAllAnimFinished = OnPowerUpAnimFinished;
        }

        private PowerUpCrate InstantiateCrate(CrateType crateType, GameObject prefab, Transform parent, Transform targetTranform)
        {
            PowerUpCrate breakableCrate = Instantiate(prefab, parent).GetComponent<PowerUpCrate>();
            breakableCrate.SetPowerUpType(crateType);
            breakableCrate.SetPowerUpTargetTransform(targetTranform);
            return breakableCrate;
        }

        private void OnPowerUpAnimFinished()
        {
            hasVisibleCrate = false;
            activeCrate.gameObject.SetActive(false);
            ProcessQueue();
        }
    }
}