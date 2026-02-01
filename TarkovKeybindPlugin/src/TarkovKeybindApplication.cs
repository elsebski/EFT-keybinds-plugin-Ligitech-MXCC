namespace Loupedeck.TarkovKeybindPlugin
{
    using System;

    public class TarkovKeybindApplication : ClientApplication
    {
        public TarkovKeybindApplication()
        {
        }

        protected override String GetProcessName() => "";
        protected override String GetBundleName() => "";
        public override ClientApplicationStatus GetApplicationStatus() => ClientApplicationStatus.Unknown;
    }
}
