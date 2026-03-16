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

        var status = new Dictionary<string, bool>();

        foreach(var printer in printers)
        {
            if(string.IsNullOrEmpty(printer.Ip))
                continue;

            try
            {
                status[printer.Ip] = _printerService.IsOnline(printer.Ip);
            }
            catch
            {
                status[printer.Ip] = false;
            }
        }

        ViewBag.Status = status;
        ViewBag.TotalPrinters = printers.Count;

        return View(printers);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Printer printer)
    {
        if(string.IsNullOrEmpty(printer.Ip))
               printer.Ip = "0.0.0.0";

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

    public async Task<IActionResult> Discover(string network)
    {
        var ips = await PrinterSnmpService.Discover(network);

        var snmp = new PrinterSnmpService();

        var newPrinters = new List<Printer>(); 

        var existentes = _printerService.GetAll();
        
        foreach(var ip in ips)
        {
            var nome = snmp.ObterNomeImpressora(ip);
            var modelo = snmp.ObterModeloImpressora(ip);
            var serial = snmp.ObterSerial(ip);

            if(string.IsNullOrEmpty(serial))
                continue;

            var printerExistente = existentes
                .FirstOrDefault(p => p.SerialNumer == serial);

            if(printerExistente != null)
            {
                printerExistente.Ip = ip;
                continue;
            }

            if(string.IsNullOrEmpty(nome))
                nome = "Impressora desconhecida";

            if(string.IsNullOrEmpty(modelo))
                modelo = "Modelo desconhecido";

            newPrinters.Add(new Printer
            {
                SerialNumer = serial,
                Setor = "Descoberta automática",
                Nome = nome,
                Ip = ip,
                Modelo = modelo
            });
        }
        _printerService.AddRange(newPrinters);

        return RedirectToAction("Index");
    }

    public IActionResult UpdateAll()
    {
        
        _printerService.UpdateInfoPrinters();

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Status()
    {
        var printers = _printerService.GetAll();

        var status = new Dictionary<string, bool>();

        foreach(var printer in printers)
        {
            if(!string.IsNullOrEmpty(printer.Ip))
                status[printer.Ip] = _printerService.IsOnline(printer.Ip);
        }

        return Json(status);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("Error");
    }
}
