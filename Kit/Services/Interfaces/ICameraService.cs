﻿using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kit.Services.Interfaces
{
    public class ICameraService
    {
        PermissionStatus cameraOK;
        PermissionStatus storageOK;

        public async Task Init()
        {
            await CrossMedia.Current.Initialize();
            cameraOK = await CrossPermissions.Current.CheckPermissionStatusAsync<CameraPermission>();
            storageOK = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
            if (cameraOK != PermissionStatus.Granted || storageOK != PermissionStatus.Granted)
            {
                cameraOK = await CrossPermissions.Current.RequestPermissionAsync<CameraPermission>();
                storageOK = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
            }
        }

        public async Task<MediaFile> TakePhoto()
        {
            if (cameraOK == PermissionStatus.Granted
                && storageOK == PermissionStatus.Granted
                && CrossMedia.Current.IsCameraAvailable
                && CrossMedia.Current.IsTakePhotoSupported)
            {
                var options = new StoreCameraMediaOptions
                {
                    DefaultCamera = CameraDevice.Front, // Doesn't always work on Android, depends on Device
                    AllowCropping = true, // UWP & iOS only,
                    PhotoSize = PhotoSize.Medium, // if Custom, you can set CustomPhotoSize = percentage_value 
                    CompressionQuality = 90,
                    Directory = "DemoCamara",
                    Name = $"{Guid.NewGuid()}.jpg",
                    SaveToAlbum = true
                };

                var file = await CrossMedia.Current.TakePhotoAsync(options);
                return file;
            }

            return null;
        }

        public async Task<MediaFile> ChoosePhoto()
        {
            if (CrossMedia.Current.IsPickPhotoSupported)
            {
                var file = await CrossMedia.Current.PickPhotoAsync();
                return file;
            }

            return null;
        }

        public async Task<MediaFile> TakeVideo()
        {
            if (cameraOK == PermissionStatus.Granted
                && storageOK == PermissionStatus.Granted
                && CrossMedia.Current.IsTakeVideoSupported)
            {
                var options = new StoreVideoOptions { SaveToAlbum = true };
                var file = await CrossMedia.Current.TakeVideoAsync(options);
                return file;
            }

            return null;
        }

        public async Task<MediaFile> ChooseVideo()
        {
            if (CrossMedia.Current.IsPickVideoSupported)
            {
                var file = await CrossMedia.Current.PickVideoAsync();
                return file;
            }

            return null;
        }
    }
}

