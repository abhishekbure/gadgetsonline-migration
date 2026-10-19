using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GadgetsOnline.Models
{
    public class Order
    {
        [ScaffoldColumn(false)]
        [BindNever]
        [ValidateNever]
        public int OrderId { get; set; }

        [ScaffoldColumn(false)]
        [BindNever]
        [ValidateNever]
        public System.DateTime OrderDate { get; set; }

        [ScaffoldColumn(false)]
        [BindNever]
        [ValidateNever]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "First Name is required")]
        [DisplayName("First Name")]
        [StringLength(160)]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last Name is required")]
        [DisplayName("Last Name")]
        [StringLength(160)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(70)]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "City is required")]
        [StringLength(40)]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "State is required")]
        [StringLength(40)]
        public string State { get; set; } = null!;

        [Required(ErrorMessage = "Postal Code is required")]
        [DisplayName("Postal Code")]
        [StringLength(10)]
        public string PostalCode { get; set; } = null!;

        [Required(ErrorMessage = "Country is required")]
        [StringLength(40)]
        public string Country { get; set; } = null!;

        [Required(ErrorMessage = "Phone is required")]
        [StringLength(24)]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Email Address is required")]
        [DisplayName("Email Address")]
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;

        [ScaffoldColumn(false)]
        [BindNever]
        [ValidateNever]
        public decimal Total { get; set; }

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
