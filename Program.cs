using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                        File.WriteAllBytes(@$"{Environment.CurrentDirectory}/certificate/{sender.RequestUri.Host}.cer", certificate.GetRawCertData());

                        return true;
                    }
                };

                var tasks = new List<Task>();

                using (var client = new HttpClient(handler))
                {
                    var pattern = "((?=www)|(?=transparencia)).*.gov.br";

                    foreach (var site in sites)
                    {
                        var match = Regex.Match(site.ToString(), pattern);

                        if (match.Success)
                            Task.Run(async () => await client.GetAsync($"https://{match.Value}"));
                        else
                            Console.WriteLine($"Skip: {site.ToString()}");
                    }
                }

                Task.WaitAll(tasks.ToArray());
            }

            Console.WriteLine(DateTime.Now);
        }
    }
}
