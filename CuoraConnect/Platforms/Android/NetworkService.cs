﻿using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using CuoraConnect.Components.Pages;
using CuoraConnect.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Application = Android.App.Application;
using Microsoft.JSInterop;

[assembly: Dependency(typeof(CuoraConnect.Platforms.Android.NetworkService))]
namespace CuoraConnect.Platforms.Android
{

    public class NetworkService : INetworkService
    {
        public string nameinterface = "";

        private readonly IJSRuntime _jsRuntime;




        public async Task<string> GetDefaultGateway()
        {
           

            int count = 0;
            while (count <= 4)
            {

                Debug.WriteLine($"Tentativa {count} de verificar Gateway do roteador");
                count++;

                CheckAndRequestLocationPermission();
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
                    if (gateway == "0.0.0.0")
                    {
                        gateway = "";
                    }

                    return gateway;
                }
            }
            return "Não foi possível encontrar IP disponível na rede.";
        }




        public async Task<string> GetInfoInterface(string type)
        {
         
         
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
                //Nenhuma conexão Wi-Fi ativa
                return "";
            }
            int count = 1;
            while (count <= 4)
            {
                Debug.WriteLine($"Tentativa {count} de encontrar {type}");
                count++;

                // Obtém informações sobre a conexão
                var info = wifiManager.ConnectionInfo;
                Debug.WriteLine($"SSID INFO:{info}");
                if (info == null || info.NetworkId == -1)
                {
                    return "Wi-Fi Desconectado ou Informação Indisponível.";
                }

                if (type == "SSID")
                {
                    // Retorna o SSID, removendo aspas
                    var ssid = info.SSID?.Replace("\"", "");

                    if (string.IsNullOrEmpty(ssid))
                    {
                        return "SSID Indisponível.";
                    }
                    return ssid;
                }

                if (type == "BSSID")
                {
                    var bssid = info.BSSID?.Replace("\"", "");
                    // Remove os dois pontos do BSSID
                    bssid = bssid.Replace(":", string.Empty);
                    bssid = bssid.ToUpper();

                    if (string.IsNullOrEmpty(bssid))
                    {
                        return "BSSID Indisponível.";
                    }
                    return bssid;
                }   
            }

