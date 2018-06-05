﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace YouAreNotAlone
{
    public static class VehicleName
    {
        private delegate IntPtr GetModelInfoDelegate(int hash, IntPtr indexPtr);
        private static IntPtr address = FindPattern("0F B7 05 ?? ?? ?? ?? 45 33 C9 4C 8B DA 66 85 C0 0F 84 ?? ?? ?? ?? 44 0F B7 C0 33 D2 8B C1 41 F7 F0 48 8B 05 ?? ?? ?? ?? 4C 8B 14 D0 EB 09 41 3B 0A 74 54");
        private static GetModelInfoDelegate GetModelInfo { get { return Marshal.GetDelegateForFunctionPointer<GetModelInfoDelegate>(address); } }

        public static string GetNameOf(int hash)
        {
            int index = -1;
            GCHandle handle = GCHandle.Alloc(index, GCHandleType.Pinned);
            IntPtr modelInfo = GetModelInfo(hash, handle.AddrOfPinnedObject());

            string displayName = GTA.Game.GetGXTEntry(Marshal.PtrToStringAnsi(modelInfo + 664));
            string makeName = GTA.Game.GetGXTEntry(Marshal.PtrToStringAnsi(modelInfo + 676));
            handle.Free();

            string vehicleName = "";

            if (makeName != "NULL") vehicleName += makeName + " ";
            if (displayName != "NULL") vehicleName += displayName;

            return vehicleName;
        }

        private unsafe static bool Compare(IntPtr data, byte[] bytesArray)
        {
            for (int i = 0; i < bytesArray.Length; i++)
            {
                if (bytesArray[i] != 0 && Marshal.ReadByte(data + i) != bytesArray[i]) return false;
            }

            return true;
        }

        private unsafe static IntPtr FindPattern(string pattern)
        {
            ProcessModule module = Process.GetCurrentProcess().MainModule;
            long address = module.BaseAddress.ToInt64();
            long endAddress = address + module.ModuleMemorySize;

            pattern = pattern.Replace(" ", "").Replace("??", "00");
            byte[] bytesArray = new byte[pattern.Length / 2];

            for (int i = 0; i < pattern.Length; i += 2) bytesArray[i / 2] = byte.Parse(pattern.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);

            for (; address < endAddress; address++)
            {
                IntPtr data = new IntPtr(address);

                if (Compare(data, bytesArray)) return data;
            }

            return IntPtr.Zero;
        }
    }
}