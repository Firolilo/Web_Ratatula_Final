using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models.ViewModels
{
  public class ReporteViewModel
  {
    public string IdLocal { get; set; }
    public string FechaInicio { get; set; }
    public string FechaFin { get; set; }
    public float VentasTotales { get; set; }
    public int PedidosCompletados { get; set; }
    public List<string> PedidosVendidos { get; set; }
  }
  public class CreateReporteViewModel
  {
    public string FechaInicio { get; set; }
    public string FechaFin { get; set; }
  }
}
