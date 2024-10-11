using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ApiManager : MonoBehaviour
{
    private const string RANDOM_API_URL = "https://www.random.org/integers/?num=1&min=1&max=6&col=1&base=10&format=plain&rnd=new";


    public static ApiManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public IEnumerator GetRandomNumber(Action<int> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(RANDOM_API_URL))
        {
            webRequest.SetRequestHeader("User-Agent", "UnityWebRequest");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                if (int.TryParse(webRequest.downloadHandler.text.Trim(), out int result))
                {
                    callback(result);
                }
                else
                {
                    Debug.LogError("Failed to parse random number");
                    callback(0);
                }
            }
            else
            {
                Debug.LogError("Error fetching random number: " + webRequest.error);
                callback(0);
            }
        }

    }
}
