using Entidades;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Datos
{
    public static class DbSeeder
    {
        public static void Inicializar(AppDbContext context)
        {
           
            if (context.Usuarios.Any())
            {
                return; 
            }

            
            var admin = new Usuario
            {
                NombreUsuario = "admin",
                NombreCompleto = "Administrador",
                Rol = "Admin",
                Activo = true,
                
                Password = BCrypt.Net.BCrypt.HashPassword("123")
            };
            context.SaveChanges();

            context.Usuarios.Add(admin);
            var categoriasNombres = new List<string> { "Bebidas", "Almacén", "Golosinas", "Lácteos", "Limpieza", "Perfumería", "Fiambrería" };

            foreach (var nombre in categoriasNombres)
            {
                if (!context.Categorias.Any(c => c.Nombre == nombre))
                {
                    context.Categorias.Add(new Categoria { Nombre = nombre, Activo = true });
                }
            }
            context.SaveChanges();

            // -----------------------------------------------------------
            // GENERADOR DE 100 PRODUCTOS
            // -----------------------------------------------------------
            if (!context.Productos.Any())
            {
                var random = new Random();
                var productosGenerados = new List<Producto>();

               
                var cats = context.Categorias.ToDictionary(c => c.Nombre, c => c.Id);
                int idUnidad = 1; 
                int idKilo = 2;   

               
                var datosSemilla = new[]
                {
                    new { Cat = "Bebidas",    Unidad = idUnidad, Nombres = new[] { "Coca Cola", "Sprite", "Fanta", "Manaos", "Pepsi", "Agua Villavicencio", "Cerveza Quilmes", "Vino Toro", "Jugo Tang", "Gatorade" } },
                    new { Cat = "Almacén",    Unidad = idUnidad, Nombres = new[] { "Arroz Gallo", "Fideos Matarazzo", "Harina Cañuelas", "Aceite Natura", "Puré de Tomate", "Sal Dos Anclas", "Yerba Playadito", "Azúcar Ledesma", "Polenta Presto", "Atún La Campagnola" } },
                    new { Cat = "Golosinas",  Unidad = idUnidad, Nombres = new[] { "Alfajor Guaymallén", "Chicle Beldent", "Caramelos Sugus", "Chocolate Cofler", "Turrón Arcor", "Galletitas Oreo", "Pepitos", "Merengadas", "Rhodesia", "Tita" } },
                    new { Cat = "Lácteos",    Unidad = idUnidad, Nombres = new[] { "Leche La Serenísima", "Yogur Bebible", "Manteca Sancor", "Dulce de Leche", "Queso Crema", "Postrecito Serenito" } },
                    new { Cat = "Limpieza",   Unidad = idUnidad, Nombres = new[] { "Lavandina Ayudín", "Detergente Ala", "Jabón en Polvo", "Desodorante Poett", "Esponja Virulana", "Rollo de Cocina" } },
                    new { Cat = "Perfumería", Unidad = idUnidad, Nombres = new[] { "Shampoo Plusbelle", "Jabón Dove", "Pasta Dental Colgate", "Desodorante Axe", "Crema Nivea", "Papel Higiénico" } },
                    new { Cat = "Fiambrería", Unidad = idKilo,   Nombres = new[] { "Jamón Cocido", "Queso Tybo", "Salame Milán", "Mortadela", "Queso Roquefort", "Bondiola" } } // Estos van por Kilo
                };

                // Bucle para crear exactamente 100 productos
                for (int i = 0; i < 100; i++)
                {
                   
                    var grupo = datosSemilla[random.Next(datosSemilla.Length)];

                    
                    string nombreBase = grupo.Nombres[random.Next(grupo.Nombres.Length)];
                    string[] variedades = { "Clásico", "Light", "Extra", "Grande", "Chico", "Especial", "Premium" };
                    string variedad = variedades[random.Next(variedades.Length)];

                   
                    decimal costo = random.Next(500, 5000); // Costo entre $500 y $5000
                    decimal venta = costo * (decimal)(1.30 + (random.NextDouble() * 0.40)); // Margen entre 30% y 70%

                    productosGenerados.Add(new Producto
                    {
                        CodigoBarras = "779" + random.Next(100000000, 999999999).ToString(),
                        Nombre = $"{nombreBase} {variedad}",
                        PrecioCompra = Math.Round(costo, 2),
                        PrecioVenta = Math.Round(venta, 2),
                        Stock = random.Next(0, 50), 
                        StockMinimo = 5,
                        CategoriaId = cats.ContainsKey(grupo.Cat) ? cats[grupo.Cat] : cats.Values.First(),
                        UnidadMedidaId = grupo.Unidad,
                        Activo = true
                    });
                }

                context.Productos.AddRange(productosGenerados);
                context.SaveChanges();
            }
        }
    }
}
