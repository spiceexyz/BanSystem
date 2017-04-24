using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oxide;
using Oxide.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("BanSystem", "PsychoTea", "1.0.0")]

    class BanSystem : RustPlugin
    {
        [ChatCommand("test")]
        void testCommand(BasePlayer player, string command, string[] args)
        {

        }
    }
}
