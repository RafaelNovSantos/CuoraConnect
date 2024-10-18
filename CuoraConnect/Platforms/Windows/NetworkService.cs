﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using CuoraConnect.Services;
using Microsoft.Maui.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Media.Devices;
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


        public async Task<bool> IsConnectedTo5G()
{
            // Obtém o perfil de conexão atual
            var connectedSsid = GetConnectedSSID();

            if (connectedSsid == null)
            {
                return false; // Nenhuma rede conectada
            }

            // Obtém os adaptadores Wi-Fi disponíveis no dispositivo
            var wifiAdapters = await WiFiAdapter.FindAllAdaptersAsync();
            int countVerific5g = 0;

            while (countVerific5g <= 3)
            {
                countVerific5g++;
                foreach (var adapter in wifiAdapters)
                {
                    Debug.WriteLine($"Tentativa {countVerific5g} de verificar conexão 2.4g ");
                    // Escaneia todas as redes visíveis
                    await adapter.ScanAsync();

                    foreach (var network in adapter.NetworkReport.AvailableNetworks)
                    {
                        Debug.WriteLine($"Rede:{network.Ssid} Frequência:{network.ChannelCenterFrequencyInKilohertz}");
                        // Verifica se o SSID da rede escaneada é igual ao SSID conectado
                        if (network.Ssid == connectedSsid)
                        {
                            // Verifica se a rede está na frequência de 2.4 GHz (bandas 2400-2500 MHz)
                            if (network.ChannelCenterFrequencyInKilohertz >= 2400000 && network.ChannelCenterFrequencyInKilohertz <= 2500000)
                            {
                                return false; // Pelo menos um BSSID está em 2.4 GHz
                            }
                        }
                    }
                }
            }
            return true; // Nenhuma rede 2.4 GHz encontrada na conexão ativa
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
            if (_wifiAdapter != null)
            {
                _wifiAdapter.Disconnect();
            }
        }
    }


}