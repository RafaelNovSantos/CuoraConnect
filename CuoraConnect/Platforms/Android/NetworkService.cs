using System.Net;
using System.Net.NetworkInformation;
using CuoraConnect.Services;
using Microsoft.Maui.Controls;
using Android.Net.Wifi;
using System.Threading.Tasks;
using Android.Content;
using Microsoft.Maui.ApplicationModel;
using Android.App;
using Android.Net;
using Application = Android.App.Application;
using System.Diagnostics;

[assembly: Dependency(typeof(CuoraConnect.Platforms.Android.NetworkService))]
namespace CuoraConnect.Platforms.Android
{
    public class NetworkService : INetworkService
    {

        private async Task<bool> CheckAndRequestPermissions()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            return status == PermissionStatus.Granted;
        }



        public string GetDefaultGateway()
        {
            var wifiManager = (WifiManager)Application.Context.GetSystemService(Context.WifiService);
            var dhcpInfo = wifiManager.DhcpInfo;

            if (dhcpInfo != null)
            {
                // Converte o número inteiro para um endereço IP
                var gateway = string.Format("{0}.{1}.{2}.{3}",
                    (dhcpInfo.Gateway & 0xff),
                    (dhcpInfo.Gateway >> 8 & 0xff),
                    (dhcpInfo.Gateway >> 16 & 0xff),
                    (dhcpInfo.Gateway >> 24 & 0xff));

                return gateway;
            }
            return "Gateway padrão não encontrado.";
        }




        public async Task<string> GetCurrentSSID()
        {
            // Verifica permissões de rede
            if (!await CheckAndRequestPermissions())
            {
                return "Permissão não concedida para acessar informações de rede.";
            }

            // Obtém o contexto da aplicação
            var context = Application.Context;
            if (context == null)
            {
                return "Não foi possível obter o contexto da aplicação.";
            }

            // Obtém o WifiManager
            var wifiManager = (WifiManager)context.GetSystemService(Context.WifiService);
            if (wifiManager == null)
            {
                return "Não foi possível obter o WifiManager.";
            }

            // Obtém o ConnectivityManager para verificar a conexão ativa
            var connectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            var activeNetwork = connectivityManager.ActiveNetworkInfo;

            // Verifica se há uma conexão Wi-Fi ativa
            if (activeNetwork == null || !activeNetwork.IsConnected || activeNetwork.Type != ConnectivityType.Wifi)
            {
                return "Nenhuma conexão Wi-Fi ativa.";
            }

            // Obtém informações sobre a conexão
            var info = wifiManager.ConnectionInfo;
            Debug.WriteLine($"SSID INFO:{info}");
            if (info == null || info.NetworkId == -1)
            {
                return "Wi-Fi Desconectado ou Informação Indisponível.";
            }

            // Retorna o SSID, removendo aspas
            var ssid = info.SSID?.Replace("\"", "");
            return !string.IsNullOrEmpty(ssid) ? ssid : "SSID Indisponível.";
        }





        public void ConnectToWifi(string ssid, string password)
        {
#if ANDROID
            var wifiManager = (WifiManager)Application.Context.GetSystemService(Context.WifiService);

            var wifiConfig = new WifiConfiguration
            {
                Ssid = $"\"{ssid}\"", // Adicione aspas ao SSID
                PreSharedKey = $"\"{password}\"" // Adicione aspas à senha
            };

            int networkId = wifiManager.AddNetwork(wifiConfig);
            if (networkId == -1)
            {
                Console.WriteLine($"Não foi possível adicionar a rede: {ssid}");
                return;
            }

            wifiManager.Disconnect();
            wifiManager.EnableNetwork(networkId, true);
            wifiManager.Reconnect();
            Debug.WriteLine($"Conectado à rede: {ssid}");
#endif
        }




        public string GetLocalIPAddress()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation ipAddress in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    if (ipAddress.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ipAddress.Address.ToString();
                    }
                }
            }
            return "IP Não Encontrado";
        }

        public List<string> GetAvailableWifiNetworks()
        {
            var context = Application.Context;
            var wifiManager = (WifiManager)context.GetSystemService(Context.WifiService);

            // Inicia o escaneamento de redes Wi-Fi
            wifiManager.StartScan();

            // Obtém os resultados do escaneamento
            var results = wifiManager.ScanResults;

            // Cria uma lista para armazenar os SSIDs
            List<string> availableNetworks = new List<string>();

            foreach (var scanResult in results)
            {
                availableNetworks.Add(scanResult.Ssid);
            }

            return availableNetworks;
        }

        public string GetAvailableIPAddress()
        {
            string localIPAddress = GetLocalIPAddress();
            if (localIPAddress == "IP Não Encontrado")
            {
                return "IP Não Encontrado";
            }

            string[] ipParts = localIPAddress.Split('.');
            string subnetPrefix = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.";

            for (int i = 1; i <= 254; i++)
            {
                string ip = subnetPrefix + i;

                if (ip != localIPAddress)
                {
                    try
                    {
                        int count = 0;
                        int failedPingCount = 0;

                        while (count < 5)
                        {
                            count++;
                            try
                            {
                                Ping ping = new Ping();
                                PingReply reply = ping.Send(ip, 100); // Timeout de 100ms

                                // Se o ping for bem-sucedido, o IP está em uso
                                if (reply.Status == IPStatus.Success)
                                {
                                    // Saímos do loop e vamos testar o próximo IP
                                    break;
                                }
                                else
                                {
                                    failedPingCount++;
                                }

                                // Se falhou nos 5 pings, o IP pode ser considerado disponível
                                if (failedPingCount == 5)
                                {
                                    return ip; // IP está disponível
                                }
                            }
                            catch
                            {
                                failedPingCount++;
                                if (failedPingCount == 5)
                                {
                                    return ip; // IP está disponível mesmo com exceção
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ignorar exceções externas ao ping
                    }
                }
            }

            return "Nenhum IP disponível encontrado.";
        }
    }
    }