using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using AsusFanControl;
using Microsoft.Win32;

namespace AsusFanControlGUI
{
    public partial class Form1 : Form
    {
        private readonly AsusControl _asusControl = new AsusControl();
        private int _fanSpeed;

        public Form1()
        {
            InitializeComponent();
            LoadSettings();
            Application.ApplicationExit += OnApplicationExit;

            Task.Run(() =>
            {
                _asusControl.ControlFanSpeed((speed, temperature) =>
                {
                    Invoke(new Action(() =>
                    {
                        trackBarFanSpeed.Value = speed;
                        labelCPUTemp.Text = temperature.ToString();
                        labelRPM.Text = string.Join(" ", _asusControl.GetFanSpeeds());
                        labelValue.Text = speed.ToString();
                    }));
                });
            });
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.turnOffControlOnExit)
            {
                _asusControl.SetFanSpeeds(0);
            }
        }

        private void LoadSettings()
        {
            toolStripMenuItemTurnOffControlOnExit.Checked = Properties.Settings.Default.turnOffControlOnExit;
            toolStripMenuItemForbidUnsafeSettings.Checked = Properties.Settings.Default.forbidUnsafeSettings;
            trackBarFanSpeed.Value = Properties.Settings.Default.fanSpeed;
            checkBoxTurnOn.Checked = Properties.Settings.Default.fanSpeed > 0;
        }

        private void ToolStripMenuItemTurnOffControlOnExitCheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.turnOffControlOnExit = toolStripMenuItemTurnOffControlOnExit.Checked;
            Properties.Settings.Default.Save();
        }

        private void ToolStripMenuItemForbidUnsafeSettingsCheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.forbidUnsafeSettings = toolStripMenuItemForbidUnsafeSettings.Checked;
            Properties.Settings.Default.Save();
        }

        private void RegisterOnStartup(object sender, EventArgs e)
        {
            UnregisterOnStartup();
            var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (key != null)
            {
                string exePath = Application.ExecutablePath;
                string value = $"\"{exePath}\" --silent";
                key.SetValue("AsusFanControl", value);
            }

            MessageBox.Show("Registered on startup");
        }

        private void UnregisterOnStartup()
        {
            var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (key != null && key.GetValue("AsusFanControl") != null)
            {
                key.DeleteValue("AsusFanControl");
            }
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

        private void CheckBoxTurnOnCheckedChanged(object sender, EventArgs e)
        {
            SetFanSpeed();
        }

        private void TrackBarFanSpeedValueChanged(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.forbidUnsafeSettings)
            {
                trackBarFanSpeed.Value = Clamp(trackBarFanSpeed.Value, 40, 90);
            }

            SetFanSpeed();
        }


        private int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            return value > max ? max : value;
        }

        private void TrackBarFanSpeedKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                TrackBarFanSpeedValueChanged(sender, e);
            }
        }

        private void ButtonShowFanSpeeds_Click(object sender, EventArgs e)
        {
            labelRPM.Text = string.Join(" ", _asusControl.GetFanSpeeds());
        }

        private void ButtonShowCpuTemp_Click(object sender, EventArgs e)
        {
            labelCPUTemp.Text = _asusControl.Thermal_Read_Cpu_Temperature().ToString();
        }
    }
}