using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using AsusFanControl;

namespace AsusFanControlGUI;

public class BackgroundWorkerService : ServiceBase
{
    private static BackgroundWorkerService _instance;
    private readonly AsusControl _asusControl = new();
    private Thread _listenerThread;

    public static BackgroundWorkerService GetInstance()
    {
        return _instance ??= new BackgroundWorkerService();
    }

    private BackgroundWorkerService()
    {
        ServiceName = "AsusFanControlService";
    }

    protected override void OnStart(string[] args)
    {
        _listenerThread = new Thread(StartListener);
        _listenerThread.Start();
    }

    protected override void OnStop()
    {
        _asusControl.SetFanSpeed(0);
    }

    public void StartListener()
    {
        _asusControl.ControlFanSpeed((speed, temperature) =>
        {
            Console.WriteLine("Fan speed: " + speed + "%, CPU temperature: " + temperature + "°C");
        });
    }
}