using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PrinterMonitor.Models;
using PrinterMonitor.Services;

namespace PrinterMonitor.Controllers;

public class PrintersController : Controller
{
    private readonly PrinterService _printerService = new();

    public IActionResult Index()
    {
        var printers = _printerService.GetAll();

        return View(printers);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Printer printer)
    {
        _printerService.Add(printer);

        return RedirectToAction("Index");
    }

    public IActionResult Edit(Guid id)
    {
        var printer = _printerService.GetById(id);

        return View(printer);
    }

    [HttpPost]
    public IActionResult Edit(Printer printer)
    {
        _printerService.Update(printer);

        return RedirectToAction("Index");
    }

    public IActionResult Delete(Guid id)
    {
        _printerService.Delete(id);

        return RedirectToAction("Index");
    }

    public IActionResult TestSnmp(string ip)
    {
        try
        {
            var result = PrinterSnmpService.Test(ip);

            return Json(new { online = result });
        }
        catch
        {
            return Json(new { online = false });
        }
    }

    public async Task<IActionResult> Discover()
    {
        var ips = await PrinterSnmpService.Discover("192.168.0");

        var snmp = new PrinterSnmpService();

        var newPrinters = new List<Printer>(); 
        
        foreach(var ip in ips)
        {
            var nome = snmp.ObterNomeImpressora(ip);
            var modelo = snmp.ObterModeloImpressora(ip);

            if(string.IsNullOrEmpty(nome))
                nome = "Impressora desconhecida";

            if(string.IsNullOrEmpty(modelo))
                modelo = "Modelo desconhecido";

            newPrinters.Add(new Printer
            {
                Setor = "Descoberta automática",
                Nome = nome,
                Ip = ip,
                Modelo = modelo
            });
        }
        _printerService.AddRange(newPrinters);

        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("Error");
    }
}