           return "";
        }






        private ConnectivityManager.NetworkCallback _networkCallback;

        public async Task<bool> ConnectToWifiAsync(string ssid, string password)
        {


            

            var wifiNetworkSpecifier = new WifiNetworkSpecifier.Builder()
                .SetSsid(ssid)
                .SetWpa2Passphrase(password)
                .Build();

            var networkRequest = new NetworkRequest.Builder()
                .AddTransportType(TransportType.Wifi)
                .SetNetworkSpecifier(wifiNetworkSpecifier)
                .Build();

            var connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);

            var tcs = new TaskCompletionSource<bool>();

            _networkCallback = new CustomNetworkCallback(tcs);
            connectivityManager.RequestNetwork(networkRequest, _networkCallback);

            return await tcs.Task;
        }

        public void DisconnectFromWifi()
        {
            var connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            if (_networkCallback != null)
            {
                connectivityManager.UnregisterNetworkCallback(_networkCallback);
                _networkCallback = null;
            }
        }

        private class CustomNetworkCallback : ConnectivityManager.NetworkCallback
        {
            private TaskCompletionSource<bool> _tcs;

            public CustomNetworkCallback(TaskCompletionSource<bool> tcs)
            {
                _tcs = tcs;
            }

            public override void OnAvailable(Network network)
            {
                base.OnAvailable(network);
                _tcs.TrySetResult(true);
            }

            public override void OnUnavailable()
            {
                base.OnUnavailable();
                _tcs.TrySetResult(false);
            }

            public override void OnLost(Network network)
            {
                base.OnLost(network);
                _tcs.TrySetResult(false);
            }
        }



        public async Task<PermissionStatus> CheckAndRequestLocationPermission()
        {
            Wi_FiConfig config = new Wi_FiConfig();
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
                return status;

            // solicita a permissão
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

          
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

        public async Task<string> GetAvailableIPAddress()
        {
           await Task.Delay(2000);
            string localIPAddress = GetLocalIPAddress();
            if (localIPAddress == "IP Não Encontrado")
            {
                return "IP Não Encontrado";
            }

            string[] ipParts = localIPAddress.Split('.');
            string subnetPrefix = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.";

            for (int i = 70; i <= 254; i++)
            {
                string ip = subnetPrefix + i;

                if (ip != localIPAddress)
                {
                    try
                    {
                        int count = 0;
                        int failedPingCount = 0;

                        while (count < 20)
                        {
                            count++;
                            try
                            {
                                Ping ping = new Ping();
                                PingReply reply = ping.Send(ip, 200); // Timeout de 100ms

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

                                // Se falhou nos 10 pings, o IP pode ser considerado disponível
                                if (failedPingCount == 20)
                                {
                                    return ip; // IP está disponível
                                }
                            }
                            catch
                            {
                                failedPingCount++;
                                if (failedPingCount == 20)
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

            //Nenhum IP disponível encontrado.
            return "";
        }


        public async Task<string> GetSubnetMask()
        {
            return await Task.Run(() =>
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
                                return subnetMask; // Retorna a máscara de sub-rede
                            }
                        }
                    }
                }
                // Máscara de rede não encontrada.
                return "";
            });
        }



        public async Task<string> IsConnectedTo5G()
        {
            await Task.Delay(1000);
            // Obtém o contexto da aplicação
            var context = Application.Context;

            // Obtém o WifiManager
            var wifiManager = (WifiManager)context.GetSystemService(Context.WifiService);


            
            // Reinicia o WifiManager
            RestartWifiManager(wifiManager);

            // Obtém o SSID da rede conectada
            string connectedSsid = await GetInfoInterface("SSID");

            // Verifica se não há conexão
            if (string.IsNullOrEmpty(connectedSsid))
            {
                return "Nenhuma rede conectada"; // Caso não esteja conectado a nenhuma rede
            }

           

            // Verifica se o WifiManager foi obtido corretamente
            if (wifiManager == null)
            {
                return "Não foi possível obter a interface de rede."; // Erro ao obter o WifiManager
            }



            // Inicia a varredura das redes
            wifiManager.StartScan();
            await Task.Delay(1000); // Aguarda o scan

            // Verifica as redes escaneadas
          
            int countVerific5g = 0;

            while (countVerific5g <= 6)
            {
                await Task.Delay(1000);
                countVerific5g++;
                Debug.WriteLine($"Tentativa {countVerific5g} de verificar conexão 2.4g ");

                foreach (var network in wifiManager.ScanResults)
                {

                    // Reinicia o WifiManager
                    await RestartWifiManager(wifiManager);

                    // Verifica se o SSID da rede escaneada é igual ao SSID conectado
                    if (network.Ssid.Equals(connectedSsid))
                    {
                        Debug.WriteLine($"Rede: {network.Ssid} Frequência: {network.Frequency}");

                        // Verifica se a rede está na frequência de 2.4 GHz (bandas 2400-2500 MHz)
                        if (network.Frequency >= 2400 && network.Frequency <= 2500)
                        {
                            
                            return "Conectado a 2.4 GHz"; // Não precisamos continuar verificando
                        }
                    }
                }
            }
            return "Conectado a 5 GHz";
        }



        // Método para reiniciar o WifiManager de forma assíncrona
        private async Task RestartWifiManager(WifiManager wifiManager)
        {
            // Desativa o Wi-Fi
             wifiManager.SetWifiEnabled(false);
             wifiManager.Reassociate();
            // Ativa o Wi-Fi novamente
             wifiManager.SetWifiEnabled(true);
 
        }

        public bool IsMobileDataEnabled()
        {
            var context = Application.Context;
            var connectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            try
            {
                // Usa reflexão para acessar o método "getMobileDataEnabled"
                var method = connectivityManager.Class.GetDeclaredMethod("getMobileDataEnabled");
                method.Accessible = true;
                bool isMobileDataEnabled = (bool)method.Invoke(connectivityManager);
                return isMobileDataEnabled;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao verificar dados móveis: " + ex.Message);
                return false;
            }
        }
    }
}