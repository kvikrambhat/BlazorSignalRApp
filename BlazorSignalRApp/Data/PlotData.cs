using BlazorSignalRApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace BlazorSignalRApp.Data
{
    public class PlotDataItem
    {
        //Class representing each point on the data plot
        public string Quarter { get; set; } //X-Axis
        public double Revenue { get; set; } //Y-Axis
    }

    public interface IPlotData
    {
        //Interface that will get injected into the SignalR hub to send plot data
        void ResetPlotDataSource();
        void SetSamplesRate(int samplesPerTimeInterval, int timeIntervalInMs);
    }

    public class PlotData : IPlotData
    {
        //Implementation of the above interface that will send data periodically as configured to the SignalR clients

        private int SampleSentCount = 1; //Count of total samples sent to the UI

        private readonly IHubContext<PlotHub> _hubContext;//Injecting hub context to send data to the required SignalR Hub

        public static List<PlotDataItem> Samples = new();//List of current samples to be sent

        Random RandomGenerator = new Random();//Random number generator for samples data

        public int SamplesPerTimeInterval = 10;//Number of samples to be sent for the configured time interval

        public int TimeIntervalInMs = 10;//Time interval in ms at which data needs to be sent

        private static System.Timers.Timer Timer;//Timer to send data periodically 
        public PlotData(IHubContext<PlotHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void SetSamplesRate(int samplesPerTimeInterval, int timeIntervalInMs)
        {
            TimeIntervalInMs = timeIntervalInMs;
            SamplesPerTimeInterval = samplesPerTimeInterval;
            SetTimer(TimeIntervalInMs);//Configures a timer to send data
        }

        public void ResetPlotDataSource()
        {
            //Resets all plot data
            if (Timer is not null)
            {
                Timer.Stop();
                Timer.Dispose();
                Samples = new();
                SampleSentCount = 1;
            }
        }
        private void SetTimer(int timeIntervalInMs)
        {
            //Reset plot data on reconfigure
            ResetPlotDataSource();
            //Configure timer 
            Timer = new System.Timers.Timer(timeIntervalInMs);
            Timer.Elapsed += OnTimedEvent;
            Timer.AutoReset = true;
            Timer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            AddNewSamples();
        }
        private Task AddNewSamples()
        {
            //Clear and generate new samples for this timer tick
            Samples.Clear();
            var prevCount = Samples.Count;
            for (int i = 0; i < SamplesPerTimeInterval; i++)
            {
                Samples.Add(new PlotDataItem { Quarter = "Q" + (SampleSentCount), Revenue = RandomGenerator.Next() % 10 });
                SampleSentCount++;

            }
            var samplesDiff = Samples.Count - prevCount;
            Debug.WriteLine($"SER LOG : {DateTime.Now} :{Samples.Count}, Diff : {samplesDiff}, Count : {SampleSentCount}");
            //Send timer data to all SignalR Hub clients
            return _hubContext.Clients.All.SendAsync("ReceiveSamples", Samples.ToArray());
        }
    }
}
