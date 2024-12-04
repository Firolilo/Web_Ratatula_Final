namespace AspnetCoreMvcFull.Models;

public class Reporte
{
  public string Id { get; set; }

  public string IdLocal { get; set; }
  public string FechaInicio { get; set; }
  public string FechaFin { get; set; }
  public float VentasTotales { get; set; }
  public int PedidosCompletados { get; set; }
  public List<string> PedidosVendidos { get; set; }
}
public class ReporteInput
{
  public string IdLocal { get; set; }
  public string FechaInicio { get; set; }
  public string FechaFin { get; set; }
}

