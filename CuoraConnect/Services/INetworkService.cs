using System.Collections.Generic; 
using System.Threading.Tasks; 

namespace CuoraConnect.Services
{
    public interface INetworkService
    {
        Task<string> GetCurrentSSID();
        string GetLocalIPAddress();
        string GetAvailableIPAddress();
        string GetDefaultGateway();
        List<string> GetAvailableWifiNetworks();
        string GetSubnetMask();
        bool IsConnectedTo5G();

        Task<List<(string BSSID, int Channel, string Frequency)>> GetConnectedNetworkBSSIDs();

        bool IsMobileDataEnabled();
        Task<bool> ConnectToWifiAsync(string ssid, string password);
        void DisconnectFromWifi();

    }
}
