using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrinterMonitor.Models
{
    public class Printer
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Setor { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Ip { get; set; } = "";
        public string Modelo { get; set; } = "";
        public bool Online { get; set; }
        public List<TonerInfo> Toners { get; set; } = new();
    }
}