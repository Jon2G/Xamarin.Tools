using Acr.UserDialogs.Infrastructure;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Log = SQLHelper.Log;
namespace Plugin.Xamarin.Tools.Droid.Services
{
    public class FolderPermissions : IFolderPermissions
    {
        public override async Task<bool> CanRead(string directoryInfo)
        {
            await Task.Yield();
            Java.IO.File f = new Java.IO.File(directoryInfo);
            return f.CanRead();
        }

        public override async Task<bool> CanWrite(string directoryInfo)
        {
            await Task.Yield();
            Java.IO.File f = new Java.IO.File(directoryInfo);
            return f.CanWrite();
        }

        public override async Task<bool> TryToUnlock(string Path)
        {
            await Task.Yield();
            try
            {
                Java.IO.File f = new Java.IO.File(Path);
                f.SetWritable(true);
                f.SetReadable(true);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
            }
            return false;
        }
    }
}
