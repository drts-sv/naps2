﻿using System.Runtime.InteropServices;

namespace NAPS2.Platform;

internal class PlatformCompat
{
    private static ISystemCompat _systemCompat;

    static PlatformCompat()
    {
        // TODO: This might be wrong for netstandard
#if NET6_0_OR_GREATER
        if (OperatingSystem.IsWindows())
        {
            _systemCompat = GetWindowsSystemCompat();
        }
        else if (OperatingSystem.IsMacOS())
        {
            _systemCompat = new MacSystemCompat();
        }
        else if (OperatingSystem.IsLinux())
        {
            _systemCompat = new LinuxSystemCompat();
        }
        else
        {
            throw new InvalidOperationException("Unsupported platform");
        }
#else
        _systemCompat = GetWindowsSystemCompat();
#endif
    }

    private static ISystemCompat GetWindowsSystemCompat() =>
        RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm64 => new WindowsArm64SystemCompat(),
            Architecture.X86 => new Windows32SystemCompat(),
            _ => new Windows64SystemCompat()
        };

    public static ISystemCompat System
    {
        get => _systemCompat;
        set => _systemCompat = value ?? throw new ArgumentNullException(nameof(value));
    }
}