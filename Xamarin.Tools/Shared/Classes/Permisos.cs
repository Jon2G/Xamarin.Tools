using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Xamarin.Tools.Shared.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using PermissionStatus = Plugin.Permissions.Abstractions.PermissionStatus;

namespace Plugin.Xamarin.Tools.Shared.Classes
{
    public static class Permisos
    {
        public static async Task<bool> PedirPermiso(Plugin.Permissions.Abstractions.Permission Permiso, string Mensaje = "Permita el acceso")
        {
            return await Device.InvokeOnMainThreadAsync(async () =>
            {
                Plugin.Permissions.BasePermission basePermission = new BasePermission(Permiso);
                PermissionStatus status = await basePermission.CheckPermissionStatusAsync();
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permiso))
                    {
                        await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(Mensaje, "Atención.");
                    }
                    await CrossPermissions.Current.RequestPermissionsAsync(Permiso);
                    status = await basePermission.CheckPermissionStatusAsync();
                }
                return (status == PermissionStatus.Granted);
            });
        }
        public static async Task<bool> TenemosPermiso(Plugin.Permissions.Abstractions.Permission Permiso)
        {
            if (Device.RuntimePlatform == Device.iOS && Permiso == Permission.Storage)
            {
                return true;
            }
            return await Device.InvokeOnMainThreadAsync(async () =>
            {
                Plugin.Permissions.BasePermission basePermission = new BasePermission(Permiso);
                PermissionStatus status = await basePermission.CheckPermissionStatusAsync();
                return (status == PermissionStatus.Granted);
            });
        }
    }
}
