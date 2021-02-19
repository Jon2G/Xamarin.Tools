using Kit.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Kit.Services.Interfaces;
using Windows.Storage;
namespace Kit.UWP.Services
{
    public class FolderPermissions : IFolderPermissions
    {
        public override async Task<bool> CanRead(string directoryInfo)
        {
            await Task.Yield();
            return true;
        }


        public override async Task<bool> CanWrite(string directoryInfo)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(directoryInfo);
            return !file.Attributes.HasFlag(Windows.Storage.FileAttributes.ReadOnly);

        }


        public override async Task<bool> TryToUnlock(string path)
        {
            await Task.Yield();
            try
            {
                System.IO.FileAttributes attributes = RemoveAttribute(File.GetAttributes(path), System.IO.FileAttributes.ReadOnly);
                File.SetAttributes(path, attributes);
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex,"Unlocking folder",path);
                return false;
            }


        }
        private System.IO.FileAttributes RemoveAttribute(System.IO.FileAttributes attributes, System.IO.FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }
    }
}
