using SQLite;
using System;
using System.IO;
using System.Linq;
using Android.App;
using SQLite;
using CuoraConnect.Models;
using CuoraConnect.Platforms.Android;
using CuoraConnect.Services;
using Application = Android.App.Application;
using System.Diagnostics;

[assembly: Dependency(typeof(FileUploadService))]
namespace CuoraConnect.Platforms.Android
{
    public class FileUploadService : IFileUploadService
    {
        private string _filePath;

        public FileUploadService()
        {
            // Inicializa o caminho do arquivo
            _filePath = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "config_default.xml");
        }

        public string SaveXmlToFile()
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                _filePath = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "config_default.xml");
            }

            // Verifica se o arquivo já existe
            if (File.Exists(_filePath))
            {
                // Apaga o arquivo existente
                File.Delete(_filePath);
                System.Diagnostics.Debug.WriteLine($"Arquivo existente apagado: {_filePath}");
            }

            // Gera o novo conteúdo XML com os dados do banco de dados
            var xmlContent = GenerateXml();

            File.WriteAllText(_filePath, xmlContent);
            System.Diagnostics.Debug.WriteLine($"Novo XML gerado e salvo em: {_filePath}");

            return _filePath;
        }

        public string pathDB()
        {
            var dbPath = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "networkinfo.db");
            return dbPath;
        }

        public string GenerateXml()
        {
            // Conecta ao banco de dados
            var dbPath = pathDB();
            using var db = new SQLiteConnection(dbPath);

            // Buscar os dados que serão inseridos no XML
            var NETConfig = db.Table<NetworkInfo>().FirstOrDefault(c => c.Id == "$");

            var ssid = NETConfig.SSID;
            var ipAddressConfig = NETConfig.AvailableIP;
            var ipDefault = NETConfig.Gateway;
            var password = NETConfig.Password;
            var cidr = NETConfig.CIDR;



            // Gera o conteúdo XML em uma string com os dados do banco de dados
            string xmlContent = $@"<?xml version='1.0' standalone='yes'?>
<!-- Automatically generated XML -->
<!DOCTYPE configrecord [
   <!ELEMENT configrecord (configgroup+)>
   <!ELEMENT configgroup (configitem+)>
   <!ELEMENT configitem (value+)>
   <!ELEMENT value (#PCDATA)>
   <!ATTLIST configrecord version CDATA #IMPLIED>
   <!ATTLIST configgroup name CDATA #IMPLIED>
   <!ATTLIST configgroup instance CDATA #IMPLIED>
   <!ATTLIST configitem name CDATA #IMPLIED>
   <!ATTLIST configitem instance CDATA #IMPLIED>
   <!ATTLIST value name CDATA #IMPLIED>
]>
<configrecord version = '0.1.0.1'>
   <configgroup name = 'Access Point' instance = 'ap0'>
      <configitem name = 'SSID'>
         <value>CUORA_MAX_%s</value>
      </configitem>
      <configitem name = 'Guest'>
         <value>Enabled</value>
      </configitem>
      <configitem name = 'Channel'>
         <value>1</value>
      </configitem>
      <configitem name = 'Suite'>
         <value>WPA2</value>
      </configitem>
      <configitem name = 'Encryption'>
         <value>CCMP</value>
      </configitem>
      <configitem name = 'Passphrase'>
         <value>SYSTEL1234</value>
      </configitem>
      <configitem name = 'Mode'>
         <value>Always Up</value>
      </configitem>
      <configitem name = 'DNS Redirect'>
         <value>xpicowifi.lantronix.com</value>
      </configitem>
   </configgroup>
   <configgroup name = 'Interface' instance = 'ap0'>
      <configitem name = 'State'>
         <value>Enabled</value>
      </configitem>
      <configitem name = 'IP Address'>
         <value>192.168.0.1/24</value>
      </configitem>
      <configitem name = 'MSS'>
         <value>1460 bytes</value>
      </configitem>
      <configitem name = 'DHCP IP Address Range'>
         <value name = 'Start'>&lt;Minimum&gt;</value>
         <value name = 'End'>&lt;Maximum&gt;</value>
      </configitem>
   </configgroup>
   <configgroup name = 'Interface' instance = 'wlan0'>
      <configitem name = 'State'>
         <value>Enabled</value>
      </configitem>
      <configitem name = 'DHCP Client'>
         <value>Disabled</value>
      </configitem>
      <configitem name = 'IP Address'>
         <value>{ipAddressConfig ?? ""}/{(cidr != 0 ? cidr.ToString() : "24")}</value>

      </configitem>
      <configitem name = 'Default Gateway'>
         <value>{ipDefault ?? "192.168.1.1"}</value>
      </configitem>
      <configitem name = 'Hostname'>
         <value></value>
      </configitem>
      <configitem name = 'Primary DNS'>
         <value>8.8.8.8</value>
      </configitem>
      <configitem name = 'Secondary DNS'>
         <value>1.1.1.1</value>
      </configitem>
      <configitem name = 'MSS'>
         <value>1460 bytes</value>
      </configitem>
   </configgroup>
   <configgroup name = 'WLAN Profile' instance = '{ssid ?? "CUORA_MAX"}'>
      <configitem name = 'Basic'>
         <value name = 'Network Name'>{ssid ?? "CUORA_MAX"}</value>
         <value name = 'State'>Enabled</value>
      </configitem>
      <configitem name = 'Security'>
         <value name = 'Suite'>WPA2</value>
         <value name = 'WEP Key Size'>40</value>
         <value name = 'WEP TX Key Index'>1</value>
         <value name = 'WEP Key 1 Key'></value>
         <value name = 'WEP Key 2 Key'></value>
         <value name = 'WEP Key 3 Key'></value>
         <value name = 'WEP Key 4 Key'></value>
         <value name = 'WPAx Key Type'>Passphrase</value>
         <value name = 'WPAx Passphrase'>{password ?? "senhaRede"}</value>
         <value name = 'WPAx Key'></value>
         <value name = 'WPAx Encryption'>CCMP</value>
      </configitem>
      <configitem name = 'Advanced'>
         <value name = 'TX Power Maximum'>17 dBm</value>
         <value name = 'Power Management'>Enabled</value>
         <value name = 'PM Interval'>1 beacons (100 ms each)</value>
      </configitem>
   </configgroup>
   <configgroup name = 'XML Import Control'>
      <configitem name = 'Restore Factory Configuration'>
         <value>Disabled</value>
      </configitem>
      <configitem name = 'Reboot'>
         <value>Enabled</value>
      </configitem>
      <configitem name = 'Missing Values'>
         <value>Set to Default</value>
      </configitem>
      <configitem name = 'Delete WLAN Profiles'>
         <value>Enabled</value>
      </configitem>
      <configitem name = 'WLAN Profile delete'>
         <value name = 'name'></value>
      </configitem>
   </configgroup>
</configrecord>";

            Debug.WriteLine($"{xmlContent}");
            return xmlContent;
        }
    }
}