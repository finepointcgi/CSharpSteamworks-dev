using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSteamworks.Networking
{
    public static class Extensions
    {

        public static void Print(this Dictionary<string, string> packet)
        {
            GD.Print("{");

            foreach (KeyValuePair<string, string> kvp in packet)
            {
                GD.Print($"\t\"{kvp.Key}\": \"{kvp.Value}\"");
            }

            GD.Print("}");
        }
    }
}
