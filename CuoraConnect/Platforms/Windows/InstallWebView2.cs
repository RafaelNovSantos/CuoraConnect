using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CuoraConnect.Platforms.Windows
{
    public class InstallWebView2
    {
        public async Task InstallWebView2IfNeeded()
        {
            string webView2InstallerUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";  // URL do WebView2 Bootstrapper
            string installerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebView2Setup.exe");

            // Verifica se o WebView2 já está instalado
            if (!IsWebView2Installed())
            {
                // Baixa o WebView2 Bootstrapper
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(webView2InstallerUrl);
                    var bytes = await response.Content.ReadAsByteArrayAsync();

                    // Salva o instalador na pasta de dados locais
                    await File.WriteAllBytesAsync(installerPath, bytes);

                    // Executa o instalador
                    Process.Start(new ProcessStartInfo(installerPath) { UseShellExecute = true });
                }
            }
        }

        public bool IsWebView2Installed()
        {
            // Aqui você pode implementar uma verificação se o WebView2 já está instalado
            // Pode verificar no registro ou até tentar inicializar um WebView2
            return false; // Supondo que não esteja instalado
        }
    }
}
