namespace AspnetCoreMvcFull.Models;

public class NotaVenta
{
  public string id { get; set; }
  public string idPedido { get; set; }
  public string metodo { get; set; }
  public float montoTotal { get; set; }
  public float montoPagado { get; set; }
  public float cambio { get; set; }
  public string fechaPago { get; set; }
}
