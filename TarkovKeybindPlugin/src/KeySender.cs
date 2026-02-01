namespace Loupedeck.TarkovKeybindPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;

    public static class KeySender
    {
        // Modifier key codes
        public const Int32 VK_SHIFT = 0x10;
        public const Int32 VK_CONTROL = 0x11;
        public const Int32 VK_ALT = 0x12;       // VK_MENU
        public const Int32 VK_LSHIFT = 0xA0;
        public const Int32 VK_RSHIFT = 0xA1;
        public const Int32 VK_LCONTROL = 0xA2;
        public const Int32 VK_RCONTROL = 0xA3;
        public const Int32 VK_LALT = 0xA4;      // VK_LMENU
        public const Int32 VK_RALT = 0xA5;      // VK_RMENU

        // Letter keys A-Z (0x41-0x5A)
        public const Int32 VK_A = 0x41;
        public const Int32 VK_B = 0x42;
        public const Int32 VK_C = 0x43;
        public const Int32 VK_D = 0x44;
        public const Int32 VK_E = 0x45;
        public const Int32 VK_F = 0x46;
        public const Int32 VK_G = 0x47;
        public const Int32 VK_H = 0x48;
        public const Int32 VK_I = 0x49;
        public const Int32 VK_J = 0x4A;
        public const Int32 VK_K = 0x4B;
        public const Int32 VK_L = 0x4C;
        public const Int32 VK_M = 0x4D;
        public const Int32 VK_N = 0x4E;
        public const Int32 VK_O = 0x4F;
        public const Int32 VK_P = 0x50;
        public const Int32 VK_Q = 0x51;
        public const Int32 VK_R = 0x52;
        public const Int32 VK_S = 0x53;
        public const Int32 VK_T = 0x54;
        public const Int32 VK_U = 0x55;
        public const Int32 VK_V = 0x56;
        public const Int32 VK_W = 0x57;
        public const Int32 VK_X = 0x58;
        public const Int32 VK_Y = 0x59;
        public const Int32 VK_Z = 0x5A;

        // Number keys 0-9 (0x30-0x39)
        public const Int32 VK_0 = 0x30;
        public const Int32 VK_1 = 0x31;
        public const Int32 VK_2 = 0x32;
        public const Int32 VK_3 = 0x33;
        public const Int32 VK_4 = 0x34;
        public const Int32 VK_5 = 0x35;
        public const Int32 VK_6 = 0x36;
        public const Int32 VK_7 = 0x37;
        public const Int32 VK_8 = 0x38;
        public const Int32 VK_9 = 0x39;

        // Function keys F1-F12 (0x70-0x7B)
        public const Int32 VK_F1 = 0x70;
        public const Int32 VK_F2 = 0x71;
        public const Int32 VK_F3 = 0x72;
        public const Int32 VK_F4 = 0x73;
        public const Int32 VK_F5 = 0x74;
        public const Int32 VK_F6 = 0x75;
        public const Int32 VK_F7 = 0x76;
        public const Int32 VK_F8 = 0x77;
        public const Int32 VK_F9 = 0x78;
        public const Int32 VK_F10 = 0x79;
        public const Int32 VK_F11 = 0x7A;
        public const Int32 VK_F12 = 0x7B;

        // Numpad keys (0x60-0x69)
        public const Int32 VK_NUMPAD0 = 0x60;
        public const Int32 VK_NUMPAD1 = 0x61;
        public const Int32 VK_NUMPAD2 = 0x62;
        public const Int32 VK_NUMPAD3 = 0x63;
        public const Int32 VK_NUMPAD4 = 0x64;
        public const Int32 VK_NUMPAD5 = 0x65;
        public const Int32 VK_NUMPAD6 = 0x66;
        public const Int32 VK_NUMPAD7 = 0x67;
        public const Int32 VK_NUMPAD8 = 0x68;
        public const Int32 VK_NUMPAD9 = 0x69;

        // Special keys
        public const Int32 VK_SPACE = 0x20;
        public const Int32 VK_RETURN = 0x0D;    // Enter
        public const Int32 VK_ESCAPE = 0x1B;
        public const Int32 VK_TAB = 0x09;
        public const Int32 VK_BACK = 0x08;      // Backspace
        public const Int32 VK_INSERT = 0x2D;
        public const Int32 VK_DELETE = 0x2E;
        public const Int32 VK_HOME = 0x24;
        public const Int32 VK_END = 0x23;
        public const Int32 VK_PRIOR = 0x21;     // Page Up
        public const Int32 VK_NEXT = 0x22;      // Page Down

        // Arrow keys
        public const Int32 VK_LEFT = 0x25;
        public const Int32 VK_UP = 0x26;
        public const Int32 VK_RIGHT = 0x27;
        public const Int32 VK_DOWN = 0x28;

        // Mouse buttons (for reference, not used with SendInput keyboard)
        public const Int32 VK_LBUTTON = 0x01;
        public const Int32 VK_RBUTTON = 0x02;
        public const Int32 VK_MBUTTON = 0x04;

        // Correct struct layout for x64 Windows
        // The INPUT structure is 28 bytes on x64 (4 + 4 padding + 20 for KEYBDINPUT aligned to 8)
        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public UInt16 wVk;
            public UInt16 wScan;
            public UInt32 dwFlags;
            public UInt32 time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public Int32 dx;
            public Int32 dy;
            public UInt32 mouseData;
            public UInt32 dwFlags;
            public UInt32 time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public UInt32 uMsg;
            public UInt16 wParamL;
            public UInt16 wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUTUNION
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public UInt32 type;
            public INPUTUNION u;
        }

        private const UInt32 INPUT_KEYBOARD = 1;
        private const UInt32 KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const UInt32 KEYEVENTF_KEYUP = 0x0002;
        private const UInt32 KEYEVENTF_SCANCODE = 0x0008;

        // Print Screen key code
        public const Int32 VK_SNAPSHOT = 0x2C;

        /// <summary>
        /// Check if a key is an extended key (requires KEYEVENTF_EXTENDEDKEY flag)
        /// Extended keys: Insert, Delete, Home, End, Page Up, Page Down, Arrow keys,
        /// Print Screen, Num Lock, Break/Pause, Divide, Enter (numpad)
        /// </summary>
        private static Boolean IsExtendedKey(Int32 keyCode)
        {
            return keyCode == VK_INSERT ||
                   keyCode == VK_DELETE ||
                   keyCode == VK_HOME ||
                   keyCode == VK_END ||
                   keyCode == VK_PRIOR ||    // Page Up
                   keyCode == VK_NEXT ||     // Page Down
                   keyCode == VK_LEFT ||
                   keyCode == VK_UP ||
                   keyCode == VK_RIGHT ||
                   keyCode == VK_DOWN ||
                   keyCode == VK_SNAPSHOT || // Print Screen
                   keyCode == 0x90 ||        // Num Lock
                   keyCode == 0x13 ||        // Pause/Break
                   keyCode == 0x6F ||        // Numpad Divide
                   keyCode == 0x0D;          // Enter (when from numpad, but we'll include it)
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern UInt32 SendInput(UInt32 nInputs, INPUT[] pInputs, Int32 cbSize);

        [DllImport("user32.dll")]
        private static extern UInt32 MapVirtualKey(UInt32 uCode, UInt32 uMapType);

        [DllImport("kernel32.dll")]
        private static extern UInt32 GetLastError();

        /// <summary>
        /// Sends a key combination with modifiers held properly.
        /// Example: SendKeyCombination(VK_T, shift: true) sends Shift+T
        /// This sends: Shift DOWN -> delay -> T DOWN -> T UP -> delay -> Shift UP
        /// </summary>
        public static void SendKeyCombination(Int32 keyCode, Boolean shift = false, Boolean ctrl = false, Boolean alt = false)
        {
            PluginLog.Write($"SendKeyCombination: key=0x{keyCode:X2}, shift={shift}, ctrl={ctrl}, alt={alt}");

            var inputs = new List<INPUT>();

            // Press modifiers first
            if (shift) inputs.Add(CreateKeyInput(VK_SHIFT, false));
            if (ctrl) inputs.Add(CreateKeyInput(VK_CONTROL, false));
            if (alt) inputs.Add(CreateKeyInput(VK_ALT, false));

            // Small delay after modifiers so game registers them
            if (shift || ctrl || alt)
            {
                SendInputs(inputs.ToArray());
                inputs.Clear();
                Thread.Sleep(15); // Brief delay so game registers modifier
            }

            // Press and release main key
            inputs.Add(CreateKeyInput(keyCode, false));  // Key down
            inputs.Add(CreateKeyInput(keyCode, true));   // Key up

            SendInputs(inputs.ToArray());
            inputs.Clear();

            // Small delay before releasing modifiers
            Thread.Sleep(15);

            // Release modifiers in reverse order
            if (alt) inputs.Add(CreateKeyInput(VK_ALT, true));
            if (ctrl) inputs.Add(CreateKeyInput(VK_CONTROL, true));
            if (shift) inputs.Add(CreateKeyInput(VK_SHIFT, true));

            if (inputs.Count > 0)
            {
                SendInputs(inputs.ToArray());
            }

            PluginLog.Write("SendKeyCombination complete");
        }

        /// <summary>
        /// Send a simple key press (no modifiers)
        /// </summary>
        public static void SendKey(Int32 keyCode)
        {
            PluginLog.Write($"SendKey: key=0x{keyCode:X2}");

            var inputs = new INPUT[]
            {
                CreateKeyInput(keyCode, false),
                CreateKeyInput(keyCode, true)
            };
            SendInputs(inputs);

            PluginLog.Write("SendKey complete");
        }

        /// <summary>
        /// Hold a key down (for continuous actions like holding breath)
        /// Call ReleaseKey when done
        /// </summary>
        public static void HoldKey(Int32 keyCode)
        {
            PluginLog.Write($"HoldKey: key=0x{keyCode:X2}");
            var input = CreateKeyInput(keyCode, false);
            SendInputs(new INPUT[] { input });
        }

        /// <summary>
        /// Release a held key
        /// </summary>
        public static void ReleaseKey(Int32 keyCode)
        {
            PluginLog.Write($"ReleaseKey: key=0x{keyCode:X2}");
            var input = CreateKeyInput(keyCode, true);
            SendInputs(new INPUT[] { input });
        }

        /// <summary>
        /// Send a key using scan code mode (for games that read raw scan codes)
        /// </summary>
        public static void SendKeyScanCode(UInt16 scanCode, Boolean extended = false)
        {
            PluginLog.Write($"SendKeyScanCode: scan=0x{scanCode:X2}, extended={extended}");

            UInt32 flagsDown = KEYEVENTF_SCANCODE;
            UInt32 flagsUp = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;

            if (extended)
            {
                flagsDown |= KEYEVENTF_EXTENDEDKEY;
                flagsUp |= KEYEVENTF_EXTENDEDKEY;
            }

            var inputs = new INPUT[]
            {
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,  // Not used in scan code mode
                            wScan = scanCode,
                            dwFlags = flagsDown,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                },
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = scanCode,
                            dwFlags = flagsUp,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                }
            };

            SendInputs(inputs);
            PluginLog.Write("SendKeyScanCode complete");
        }

        // Common scan codes
        public const UInt16 SC_PRINTSCREEN = 0x37;  // Print Screen scan code (needs extended flag)

        /// <summary>
        /// Send a key WITHOUT the extended flag (for testing)
        /// </summary>
        public static void SendKeyNoExtended(Int32 keyCode)
        {
            PluginLog.Write($"SendKeyNoExtended: key=0x{keyCode:X2}");

            var scanCode = MapVirtualKey((UInt32)keyCode, 0);
            var inputs = new INPUT[]
            {
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = (UInt16)keyCode,
                            wScan = (UInt16)scanCode,
                            dwFlags = 0,  // No flags at all
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                },
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = (UInt16)keyCode,
                            wScan = (UInt16)scanCode,
                            dwFlags = KEYEVENTF_KEYUP,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                }
            };

            SendInputs(inputs);
            PluginLog.Write("SendKeyNoExtended complete");
        }

        /// <summary>
        /// Legacy Print Screen sequence - historically PrtSc sent a fake shift
        /// </summary>
        public static void SendPrintScreenLegacy()
        {
            PluginLog.Write("SendPrintScreenLegacy: E0,2A then E0,37");

            // Legacy sequence: E0,2A (fake LShift) then E0,37 (PrtSc)
            // Press sequence
            var inputs = new INPUT[]
            {
                // Fake shift down (scan code 0x2A with extended)
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = 0x2A,
                            dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_EXTENDEDKEY,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                },
                // Print Screen down
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = 0x37,
                            dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_EXTENDEDKEY,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                },
                // Print Screen up
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = 0x37,
                            dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                },
                // Fake shift up
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = 0x2A,
                            dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                }
            };

            SendInputs(inputs);
            PluginLog.Write("SendPrintScreenLegacy complete");
        }

        /// <summary>
        /// Send with both VK code and scan code set
        /// </summary>
        public static void SendKeyWithBoth(Int32 keyCode, UInt16 scanCode, Boolean extended)
        {
            PluginLog.Write($"SendKeyWithBoth: vk=0x{keyCode:X2}, scan=0x{scanCode:X2}, ext={extended}");

            UInt32 flagsDown = extended ? KEYEVENTF_EXTENDEDKEY : 0;
            UInt32 flagsUp = KEYEVENTF_KEYUP | (extended ? KEYEVENTF_EXTENDEDKEY : 0);

            var inputs = new INPUT[]
            {
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = (UInt16)keyCode,
                            wScan = scanCode,
                            dwFlags = flagsDown,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                },
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = (UInt16)keyCode,
                            wScan = scanCode,
                            dwFlags = flagsUp,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                }
            };

            SendInputs(inputs);
            PluginLog.Write("SendKeyWithBoth complete");
        }

        private static INPUT CreateKeyInput(Int32 keyCode, Boolean keyUp)
        {
            var scanCode = MapVirtualKey((UInt32)keyCode, 0);

            // Build flags - extended keys need KEYEVENTF_EXTENDEDKEY
            UInt32 flags = 0;
            if (IsExtendedKey(keyCode))
            {
                flags |= KEYEVENTF_EXTENDEDKEY;
            }
            if (keyUp)
            {
                flags |= KEYEVENTF_KEYUP;
            }

            return new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (UInt16)keyCode,
                        wScan = (UInt16)scanCode,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
        }

        private static void SendInputs(INPUT[] inputs)
        {
            var structSize = Marshal.SizeOf(typeof(INPUT));
            PluginLog.Write($"INPUT struct size: {structSize} bytes");
            var result = SendInput((UInt32)inputs.Length, inputs, structSize);
            if (result == 0)
            {
                var errorCode = GetLastError();
                PluginLog.Write($"SendInput FAILED: returned 0, GetLastError={errorCode} (tried to send {inputs.Length} inputs)");
            }
            else
            {
                PluginLog.Write($"SendInput SUCCESS: returned {result} (sent {inputs.Length} inputs)");
            }
        }
    }
}
