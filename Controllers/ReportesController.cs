using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AspnetCoreMvcFull.Models.ViewModels;
using Newtonsoft.Json;

namespace AspnetCoreMvcFull.Controllers
{
    public class ReportesController : Controller
    {
        private readonly GraphQLService _graphQLService;

        public ReportesController(GraphQLService graphQLService)
        {
            _graphQLService = graphQLService;
        }

        // Acción GET para mostrar el formulario de creación de reporte
        [HttpGet]
        public IActionResult CrearReporte()
        {
          // Asegúrate de que el modelo esté correctamente inicializado
          var viewModel = new CreateReporteViewModel(); // Inicia el ViewModel si es necesario
          return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> CrearReporte([FromForm] CreateReporteViewModel model)
        {
          var idLocal = UserSingleton.Instance.Id;

          if (!ModelState.IsValid)
          {
            foreach (var key in ModelState.Keys)
            {
              var value = ModelState[key].Errors;
              Console.WriteLine($"{key}: {string.Join(", ", value.Select(e => e.ErrorMessage))}");
            }
            return View(model);
          }

          const string query = @"
           mutation NuevoReporte($input: ReporteInput) {
            nuevoReporte(input: $input) {
              id
              idLocal
              fechaInicio
              fechaFin
              ventasTotales
              pedidosCompletados
              pedidosVendidos
            }
          }";

          var input = new
          {
            idLocal = idLocal,
            fechaInicio = model.FechaInicio,
            fechaFin = model.FechaFin
          };

          var variables = new { input };
          Console.WriteLine($"Executando Reporte: {variables} " + "Auth: " + HttpContext.Session.GetString("AuthToken"));

          try
          {
            var response = await _graphQLService.ExecuteQueryAsync<Data>(query, variables, HttpContext.Session.GetString("AuthToken"));
            var jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            Console.WriteLine(jsonResponse);

            if (response != null)
            {
              // Redirigir al detalle del reporte o mostrar un mensaje de éxito
              return RedirectToAction("DetalleReporte", new { id = response.nuevoReporte.Id });
            }
            else
            {
              ModelState.AddModelError("", "Hubo un problema al crear el reporte.");
              return View(model);
            }
          }
          catch (Exception ex)
          {
            ModelState.AddModelError("", $"Error: {ex.Message}");
            return View(model);
          }
        }

        [HttpGet]
        public async Task<IActionResult> DetalleReporte(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError("", "El ID del reporte no es válido.");
                return RedirectToAction("ObtenerReportes");
            }

            // Consulta GraphQL para obtener el reporte por su ID
            const string queryReporte = @"
            query ObtenerReporte($id: ID!) {
                obtenerReporte(id: $id) {
                    id
                    idLocal
                    fechaInicio
                    fechaFin
                    ventasTotales
                    pedidosCompletados
                    pedidosVendidos
                }
            }";

            var variablesReporte = new { id };

            try
            {
                // Obtener el reporte
                var responseReporte = await _graphQLService.ExecuteQueryAsync<Data>(queryReporte, variablesReporte, HttpContext.Session.GetString("AuthToken"));

                if (responseReporte?.obtenerReporte == null)
                {
                    ModelState.AddModelError("", "No se pudo encontrar el reporte.");
                    return RedirectToAction("ObtenerReportes");
                }

                // Obtener los detalles de cada pedido si existen
                var pedidosDetalles = new List<Pedido>();

                if (responseReporte.obtenerReporte.PedidosVendidos != null && responseReporte.obtenerReporte.PedidosVendidos.Count > 0)
                {
                    foreach (var pedidoId in responseReporte.obtenerReporte.PedidosVendidos)
                    {
                        const string queryPedido = @"
                        query ObtenerPedidoPorID($id: ID!) {
                            obtenerPedidoPorID(id: $id) {
                                id
                                idCliente
                                idLocal
                                productos {
                                    idProducto
                                    cantidad
                                }
                                promociones {
                                    idPromo
                                    cantidad
                                }
                                tiempoPreparacion
                                estado
                                estadoVenta
                                total
                                fechaCreacion
                                urlqr
                                qr
                                transaccionID
                            }
                        }";

                        var variablesPedido = new { id = pedidoId };

                        var responsePedido = await _graphQLService.ExecuteQueryAsync<Data>(queryPedido, variablesPedido, HttpContext.Session.GetString("AuthToken"));

                        if (responsePedido?.obtenerPedidoPorID != null)
                        {
                            pedidosDetalles.Add(responsePedido.obtenerPedidoPorID);
                        }
                        else
                        {
                            Console.WriteLine($"No se pudo obtener el pedido con ID {pedidoId}");
                        }
                    }
                }

                // Aquí pasas los detalles del reporte y los detalles de los pedidos a la vista
                var detalleReporteViewModel = new DetalleReporteViewModel
                {
                    Reporte = responseReporte.obtenerReporte,
                    Pedidos = pedidosDetalles
                };

                return View(detalleReporteViewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al obtener el reporte: {ex.Message}");
                return RedirectToAction("ObtenerReportes");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerReportes()
        {
          var idLocal = UserSingleton.Instance.Id;
          const string query = @"
            query ObtenerReportes($idLocal: ID!) {
                obtenerReportes(idLocal: $idLocal) {
                    id
                    fechaInicio
                    fechaFin
                    ventasTotales
                    pedidosCompletados
                    pedidosVendidos
                }
            }";

          var variables = new { idLocal };

          Console.WriteLine(variables.idLocal);

          try
          {
            var response = await _graphQLService.ExecuteQueryAsync<Data>(query, variables, HttpContext.Session.GetString("AuthToken"));
            var jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            Console.WriteLine(jsonResponse);
            if (response.obtenerReportes != null)
            {
              return View(response.obtenerReportes); // Pasa los reportes a la vista
            }
            else
            {
              ViewBag.Mensaje = "No se encontraron reportes para este local.";
              return View(new List<Reporte>());
            }
          }
          catch (Exception ex)
          {
            ViewBag.Mensaje = $"Error al obtener los reportes: {ex.Message}";
            return View(new List<Reporte>());
          }
        }
    }
}
