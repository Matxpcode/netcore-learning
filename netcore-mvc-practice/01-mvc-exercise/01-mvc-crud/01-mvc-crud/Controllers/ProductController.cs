using _01_mvc_crud.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;

namespace _01_mvc_crud.Controllers
{
    public class ProductController : Controller
    {
        //1.Creamos el atributo de conexion (unico)
        private readonly string _conexion;

        //2.Constructor para injection dependency el cual implementa a Iconfiguration 
        //que detecta el appsetting.json con su clave "conexion"
        /*
         Explicacion detallada del "controller" y "Inyeccion de dependencia":
        Cuando agregamos un parametro al controlador, c# detecta que le solicitas "algo", 
        asi que busca ese "algo" en su "contenedor de servicios" lo encuentra y lo inyecta
        a su instancia del parametro(en nuestro caso "configuration"), para posteriormente
        igualarlo al atributo privado "_conexion", el cual al ser "global" podra ser accedido
        en todos los metodos del controller, el cual tambien podra usarse para diferentes casos
        como DAO, entre otros, recordad que debe el controller estar limpio.(.net simplifica más)
         */

        public ProductController(IConfiguration configuration)
        {
            _conexion = configuration.GetConnectionString("conexion"); //Recibe solo las credenciales
        }


        //METODOS DE PRODUCTO
        //3.Metodo para "listar-products-porNombre"
        private List<Product> getProducts(string nombre)
        {
            List<Product> productos = new List<Product>();

            //Se reaiza recien la conexion con la BD, gracias a las credenciales = _conexion
            using (SqlConnection con = new SqlConnection(_conexion))
            {
                //Encargado de las operacion con la BD(PreparedStatement) -> "query" y "conexion"
                using (SqlCommand cmd = new SqlCommand("sp_list_products_by_name", con))
                {
                    //Indicamos el tipo de comando(query)
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    //Opcional, si tienes parametros -> "Parametro de BD" y "Parametro de metodo"
                    cmd.Parameters.AddWithValue("@ProductName", nombre);

                    //Recien abrimos la conexion, antes de los parametros
                    con.Open();

                    //Encargado de leer los datos, "SOLO DE LECTURA" NO MODIFICA A LA BD
                    //Usado para "listar", "filtrar","buscar", etc.
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        //Si son "varios objetos"(filas en BD) usas "bucle" sino una "condicional"
                        while (dr.Read()) //Usamos .Read() para avanzar de fila en fila (.next())
                        {
                            //Llenamos los datos que recopilo el "dr" en el objecto producto "p"
                            Product p = new Product
                            {
                                ProductID = dr.GetInt32(0),
                                ProductName = dr.GetString(1),
                                UnitPrice = dr.GetDecimal(2),
                                UnitsInStock = dr.GetInt16(3),
                                CompanyName = dr.GetString(4),
                                CategoryName = dr.GetString(5)
                            };
                            productos.Add(p);
                        }
                    }
                }
            }
            return productos;
        }

        //Metodo listar categorias (Dropdown)
        private List<Category> getCategory()
        {
            List<Category> categorias = new List<Category>();

            using (SqlConnection cn = new SqlConnection(_conexion))
            {
                using (SqlCommand cmd = new SqlCommand("sp_list_categories", cn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    //No hay parametros, pasamos a abrir la conexion
                    cn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Category category = new Category
                            {
                                CategoryID = dr.GetInt32(0),
                                CategoryName = dr.GetString(1),
                            };
                            categorias.Add(category);
                        }
                    }
                }
            }
            return categorias;
        }

        //Metodo listar suppliers (Dropdown)
        private List<Supplier> GetSuppliers()
        {
            List<Supplier> suppliers = new List<Supplier>();

            using (SqlConnection cn = new SqlConnection(_conexion))
            {
                using (SqlCommand cmd = new SqlCommand("sp_list_suppliers", cn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    //No hay parametros, proseguimos con abrir la conexion
                    cn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Supplier supplier = new Supplier
                            {
                                SupplierID = dr.GetInt32(0),
                                CompanyName = dr.GetString(1),
                            };
                            suppliers.Add(supplier);
                        }
                    }
                }
            }
            return suppliers;
        }

        //Metodo insercion productos (Transaction)
        private bool insertProduct(Product p)
        {
            using (SqlConnection cn = new SqlConnection(_conexion))
            {
                cn.Open();

                using (SqlTransaction ts = cn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_insert_product", cn,ts))
                        {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ProductName", p.ProductName);
                            cmd.Parameters.AddWithValue("@UnitPrice", p.UnitPrice);
                            cmd.Parameters.AddWithValue("@UnitsInStock", p.UnitsInStock);
                            cmd.Parameters.AddWithValue("@SupplierID", p.SupplierID);
                            cmd.Parameters.AddWithValue("@CategoryID", p.CategoryID);

                            int filasAfectadas = cmd.ExecuteNonQuery();

                            //Si todo sale bien, confirmamos los cambios a la BD
                            ts.Commit();
                            return filasAfectadas > 0;
                        }
                    }
                    catch (Exception)
                    {
                        //Si hay error, deshacemos todo para no corromper la BD
                        ts.Rollback();
                        return false;
                    }
                }
            }
        }

        //Metodo para buscar producto por id
        //Metodo actualizar producto (Transaction)
        //Metodo para eliminar producto (Transaction)


        //ACTION RESULT
        //4.Vista para "listar-categorias"
        public IActionResult ListProductName(string nombre = "")
        {
            return View(getProducts(nombre));
        }

        //Vista para mostrar formulario vacio (GET Create Categoria)
        public ActionResult Create()
        {
            ViewBag.Categories = new SelectList(getCategory(),"CategoryID","CategoryName");
            ViewBag.Suppliers = new SelectList(GetSuppliers(), "SupplierID", "CompanyName");
            return View(new Product());
        }

        //Vista para enviar los datos del formulario y guardarlos
        [HttpPost]
        public ActionResult Create(Product product)
        {
            //Validamos antes de procesar en la BD
            if (!ModelState.IsValid)
            {
                //Si hay error de validaciones, recargamos los dropdowns
                ViewBag.Categories = new SelectList(getCategory(), "CategoryID", "CategoryName");
                ViewBag.Suppliers = new SelectList(GetSuppliers(),"SupplierID","CompanyName");
                return View(product);
            }

            //Si pasa la validacion, intentamos insertar
            bool exito = insertProduct(product);
            if (exito)
            {
                //mensaje temporal q se muestra una sola vez despues de la redireccion
                TempData["Mensaje"] = "PRODUCTO REGISTRADO CORRECTAMENTE!!";
                return RedirectToAction("ListProductName");
            }
            else
            {
                ModelState.AddModelError("", "Error al registrar el producto en la BD");

                //Recargamos los dropdowns por si acaso
                ViewBag.Categories = new SelectList(getCategory(), "CategoryID", "CategoryName" );
                ViewBag.Suppliers = new SelectList(GetSuppliers(), "SupplierID", "CompanyName");
                return View(product);
            }

        }

    }
}
