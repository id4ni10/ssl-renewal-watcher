using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ssl_renewal_watcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(DateTime.Now);

            var fullfilepath = @"%windir%\System32\inetsrv\config\applicationHost.config";

            if (!File.Exists(fullfilepath))
            {
                var xml = XDocument.Load(@"C:\Users\id4ni\Desktop\applicationHost.config"); //fullfilepath;

                var sites = xml.XPathSelectElements("//site[contains(@name, 'sai.io.org.br')]//binding[contains(@protocol, 'https')]");

                var handler = new HttpClientHandler
                {
                    UseDefaultCredentials = true,

                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, error) =>
                    {
                        var pem = $"-----BEGIN CERTIFICATE-----\r\n{Convert.ToBase64String(certificate.RawData, Base64FormattingOptions.InsertLineBreaks)}\r\n-----END CERTIFICATE-----";

                        File.WriteAllText(@$"{Environment.CurrentDirectory}/certificate/{(certificate.Verify() ? "valid" : "invalid")}/{sender.RequestUri.Host}.cer", pem);

                        return true;
                    }
                };

                var client = new HttpClient(handler);

                var pattern = "((?=www)|(?=transparencia)).*.gov.br";

                foreach (var site in sites)
                {
                    try
                    {
                        var match = Regex.Match(site.ToString(), pattern);

                        if (match.Success)
                            client.GetAsync($"https://{match.Value}").Wait();
                        else
                            Console.WriteLine($"Skip: {site.ToString()}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Err: {site.ToString()}");
                    }
                }
            }

            Console.WriteLine(DateTime.Now);
        }
    }
}

