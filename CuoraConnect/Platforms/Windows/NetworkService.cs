using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using CuoraConnect.Services;
using Microsoft.Maui.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;

[assembly: Dependency(typeof(CuoraConnect.Platforms.Windows.NetworkService))]
namespace CuoraConnect.Platforms.Windows
{
    public class NetworkService : INetworkService
    {
        public async Task<string> GetCurrentSSID()
        {
            return await Task.Run(() =>
            {
                string ssid = string.Empty;
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = "wlan show interfaces",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("SSID"))
                    {
                        ssid = line.Split(':')[1].Trim();
                        break;
                    }
                }

                return string.IsNullOrEmpty(ssid) ? "SSID Indisponível." : ssid;
            });
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

        public string GetAvailableIPAddress()
        {
            string localIPAddress = GetLocalIPAddress();
            if (localIPAddress == "IP Não Encontrado")
            {
                return "IP Não Encontrado";
            }

            string gateway = GetDefaultGateway();
            if (gateway == "Gateway padrão não encontrado.")
            {
                return "Gateway padrão não encontrado.";
            }

            string[] gatewayParts = gateway.Split('.');
            if (gatewayParts.Length < 4)
            {
                return "Gateway inválido.";
            }

            string subnetPrefix = $"{gatewayParts[0]}.{gatewayParts[1]}.{gatewayParts[2]}.";

            for (int i = 70; i <= 200; i++)
            {
                string ip = subnetPrefix + i;

                if (ip != localIPAddress)
                {
                    try
                    {
                        Ping ping = new Ping();
                        PingReply reply = ping.Send(ip, 100); // Timeout de 100ms

                        if (reply.Status != IPStatus.Success)
                        {
                            return ip; // IP disponível
                        }
                    }
                    catch
                    {
                        // Ignorar exceções
                    }
                }
            }

            return "Nenhum IP disponível encontrado.";
        }


