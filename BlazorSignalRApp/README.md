# Blazor SignalR Demo

Sample application to demonstrate integrating web sockets into a balzor server application using [SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-7.0).

## Code Organization
- **Hubs** - Hubs define the methods that will be exposed as RPCs to the clinet to invoke or subscribe to, these hubs are stateless in nature and get distroyed and created inline with the life cycle of the UI component
- **Data** - Data classes are used to define the format of data that will be transmitted over the SignalR hubs.
- **Pages** - Pages represent the razor pages that will consume data from SignalR. 

## Steps For WebSockets
- **Step 1** Install Nuget Package [Microsoft.AspNetCore.SignalR.Client ](https://www.nuget.org/packages/Microsoft.AspNetCore.SignalR.Client), Make sure to install the Client library and not the named Core.
- **Step 2** Configure the server to be able to octect streams to send data over websockets and initialize the SignalR Hubs in the "Program.cs" file. Make sure to import the requrired namespaces form the isntalled nuget package
    ```
    using Microsoft.AspNetCore.ResponseCompression;
    using BlazorSignalRApp.Hubs;
    
    builder.Services.AddResponseCompression(opts =>
    {
        opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
    });//Configure server to send respones as octet stream over websockets
    ```
    
    ```
    app.MapHub<ChatHub>("/chathub");
    app.MapHub<PlotHub>("/plothub", (options) =>
    {
        //options.TransportMaxBufferSize = 131072;
    });
    ```    
- **Step 3** Define Hubs for data transfer, Hubs cab be a good way to organize diffrent data streams. Methods exposed by the hub can be invoked from razor pages to transfer data back to the server.
    ```
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
    ```
- **Step 4** Use the "SendAsync" method to send data to all the registerd clients from the server. The method named defined here is used in the razor pages to hook up call back methods when data is made available on these streams.
    ```
    return _hubContext.Clients.All.SendAsync("ReceiveSamples", Samples.ToArray());
    ```
- **Step 5** Overide the "OnInitializedAsync" life cycle method to setup connection the signalR hub and configure the call back methods for the streams that are of intrest to the component.
    ```
    protected override async Task OnInitializedAsync()
    {
        //Create a signalR client for the required hub
        hubConnection = new HubConnectionBuilder()
        .WithUrl(NavManager.ToAbsoluteUri("/plothub"))
        .WithAutomaticReconnect()
        .Build();

        //Subscribe and register call back for the methods that you want to listen to
        hubConnection.On<PlotDataItem[]>("ReceiveSamples", (samples) => InitializeDataReception(samples)

       );

        await hubConnection.StartAsync();
    }
    ```
- **Step 6** Invoke any Hub method to pass back any data to the server.
    ```
     private async Task Send()
        {
            if (hubConnection is not null)
            {//Invoke signalR hub method to configure plot sample rate
                await hubConnection.SendAsync("SetSamplesRate", SamplePerTI, TimeIntervalInMS);
            }
        }
    ```
    
    
    
    
    
    
    