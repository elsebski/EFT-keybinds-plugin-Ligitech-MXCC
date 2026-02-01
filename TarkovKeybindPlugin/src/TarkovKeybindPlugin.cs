namespace Loupedeck.TarkovKeybindPlugin
{
    using System;

    public class TarkovKeybindPlugin : Plugin
    {
        public override Boolean UsesApplicationApiOnly => true;
        public override Boolean HasNoApplication => true;

        public TarkovKeybindPlugin()
        {
        }

        public override void Load()
        {
            // Plugin loaded - commands are auto-discovered
            PluginLog.Clear();
            PluginLog.Write("TarkovKeybindPlugin loaded");
        }

        public override void Unload()
        {
            PluginLog.Write("TarkovKeybindPlugin unloaded");
        }
    }
}
