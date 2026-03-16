using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrinterMonitor.Models;
using System.Text.Json;
using System.Xml.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Mvc;

namespace PrinterMonitor.Services
{
    public class PrinterService
    {
        private readonly string _filePath = "Data/printers.json";

        public List<Printer> GetAll()
        {
            if (!File.Exists(_filePath))
                return new List<Printer>();
            
            var json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<List<Printer>>(json) ?? new List<Printer>();
        }

        public void Add(Printer printer)
        {
            var printers = GetAll();

            printers.Add(printer);

            var json = JsonSerializer.Serialize(printers, new JsonSerializerOptions{WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)});

            File.WriteAllText(_filePath, json);
        }

        public Printer ? GetById(Guid id)
        {
            return GetAll().FirstOrDefault(p => p.Id == id);
        }

        public void Update(Printer printer)
        {
            var printers = GetAll();

            var index = printers.FindIndex(p => p.Id == printer.Id);

            if (index >= 0)
                printers[index] = printer;

            Save(printers);
        }

        public void Delete(Guid id)
        {
            var printers = GetAll();

            printers.RemoveAll(p => p.Id == id);

            Save(printers);
        }

        private void Save(List<Printer> printers)
        {
            var json = JsonSerializer.Serialize(printers, new JsonSerializerOptions{WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)});
            File.WriteAllText(_filePath, json);
        }

        public void AddRange(List<Printer> newPrinters)
        {
            var printers = GetAll();

            foreach(var printer in newPrinters)
            {
                if(!printers.Any(p => p.Id == printer.Id))
                {
                    printers.Add(printer);
                }
            }

            Save(printers);
        }

        public void UpdateInfoPrinters()
        {
            var printers = GetAll();

            var snmp = new PrinterSnmpService();

            foreach(var printer in printers)
            {
                var nome = snmp.ObterNomeImpressora(printer.Ip);
                var modelo = snmp.ObterModeloImpressora(printer.Ip);

                if(!string.IsNullOrEmpty(nome))
                    printer.Nome = nome;

                if(!string.IsNullOrEmpty(modelo))
                    printer.Modelo = modelo;
            }

            Save(printers);
        }

        public bool IsOnline(string ip)
        {
            try
            {
                return PrinterSnmpService.Test(ip);
            }
            catch
            {
                return false;
            }
        }
    }
}