using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CuoraConnect
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            // Verifica e instala o WebView2, se necessário, apenas no Windows
            InstallWebView2IfNeeded();
        }

        private async Task InstallWebView2IfNeeded()
        {
            // Verifica se a plataforma é Windows
#if WINDOWS
            string webView2InstallerUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";  // URL do WebView2 Bootstrapper
            string installerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebView2Setup.exe");

            // Verifica se o WebView2 já está instalado
            if (!IsWebView2Installed())
            {
                // Executa o download e instalação em segundo plano
                await Task.Run(async () =>
                {
                    // Baixa o WebView2 Bootstrapper
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(webView2InstallerUrl);
                        var bytes = await response.Content.ReadAsByteArrayAsync();

                        // Salva o instalador na pasta de dados locais
                        await File.WriteAllBytesAsync(installerPath, bytes);

                        // Executa o instalador com privilégios de administrador
                        RunInstallerAsAdmin(installerPath);
                    }
                });
            }
#endif
        }

        private void RunInstallerAsAdmin(string installerPath)
        {
            try
            {
                // Configura o processo para executar o instalador como administrador
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    Verb = "runas",  // Solicita privilégios de administrador
                    UseShellExecute = true, // Necessário para usar "runas"
                    CreateNoWindow = true // Opcional, se não quiser mostrar a janela do terminal
                };

                // Inicia o processo
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                // Lida com erros caso o processo falhe (por exemplo, se o usuário não permitir a elevação)
                Console.WriteLine("Erro ao tentar executar o instalador como administrador: " + ex.Message);
            }
        }

        private bool IsWebView2Installed()
        {
            // Verifica a presença do WebView2 no registro do Windows
            string registryKey = @"HKEY_CURRENT_USER\Software\Microsoft\EdgeWebView";
            bool WebView2Installed = false;

            try
            {
                // Abre a chave de registro sem precisar especificar um valor específico
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\EdgeWebView"))
                {
                    // Se a chave existir, define como true
                    if (key != null)
                    {
                        WebView2Installed = true;
                    }
                }

                // Obtém a letra da unidade onde o sistema está instalado (geralmente C:)
                string systemDrive = Environment.GetEnvironmentVariable("SystemDrive");

                // Caminho para a instalação do WebView2
                string pathWebView32bits = Path.Combine(systemDrive, @"Program Files (x86)\Microsoft\EdgeWebView\Application");
                string pathWebView64bits = Path.Combine(systemDrive, @"Program Files\Microsoft\EdgeWebView\Application");

                if (Directory.Exists(pathWebView64bits) || Directory.Exists(pathWebView32bits))
                {
                    WebView2Installed = true;

                }
                // Retorna se o WebView2 está instalado
                return WebView2Installed;
            }
            catch (Exception ex)
            {
                // Em caso de erro ao acessar o registro, trata a exceção e retorna que o WebView2 não está instalado
                Console.WriteLine("Erro ao verificar o WebView2 no registro: " + ex.Message);
                return false;
            }
        }
    }
}
