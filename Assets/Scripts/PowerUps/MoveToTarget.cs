using System;
using System.Collections;
using UnityEngine;

namespace MicroWar
{
    public class MoveToTarget : MonoBehaviour
    {
        public void MoveToTargetTransform(Transform target, float travelTime = 1.5f, float scaleModifier = 1f, Action callback = null)
        {
            StartCoroutine(LerpPosition(target, travelTime, scaleModifier, callback));
        }


        IEnumerator LerpPosition(Transform target, float duration, float scaleModifier, Action callback)
        {
            float time = 0;
            Vector3 targetPos = target.position;
            Vector3 startPosition = transform.position;
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = startScale * scaleModifier;
            while (time < duration)
            {
                //Update target position each frame in case the target is moving
                if (target != null) targetPos = target.position; 
                float step = time / duration;
                transform.position = Vector3.Lerp(startPosition, targetPos, step);
                transform.localScale = Vector3.Lerp(startScale, targetScale, step); 
                time += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPos;
            callback?.Invoke();
        }
    }
}
