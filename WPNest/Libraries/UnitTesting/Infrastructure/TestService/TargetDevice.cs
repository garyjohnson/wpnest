// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using Microsoft.SmartDevice.Connectivity;

namespace Microsoft.Silverlight.Testing.Tools
{
    /// <summary>
    /// Target device
    /// </summary>
    public class TargetDevice
    {
        private readonly TargetDeviceInfo deviceInfo;
        private readonly DatastoreManager manager;

        private Device currentDevice;
        private RemoteApplication application;

        /// <summary>
        /// Initializes a new instance of the TargetDevice type.
        /// </summary>
        public TargetDevice(TargetDeviceInfo deviceInfo)
        {
            this.deviceInfo = deviceInfo;

            this.manager = new DatastoreManager(deviceInfo.Locale);
        }

        public void Start()
        {
            var platform = manager.GetPlatform(new ObjectId(deviceInfo.PlatformId));
            currentDevice = platform.GetDevice(new ObjectId(deviceInfo.DeviceId));

            currentDevice.Connect();
        }

        /// <summary>
        /// Starts the web browser, pointing at a given address.
        /// </summary>
        /// /// <param name="applicationProductId"></param>
        /// <param name="applicationGenre">The genre of the application being installed</param>
        /// <param name="pathToXap">The path to the local XAP file.</param>
        /// <param name="update">If true, the application will be updated if already on the device; otherwise, the application will be uninstalled before being reinstalled</param>
        public void Start(Guid applicationProductId, string applicationGenre, string pathToXap, bool update)
        {
            this.Start();

            if (currentDevice.IsApplicationInstalled(applicationProductId))
            {
                application = currentDevice.GetApplication(applicationProductId);

                if (update)
                {
                    application.UpdateApplication(applicationGenre, "", pathToXap);
                }
                else
                {
                    application.Uninstall();
                    application = currentDevice.InstallApplication(applicationProductId, applicationProductId,
                        applicationGenre, "", pathToXap);
                }
            }
            else
            {
                application = currentDevice.InstallApplication(applicationProductId, applicationProductId,
                        applicationGenre, "", pathToXap);
            }
            
            application.Launch();
        }

        /// <summary>
        /// Closes the target devuce.
        /// </summary>
        public void Close()
        {
            if (application != null)
            {
                application.TerminateRunningInstances();
            }

            if (currentDevice != null)
            {
                currentDevice.Disconnect();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the target device is running.
        /// </summary>
        public bool IsRunning
        {
            get { return currentDevice != null && currentDevice.IsConnected(); }
        }
    }
}