using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CuoraConnect.Services;
using System.Diagnostics;
using System.Threading.Tasks;

public class DigestAuthService
{
    private readonly IFileUploadService _fileUploadService;

    public DigestAuthService(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    // Função para calcular o hash MD5 de uma string
    // Função para calcular o hash MD5 de uma string
    public static string Md5Hash(string data)
    {
        using (var md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    // Função para extrair valores dos parâmetros do cabeçalho WWW-Authenticate
    public static Dictionary<string, string> ExtractDigestParts(string header)
    {
        var values = new Dictionary<string, string>();
        var pattern = new Regex(@"(\w+)=[\""']?([^\"",]+)[\""']?");
        var matches = pattern.Matches(header);

        foreach (Match match in matches)
        {
            values[match.Groups[1].Value] = match.Groups[2].Value;
        }

        return values;
    }

    // Função para obter o tipo MIME de um arquivo
    public static string GetMimeType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        switch (extension)
        {
            case ".txt": return "text/plain";
            case ".xml": return "application/xml";
            case ".jpg": return "image/jpeg";
            case ".png": return "image/png";
            case ".pdf": return "application/pdf";
            // Adicione mais extensões conforme necessário
            default: return "application/octet-stream";
        }
    }


   

    // Função para realizar a autenticação digest com MD5 e enviar o arquivo
    // Função para realizar a autenticação digest com MD5 e enviar o arquivo
    public static HttpWebResponse DigestAuthWithFile(string urlString, string username, string password, string filePath)
    {
        var url = new Uri(urlString);
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "POST";

        // Primeira requisição sem autenticação para obter o desafio digest
        HttpWebResponse response;
        try
        {
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }
        }
        catch (WebException ex)
        {
            response = (HttpWebResponse)ex.Response;
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                throw;
            }
        }

        // Obtém o cabeçalho WWW-Authenticate que contém os parâmetros do Digest
        string authHeader = response.Headers["WWW-Authenticate"];
        if (string.IsNullOrEmpty(authHeader))
        {
            throw new InvalidOperationException("O cabeçalho WWW-Authenticate não foi encontrado na resposta.");
        }

        var parts = ExtractDigestParts(authHeader);

        // Extrai valores de realm, nonce, etc.
        if (!parts.TryGetValue("realm", out var realm) ||
            !parts.TryGetValue("nonce", out var nonce) ||
            !parts.TryGetValue("qop", out var qop))
        {
            throw new InvalidOperationException("Os valores 'realm', 'nonce' ou 'qop' não podem ser nulos.");
        }

        // Configura os valores para o cálculo do hash MD5
        var method = "POST";
        var uri = urlString;

        // HA1: md5(username:realm:password)
        var ha1 = Md5Hash($"{username}:{realm}:{password}");

        // HA2: md5(method:uri)
        var ha2 = Md5Hash($"{method}:{uri}");

        // Resposta Digest: md5(HA1:nonce:nc:cnonce:qop:HA2)
        var nc = "00000001"; // Nonce Count
        var cnonce = "xyz";  // Client Nonce (valor qualquer, como "xyz")
        var responseDigest = Md5Hash($"{ha1}:{nonce}:{nc}:{cnonce}:{qop}:{ha2}");

        // Monta o valor do cabeçalho Authorization conforme o padrão Digest
        var authValue = $"Digest username=\"{username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{uri}\", " +
                        $"response=\"{responseDigest}\", qop={qop}, nc={nc}, cnonce=\"{cnonce}\", algorithm=MD5";

        // Cria a nova conexão para enviar o arquivo com autenticação
        request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "POST";
        request.Headers["Authorization"] = authValue;

        // Cria o body da requisição multipart/form-data
        var boundary = "===" + DateTime.Now.Ticks.ToString("x") + "===";
        request.ContentType = $"multipart/form-data; boundary={boundary}";
        request.AllowWriteStreamBuffering = true;

        using (var requestStream = request.GetRequestStream())
        using (var writer = new StreamWriter(requestStream, Encoding.UTF8))
        {
            // Adiciona o arquivo ao multipart/form-data
            var fileName = Path.GetFileName(filePath);
            writer.Write($"--{boundary}\r\n");
            writer.Write($"Content-Disposition: form-data; name=\"configrecord\"; filename=\"{fileName}\"\r\n");
            string mimeType = GetMimeType(filePath);
            writer.Write($"Content-Type: {mimeType}\r\n");
            writer.Write("\r\n");
            writer.Flush();

            // Copia o conteúdo do arquivo
            using (var fileStream = File.OpenRead(filePath))
            {
                fileStream.CopyTo(requestStream);
            }

            writer.Write("\r\n");
            writer.Write($"--{boundary}--\r\n");
            writer.Flush();
        }

        return (HttpWebResponse)request.GetResponse();
    }



   

    public async Task SendFileAsync(string url, string username, string password)
    {
        Debug.WriteLine("Iniciando SendFileAsync...");

        string _filePath = _fileUploadService.SaveXmlToFile();
        Debug.WriteLine($"Caminho do arquivo XML: {_filePath}");

        if (string.IsNullOrEmpty(_filePath))
        {
            Debug.WriteLine("Erro: Caminho do arquivo está nulo ou vazio.");
            return;
        }

        try
        {
            var response = DigestAuthWithFile(url, username, password, _filePath);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Debug.WriteLine("Arquivo enviado com sucesso!");
            }
            else
            {
                Debug.WriteLine($"Erro: {response.StatusDescription}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao enviar a requisição: {ex.Message}");
        }
    }
}
