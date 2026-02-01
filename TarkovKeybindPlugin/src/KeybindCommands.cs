namespace Loupedeck.TarkovKeybindPlugin
{
    using System;

    // ============================================================
    // Base class for keybind buttons
    // ============================================================
    public abstract class KeybindCommandBase : PluginDynamicCommand
    {
        protected abstract String KeyName { get; }
        protected abstract void ExecuteKeybind();

        protected KeybindCommandBase(String displayName, String description)
            : base(displayName, description, "Tarkov Keybinds")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            PluginLog.Write($"Command executed: {this.DisplayName}");
            this.ExecuteKeybind();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            using (var builder = new BitmapBuilder(imageSize))
            {
                builder.Clear(new BitmapColor(40, 40, 40));
                builder.DrawText(this.KeyName, BitmapColor.White);
                return builder.ToImage();
            }
        }
    }

    // ============================================================
    // MOVEMENT & STANCE CONTROLS
    // ============================================================

    /// <summary>E - Lean Right</summary>
    public class LeanRightCommand : KeybindCommandBase
    {
        protected override String KeyName => "Lean\nRight";

        public LeanRightCommand()
            : base("Lean Right", "E - Lean right")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_E);
        }
    }

    /// <summary>Q - Lean Left</summary>
    public class LeanLeftCommand : KeybindCommandBase
    {
        protected override String KeyName => "Lean\nLeft";

        public LeanLeftCommand()
            : base("Lean Left", "Q - Lean left")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_Q);
        }
    }

    /// <summary>LAlt+D - Smoothly Lean Right</summary>
    public class SmoothLeanRightCommand : KeybindCommandBase
    {
        protected override String KeyName => "Smooth\nLean R";

        public SmoothLeanRightCommand()
            : base("Smooth Lean Right", "LAlt+D - Smoothly lean right")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_D, alt: true);
        }
    }

    /// <summary>LAlt+A - Smoothly Lean Left</summary>
    public class SmoothLeanLeftCommand : KeybindCommandBase
    {
        protected override String KeyName => "Smooth\nLean L";

        public SmoothLeanLeftCommand()
            : base("Smooth Lean Left", "LAlt+A - Smoothly lean left")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_A, alt: true);
        }
    }

    /// <summary>LAlt+E - Sidestep Right</summary>
    public class SidestepRightCommand : KeybindCommandBase
    {
        protected override String KeyName => "Sidestep\nRight";

        public SidestepRightCommand()
            : base("Sidestep Right", "LAlt+E - Sidestep right")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_E, alt: true);
        }
    }

    /// <summary>LAlt+Q - Sidestep Left</summary>
    public class SidestepLeftCommand : KeybindCommandBase
    {
        protected override String KeyName => "Sidestep\nLeft";

        public SidestepLeftCommand()
            : base("Sidestep Left", "LAlt+Q - Sidestep left")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_Q, alt: true);
        }
    }

    /// <summary>Caps - Walk Toggle</summary>
    public class WalkToggleCommand : KeybindCommandBase
    {
        protected override String KeyName => "Walk";

        public WalkToggleCommand()
            : base("Walk Toggle", "CapsLock - Toggle walk")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(0x14); // VK_CAPITAL
        }
    }

    /// <summary>Space - Jump</summary>
    public class JumpCommand : KeybindCommandBase
    {
        protected override String KeyName => "Jump";

        public JumpCommand()
            : base("Jump", "Space - Jump/Vault")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_SPACE);
        }
    }

    /// <summary>C - Crouch</summary>
    public class CrouchCommand : KeybindCommandBase
    {
        protected override String KeyName => "Crouch";

        public CrouchCommand()
            : base("Crouch", "C - Toggle crouch")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_C);
        }
    }

    /// <summary>X - Prone</summary>
    public class ProneCommand : KeybindCommandBase
    {
        protected override String KeyName => "Prone";

        public ProneCommand()
            : base("Prone", "X - Go prone")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_X);
        }
    }

    /// <summary>LAlt+W - Overhead Blind Fire</summary>
    public class BlindFireUpCommand : KeybindCommandBase
    {
        protected override String KeyName => "Blind\nUp";

        public BlindFireUpCommand()
            : base("Blind Fire Up", "LAlt+W - Overhead blind fire")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_W, alt: true);
        }
    }

    /// <summary>LAlt+S - Right Side Blind Fire</summary>
    public class BlindFireRightCommand : KeybindCommandBase
    {
        protected override String KeyName => "Blind\nRight";

        public BlindFireRightCommand()
            : base("Blind Fire Right", "LAlt+S - Right side blind fire")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_S, alt: true);
        }
    }

    // ============================================================
    // WEAPON CONTROLS
    // ============================================================

    /// <summary>R - Reload Weapon</summary>
    public class ReloadCommand : KeybindCommandBase
    {
        protected override String KeyName => "Reload";

        public ReloadCommand()
            : base("Reload", "R - Reload weapon")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_R);
        }
    }

    /// <summary>B - Switch Fire Mode</summary>
    public class FireModeCommand : KeybindCommandBase
    {
        protected override String KeyName => "Fire\nMode";

        public FireModeCommand()
            : base("Fire Mode", "B - Switch fire mode")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_B);
        }
    }

    /// <summary>LAlt+B - Check Fire Mode</summary>
    public class CheckFireModeCommand : KeybindCommandBase
    {
        protected override String KeyName => "Check\nFire Mode";

        public CheckFireModeCommand()
            : base("Check Fire Mode", "LAlt+B - Check current fire mode")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_B, alt: true);
        }
    }

    /// <summary>LAlt+R - Detach Magazine</summary>
    public class DetachMagCommand : KeybindCommandBase
    {
        protected override String KeyName => "Detach\nMag";

        public DetachMagCommand()
            : base("Detach Magazine", "LAlt+R - Detach magazine")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_R, alt: true);
        }
    }

    /// <summary>LCtrl+R - Unload Chamber</summary>
    public class UnloadChamberCommand : KeybindCommandBase
    {
        protected override String KeyName => "Unload\nChamber";

        public UnloadChamberCommand()
            : base("Unload Chamber", "LCtrl+R - Unload chamber")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_R, ctrl: true);
        }
    }

    /// <summary>LAlt+T - Check Ammo</summary>
    public class CheckAmmoCommand : KeybindCommandBase
    {
        protected override String KeyName => "Check\nAmmo";

        public CheckAmmoCommand()
            : base("Check Ammo", "LAlt+T - Check ammo count")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_T, alt: true);
        }
    }

    /// <summary>LShift+T - Check Chamber / Fix Malfunction</summary>
    public class CheckChamberCommand : KeybindCommandBase
    {
        protected override String KeyName => "Check\nChamber";

        public CheckChamberCommand()
            : base("Check Chamber", "LShift+T - Check chamber / Fix malfunction")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_T, shift: true);
        }
    }

    /// <summary>L - Inspect Weapon</summary>
    public class InspectCommand : KeybindCommandBase
    {
        protected override String KeyName => "Inspect";

        public InspectCommand()
            : base("Inspect Weapon", "L - Inspect current weapon")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_L);
        }
    }

    /// <summary>LAlt+L - Fold/Unfold Stock</summary>
    public class FoldStockCommand : KeybindCommandBase
    {
        protected override String KeyName => "Fold\nStock";

        public FoldStockCommand()
            : base("Fold Stock", "LAlt+L - Fold or unfold stock")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_L, alt: true);
        }
    }

    /// <summary>V - Mount Weapon</summary>
    public class MountWeaponCommand : KeybindCommandBase
    {
        protected override String KeyName => "Mount";

        public MountWeaponCommand()
            : base("Mount Weapon", "V - Mount weapon on surface")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_V);
        }
    }

    /// <summary>LCtrl+V - Toggle Bipod</summary>
    public class ToggleBipodCommand : KeybindCommandBase
    {
        protected override String KeyName => "Bipod";

        public ToggleBipodCommand()
            : base("Toggle Bipod", "LCtrl+V - Toggle bipod")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_V, ctrl: true);
        }
    }

    // ============================================================
    // SCOPE & SIGHTS
    // ============================================================

    /// <summary>PgUp - Scope Elevation Up</summary>
    public class ZeroingUpCommand : KeybindCommandBase
    {
        protected override String KeyName => "Zero\nUp";

        public ZeroingUpCommand()
            : base("Zeroing Up", "PgUp - Scope elevation up")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_PRIOR);
        }
    }

    /// <summary>PgDn - Scope Elevation Down</summary>
    public class ZeroingDownCommand : KeybindCommandBase
    {
        protected override String KeyName => "Zero\nDown";

        public ZeroingDownCommand()
            : base("Zeroing Down", "PgDn - Scope elevation down")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_NEXT);
        }
    }

    // ============================================================
    // TACTICAL DEVICES
    // ============================================================

    /// <summary>T - Toggle Tactical Devices</summary>
    public class TacticalToggleCommand : KeybindCommandBase
    {
        protected override String KeyName => "Tactical\nToggle";

        public TacticalToggleCommand()
            : base("Tactical Toggle", "T - Toggle tactical devices on/off")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_T);
        }
    }

    /// <summary>LCtrl+T - Switch Tactical Device Mode</summary>
    public class TacticalModeCommand : KeybindCommandBase
    {
        protected override String KeyName => "Tactical\nMode";

        public TacticalModeCommand()
            : base("Tactical Mode", "LCtrl+T - Switch tactical device mode")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_T, ctrl: true);
        }
    }

    /// <summary>T+B - Tactical Device Activation Mode</summary>
    public class TacticalActivationModeCommand : KeybindCommandBase
    {
        protected override String KeyName => "Tactical\nActiv";

        public TacticalActivationModeCommand()
            : base("Tactical Activation Mode", "T+B - Tactical device activation mode")
        {
        }

        protected override void ExecuteKeybind()
        {
            // Press T, then B while T is held
            KeySender.HoldKey(KeySender.VK_T);
            System.Threading.Thread.Sleep(50);
            KeySender.SendKey(KeySender.VK_B);
            System.Threading.Thread.Sleep(50);
            KeySender.ReleaseKey(KeySender.VK_T);
        }
    }

    /// <summary>H - Toggle Helmet Tactical Device</summary>
    public class HelmetTacticalToggleCommand : KeybindCommandBase
    {
        protected override String KeyName => "Helmet\nLight";

        public HelmetTacticalToggleCommand()
            : base("Helmet Tactical Toggle", "H - Toggle helmet tactical device")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_H);
        }
    }

    /// <summary>LCtrl+H - Switch Helmet Tactical Device Mode</summary>
    public class HelmetTacticalModeCommand : KeybindCommandBase
    {
        protected override String KeyName => "Helmet\nMode";

        public HelmetTacticalModeCommand()
            : base("Helmet Tactical Mode", "LCtrl+H - Switch helmet tactical device mode")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKeyCombination(KeySender.VK_H, ctrl: true);
        }
    }

    /// <summary>N - Toggle NVG/Face Shield</summary>
    public class ToggleNVGCommand : KeybindCommandBase
    {
        protected override String KeyName => "NVG\nToggle";

        public ToggleNVGCommand()
            : base("Toggle NVG/Face Shield", "N - Toggle on-head equipment")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_N);
        }
    }

    // ============================================================
    // GRENADE & MELEE
    // ============================================================

    /// <summary>G - Prepare Grenade</summary>
    public class GrenadeCommand : KeybindCommandBase
    {
        protected override String KeyName => "Grenade";

        public GrenadeCommand()
            : base("Prepare Grenade", "G - Prepare a grenade")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_G);
        }
    }

    /// <summary>U - Melee Weapon / Attack</summary>
    public class MeleeCommand : KeybindCommandBase
    {
        protected override String KeyName => "Melee";

        public MeleeCommand()
            : base("Melee Attack", "U - Melee weapon / attack")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_U);
        }
    }

    // ============================================================
    // WEAPON SLOTS
    // ============================================================

    /// <summary>1 - Sidearm</summary>
    public class SidearmCommand : KeybindCommandBase
    {
        protected override String KeyName => "Sidearm";

        public SidearmCommand()
            : base("Sidearm", "1 - Switch to sidearm")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_1);
        }
    }

    /// <summary>2 - Weapon on Sling</summary>
    public class PrimaryWeaponCommand : KeybindCommandBase
    {
        protected override String KeyName => "Primary";

        public PrimaryWeaponCommand()
            : base("Primary Weapon", "2 - Weapon on sling")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_2);
        }
    }

    /// <summary>3 - Weapon on Back</summary>
    public class SecondaryWeaponCommand : KeybindCommandBase
    {
        protected override String KeyName => "Secondary";

        public SecondaryWeaponCommand()
            : base("Secondary Weapon", "3 - Weapon on back")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_3);
        }
    }

    // ============================================================
    // QUICK SLOTS (4-9)
    // ============================================================

    /// <summary>4 - Quick Slot 4</summary>
    public class Slot4Command : KeybindCommandBase
    {
        protected override String KeyName => "Slot 4";

        public Slot4Command()
            : base("Quick Slot 4", "4 - Use item in slot 4")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_4);
        }
    }

    /// <summary>5 - Quick Slot 5</summary>
    public class Slot5Command : KeybindCommandBase
    {
        protected override String KeyName => "Slot 5";

        public Slot5Command()
            : base("Quick Slot 5", "5 - Use item in slot 5")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_5);
        }
    }

    /// <summary>6 - Quick Slot 6</summary>
    public class Slot6Command : KeybindCommandBase
    {
        protected override String KeyName => "Slot 6";

        public Slot6Command()
            : base("Quick Slot 6", "6 - Use item in slot 6")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_6);
        }
    }

    /// <summary>7 - Quick Slot 7</summary>
    public class Slot7Command : KeybindCommandBase
    {
        protected override String KeyName => "Slot 7";

        public Slot7Command()
            : base("Quick Slot 7", "7 - Use item in slot 7")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_7);
        }
    }

    /// <summary>8 - Quick Slot 8</summary>
    public class Slot8Command : KeybindCommandBase
    {
        protected override String KeyName => "Slot 8";

        public Slot8Command()
            : base("Quick Slot 8", "8 - Use item in slot 8")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_8);
        }
    }

    /// <summary>9 - Quick Slot 9</summary>
    public class Slot9Command : KeybindCommandBase
    {
        protected override String KeyName => "Slot 9";

        public Slot9Command()
            : base("Quick Slot 9", "9 - Use item in slot 9")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_9);
        }
    }

    // ============================================================
    // INTERACTION & UTILITY
    // ============================================================

    /// <summary>F - Interact</summary>
    public class InteractCommand : KeybindCommandBase
    {
        protected override String KeyName => "Interact";

        public InteractCommand()
            : base("Interact", "F - Interact with object")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_F);
        }
    }

    /// <summary>Tab - Toggle Inventory</summary>
    public class InventoryCommand : KeybindCommandBase
    {
        protected override String KeyName => "Inventory";

        public InventoryCommand()
            : base("Toggle Inventory", "Tab - Open/close inventory")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_TAB);
        }
    }

    /// <summary>O - Check Time and Exits</summary>
    public class CheckTimeCommand : KeybindCommandBase
    {
        protected override String KeyName => "Check\nTime";

        public CheckTimeCommand()
            : base("Check Time", "O - Check time and exits")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_O);
        }
    }

    /// <summary>Double O - Check Time (double tap for compass)</summary>
    public class CompassCommand : KeybindCommandBase
    {
        protected override String KeyName => "Compass";

        public CompassCommand()
            : base("Compass", "Double O - Check compass")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_O);
            System.Threading.Thread.Sleep(50);
            KeySender.SendKey(KeySender.VK_O);
        }
    }

    /// <summary>Y - Phrase Menu / Voice Commands</summary>
    public class PhraseMenuCommand : KeybindCommandBase
    {
        protected override String KeyName => "Phrase\nMenu";

        public PhraseMenuCommand()
            : base("Phrase Menu", "Y - Open voice command menu")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_Y);
        }
    }

    /// <summary>Double Y - Quick Voice Phrase</summary>
    public class QuickPhraseCommand : KeybindCommandBase
    {
        protected override String KeyName => "Quick\nPhrase";

        public QuickPhraseCommand()
            : base("Quick Phrase", "Double Y - Quick voice phrase")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_Y);
            System.Threading.Thread.Sleep(50);
            KeySender.SendKey(KeySender.VK_Y);
        }
    }

    /// <summary>K - Push to Talk</summary>
    public class PushToTalkCommand : KeybindCommandBase
    {
        protected override String KeyName => "PTT";

        public PushToTalkCommand()
            : base("Push to Talk", "K - VoIP push to talk")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_K);
        }
    }

    /// <summary>Del - Discard Item</summary>
    public class DiscardCommand : KeybindCommandBase
    {
        protected override String KeyName => "Discard";

        public DiscardCommand()
            : base("Discard Item", "Del - Discard selected item")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_DELETE);
        }
    }

    /// <summary>Z - Drop Backpack</summary>
    public class DropBackpackCommand : KeybindCommandBase
    {
        protected override String KeyName => "Drop\nBackpack";

        public DropBackpackCommand()
            : base("Drop Backpack", "Z - Drop backpack")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_Z);
        }
    }

    /// <summary>M - Play Tape / Read Note</summary>
    public class PlayTapeCommand : KeybindCommandBase
    {
        protected override String KeyName => "Play\nTape";

        public PlayTapeCommand()
            : base("Play Tape", "M - Play tape / Read note")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_M);
        }
    }

    /// <summary>I - Icons Toggle</summary>
    public class IconsToggleCommand : KeybindCommandBase
    {
        protected override String KeyName => "Icons";

        public IconsToggleCommand()
            : base("Icons Toggle", "I - Toggle icons display")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_I);
        }
    }

    /// <summary>P - Toggle Item Pin Mode</summary>
    public class PinModeCommand : KeybindCommandBase
    {
        protected override String KeyName => "Pin\nMode";

        public PinModeCommand()
            : base("Pin Mode", "P - Toggle item pin mode")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_P);
        }
    }

    /// <summary>Print - Screenshot (rebind in Tarkov to "Print")</summary>
    public class ScreenshotCommand : KeybindCommandBase
    {
        protected override String KeyName => "Screenshot";

        public ScreenshotCommand()
            : base("Screenshot", "Print - Rebind in Tarkov to 'Print' instead of 'PrtScn'")
        {
        }

        protected override void ExecuteKeybind()
        {
            // Sends as "Print" in Tarkov - rebind screenshot to Print in game settings
            KeySender.SendKey(KeySender.VK_SNAPSHOT);
        }
    }

    /// <summary>Esc - Escape/Menu</summary>
    public class EscapeCommand : KeybindCommandBase
    {
        protected override String KeyName => "Escape";

        public EscapeCommand()
            : base("Escape", "Esc - Open menu / Cancel")
        {
        }

        protected override void ExecuteKeybind()
        {
            KeySender.SendKey(KeySender.VK_ESCAPE);
        }
    }
}
