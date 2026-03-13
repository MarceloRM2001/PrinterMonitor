using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using PrinterMonitor.Models;

namespace PrinterMonitor.Services
{
    public class PrinterSnmpService
    {
        public string ObterNomeImpressora(string ip)
        {
            try
            {
                
            
                var result = Messenger.Get(
                    VersionCode.V1,
                    new IPEndPoint(IPAddress.Parse(ip), 161),
                    new OctetString("public"),
                    new List<Variable> { new Variable(new ObjectIdentifier("1.3.6.1.2.1.1.5.0")) },
                    6000
                );

                return result.First().Data.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine($"IP: {ip}, Mensagem de Exceção no ObterNome: {e.Message}");
                return "";
            }
        }

        public string ObterModeloImpressora(string ip)
        {
            try
            {
                var result = Messenger.Get(
                    VersionCode.V1,
                    new IPEndPoint(IPAddress.Parse(ip), 161),
                    new OctetString("public"),
                    new List<Variable> { new Variable(new ObjectIdentifier("1.3.6.1.2.1.25.3.2.1.3.1")) },
                    6000
                );

                return result.First().Data.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine($"IP: {ip}, Mensagem de Exceção no ObterModelo: {e.Message}");
                return "";
            }
        }

        public List<TonerInfo> WalkImpressora(string ip)
        {
            var result = new List<Variable>();

            var descricao = new Dictionary<int, string>();
            var capacidade = new Dictionary<int, int>();
            var nivel = new Dictionary<int, int>();
            var tipo = new Dictionary<int, int>();

            var ignorar = new[]
            {
                "fuser",
                "roller",
                "transfer",
                "maintenance",
                "kit",
                "waste",
                "drum",
                "belt",
                "tray"
            };

            try
            {
                Messenger.Walk(
                    VersionCode.V1,
                    new IPEndPoint(IPAddress.Parse(ip), 161),
                    new OctetString("public"),
                    new ObjectIdentifier("1.3.6.1.2.1.43.11.1.1"),
                    result,
                    6000,
                    WalkMode.WithinSubtree
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"IP: {ip}, Mensagem de Exceção no Walk: {e.Message}");
            }

            foreach(var item in result)
            {
                var oid = item.Id.ToString().Split('.');
                int coluna = int.Parse(oid[10]);
                int indice = int.Parse(oid.Last());

                if(coluna == 4)
                {
                    tipo[indice] = int.Parse(item.Data.ToString());
                }
                else if(coluna == 6)
                {   
                    if(item.Data.ToString().Contains("Black"))
                        descricao[indice] = "Black";
                    else if(item.Data.ToString().Contains("Magent"))
                        descricao[indice] = "Magent";
                    else if(item.Data.ToString().Contains("Cyan"))
                        descricao[indice] = "Cyan";
                    else if(item.Data.ToString().Contains("Yellow"))
                        descricao[indice] = "Yellow";
                }
                else if(coluna == 8)
                {
                    capacidade[indice] = int.Parse(item.Data.ToString());
                }
                else if(coluna ==9)
                {
                    nivel[indice] = int.Parse(item.Data.ToString());   
                }
            }

            var toners = new List<TonerInfo>();

            foreach (var i in descricao.Keys)
            {
                if (!tipo.ContainsKey(i))
                    continue;
                
                int classe = tipo[i];

                //3 = Toner | 4 = ink (tinta)
                if (classe != 3  && classe != 4)
                    continue;

                var desc = descricao[i].ToLower();

                if (ignorar.Any(p => desc.Contains(p)))
                    continue;

                double porcentagem = -1;

                if (capacidade.ContainsKey(i) && nivel.ContainsKey(i))
                {
                    if (nivel[i] >= 0 && capacidade[i] > 0)
                    {
                        porcentagem = ((double)nivel[i] / capacidade[i]) * 100;
                    }
                }

                toners.Add(new TonerInfo
                {
                    Descricao = descricao[i],
                    Porcentagem = porcentagem
                });
            }

            return toners;
        }
        
    }
}