using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Pumpkin.Wizzard
{
	public sealed partial class MainPage : Page
    {
		// Regex pattern for matching SSID
		private string ssidRegexPattern = "^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$";

		private string mainSSID = "WLAN_Gitok";
		private string mainPass = "ARbs3928";

		// UDP settings
		private DatagramSocket udpSocket;
		private string udpServerIp = "255.255.255.255"; // ESP8266 IP address
		private int udpServerPort = 8888; // ESP8266 UDP port

		public MainPage()
        {
            InitializeComponent();
        }

		private async void SendData()
		{
			WiFiAdapter wifiAdapter = await WiFiAdapter.FromIdAsync(WiFiAdapter.GetDeviceSelector());

			// Scan for available networks
			await wifiAdapter.ScanAsync();

			// Get matching networks based on the SSID regex pattern
			IEnumerable<WiFiAvailableNetwork> matchingNetworks = wifiAdapter.NetworkReport.AvailableNetworks.Where(network => Regex.IsMatch(network.Ssid, ssidRegexPattern));
			for (int i = 0; i < matchingNetworks.Count(); i++) await ConnectAndSend(matchingNetworks.ElementAt(i).Ssid);
		}

		private async Task ConnectAndSend(string ssid)
		{
			
		}

		private async void MainPage_Loaded(object sender, RoutedEventArgs e)
		{
			// Connect to matching networks
			await ConnectToNetworks();

			// Send UDP packet with SSID and password
			await SendUdpPacket("MyCustomSSID", "MyCustomPassword");
		}

		private async Task ConnectToNetworks()
		{
			// Create WiFi adapter

			foreach (WiFiAvailableNetwork network in matchingNetworks)
			{
				// Connect to the network with the same password as the SSID
				WiFiConnectionResult result = await wifiAdapter.ConnectAsync(network, WiFiReconnectionKind.Automatic, network.Ssid);

				if (result.ConnectionStatus == WiFiConnectionStatus.Success)
				{
					// Connection successful
					string ssid = network.Ssid;
					string password = network.Ssid;

					// Send UDP packet with SSID and password
					await SendUdpPacket(ssid, password);
				}
				else
				{
					// Connection failed
					string error = result.ConnectionStatus.ToString();
					// Handle the connection error
				}
			}
		}

		private async Task SendUdpPacket(string ssid, string password)
		{
			if (udpSocket == null)
			{
				udpSocket = new DatagramSocket();
			}

			// Create the UDP packet payload
			string packetPayload = ssid + ":" + password;

			// Convert the payload to bytes
			DataWriter writer = new DataWriter();
			writer.WriteString(packetPayload);
			byte[] packetBytes = writer.DetachBuffer().ToArray();

			// Create the destination endpoint
			HostName serverHost = new HostName(udpServerIp);

			// Send the UDP packet
			await udpSocket.ConnectAsync(serverHost, udpServerPort.ToString());
			await udpSocket.OutputStream.WriteAsync(packetBytes.AsBuffer());
		}
	}
}
