using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marler.Tank
{
    public static class Settings
    {
        //
        // Network
        //
        public const int lanServerPort               = 40567;
        public const int lanClientPort               = 40568;

        public const int serverPort                  = 40569;

        public const int clientJoinAttempts          = 6;
        public const int clientJoinRecvTimeoutMillis = 500;

        public const String internetServerHostName = "tank.marler.info";

        
        //
        // Constants
        //   
        public const float glWallHeight              = 20f;
        public const float glBoundaryWallWidth       = 20f;

    }
}
