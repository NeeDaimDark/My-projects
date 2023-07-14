using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CardGameController : MonoBehaviour
{
    // URL for the search public key endpoint
    private string searchPublicKeyURL = "http://your_server_url/publickey";

    // Function to search public key by code
    IEnumerator SearchPublicKeyByCode(string code)
    {
        // Create a request object
        UnityWebRequest request = UnityWebRequest.Post(searchPublicKeyURL, code);

        // Send the request
        yield return request.SendWebRequest();

        // Check for any errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error searching public key: " + request.error);
            yield break;
        }

        // Retrieve the response data
        string publicKey = request.downloadHandler.text;
        // Process the publicKey as needed
        Debug.Log("Public Key: " + publicKey);
    }

    // Usage example
    void Start()
    {
        string code = "your_verification_code";
        StartCoroutine(SearchPublicKeyByCode(code));
    }
}
