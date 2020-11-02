using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Diagnostics;
using System.Security.Principal;
using Log = SQLHelper.Log;
using Kit.Services.Interfaces;

namespace Kit.WPF.Services
{
    public class FolderPermissions : IFolderPermissions
    {
        public override async Task<bool> CanRead(string directoryInfo)
        {
            await Task.Yield();
            AuthorizationRuleCollection rules;
            WindowsIdentity identity;
            try
            {
                rules = new DirectoryInfo(directoryInfo).GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
                identity = WindowsIdentity.GetCurrent();
            }
            catch (UnauthorizedAccessException uae)
            {
                Debug.WriteLine(uae.ToString());
                return false;
            }

            bool isAllow = false;
            string userSID = identity.User.Value;

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.IdentityReference.ToString() == userSID || identity.Groups.Contains(rule.IdentityReference))
                {
                    if ((rule.FileSystemRights.HasFlag(FileSystemRights.Read) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadAttributes) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadData)) && rule.AccessControlType == AccessControlType.Deny)
                        return false;
                    else if (rule.FileSystemRights.HasFlag(FileSystemRights.Read) &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadAttributes) &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadData) && rule.AccessControlType == AccessControlType.Allow)
                        isAllow = true;

                }
            }
            return isAllow;
        }

        public override async Task<bool> CanWrite(string directoryInfo)
        {
            await Task.Yield();
            AuthorizationRuleCollection rules;
            WindowsIdentity identity;
            try
            {
                rules = new DirectoryInfo(directoryInfo).GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
                identity = WindowsIdentity.GetCurrent();
            }
            catch (UnauthorizedAccessException uae)
            {
                Debug.WriteLine(uae.ToString());
                return false;
            }

            bool isAllow = false;
            string userSID = identity.User.Value;

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.IdentityReference.ToString() == userSID || identity.Groups.Contains(rule.IdentityReference))
                {
                    if ((rule.FileSystemRights.HasFlag(FileSystemRights.Write) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteAttributes) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteData) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.CreateDirectories) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.CreateFiles)) && rule.AccessControlType == AccessControlType.Deny)
                        return false;
                    else if (rule.FileSystemRights.HasFlag(FileSystemRights.Write) &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteAttributes) &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteData) &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.CreateDirectories) &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.CreateFiles) && rule.AccessControlType == AccessControlType.Allow)
                        isAllow = true;

                }
            }
            return isAllow;
        }

        public override async Task<bool> TryToUnlock(string path)
        {
            await Task.Yield();
            try
            {
                FileAttributes attributes = RemoveAttribute(File.GetAttributes(path), FileAttributes.ReadOnly);
                File.SetAttributes(path, attributes);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogMe(ex);
                return false;
            }


        }
        private FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }
    }
}
