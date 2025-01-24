using CuoraConnect.Services;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Security.Credentials;

[assembly: Dependency(typeof(CuoraConnect.Platforms.Windows.NetworkService))]
namespace CuoraConnect.Platforms.Windows
{
    public class NetworkService : INetworkService
    {
        public string nameinterface = "";
        public string bssid = "";
        public async Task<string> GetInfoInterface(string type)
        {
            return await Task.Run(() =>
            {
                string ssid = string.Empty;
                int count = 1;
                while (count <= 4)
                {
                    Debug.WriteLine($"Tentativa {count} de encontrar {type}");
                    count++;
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
                        if (type == "SSID")
                        {


                            if (line.Trim().StartsWith("SSID"))
                            {
                                ssid = line.Split(':')[1].Trim();
                                if (string.IsNullOrEmpty(ssid))
                                {
                                    continue;
                                }
                                else
                                {
                                    return ssid;
                                }
                            }
                        }
                        if (type == "BSSID")
                        {
                            if (line.Trim().StartsWith("BSSID"))
                            {
                                // Encontra o índice do primeiro ':'
                                int index = line.IndexOf(':');

                                if (index != -1) // Verifica se o ':' foi encontrado
                                {
                                    // Captura tudo que vem após o primeiro ':', removendo espaços em branco
                                    bssid = line.Substring(index + 1).Trim();

                                    // Remove os dois pontos do BSSID
                                    bssid = bssid.Replace(":", string.Empty);
                                    bssid = bssid.ToUpper();

                                    if (!string.IsNullOrEmpty(bssid))
                                    {
                                        return bssid; // Retorna o BSSID completo
                                    }
                                }
                            }

                        }
                        if (type == "NameInterface")
                        {

                            if (line.Trim().StartsWith("Nome") || line.Trim().StartsWith("Name") || line.Trim().StartsWith("Nombre"))
                            {
                                nameinterface = line.Split(':')[1].Trim();
                                if (!string.IsNullOrEmpty(nameinterface))
                                {
                                    return nameinterface;
                                }
                            
                            }
                        }
                    }
                }
                return "SSID Indisponível.";
            });
        }

        public async Task<PermissionStatus> CheckAndRequestLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            return status;
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

