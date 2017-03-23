using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Askker.App.iOS
{
    class Utils
    {
        public static Task<nint> ShowAlert(string title, string message, params string[] buttons)
        {
            var tcs = new TaskCompletionSource<nint>();
            var alert = new UIAlertView
            {
                Title = title,
                Message = message
            };
            foreach (var button in buttons)
                alert.AddButton(button);
            alert.Clicked += (s, e) => tcs.TrySetResult(e.ButtonIndex);
            alert.Show();
            return tcs.Task;
        }

    }
}
