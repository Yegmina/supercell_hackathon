using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

/// <summary>
/// Periodically synchronizes local event and state buffers with the backend.
/// Events are POSTed (appended) one by one, states are PUT (replaced) batch.
/// </summary>
public class EventStateSync : MonoBehaviour
{
    [Tooltip("Backend URL without trailing slash, e.g. http://127.0.0.1:5000")]
    public string baseUrl = "http://127.0.0.1:5000";

    [Tooltip("Interval in seconds between syncs")]
    public float syncInterval = 1f;

    private void Start()
    {
        StartCoroutine(SyncLoop());
    }

    private IEnumerator SyncLoop()
    {
        while (true)
        {
            yield return SyncEvents();
            yield return SyncStates();
            yield return new WaitForSeconds(syncInterval);
        }
    }

    private IEnumerator SyncEvents()
    {
        // Pull local events; if none, skip
        var pulled = EventBuffer.PullEvents();
        if (pulled.Count == 0)
            yield break;

        foreach (var ev in pulled)
        {
            // Build JSON manually to match {"event":"..."}
            string escaped = JsonUtility.ToJson(new Wrapper { value = ev.ToString() });
            // JsonUtility produces {"value":"..."}, so remap
            string eventJson = escaped.Replace("value", "event");
            Debug.Log($"[EventStateSync] POST EVENT: {eventJson}");

            var url = $"{baseUrl}/api/events";
            var req = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(eventJson);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError($"[EventStateSync] Failed to POST event '{ev}': {req.error}");
            }
            else
            {
                Debug.Log($"[EventStateSync] Event POST response: {req.downloadHandler.text}");
            }
        }
    }

    private IEnumerator SyncStates()
    {
        // Flatten state dictionary as key:comma-separated-values
        List<string> statesList = new List<string>();
        foreach (var kvp in EventBuffer.state)
        {
            string line = kvp.Key + ":" + string.Join(",", kvp.Value);
            statesList.Add(line);
        }

        string json = JsonUtility.ToJson(new StatesPayload { states = statesList });
        Debug.Log($"[EventStateSync] PUT STATES: {json}");

        var url = $"{baseUrl}/api/states";
        var req = new UnityWebRequest(url, "PUT");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (req.result != UnityWebRequest.Result.Success)
#else
        if (req.isNetworkError || req.isHttpError)
#endif
        {
            Debug.LogError($"[EventStateSync] Failed to PUT states: {req.error}");
        }
        else
        {
            Debug.Log($"[EventStateSync] States response: {req.downloadHandler.text}");
        }
    }

    // Helper wrapper to produce a single-field JSON
    [System.Serializable]
    private class Wrapper { public string value; }

    [System.Serializable]
    private class StatesPayload
    {
        public List<string> states;
    }
}