        public async Task<string> GetAvailableIPAddress()
        {
            await Task.Delay(1000);
            string localIPAddress = GetLocalIPAddress();
            if (localIPAddress == "IP Não Encontrado")
            {
                return "IP Não Encontrado";
            }

            string gateway = GetDefaultGateway().Result;
            if (gateway == "Gateway padrão não encontrado.")
            {
                return "Gateway padrão não encontrado.";
            }

            string[] gatewayParts = gateway.Split('.');
            if (gatewayParts.Length < 4)
            {
                return "Não foi possível encontrar IP disponível na rede.";
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
                        PingReply reply = ping.Send(ip, 200); // Timeout de 100ms

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

        // Método para obter o gateway do Wi-Fi com base no SSID
        public async Task<string> GetDefaultGateway()
        {
            string targetSSID = await GetInfoInterface("NameInterface");


            return await Task.Run(() =>
            {
                int count = 0;
                while (count <= 4)
                {
                    count++;
                    Debug.WriteLine($"Tentativa {count} de verificar Gateway do roteador");
                
                foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // Verifica se a interface é Wi-Fi e está ativa
                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                        networkInterface.OperationalStatus == OperationalStatus.Up)
                    {
                        var properties = networkInterface.GetIPProperties();

                        // Compara o SSID da interface Wi-Fi com o SSID desejado
                        if (networkInterface.Name == targetSSID)
                        {
                            foreach (var gateway in properties.GatewayAddresses)
                            {
                                if (gateway.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    return gateway.Address.ToString(); // Retorna o gateway IPv4
                                }
                            }
                        }
                    }
                }
                }
                return "Gateway não encontrado para o SSID especificado.";
            });
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

public async Task<string> GetSubnetMask()
{
    return await Task.Run(() =>
    {
        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) // Verifica se é Wi-Fi
            {
                foreach (var ipInfo in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        string subnetMask = ipInfo.IPv4Mask.ToString();
                        Debug.WriteLine($"Máscara de Sub-rede Wi-Fi encontrada: {subnetMask}");
                        return subnetMask; // Retorna a máscara de sub-rede
                    }
                }
            }
        }
        return "Máscara de rede Wi-Fi não encontrada."; // Retorna mensagem se não encontrar
    });
}




        public bool IsMobileDataEnabled()
        {

            return false; // Não há dados móveis ativos
        }


        public async Task<string> IsConnectedTo5G()
        {
            await Task.Delay(1000);
            // Obtém o SSID da rede atual
            var connectedSsid = GetConnectedSSID();
            var wifiAdapters = await WiFiAdapter.FindAllAdaptersAsync();

            if (wifiAdapters == null || wifiAdapters.Count == 0)
            {
                Debug.WriteLine("Nenhum adaptador Wi-Fi encontrado.");
;
                await Task.Delay(1000); // Espera antes de tentar novamente

            }



     


            if (connectedSsid == null)
            {
                return "Nenhuma rede conectada"; // Caso não esteja conectado a nenhuma rede
            }

            int maxAttempts = 3; // Número máximo de tentativas
            int attemptCount = 0;
            int adpterErrorCount = 0;

            while (attemptCount <= maxAttempts || adpterErrorCount <= maxAttempts)
            {
                attemptCount++;
                try
                {
                    Debug.WriteLine($"Tentativa {attemptCount} de verificar conexão 2.4g ou 5g");
                    // Obtém os adaptadores Wi-Fi disponíveis no dispositivo
               

                    if (wifiAdapters == null || wifiAdapters.Count == 0)
                    {
                        Debug.WriteLine("Nenhum adaptador Wi-Fi encontrado. Tentando novamente...");
                        adpterErrorCount++;
                        await Task.Delay(1000); // Espera antes de tentar novamente
                        continue;
                    }



                    foreach (var adapter in wifiAdapters)
                    {
                        int countAdapterReconnect = 0;
                        bool sucessAdapterReconnect = false;
                        while (countAdapterReconnect <= 4 && sucessAdapterReconnect == false) {
                            countAdapterReconnect++;
                            Debug.WriteLine($"Tentativa {countAdapterReconnect} de reconectar no Wi-Fi.");

                            // Encontra a rede desejada (SSID) entre as redes disponíveis no adaptador atual
                            var targetNetwork = adapter.NetworkReport.AvailableNetworks
                            .FirstOrDefault(network => network.Ssid.Equals(connectedSsid, StringComparison.OrdinalIgnoreCase));

                        // Se a rede estiver disponível no adaptador atual
                        if (targetNetwork != null)
                        {
                            // Desconecta o adaptador
                            adapter.Disconnect();
                            Debug.WriteLine($"Desconectado do adaptador {adapter.NetworkAdapter.NetworkAdapterId}.");

                           

                            

                            // Reconecta à mesma rede usando o adaptador atual
                            var connectionResult = await adapter.ConnectAsync(targetNetwork, WiFiReconnectionKind.Automatic);

                            if (connectionResult.ConnectionStatus == WiFiConnectionStatus.Success)
                            {
                                Debug.WriteLine($"Reconectado à rede {connectedSsid} com sucesso no adaptador {adapter.NetworkAdapter.NetworkAdapterId}.");
                                    sucessAdapterReconnect = true;
                            }
                            else
                            {
                                Debug.WriteLine($"Falha ao reconectar à rede {connectedSsid} no adaptador {adapter.NetworkAdapter.NetworkAdapterId}. Status: {connectionResult.ConnectionStatus}");
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"Rede {connectedSsid} não encontrada no adaptador {adapter.NetworkAdapter.NetworkAdapterId}");
                        }

                        }

                        foreach (var network in adapter.NetworkReport.AvailableNetworks)
                        {
                            Debug.WriteLine($"Rede: {network.Ssid} Frequência: {network.ChannelCenterFrequencyInKilohertz}");

                            // Verifica se o SSID da rede escaneada é igual ao SSID conectado
                            if (network.Ssid == connectedSsid)
                            {
                                adpterErrorCount++;
                                // Verifica se a rede está na frequência de 2.4 GHz (bandas 2400-2500 MHz)
                                if (network.ChannelCenterFrequencyInKilohertz >= 2400000 && network.ChannelCenterFrequencyInKilohertz <= 2500000)
                                {
                                    return "Conectado a 2.4 GHz"; // Rede está em 2.4 GHz
                                }
                            }
                        }
                    }
                    

                }

                catch (Exception ex)
                {
                    Debug.WriteLine($"Erro ao buscar adaptadores em IsConnectedTo5G: {ex.Message}");
                    adpterErrorCount++;
                    await Task.Delay(1000); // Espera antes de tentar novamente

                    if (ex.Message == null)
                    {
                        Debug.WriteLine("Nenhum adaptador Wi-Fi encontrado. Tentando novamente...");
                        adpterErrorCount++;
                        await Task.Delay(1000); // Espera antes de tentar novamente
                        continue;
                    }

                    if (adpterErrorCount <= maxAttempts)
                    {
                        return "Nenhum adaptador Wi-Fi encontrado. Tentando novamente.";
                    }
                }
                
            }

            // Se não encontrou uma rede 2.4 GHz após escanear
            return "Conectado a 5 GHz";
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
            //if (_wifiAdapter != null)
            //{
            //    _wifiAdapter.Disconnect();
            //}
        }
    }


}