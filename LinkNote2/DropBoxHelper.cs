using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DropNet;

namespace LinkNote2
{
    class DropBoxHelper
    {
        private static DropNetClient __Client = new DropNetClient(Config.AppKey, Config.AppSecret);

        public static DropNetClient CurrentClient()
        {
            return __Client;
        }
    }
}
