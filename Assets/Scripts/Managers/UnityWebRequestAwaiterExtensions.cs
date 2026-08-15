using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Managers
{
    // Lets you write `await someRequest.SendWebRequest();` directly instead
    // of wrapping every network call in a coroutine. No extra package needed.
    public static class UnityWebRequestAwaiterExtensions
    {
        public static TaskAwaiter<UnityWebRequest> GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<UnityWebRequest>();
            asyncOp.completed += _ => tcs.SetResult(asyncOp.webRequest);
            return tcs.Task.GetAwaiter();
        }
    }
}