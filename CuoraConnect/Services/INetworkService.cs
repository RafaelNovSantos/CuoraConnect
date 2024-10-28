namespace CuoraConnect.Services
{
    public interface INetworkService
    {
        Task<string> GetInfoInterface(string type);
        string GetLocalIPAddress();
        string GetAvailableIPAddress();
        Task<string> GetDefaultGateway();
        List<string> GetAvailableWifiNetworks();
        Task<string> GetSubnetMask();
        Task<string> IsConnectedTo5G();
        bool IsMobileDataEnabled();
        Task<bool> ConnectToWifiAsync(string ssid, string password);
        void DisconnectFromWifi();


        Task<PermissionStatus> CheckAndRequestLocationPermission();

    }
}
