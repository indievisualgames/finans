using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class InternetConnectivityChecker : MonoBehaviour
{
    private const string fallbackUrl1 = "https://clients3.google.com/generate_204";
    private const string fallbackUrl2 = "https://www.gstatic.com/generate_204";
    private const int requestTimeout = 7;

    public static async Task<bool> CheckInternetConnectivityAsync()
    {
        if (await TryCheckUrlAsync(fallbackUrl1)) return true;
        if (await TryCheckUrlAsync(fallbackUrl2)) return true;
        return false;
    }

    private static async Task<bool> TryCheckUrlAsync(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = requestTimeout;
            await SendWebRequestAsync(request);
            if (request.result == UnityWebRequest.Result.Success)
            {
                // generate_204 returns HTTP 204 on success (no content)
                if (request.responseCode == 204 || request.responseCode == 200)
                {
                    return true;
                }
            }
            return false;
        }
    }

    // Helper static method to await a UnityWebRequest asynchronously
    private static Task SendWebRequestAsync(UnityWebRequest request)
    {
        var tcs = new TaskCompletionSource<bool>();

        // Start the request
        request.SendWebRequest().completed += operation =>
        {
            // Complete the TaskCompletionSource when the operation completes
            tcs.SetResult(true);
        };

        // Return the task for awaiting
        return tcs.Task;
    }
}
