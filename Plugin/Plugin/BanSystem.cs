using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oxide;
using Plugin;
using UnityEngine;

namespace Plugin
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
