using Newtonsoft.Json;

namespace AspnetCoreMvcFull.Models.ViewModels
{
  public class GraphQLResponse
  {
    public Data data { get; set; }
  }

  public class Data
  {
    public List<Reporte> obtenerReportes { get; set; }
    public Reporte obtenerReporte{ get; set; }
    public Pedido obtenerPedidoPorID { get; set; }
    public NotaVenta nuevaNotaDeVenta { get; set; }
    public Reporte nuevoReporte { get; set; }
  }
}
