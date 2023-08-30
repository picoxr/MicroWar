using MicroWar.AI;
using MicroWar.Multiplayer;
using System;
using UnityEngine;

namespace MicroWar
{
    public class TankShooting_AI : TankShooting , ITankShooting
    {
        private bool m_triggerCurState;
        private bool m_triggerPrevState;

        private Vector3 targetPosition;
        private bool hasTarget = false;

        private bool shouldRotateGun = false;
        private float currentTargetLaunchForce = 0f;
        private bool isFireRequested = false;

        public event Action<ShotRecord> OnShotHit;
        public event Action<float> OnTankShooting;
        private NetPlayer networkPlayer;
        public float MaxShootingDistance { get; private set; }
        public float MinShootingDistance { get; private set; }

        private void Awake()
        {
            SetMinMaxShootingDistance();
            networkPlayer = GetComponent<NetPlayer>();
        }

        private void Update()
        {
            // The slider should have a default value of the minimum launch force.
            m_AimSlider.value = m_MinLaunchForce;

            if (isFireRequested && currentTargetLaunchForce > m_CurrentLaunchForce)
            {
                m_triggerCurState = true;
            }

            // If the max force has been exceeded and the shell hasn't yet been launched...
            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
            {
                // ... use the max force and launch the shell.
                m_CurrentLaunchForce = m_MaxLaunchForce;
                Fire(0f, m_CurrentLaunchForce);
            }
            // Otherwise, if the fire button has just started being pressed...
            else if (m_triggerCurState && !m_triggerPrevState)
            {
                // ... reset the fired flag and reset the launch force.
                m_Fired = false;
                m_CurrentLaunchForce = m_MinLaunchForce;

                PlayChargingSound();
            }
            // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
            else if (m_triggerCurState && m_triggerPrevState && !m_Fired)
            {
                // Increment the launch force and update the slider.
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                m_AimSlider.value = m_CurrentLaunchForce;
            }
            // Otherwise, if the fire button is released and the shell hasn't been launched yet...
            else if (!m_triggerCurState && m_triggerPrevState && !m_Fired)
            {
                // ... launch the shell.
                Fire(0f, m_CurrentLaunchForce);
            }
            m_triggerPrevState = m_triggerCurState;
            m_triggerCurState = false;
        }

        public void AimAtTheTarget()
        {
            if (hasTarget)
            {
                float step = m_TurnSpeed * Time.deltaTime;
                Vector3 directionToTarget = new Vector3(targetPosition.x, m_Turret.position.y, targetPosition.z) - m_Turret.position;

                //TODO: Make turret rotation smoother but we have to align on the rotation speed.
                m_Turret.Rotate(Vector3.up, Vector3.SignedAngle(m_Turret.forward, directionToTarget, m_Turret.up));

                //TODO: Ilyas&Yagiz In Multiplayer Mode we should send the message to the clients so that they rotate their turret.
            }
        }

        public void Shoot()
        {
            
            Shoot(m_MinLaunchForce);
        }

        public void Shoot(float launchForce)
        {
            if (isFireRequested) return;

            currentTargetLaunchForce = launchForce;
            m_triggerCurState = true;
            isFireRequested = true;
        }

        public void SetTarget(Vector3 targetPosition)
        {
            this.hasTarget = true;
            this.targetPosition = targetPosition;
        }

        public void ResetTarget()
        {
            this.hasTarget = false;
        }

        protected void Fire(float chargeTime, float launchForce)
        {
            //OnTankShooting?.Invoke(launchForce);
            networkPlayer.BotFireClientRpc(launchForce);
            // Set the fired flag so Fire is only called once.
            m_Fired = true;
            isFireRequested = false;

            // Create an instance of the shell and store a reference to it's rigidbody.
            Rigidbody shellInstance =
                Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation, GameManager.Instance.BoardGameOrigin) as Rigidbody;

            // Set the shell's velocity to the launch force in the fire position's forward direction.
            shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;

            ShellExplosion shellExplosion = shellInstance.gameObject.GetComponent<ShellExplosion>();
            shellExplosion.OnExploded = (shotRecord) => 
            {
                shotRecord.launchForce = launchForce;
                shotRecord.chargeTime = chargeTime;
                OnShotHit?.Invoke(shotRecord); 
            };

            PlayShootingSound();

            // Reset the launch force.  This is a precaution in case of missing button events.
            m_CurrentLaunchForce = m_MinLaunchForce;
        }

        private void SetMinMaxShootingDistance() 
        {
            float timeToFall;
            float fallingDistance = m_FireTransform.position.y - this.transform.position.y;
            float gravity = Physics.gravity.y;
            //float shellMass = m_Shell.GetComponent<Rigidbody>().mass;

            timeToFall = Mathf.Sqrt(fallingDistance * 2 / Mathf.Abs(gravity));

            //We would normally do these steps below to calculate distance but m_MaxLaunchForce and m_MinLaunchForce represent velocity not force.
            //maxShootingDistance = timeToFallSquared * m_MaxLaunchForce / shellMass; 
            //minShootingDistance = timeToFallSquared * m_MinLaunchForce / shellMass;

            MaxShootingDistance = timeToFall * m_MaxLaunchForce; //Note: m_MaxLaunchForce is bad naming this value actually represents the velocity
            MinShootingDistance = timeToFall * m_MinLaunchForce; //Note: m_MinLaunchForce is bad naming this value actually represents the velocity
        }

        private void PlayChargingSound()
        {
            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play();
        }

        private void PlayShootingSound()
        {
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play();
        }

        public void SimulateFire(float launchForce)
        {
            Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation, GameManager.Instance.BoardGameOrigin) as Rigidbody;

            shellInstance.velocity = launchForce * m_FireTransform.forward;

            PlayShootingSound();
        }
    }
}
