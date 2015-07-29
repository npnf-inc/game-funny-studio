using System;
using UnityEngine;
using NPNF.Core.UserModule;

namespace NPNF
{
    public class UserTrackingController: IUserTrackingController
    {
        public DeviceProfile DeviceProfile { get; internal set; }

        public UnityBridgePlugin bridgeObj;

        public UserTrackingController(UnityBridgePlugin bridge)
        {
            bridgeObj = bridge;
        }
    }
}