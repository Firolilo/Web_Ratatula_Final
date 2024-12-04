using AspnetCoreMvcFull.Models.ViewModels;
using AspnetCoreMvcFull.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Controllers
{
    public class PedidosController : Controller
    {
        private readonly GraphQLService _graphQLService;

        public PedidosController(GraphQLService graphQLService)
        {
            _graphQLService = graphQLService;
        }

        // Acción para ver pedidos pendientes
        [HttpGet]
        public async Task<IActionResult> Pendientes()
        {
            // Define la consulta GraphQL para obtener pedidos pendientes de un local específico
            const string query = @"
                query ObtenerPedidosLocalPendiente($id: ID!) {
                    obtenerPedidosLocalPendiente(id: $id) {
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

            // Supongamos que el idLocal se obtiene del usuario autenticado o de otra fuente
            // Aquí lo obtenemos de la sesión, pero ajusta esto según tu lógica
            var idLocal = UserSingleton.Instance.Id;

            if (string.IsNullOrEmpty(idLocal))
            {
                return Unauthorized("No se encontró el ID del local en la sesión.");
            }

            var variables = new
            {
                id = idLocal
            };

            try
            {
                // Ejecutar la consulta GraphQL
                var response = await _graphQLService.ExecuteQueryAsync<ObtenerPedidosLocalPendienteResponse>(query, variables, HttpContext.Session.GetString("AuthToken"));

                // Verificar la respuesta y pasar los datos a la vista
                if (response?.ObtenerPedidosLocalPendiente != null)
                {
                    return View(response.ObtenerPedidosLocalPendiente);
                }
                else
                {
                    ViewBag.Mensaje = "No se encontraron pedidos pendientes para este local.";
                    return View(new List<PedidoViewModel>());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener pedidos pendientes: {ex.Message}");
                return StatusCode(500, $"Error al obtener pedidos pendientes: {ex.Message}");
            }
        }

        // Acción para ver pedidos completados
        [HttpGet]
        public async Task<IActionResult> Completados()
        {
            // Define la consulta GraphQL para obtener pedidos completados de un local específico
            const string query = @"
                query ObtenerPedidosLocalCompletado($id: ID!) {
                    obtenerPedidosLocalCompletado(id: $id) {
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

            // Obtiene el idLocal de la sesión
            var idLocal = UserSingleton.Instance.Id;

            if (string.IsNullOrEmpty(idLocal))
            {
                return Unauthorized("No se encontró el ID del local en la sesión.");
            }

            var variables = new
            {
                id = idLocal
            };

            try
            {
                // Ejecutar la consulta GraphQL
                var response = await _graphQLService.ExecuteQueryAsync<ObtenerPedidosLocalCompletadoResponse>(query, variables, HttpContext.Session.GetString("AuthToken"));

                // Verificar la respuesta y pasar los datos a la vista
                if (response?.ObtenerPedidosLocalCompletado != null)
                {
                    return View(response.ObtenerPedidosLocalCompletado);
                }
                else
                {
                    ViewBag.Mensaje = "No se encontraron pedidos completados para este local.";
                    return View(new List<PedidoViewModel>());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener pedidos completados: {ex.Message}");
                return StatusCode(500, $"Error al obtener pedidos completados: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarTiempoPedido([FromForm] string pedidoId, [FromForm] int tiempoPreparacion)
        {
          // Validación básica
          if (string.IsNullOrEmpty(pedidoId) || tiempoPreparacion <= 0)
          {
            return BadRequest("Datos inválidos.");
          }

          const string mutation = @"
        mutation ActualizarTiempoPedido($actualizarTiempoPedidoId: ID!, $tiempoPreparacion: Int!) {
            actualizarTiempoPedido(id: $actualizarTiempoPedidoId, tiempoPreparacion: $tiempoPreparacion) {
                id
            }
        }";

          var variables = new
          {
            actualizarTiempoPedidoId = pedidoId,
            tiempoPreparacion = tiempoPreparacion
          };

          try
          {
            var token = HttpContext.Session.GetString("AuthToken");
            // Ejecutamos la mutación utilizando el servicio GraphQL
            var response = await _graphQLService.ExecuteQueryAsync<Pedido>(mutation, variables,token);

            Console.WriteLine(response);
            // No necesitamos la respuesta completa, solo saber si se realizó correctamente
            if (response != null)
            {
              // Si la respuesta no es nula, consideramos que se realizó correctamente
              TempData["SuccessMessage"] = "Tiempo de preparación actualizado correctamente.";
            }
            else
            {
              TempData["ErrorMessage"] = "Hubo un problema al actualizar el tiempo de preparación.";
            }
          }
          catch (Exception ex)
          {
            // Manejo de errores
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
          }

          // Redirigimos a la misma página para reflejar el cambio
          return RedirectToAction("Completados");  // O la acción que desees redirigir
        }


        [HttpPost]
        public async Task<IActionResult> CambiarEstadoPedidoListo([FromForm] string pedidoId )
        {
          // Validación básica
          if (string.IsNullOrEmpty(pedidoId))
          {
            return BadRequest("Datos inválidos.");
          }

          const string mutation = @"
            mutation Mutation($actualizarEstadoPedidoId: ID!, $estado: String!) {
            actualizarEstadoPedido(id: $actualizarEstadoPedidoId, estado: $estado) {
              id
            }
          }";

          var variables = new
          {
            actualizarEstadoPedidoId = pedidoId,
            estado = "listo"
          };

          try
          {
            var token = HttpContext.Session.GetString("AuthToken");
            // Ejecutamos la mutación utilizando el servicio GraphQL
            var response = await _graphQLService.ExecuteQueryAsync<Pedido>(mutation, variables,token);

            Console.WriteLine(response);
            // No necesitamos la respuesta completa, solo saber si se realizó correctamente
            if (response != null)
            {
              // Si la respuesta no es nula, consideramos que se realizó correctamente
              TempData["SuccessMessage"] = "Tiempo de preparación actualizado correctamente.";
            }
            else
            {
              TempData["ErrorMessage"] = "Hubo un problema al actualizar el tiempo de preparación.";
            }
          }
          catch (Exception ex)
          {
            // Manejo de errores
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
          }

          return RedirectToAction("Completados");
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstadoPedidoEntregado([FromForm] string pedidoId )
        {
          // Validación básica
          if (string.IsNullOrEmpty(pedidoId))
          {
            return BadRequest("Datos inválidos.");
          }

          const string mutation = @"
            mutation Mutation($actualizarEstadoPedidoId: ID!, $estado: String!) {
            actualizarEstadoPedido(id: $actualizarEstadoPedidoId, estado: $estado) {
              id
            }
          }";

          var variables = new
          {
            actualizarEstadoPedidoId = pedidoId,
            estado = "entregado"
          };

          try
          {
            var token = HttpContext.Session.GetString("AuthToken");
            // Ejecutamos la mutación utilizando el servicio GraphQL
            var response = await _graphQLService.ExecuteQueryAsync<Pedido>(mutation, variables,token);

            Console.WriteLine(response);
            // No necesitamos la respuesta completa, solo saber si se realizó correctamente
            if (response != null)
            {
              // Si la respuesta no es nula, consideramos que se realizó correctamente
              TempData["SuccessMessage"] = "Tiempo de preparación actualizado correctamente.";
            }
            else
            {
              TempData["ErrorMessage"] = "Hubo un problema al actualizar el tiempo de preparación.";
            }
          }
          catch (Exception ex)
          {
            // Manejo de errores
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
          }

          return RedirectToAction("Completados");
        }

        [HttpPost]
        public async Task<IActionResult> AsignarMonto(string pedidoId, decimal monto)
        {
          if (string.IsNullOrEmpty(pedidoId) || monto <= 0)
          {
            TempData["ErrorMessage"] = "Datos inválidos.";
            return RedirectToAction("Pendientes");
          }

          const string mutation = @"
          mutation NuevaNotaDeVenta($input: NotaDeVentaInput) {
            nuevaNotaDeVenta(input: $input) {
              id
              idPedido
              metodo
              montoTotal
              montoPagado
              cambio
              fechaPago
            }
          }";
          var variables = new
          {
            input = new {
            idPedido = pedidoId,
            metodo = "efectivo",
            montoPagado = monto
           }
          };

          try
          {
            var token = HttpContext.Session.GetString("AuthToken");
            // Ejecutamos la mutación utilizando el servicio GraphQL
            var response = await _graphQLService.ExecuteQueryAsync<Data>(mutation, variables, token);

            // Verificar que la respuesta contiene los datos correctos
            if (response?.nuevaNotaDeVenta != null)
            {
              TempData["SuccessMessage"] = "Nota de venta generada correctamente.";

              var notaVentaJson = JsonConvert.SerializeObject(response.nuevaNotaDeVenta);
              Console.WriteLine(notaVentaJson);
              TempData["NotaVenta"] = notaVentaJson;
            }
            else
            {
              Console.WriteLine("ooooooooo");
              TempData["ErrorMessage"] = "Hubo un problema al generar la nota de venta.";
            }
          }
          catch (Exception ex)
          {
            // Manejo de errores
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
          }

          return RedirectToAction("Pendientes");
        }

    }
}
