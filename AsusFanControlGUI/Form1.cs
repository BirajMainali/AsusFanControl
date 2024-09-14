using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using AsusFanControl;

namespace AsusFanControlGUI
{
    public partial class Form1 : Form
    {
        readonly AsusControl _asusControl = new AsusControl();
        int _fanSpeed = 0;

        public Form1()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            toolStripMenuItemTurnOffControlOnExit.Checked = Properties.Settings.Default.turnOffControlOnExit;
            toolStripMenuItemForbidUnsafeSettings.Checked = Properties.Settings.Default.forbidUnsafeSettings;
            trackBarFanSpeed.Value = Properties.Settings.Default.fanSpeed;
            checkBoxTurnOn.Checked = Properties.Settings.Default.fanSpeed > 0;
            Task.Run(MonitorCpuTemperature);
        }
        
        private async Task MonitorCpuTemperature()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(30));
                var cpuTemp = _asusControl.Thermal_Read_Cpu_Temperature();
                Invoke(new Action(() => labelCPUTemp.Text = cpuTemp.ToString()));
                var fanSpeed = cpuTemp switch
                {
                    > 80 => 100,
                    > 70 => 80,
                    > 60 => 60,
                    > 50 => 50,
                    _ => 40
                };
                _asusControl.SetFanSpeed(fanSpeed);
            }
        }


        private void OnProcessExit(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.turnOffControlOnExit)
            {
                _asusControl.SetFanSpeeds(0);
            }
        }

        private void ToolStripMenuItemTurnOffControlOnExitCheckedChangedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.turnOffControlOnExit = toolStripMenuItemTurnOffControlOnExit.Checked;
            Properties.Settings.Default.Save();
        }

        private void ToolStripMenuItemForbidUnsafeSettingsCheckedChangedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.forbidUnsafeSettings = toolStripMenuItemForbidUnsafeSettings.Checked;
            Properties.Settings.Default.Save();
        }

        private void ToolStripMenuItemCheckForUpdatesClickClick(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Karmel0x/AsusFanControl/releases");
        }

        private void SetFanSpeed()
        {
            var value = trackBarFanSpeed.Value;
            Properties.Settings.Default.fanSpeed = value;
            Properties.Settings.Default.Save();

            if (!checkBoxTurnOn.Checked)
            {
                value = 0;
            }

            labelValue.Text = value == 0 ? "turned off" : value.ToString();

            if (_fanSpeed != value)
            {
                _fanSpeed = value;

                _asusControl.SetFanSpeeds(value);
            }
        }

        private void CheckBoxTurnOnCheckedChangedChanged(object sender, EventArgs e)
        {
            SetFanSpeed();
        }

        private void TrackBarFanSpeedMouseCaptureChangedChanged(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.forbidUnsafeSettings)
            {
                trackBarFanSpeed.Value = trackBarFanSpeed.Value switch
                {
                    < 40 => 40,
                    > 99 => 99,
                    _ => trackBarFanSpeed.Value
                };
            }

            SetFanSpeed();
        }

        private void TrackBarFanSpeedKeyUpUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Left && e.KeyCode != Keys.Right)
            {
                return;
            }
            TrackBarFanSpeedMouseCaptureChangedChanged(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            labelRPM.Text = string.Join(" ", _asusControl.GetFanSpeeds());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            labelCPUTemp.Text = $@"{_asusControl.Thermal_Read_Cpu_Temperature()}";
        }
    }
}