        public string GetDefaultGateway()
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                var properties = networkInterface.GetIPProperties();
                foreach (var gateway in properties.GatewayAddresses)
                {
                    if (gateway.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return gateway.Address.ToString();
                    }
                }
            }
            return "Gateway padrão não encontrado.";
        }

        public List<string> GetAvailableWifiNetworks()
        {
            var availableNetworks = new List<string>();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "wlan show networks",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("SSID"))
                {
                    var ssid = line.Split(':')[1].Trim();
                    availableNetworks.Add(ssid);
                }
            }

            return availableNetworks.Count > 0 ? availableNetworks : new List<string> { "Nenhuma rede disponível." };
        }

        public string GetSubnetMask()
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                    networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    networkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                {
                    foreach (var ipInfo in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            string subnetMask = ipInfo.IPv4Mask.ToString();
                            Debug.WriteLine($"Máscara de Sub-rede encontrada: {subnetMask}");
                            return subnetMask; // Retorna a máscara de sub-rede
                        }
                    }
                }
            }
            return "Máscara de rede não encontrada.";
        }


        public bool IsMobileDataEnabled()
        {

            return false; // Não há dados móveis ativos
        }

       public bool IsConnectedTo5G()
{
    // Obtém o perfil de conexão atual
    var profile = NetworkInformation.GetInternetConnectionProfile();

    if (profile != null && profile.IsWlanConnectionProfile)
    {
        // Obtém o NetworkAdapter associado ao perfil de conexão
        var networkAdapter = profile.NetworkAdapter;

        // Verifica o tipo de interface do adaptador de rede
        if (networkAdapter.IanaInterfaceType == 71) // 71 é o valor IANA para 802.11ac (5 GHz)
        {
            return true; // Conectado a uma rede de 5 GHz
        }
    }

    return false; // Não conectado a uma rede de 5 GHz
}



        private string GetConnectedSSID()
        {
            Process process = new Process();
            process.StartInfo.FileName = "netsh";
            process.StartInfo.Arguments = "wlan show interfaces";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            foreach (var line in output.Split('\n'))
            {
                if (line.Contains("SSID"))
                {
                    return line.Split(':')[1].Trim();
                }
            }

            return null;
        }


        private Dictionary<string, List<(string BSSID, string Channel)>> GetAllNetworksBSSIDs()
        {
            // Dicionário para armazenar os dados de cada SSID
            Dictionary<string, List<(string BSSID, string Channel)>> ssidData = new Dictionary<string, List<(string BSSID, string Channel)>>();

            Process process = new Process();
            process.StartInfo.FileName = "netsh";
            process.StartInfo.Arguments = "wlan show networks mode=bssid";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Debug.WriteLine("Output do comando:");
            Debug.WriteLine(output); // Exibir output completo para diagnóstico

            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string currentSSID = string.Empty;
            string currentBSSID = string.Empty;

            foreach (var line in lines)
            {
                Debug.WriteLine($"Analisando linha: {line}"); // Adicionando diagnóstico

                // Captura o SSID atual
                if (line.StartsWith("SSID"))
                {
                    currentSSID = line.Split(new[] { ':' }, 2)[1].Trim();
                    Debug.WriteLine($"Current SSID: {currentSSID}");
                }
                // Captura o BSSID e canal
                else if (line.Trim().StartsWith("BSSID"))
                {
                    currentBSSID = line.Split(new[] { ':' }, 2)[1].Trim();
                    Debug.WriteLine($"Capturando BSSID: {currentBSSID}");

                    // Espera pelo canal na linha seguinte
                    string channel = string.Empty;

                    // Procurando a linha do canal após a captura do BSSID
                    int nextLineIndex = Array.IndexOf(lines, line) + 1;

                    while (nextLineIndex < lines.Length)
                    {
                        if (lines[nextLineIndex].Trim().StartsWith("Canal"))
                        {
                            channel = lines[nextLineIndex].Split(new[] { ':' }, 2)[1].Trim();
                            Debug.WriteLine($"Canal encontrado: {channel}");
                            break;
                        }
                        nextLineIndex++;
                    }

                    // Se o SSID atual foi capturado, armazena o BSSID e canal
                    if (!string.IsNullOrEmpty(currentSSID) && !string.IsNullOrEmpty(channel))
                    {
                        if (!ssidData.ContainsKey(currentSSID))
                        {
                            ssidData[currentSSID] = new List<(string, string)>();
                        }

                        ssidData[currentSSID].Add((currentBSSID, channel));
                    }
                }
            }

            return ssidData;
        }

        public string GetFrequencyFromChannel(int channel)
        {
            // Faixa de canais 2.4GHz
            if (channel >= 1 && channel <= 11)
            {
                return "2.4 GHz";
            }
            // Faixa de canais 5GHz
            else if ((channel >= 36 && channel <= 64) || (channel >= 100 && channel <= 144))
            {
                return "5 GHz";
            }
            // Canais não reconhecidos
            return "Frequência desconhecida";
        }

        // Atualize o método GetConnectedNetworkBSSIDs para incluir a determinação da frequência


        public async Task<List<(string BSSID, int Channel, string Frequency)>> GetConnectedNetworkBSSIDs()
        {
            // Lista de tuplas para armazenar BSSID, Canal e Frequência
            var bssidList = new List<(string BSSID, int Channel, string Frequency)>();

            // Primeiro, obtemos a rede à qual estamos conectados
            string connectedSSID = GetConnectedSSID();
            if (string.IsNullOrEmpty(connectedSSID))
            {
                Debug.WriteLine("Não está conectado a nenhuma rede.");
                return bssidList; // Retorna uma lista vazia se não estiver conectado
            }

            Debug.WriteLine($"Conectado à rede: {connectedSSID}");

            // Obtemos todos os BSSIDs com seus canais
            var allNetworksData = GetAllNetworksBSSIDs();

            if (allNetworksData.TryGetValue(connectedSSID, out var bssidWithChannel))
            {
                Debug.WriteLine("BSSIDs disponíveis para a rede:");
                foreach (var (bssid, channelStr) in bssidWithChannel)
                {
                    // Converta o canal para um int
                    if (int.TryParse(channelStr, out int channel))
                    {
                        string frequency = GetFrequencyFromChannel(channel);
                        Debug.WriteLine($"BSSID: {bssid}, Canal: {channel}, Frequência: {frequency}");

                        // Adiciona os dados à lista de tuplas
                        bssidList.Add((BSSID: bssid, Channel: channel, Frequency: frequency));
                    }
                    else
                    {
                        Debug.WriteLine($"Canal inválido para BSSID: {bssid}");
                    }
                }
            }
            else
            {
                Debug.WriteLine("Nenhum BSSID encontrado para a rede.");
            }

            
            // Retorna a lista completa de BSSIDs
            return bssidList;
        }










        private WiFiAdapter _wifiAdapter;

        public async Task<bool> ConnectToWifiAsync(string ssid, string password)
        {
            // Get the first WiFi Adapter
            var result = await DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
            if (result.Count == 0)
            {
                throw new Exception("No WiFi adapters found.");
            }

            _wifiAdapter = await WiFiAdapter.FromIdAsync(result[0].Id);

            // Scan for available networks
            await _wifiAdapter.ScanAsync();

            // Find the network with the given SSID
            var network = _wifiAdapter.NetworkReport.AvailableNetworks.FirstOrDefault(n => n.Ssid == ssid);
            if (network == null)
            {
                return false; // Network not found
            }

            // Create the WiFi connection profile
            var credential = new PasswordCredential
            {
                Password = password
            };

            var connectionResult = await _wifiAdapter.ConnectAsync(network, WiFiReconnectionKind.Automatic, credential);

            // Check the connection result
            return connectionResult.ConnectionStatus == WiFiConnectionStatus.Success;
        }

        public void DisconnectFromWifi()
        {
            if (_wifiAdapter != null)
            {
                _wifiAdapter.Disconnect();
            }
        }
    }


}