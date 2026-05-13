using System.Diagnostics;
using System.Runtime.InteropServices;
using NetworkCore.Protocol;

namespace SharpView.Server.Helpers;

/// <summary>
/// Collects live CPU and RAM usage via <see cref="PerformanceCounter"/>.
/// Designed for a long-lived background loop (1 call/second).
/// </summary>
public sealed class HardwareMonitorService : IDisposable
{
    private readonly PerformanceCounter _cpuCounter;
    private bool _isWarmedUp;

    public HardwareMonitorService()
    {
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        // Dummy call — PerformanceCounter always returns 0.0 on the first read.
        // This "primes" the internal timing so subsequent calls return real values.
        _cpuCounter.NextValue();
        _isWarmedUp = false;
    }

    /// <summary>
    /// Returns a <see cref="MonitorPacket"/> with current CPU% and RAM%.
    /// The first call after construction may still return 0% CPU; this is expected.
    /// </summary>
    public MonitorPacket GetCurrentStats()
    {
        float cpuUsage = _cpuCounter.NextValue();

        // On the very first real call, mark as warmed up
        if (!_isWarmedUp)
        {
            _isWarmedUp = true;
            // The value from this call is now meaningful (primed in ctor)
        }

        float ramUsage = GetRamUsagePercentage();

        return new MonitorPacket
        {
            CpuUsage = cpuUsage,
            RamUsagePercentage = ramUsage
        };
    }

    /// <summary>
    /// Calculates RAM usage percentage using Win32 GlobalMemoryStatusEx.
    /// More reliable than PerformanceCounter for total/available RAM.
    /// </summary>
    private static float GetRamUsagePercentage()
    {
        var memStatus = new MEMORYSTATUSEX();
        if (GlobalMemoryStatusEx(memStatus))
        {
            return memStatus.dwMemoryLoad;
        }
        return 0f;
    }

    public void Dispose()
    {
        _cpuCounter.Dispose();
    }

    // ─── Win32 Interop for accurate RAM info ───

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
        public uint dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
}
