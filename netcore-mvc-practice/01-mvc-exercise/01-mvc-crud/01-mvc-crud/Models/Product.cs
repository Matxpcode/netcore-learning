using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace _01_mvc_crud.Models
{
    public class Product
    {
        [Key]
        [Display(Name ="Codigo Producto")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(80, ErrorMessage = "Maximo 80 caracteres")]
        [Display(Name ="Nombre del Producto")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage ="Debe seleccionar un proveedor")]
        [Display(Name ="Proveedor")]
        public int SupplierID { get; set; }

        [Required(ErrorMessage ="Debe seleccionar una categoria")]
        [Display(Name ="Categoria")]
        public int CategoryID { get; set; }

        //Range protege contra precios negativos o cero
        [Required(ErrorMessage ="El precio es obligatorio")]
        [Range(0.01,10000,ErrorMessage ="El precio debe estar entre 0.01 y 10 000")]
        [Display(Name ="Precio Unitario")]
        public decimal UnitPrice { get; set; }

        //Range protege contra stock negativo
        [Required(ErrorMessage ="El stock es obligatorio")]
        [Range(0,32767,ErrorMessage ="El stock no puede ser negativo")]
        [Display(Name ="Unidades en Stock")]
        public int UnitsInStock { get; set; }

        //Propiedades extra
        public string? CompanyName { get; set; }
        public string? CategoryName { get; set; }
    }
}
