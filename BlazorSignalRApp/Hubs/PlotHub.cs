using BlazorSignalRApp.Data;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Timers;


namespace BlazorSignalRApp.Hubs
{
    public class PlotHub : Hub
    {
        //Signal R Hub to configure and receive sample data for UI plot
        private IPlotData _plotData;
        public PlotHub(IHubContext<PlotHub> hubContext, IPlotData plotData)
        {
            _plotData = plotData;
        }
        public void SetSamplesRate(int samplesPerTimeInterval, int timeIntervalInMs)
        {
            //Configures the PlotData class and starts sending data 
            _plotData.SetSamplesRate(samplesPerTimeInterval, timeIntervalInMs);
        }

        public void ResetPlotDataSource()
        {
            //Resets the PlotData class
            _plotData.ResetPlotDataSource();
        }
    }

 


}
