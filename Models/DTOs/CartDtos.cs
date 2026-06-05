using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace untitled1.Models.DTOs
{
    public class AddToCartDto
    {
        [Required(ErrorMessage = "Mã gói dịch vụ (PlanId) là bắt buộc.")]
        public int PlanId { get; set; }
    }

    public class UpdateQuantityDto
    {
        [Range(1, 100, ErrorMessage = "Số lượng phải nằm trong khoảng từ 1 đến 100.")]
        public int Quantity { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public object? Errors { get; set; }

        public static ApiResponse<T> SuccessResponse(T? data = default, string message = "Thao tác thành công.")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, object? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }

    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal TotalAmount { get; set; }
        public int TotalQuantity { get; set; }
        public UserInfoDto? User { get; set; }
    }

    public class CartItemDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string AccentColor { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
    }

    public class UserInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsPremium { get; set; }
    }
}
