//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//
using System.Threading.Tasks;

namespace App1
{
    public interface IMicrophoneService
    {
        Task<bool> GetPermissionsAsync();
        void OnRequestPermissionsResult(bool isGranted);
    }
}