using System;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AsusFanControl;

namespace AsusFanControlGUI.Worker
{
    public class BackgroundWorkerService : ServiceBase
    {
        private Thread _listenerThread;
        private static BackgroundWorkerService _instance;
        private readonly AsusControl _asusControl = new AsusControl();

        public static BackgroundWorkerService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new BackgroundWorkerService();
            }

            return _instance;
        }

        private BackgroundWorkerService()
        {
        }

        protected override void OnStart(string[] args)
        {
            _listenerThread = new Thread(Initialize);
            _listenerThread.Start();
        }

        public void StartWorker()
        {
            System.IO.File.AppendAllText("args.txt", "BackgroundWorkerService started");
            _listenerThread = new Thread(Initialize);
            _listenerThread.Start();
        }

        private void Initialize()
        {
            _asusControl.SetFanSpeeds(90);
            Task.Run(async () =>
            {
                await _asusControl.ControlFanSpeed(afterControl: (speed, cpu) =>
                {
                    System.IO.File.AppendAllText("args.txt",$"Fan speed: {speed}%, CPU temp: {cpu}°C");
                });
            });
        }

        protected override void OnStop()
        {
            Application.Exit();
        }
    }
}