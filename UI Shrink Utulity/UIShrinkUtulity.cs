using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIShrinkUtulity : MonoBehaviour
{
    #region Singleton
    public static UIShrinkUtulity Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    #endregion

    private Dictionary<object,Coroutine> _coroutines = new Dictionary<object, Coroutine>(25);
    public void StartShrink(object senderObject,Transform targetTransform,Vector3 startScale, Vector3 endScale, float duration = 0.5f)
    {
        if (_coroutines.ContainsKey(senderObject))
        {
            if (!_coroutines[senderObject].Equals(null)) 
                StopCoroutine(_coroutines[senderObject]); 
        }else
        {
            _coroutines.Add(senderObject, null);
        }

        Coroutine coroutine = StartCoroutine(SmoothScale(targetTransform, startScale, endScale,duration));
        _coroutines[senderObject] = coroutine;
    }

    private IEnumerator SmoothScale(Transform targetTransform, Vector3 startScale, Vector3 endScale,float duration )
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = Mathf.Sin(elapsedTime / duration * Mathf.PI * 0.5f);
            targetTransform.localScale = Vector3.Lerp(startScale, endScale, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final scale is set
        targetTransform.localScale = endScale;
    }
}
