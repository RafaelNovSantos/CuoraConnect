using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

public class LicenseService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LicenseService> _logger;

    public bool IsLicenseValid { get; private set; } = false;
    public bool IsConnectedInternet { get; private set; } = false;

    private const string LicenseUrl = "https://raw.githubusercontent.com/RafaelNovSantos/CuoraConnect/master/CuoraConnect/Licenca/activeLicense.xml";

    public LicenseService(HttpClient httpClient, ILogger<LicenseService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task CheckLicenseAsync()
    {
        IsConnectedInternet = await CheckInternetConnectionAsync();

        if (IsConnectedInternet)
        {
            await CheckLicenseValidityAsync();
        }
    }

    private async Task<bool> CheckInternetConnectionAsync()
    {
        try
        {
            using (Ping ping = new Ping())
            {
                PingReply reply = await ping.SendPingAsync("google.com");

                if (reply.Status == IPStatus.Success)
                {
                    Debug.WriteLine($"Ping para google.com bem-sucedido! Tempo: {reply.RoundtripTime}ms");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Falha no ping: {reply.Status}");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao verificar a conexão: {ex.Message}");
            return false;
        }
    }

    private async Task CheckLicenseValidityAsync()
    {
        try
        {
            string urlWithTimestamp = $"{LicenseUrl}?_={DateTime.Now.Ticks}";
            string xmlContent = await _httpClient.GetStringAsync(urlWithTimestamp);
            xmlContent = xmlContent.Trim();

            XDocument xdoc = XDocument.Parse(xmlContent);
            var activeValue = xdoc.Root?.Attribute("value")?.Value;

            IsLicenseValid = activeValue?.ToLower() == "true";
            Debug.WriteLine($"Licença válida: {IsLicenseValid}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao acessar o XML: {ex.Message}");
        }
    }




}
