using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PrinterMonitor.Models;
using PrinterMonitor.Services;

namespace PrinterMonitor.Controllers;

public class HomeController : Controller
{
    public async Task<IActionResult> Index()
    {
        var service = new PrinterSnmpService();
        
        var impressoras = new List<Printer>
        {
            new Printer {Setor = "Gerência", Ip = "192.168.0.125"},
            new Printer {Setor = "Tempera", Ip = "192.168.0.131"},
            new Printer {Setor = "Almox", Ip = "192.168.0.123"},
            new Printer {Setor = "RH", Ip = "192.168.0.133"},
            new Printer {Setor = "Compras", Ip = "192.168.0.129"},
            new Printer {Setor = "Financeiro", Ip = "192.168.0.132"},
            new Printer {Setor = "Qualidade", Ip = "192.168.0.120"},
            new Printer {Setor = "Dobra", Ip = "192.168.0.122"},
            new Printer {Setor = "Fiscal", Ip = "192.168.0.130"},
            new Printer {Setor = "Expedição", Ip = "192.168.0.128"},
            new Printer {Setor = "PCP", Ip = "192.168.0.54"},
            new Printer {Setor = "Montagem", Ip = "192.168.0.121"},
            new Printer {Setor = "Programação", Ip = "192.168.0.126"},
            new Printer {Setor = "Logística Interna", Ip = "192.168.0.119"},
            new Printer {Setor = "2D", Ip = "192.168.0.127"},
            new Printer {Setor = "PA CNH", Ip = "192.168.90.10"},
            new Printer {Setor = "PA", Ip = "192.168.90.15"},
            new Printer {Setor = "Pintura", Ip = "192.168.90.11"},
            new Printer {Setor = "PA CAT PRD", Ip = "192.168.90.12"},
            new Printer {Setor = "PA CAT", Ip = "192.168.90.13"},
            new Printer {Setor = "Engenharia", Ip = "192.168.0.55"},
            new Printer {Setor = "Diretoria", Ip = "192.168.0.116"},
            new Printer {Setor = "Solda", Ip = "192.168.0.117"},
            new Printer {Setor = "PNT Colorida", Ip = "192.168.90.14"}
            
        };

        var tasks = impressoras.Select(async printer =>
        {
            try
            {
                var timeoutTask = Task.Delay(2000);

                var snmpTask = Task.Run(() =>
                {
                    printer.Nome = service.ObterNomeImpressora(printer.Ip);
                    printer.Modelo = service.ObterModeloImpressora(printer.Ip);
                    printer.Toners = service.WalkImpressora(printer.Ip);
                });

                var completed = await Task.WhenAny(snmpTask, timeoutTask);

                if (completed == timeoutTask)
                {
                    printer.Online = false;
                }
                else
                {
                    printer.Online = true;
                }  
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exceção no HomeController: {e.Message}");
                printer.Online = false;
            }
        });

        await Task.WhenAll(tasks);
        

        return View(impressoras);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
