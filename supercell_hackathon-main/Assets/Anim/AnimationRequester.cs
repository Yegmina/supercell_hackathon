using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AnimationRequester : MonoBehaviour
{
    [Header("Backend Settings")]
    [Tooltip("URL of the animation service endpoint")]
    public string serviceUrl = "http://127.0.0.1:5000/api/anim/witch-idle";

    [Tooltip("Interval in seconds between requests to the backend")]
    public float queryInterval = 5f;

    public Animator animator;
    public WitchController controller;
    private string currentTrigger;

    [Serializable]
    private class AnimResponse
    {
        public string animation;
        // all_candidates is ignored by JsonUtility
    }

    private void Start()
    {
        // Start the polling loop
        StartCoroutine(PollAnimationService());
    }

    private IEnumerator PollAnimationService()
    {
        while (true)
        {
            yield return RequestAndPlayAnimation();
            yield return new WaitForSeconds(queryInterval);
        }
    }

    private IEnumerator RequestAndPlayAnimation()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(serviceUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                AnimResponse resp = JsonUtility.FromJson<AnimResponse>(json);
                if (!string.IsNullOrEmpty(resp.animation))
                {
                    PlayAnimation(resp.animation);
                    Debug.Log(resp.animation);
                }
                else
                {
                    Debug.LogWarning("AnimationRequester: Received empty animation name.");
                }
            }
            else
            {
                Debug.LogError($"AnimationRequester: Request failed - {www.error}");
            }
        }
    }

    private void PlayAnimation(string triggerName)
    {
        controller.timeLeft += 2f;
        // Reset previous trigger so animations can retrigger properly
        if (!string.IsNullOrEmpty(currentTrigger))
        {
            Debug.Log("A");
            animator.ResetTrigger(currentTrigger);
        }

        // Set the new trigger
        animator.SetTrigger(triggerName);
        Debug.Log("B");
        currentTrigger = triggerName;
    }

    // Optional: stop polling when object is disabled
    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
