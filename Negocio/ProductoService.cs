using Datos;
using Entidades;
using Microsoft.EntityFrameworkCore;
using Negocio.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio
{
    public class ProductoService
    {
        private readonly AppDbContext _context;

        public ProductoService()
        {
            _context = new AppDbContext();
        }

        // 1. Obtener lista completa (con relaciones para ver nombres de Categoria/Unidad)
        
        public List<ProductoDto> GetAll()
        {
            // 1. Traemos los datos crudos de la BD
            var productos = _context.Productos
                            .Include(p => p.Categoria)
                            .Include(p => p.UnidadMedida)
                            .Where(p => p.Activo)
                            .ToList();

            // 2. Convertimos a DTO y aplicamos la LÓGICA DE NEGOCIO aquí
            var listaDto = new List<ProductoDto>();

            foreach (var p in productos)
            {
                var dto = new ProductoDto
                {
                    Id = p.Id,
                    CodigoBarras = p.CodigoBarras,
                    Nombre = p.Nombre,
                    Categoria = p.Categoria != null ? p.Categoria.Nombre : "Sin Cat.",
                    Unidad = p.UnidadMedida != null ? p.UnidadMedida.Abreviatura : "",
                    PrecioCompra = p.PrecioCompra,
                    PrecioVenta = p.PrecioVenta,
                    Stock = p.Stock
                };

                // --- LÓGICA DE NEGOCIO (Aquí es donde pertenece) ---

                // 1. Calcular Margen
                if (p.PrecioVenta > 0)
                    dto.MargenGanancia = (p.PrecioVenta - p.PrecioCompra) / p.PrecioVenta;
                else
                    dto.MargenGanancia = 0;

                // 2. Calcular Estado Stock
                if (p.Stock <= p.StockMinimo)
                    dto.EstadoStock = "STOCK BAJO";
                else if (p.Stock >= (p.StockMinimo * 3))
                    dto.EstadoStock = "STOCK ALTO";
                else
                    dto.EstadoStock = "STOCK MEDIO";

                listaDto.Add(dto);
            }

            return listaDto.OrderBy(x => x.Nombre).ToList();
        }

        public Producto GetById(int id)
        {
            // Buscamos el producto real para editarlo
            // No necesitamos .Include aquí porque los Combos de la ventana cargan sus propias listas
            return _context.Productos.FirstOrDefault(p => p.Id == id);
        }

        // 2. Buscar por código (para validaciones y ventas)
        public Producto GetByCodigo(string codigo)
        {
            return _context.Productos
                           .FirstOrDefault(p => p.CodigoBarras == codigo && p.Activo);
        }

        // 3. Guardar o Editar (Con Validaciones Profesionales)
        public void Guardar(Producto producto)
        {
            // Validaciones Básicas
            if (string.IsNullOrWhiteSpace(producto.CodigoBarras)) throw new Exception("El Código de Barras es obligatorio.");
            if (string.IsNullOrWhiteSpace(producto.Nombre)) throw new Exception("El Nombre es obligatorio.");
            if (producto.PrecioVenta <= 0) throw new Exception("El Precio de Venta debe ser mayor a 0.");
            if (producto.CategoriaId == null) throw new Exception("Debes seleccionar una Categoría.");
            if (producto.UnidadMedidaId == null) throw new Exception("Debes seleccionar una Unidad de Medida.");

            // Validación de Negocio: Precio vs Costo
            if (producto.PrecioVenta < producto.PrecioCompra)
            {
                // Podríamos lanzar error, o solo dejarlo pasar. Por ahora bloqueamos para evitar pérdidas.
                throw new Exception($"¡Cuidado! Estás vendiendo a ${producto.PrecioVenta} algo que compraste a ${producto.PrecioCompra}.");
            }

            // Validación de Duplicados (Código de Barras)
            var existe = _context.Productos
                                 .Any(p => p.CodigoBarras == producto.CodigoBarras
                                           && p.Id != producto.Id // Excluirse a sí mismo si es edición
                                           && p.Activo);
            if (existe)
            {
                throw new Exception($"El código '{producto.CodigoBarras}' ya existe en otro producto.");
            }

            if (producto.Id == 0)
            {
                _context.Productos.Add(producto); // Nuevo
            }
            else
            {
                _context.Productos.Update(producto); // Editar
            }
            _context.SaveChanges();
        }

        // 4. Eliminar (Soft Delete)
        public void Delete(int id)
        {
            var prod = _context.Productos.Find(id);
            if (prod != null)
            {
                prod.Activo = false; // Lo ocultamos, no lo borramos
                _context.Productos.Update(prod);
                _context.SaveChanges();
            }
        }

        // 5. Búsqueda para el Buscador de Ventas (Nombre o Código)
        public List<Producto> Search(string texto)
        {
            return _context.Productos
                           .Where(p => p.Activo &&
                                      (p.Nombre.Contains(texto) || p.CodigoBarras.Contains(texto)))
                           .Take(20) // Limitamos a 20 para no saturar
                           .ToList();
        }
    }
}
