using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Android.App;
using Kit.Droid.Services;

[assembly: ComVisible(false)]
[assembly: UsesPermission(name: "android.permission.READ_EXTERNAL_STORAGE")]
[assembly: UsesPermission(name: "android.permission.WRITE_EXTERNAL_STORAGE")]
[assembly: UsesPermission(name: "android.permission.ACCESS_NETWORK_STATE")]
[assembly: UsesPermission(name: "android.permission.INTERNAL_SYSTEM_WINDOW")]
[assembly: UsesPermission(name: "android.permission.INTERNET")]
[assembly: UsesPermission(name: "android.permission.FLASHLIGHT")]
[assembly: UsesPermission(name: "android.permission.CAMERA")]
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]
