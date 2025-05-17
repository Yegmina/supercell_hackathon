using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class SpeechBubbleRequester : MonoBehaviour
{
    [Tooltip("URL of the character-say endpoint")]
    public string sayServiceUrl = "http://127.0.0.1:5000/api/character-say";

    [Tooltip("Interval in seconds between polling for new speech")]
    public float pollInterval = 7f;

    [Tooltip("Text component to show the speech bubble")]
    public TextMeshProUGUI speechText;

    [Tooltip("GameObject that holds the bubble visuals")]
    public GameObject bubbleContainer;

    [System.Serializable]
    private class SayResponse
    {
        public string chosen;
    }

    private void Start()
    {
        if (speechText == null || bubbleContainer == null)
        {
            Debug.LogError("SpeechBubbleRequester: Missing references to UI elements.");
            return;
        }

        StartCoroutine(PollSayService());
    }

    private IEnumerator PollSayService()
    {
        while (true)
        {
            yield return RequestAndShowSpeech();
            yield return new WaitForSeconds(pollInterval);
        }
    }

    private IEnumerator RequestAndShowSpeech()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(sayServiceUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                SayResponse resp = JsonUtility.FromJson<SayResponse>(json);
                if (!string.IsNullOrEmpty(resp.chosen))
                {
                    DisplaySpeech(resp.chosen);
                    Debug.Log("Witch says: " + resp.chosen);
                }
            }
            else
            {
                Debug.LogError("SpeechBubbleRequester: Failed to get speech: " + www.error);
            }
        }
    }

    private void DisplaySpeech(string line)
    {
        bubbleContainer.SetActive(true);
        speechText.text = line;
    }

    // Optional: hide after some time
    public void HideSpeech()
    {
        bubbleContainer.SetActive(false);
    }
}
