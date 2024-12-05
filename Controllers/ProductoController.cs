using AspnetCoreMvcFull.Models.ViewModels;
using AspnetCoreMvcFull.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Controllers
{
    public class ProductoController : Controller
    {
        private readonly GraphQLService _graphQLService;

        public ProductoController(GraphQLService graphQLService)
        {
            _graphQLService = graphQLService;
        }

        // Acción para mostrar la vista de productos
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Definimos la consulta GraphQL para obtener productos
            const string query = @"
                query ObtenerProductosLocal($idLocal: ID!) {
                    obtenerProductosLocal(idLocal: $idLocal) {
                        id
                        nombre
                        descripcion
                        precio
                        stock
                        idLocal
                    }
                }";

            var variables = new
            {
              idLocal = UserSingleton.Instance.Id
            };

            // Llamamos al servicio GraphQL para obtener los productos
            var response = await _graphQLService.ExecuteQueryAsync<ObtenerProductosResponse>(query, variables, HttpContext.Session.GetString("AuthToken"));

            // Imprimir la respuesta de GraphQL en la consola
            Console.WriteLine("Respuesta completa de la consulta GraphQL:");
            var jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            Console.WriteLine(jsonResponse);

            return View(response.ObtenerProductosLocal);
            // Pasamos los productos a la vista
        }

        [HttpPost]
        public async Task<IActionResult> EliminarProducto(string id)
        {
          if (string.IsNullOrEmpty(id))
          {
            ModelState.AddModelError("", "El ID del producto no es válido.");
            return RedirectToAction("Index");
          }

          const string queryEliminarProducto = @"
        mutation EliminarProducto($id: ID!) {
            eliminarProducto(id: $id)
        }";

          var variables = new { id };

          try
          {
            // Ejecutar la consulta GraphQL para eliminar el producto
            var response = await _graphQLService.ExecuteQueryAsync<string>(queryEliminarProducto, variables, HttpContext.Session.GetString("AuthToken"));

            if (response != null)
            {
              return Json(new { success = true, message = "Producto eliminado correctamente." });
            }
            else
            {
              return Json(new { success = false, message = "Hubo un problema al eliminar el producto." });
            }
          }
          catch (Exception ex)
          {
            return Json(new { success = false, message = $"Error: {ex.Message}" });
          }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarProducto(string id, string nombre, string descripcion, decimal precio, int stock)
        {
          if (string.IsNullOrEmpty(id))
          {
            return BadRequest("El ID del producto no es válido.");
          }

          const string queryActualizarProducto = @"
        mutation ActualizarProducto($id: ID!, $input: ProductoInput) {
            actualizarProducto(id: $id, input: $input) {
                id
                nombre
                descripcion
                precio
                stock
            }
        }";

          var variables = new
          {
            id = id,
            input = new
            {
              nombre = nombre,
              descripcion = descripcion,
              precio = precio,
              stock = stock,
              idLocal = UserSingleton.Instance.Id
            }
          };

          try
          {
            var response = await _graphQLService.ExecuteQueryAsync<ProductoResponse>(queryActualizarProducto, variables, HttpContext.Session.GetString("AuthToken"));

            if (response?.ActualizarProducto != null)
            {
              return Json(new { success = true });
            }
            else
            {
              return Json(new { success = false, message = "Hubo un problema al actualizar el producto." });
            }
          }
          catch (Exception ex)
          {
            return Json(new { success = false, message = ex.Message });
          }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductoViewModel productoViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            const string query = @"
                mutation NuevoProducto($input: ProductoInput) {
                    nuevoProducto(input: $input) {
                        descripcion
                        id
                        idLocal
                        nombre
                        precio
                        stock
                    }
                }";

            var variables = new
            {
                input = new
                {
                    nombre = productoViewModel.Nombre,
                    descripcion = productoViewModel.Descripcion,
                    precio = productoViewModel.Precio,
                    stock = productoViewModel.Stock,
                    idLocal = UserSingleton.Instance.Id
                }
            };

            try
            {
                  var token = HttpContext.Session.GetString("AuthToken");

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("No se ha encontrado un token de autorización.");
                }

                var response = await _graphQLService.ExecuteQueryAsync<ProductoResponse>(query, variables, token);

                Console.WriteLine("Respuesta completa de la mutación GraphQL para crear producto:");
                var jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                Console.WriteLine(jsonResponse);

                if (response?.NuevoProducto != null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    Console.WriteLine("No se recibió el producto en la respuesta.");
                    ModelState.AddModelError("", "Hubo un problema al crear el producto.");
                    return View(productoViewModel);
                }
            }
            catch (Exception ex)
            {
                // Manejar errores
                Console.WriteLine($"Error al crear el producto: {ex.Message}");
                ModelState.AddModelError("", $"Error al crear el producto: {ex.Message}");
                return View(productoViewModel);
            }
        }
    }
}
