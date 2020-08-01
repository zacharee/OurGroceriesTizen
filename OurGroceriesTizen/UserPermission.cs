using System;
using System.Threading.Tasks;
using ElmSharp;
using Tizen;
using Tizen.Security;

namespace OurGroceriesTizen
{
    /// From https://github.com/Samsung/Tizen-CSharp-Samples/blob/master/Wearable/WidgetSample/src/WidgetSample/UserPermission.cs
    public class UserPermission
    {
        private TaskCompletionSource<bool> _tcs;
        
        public UserPermission()
        {
        }

        public async Task<bool> CheckAndRequestPermission(string privilege)
        {
            try
            {
                var result = PrivacyPrivilegeManager.CheckPermission(privilege);
                switch (result)
                {
                    case CheckResult.Allow:
                        return true;
                    case CheckResult.Deny:
                        return false;
                    case CheckResult.Ask:
                        PrivacyPrivilegeManager.GetResponseContext(privilege).TryGetTarget(out var context);
                        if (context == null)
                        {
                            return false;
                        }
                        _tcs = new TaskCompletionSource<bool>();
                        context.ResponseFetched += PpmResponseHandler;
                        PrivacyPrivilegeManager.RequestPermission(privilege);
                        return await _tcs.Task;
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void PpmResponseHandler(object sender, RequestResponseEventArgs e)
        {
            if (e.cause == CallCause.Error)
            {
                // Handle errors
                Log.Error("OurGroceries", "Error in Request Permission");
                _tcs.SetResult(false);
                return;
            }

            switch (e.result)
            {
                case RequestResult.AllowForever:
                    _tcs.SetResult(true);
                    break;
                case RequestResult.DenyForever:
                case RequestResult.DenyOnce:
                    _tcs.SetResult(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}