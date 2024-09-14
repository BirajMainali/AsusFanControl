using System;
using AsusSystemAnalysis;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsusFanControl
{
    public class AsusControl
    {
        public AsusControl()
        {
            AsusWinIO64.InitializeWinIo();
        }

        ~AsusControl()
        {
            AsusWinIO64.ShutdownWinIo();
        }

        private void SetFanSpeed(byte value, byte fanIndex = 0)
        {
            AsusWinIO64.HealthyTable_SetFanIndex(fanIndex);
            AsusWinIO64.HealthyTable_SetFanPwmDuty(value);
            AsusWinIO64.HealthyTable_SetFanTestMode((char)(value > 0 ? 0x01 : 0x00));
        }

        public void SetFanSpeed(int percent, byte fanIndex = 0)
        {
            var value = (byte)(percent / 100.0f * 255);
            SetFanSpeed(value, fanIndex);
        }

        public void ControlFanSpeed(Action<int, int> afterControl = null)
        {
            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(30));
                var cpuTemp = AsusWinIO64.Thermal_Read_Cpu_Temperature();
                var fanSpeed = GetFanSpeedBasedOnTemperature(cpuTemp);
                SetFanSpeed(fanSpeed);
                afterControl?.Invoke(fanSpeed, (int)cpuTemp);
            }
        }

        private int GetFanSpeedBasedOnTemperature(ulong cpuTemp)
        {
            return 
                cpuTemp > 80 ? 100 :
                cpuTemp > 75 ? 90 :
                cpuTemp > 70 ? 80 :
                cpuTemp > 65 ? 75 :
                cpuTemp > 60 ? 70 :
                cpuTemp > 55 ? 65 :
                cpuTemp > 50 ? 60 :
                cpuTemp > 45 ? 55 :
                cpuTemp > 40 ? 50 : 
                40; // cpuTemp <= 40
        }

        private void SetFanSpeeds(byte value)
        {
            var fanCount = AsusWinIO64.HealthyTable_FanCounts();
            for (byte fanIndex = 0; fanIndex < fanCount; fanIndex++)
            {
                SetFanSpeed(value, fanIndex);
            }
        }

        public void SetFanSpeeds(int percent)
        {
            var value = (byte)(percent / 100.0f * 255);
            SetFanSpeeds(value);
        }

        public int GetFanSpeed(byte fanIndex = 0)
        {
            AsusWinIO64.HealthyTable_SetFanIndex(fanIndex);
            var fanSpeed = AsusWinIO64.HealthyTable_FanRPM();
            return fanSpeed;
        }

        public List<int> GetFanSpeeds()
        {
            var fanSpeeds = new List<int>();

            var fanCount = AsusWinIO64.HealthyTable_FanCounts();
            for (byte fanIndex = 0; fanIndex < fanCount; fanIndex++)
            {
                var fanSpeed = GetFanSpeed(fanIndex);
                fanSpeeds.Add(fanSpeed);
            }

            return fanSpeeds;
        }

        public int HealthyTable_FanCounts()
        {
            return AsusWinIO64.HealthyTable_FanCounts();
        }

        public ulong Thermal_Read_Cpu_Temperature()
        {
            return AsusWinIO64.Thermal_Read_Cpu_Temperature();
        }
    }
}