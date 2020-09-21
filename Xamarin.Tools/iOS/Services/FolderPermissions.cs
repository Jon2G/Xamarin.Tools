using Foundation;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
namespace Plugin.Xamarin.Tools.iOS.Services
{
    public class FolderPermissions : IFolderPermissions
    {
        public override async Task<bool> CanRead(string directoryInfo)
        {
            await Task.Yield();
            NSFileManager dir = new NSFileManager();
            return dir.IsReadableFile(directoryInfo);
        }

        public override async Task<bool> CanWrite(string directoryInfo)
        {
            await Task.Yield();
            NSFileManager dir = new NSFileManager();
            return dir.IsWritableFile(directoryInfo);
        }

        public override async Task<bool> TryToUnlock(string Path)
        {
            await Task.Yield();
            return true;
        }
    }
}
