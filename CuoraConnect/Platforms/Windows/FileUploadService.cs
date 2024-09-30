﻿using System;
using System.IO;
using CuoraConnect.Services;

namespace CuoraConnect.Platforms.Windows
{
    public class FileUploadService : IFileUploadService
    {
        private string _filePath;

        public FileUploadService()
        {
            // Inicializa o caminho do arquivo
            _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "config_default.xml");
        }

        public string SaveXmlToFile()
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "config_default.xml");
            }

            // Verifica se o arquivo já existe
            if (File.Exists(_filePath))
            {
                // Apaga o arquivo existente
                File.Delete(_filePath);
                System.Diagnostics.Debug.WriteLine($"Arquivo existente apagado: {_filePath}");
            }

            // Gera o novo conteúdo XML
            var xmlContent = GenerateXml();
            File.WriteAllText(_filePath, xmlContent);
            System.Diagnostics.Debug.WriteLine($"Novo XML gerado e salvo em: {_filePath}");

            return _filePath;
        }

        public string pathDB()
        {
            var dbPath = "";
            return dbPath;
        }
        public string GenerateXml()
        {
            // Conteúdo XML em uma string
            string xmlContent = @"<?xml version=""1.0"" standalone=""yes""?>
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
<configrecord version = ""0.1.0.1"">
   <configgroup name = ""Access Point"" instance = ""ap0"">
      <configitem name = ""SSID"">
         <value>CUORA_MAX</value>
      </configitem>
      <configitem name = ""Guest"">
         <value>Enabled</value>
      </configitem>
      <configitem name = ""Channel"">
         <value>1</value>
      </configitem>
      <configitem name = ""Suite"">
         <value>WPA2</value>
      </configitem>
      <configitem name = ""Encryption"">
         <value>CCMP</value>
      </configitem>
      <configitem name = ""Passphrase"">
         <value>&lt;Configured&gt;</value>
      </configitem>
      <configitem name = ""Mode"">
         <value>Always Up</value>
      </configitem>
      <configitem name = ""DNS Redirect"">
         <value>xpicowifi.lantronix.com</value>
      </configitem>
   </configgroup>
   <configgroup name = ""Interface"" instance = ""ap0"">
      <configitem name = ""State"">
         <value>Enabled</value>
      </configitem>
      <configitem name = ""IP Address"">
         <value>192.168.0.1/24</value>
      </configitem>
      <configitem name = ""MSS"">
         <value>1460 bytes</value>
      </configitem>
      <configitem name = ""DHCP IP Address Range"">
         <value name = ""Start"">&lt;Minimum&gt;</value>
         <value name = ""End"">&lt;Maximum&gt;</value>
      </configitem>
   </configgroup>
   <configgroup name = ""Interface"" instance = ""wlan0"">
      <configitem name = ""State"">
         <value>Enabled</value>
      </configitem>
      <configitem name = ""DHCP Client"">
         <value>Disabled</value>
      </configitem>
      <configitem name = ""IP Address"">
         <value>192.168.1.130/24</value>
      </configitem>
      <configitem name = ""Default Gateway"">
         <value>192.168.1.1</value>
      </configitem>
      <configitem name = ""Hostname"">
         <value></value>
      </configitem>
      <configitem name = ""Primary DNS"">
         <value>8.8.8.8</value>
      </configitem>
      <configitem name = ""Secondary DNS"">
         <value>1.1.1.1</value>
      </configitem>
      <configitem name = ""MSS"">
         <value>1460 bytes</value>
      </configitem>
   </configgroup>
   <configgroup name = ""WLAN Profile"" instance = ""SYSTEL_BALANCAS"">
      <configitem name = ""Basic"">
         <value name = ""Network Name"">SYSTEL_BALANCAS</value>
         <value name = ""State"">Enabled</value>
      </configitem>
      <configitem name = ""Security"">
         <value name = ""Suite"">WPA2</value>
         <value name = ""WEP Key Size"">40</value>
         <value name = ""WEP TX Key Index"">1</value>
         <value name = ""WEP Key 1 Key""></value>
         <value name = ""WEP Key 2 Key""></value>
         <value name = ""WEP Key 3 Key""></value>
         <value name = ""WEP Key 4 Key""></value>
         <value name = ""WPAx Key Type"">Passphrase</value>
         <value name = ""WPAx Passphrase"">systel1234</value>
         <value name = ""WPAx Key""></value>
         <value name = ""WPAx Encryption"">CCMP</value>
      </configitem>
      <configitem name = ""Advanced"">
         <value name = ""TX Power Maximum"">17 dBm</value>
         <value name = ""Power Management"">Enabled</value>
         <value name = ""PM Interval"">1 beacons (100 ms each)</value>
      </configitem>
   </configgroup>
   <configgroup name = ""XML Import Control"">
      <configitem name = ""Restore Factory Configuration"">
         <value>Disabled</value>
      </configitem>
      <configitem name = ""Reboot"">
         <value>Disabled</value>
      </configitem>
      <configitem name = ""Missing Values"">
         <value>Set to Default</value>
      </configitem>
      <configitem name = ""Delete WLAN Profiles"">
         <value>Enabled</value>
      </configitem>
      <configitem name = ""WLAN Profile delete"">
         <value name = ""name""></value>
      </configitem>
   </configgroup>
</configrecord>";

            return xmlContent;
        }
    }
}
