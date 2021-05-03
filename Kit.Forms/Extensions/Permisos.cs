using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Kit.Forms.Extensions
{
    public static class Permisos
    {

        public static async Task<bool> PedirPermiso(Permissions.BasePermission Permiso, string Mensaje = "Permita el acceso")
        {
            return await Device.InvokeOnMainThreadAsync(async () =>
            {
                PermissionStatus status = await Permiso.CheckStatusAsync();
                if (status != PermissionStatus.Granted)
                {
                    if (IsDisabled(Permiso))
                    {
                        await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(Mensaje, "Atención.");
                    }
                    await Permiso.CheckStatusAsync();
                    status = await Permiso.CheckStatusAsync();
                }
                return (status == PermissionStatus.Granted);
            });
        }
        public static async Task<bool> TenemosPermiso(Permissions.BasePermission Permiso) => (await GetPermissionStatus(Permiso) == PermissionStatus.Granted);

        public static bool IsDisabled(Permissions.BasePermission permission) => permission.ShouldShowRationale();

        public static async Task<Xamarin.Essentials.PermissionStatus> GetPermissionStatus<T>() where T : Permissions.BasePermission, new()
        {
            return await GetPermissionStatus(new T());
        }
        public static async Task<Xamarin.Essentials.PermissionStatus> GetPermissionStatus(Permissions.BasePermission Permiso)
        {
            return await Device.InvokeOnMainThreadAsync(async () =>
            {
                PermissionStatus status = await Permiso.CheckStatusAsync();
                return status;
            });
        }

        public static async Task<PermissionStatus> EnsurePermission<T>(string RequestMessage, T Permiso = null) where T : Permissions.BasePermission, new()
        {
            Permiso ??= new T();
            if (await Permiso.CheckStatusAsync() != PermissionStatus.Granted)
            {
                if (await PedirPermiso(Permiso, RequestMessage))
                {
                    return PermissionStatus.Granted;
                }
                else
                {
                    return await EnsurePermission<T>(RequestMessage, Permiso);
                }
            }
            else if (await Permiso.CheckStatusAsync() == PermissionStatus.Denied &&
                     IsDisabled(Permiso))
            {
                return PermissionStatus.Denied;
            }

            return await Permiso.CheckStatusAsync();
        }
    }
}
