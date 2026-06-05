using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace untitled1.Models.DTOs
{
    public class MovieListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Genre { get; set; } = string.Empty;
        public bool IsTVSeries { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
    }

    public class MovieDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Genre { get; set; } = string.Empty;
        public bool IsTVSeries { get; set; }
        public string Director { get; set; } = string.Empty;
        public string Cast { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TrailerUrl { get; set; } = string.Empty;
        public List<int> CategoryIds { get; set; } = new List<int>();
        public List<string> Categories { get; set; } = new List<string>();
    }

    public class CreateMovieDto
    {
        [Required(ErrorMessage = "Tiêu đề phim không được để trống.")]
        [StringLength(200, ErrorMessage = "Tiêu đề phim không được vượt quá 200 ký tự.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đường dẫn ảnh poster không được để trống.")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm phát hành là bắt buộc.")]
        [Range(1888, 2100, ErrorMessage = "Năm phát hành không hợp lệ (1888 - 2100).")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Thể loại không được để trống.")]
        [StringLength(100, ErrorMessage = "Thể loại không được vượt quá 100 ký tự.")]
        public string Genre { get; set; } = string.Empty;

        public bool IsTVSeries { get; set; }

        [StringLength(100, ErrorMessage = "Tên đạo diễn không được vượt quá 100 ký tự.")]
        public string? Director { get; set; }

        [StringLength(500, ErrorMessage = "Thông tin diễn viên không được vượt quá 500 ký tự.")]
        public string? Cast { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string? Description { get; set; }

        public string? TrailerUrl { get; set; }

        public List<int> CategoryIds { get; set; } = new List<int>();
    }

    public class UpdateMovieDto
    {
        [Required(ErrorMessage = "Mã phim là bắt buộc.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề phim không được để trống.")]
        [StringLength(200, ErrorMessage = "Tiêu đề phim không được vượt quá 200 ký tự.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đường dẫn ảnh poster không được để trống.")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm phát hành là bắt buộc.")]
        [Range(1888, 2100, ErrorMessage = "Năm phát hành không hợp lệ (1888 - 2100).")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Thể loại không được để trống.")]
        [StringLength(100, ErrorMessage = "Thể loại không được vượt quá 100 ký tự.")]
        public string Genre { get; set; } = string.Empty;

        public bool IsTVSeries { get; set; }

        [StringLength(100, ErrorMessage = "Tên đạo diễn không được vượt quá 100 ký tự.")]
        public string? Director { get; set; }

        [StringLength(500, ErrorMessage = "Thông tin diễn viên không được vượt quá 500 ký tự.")]
        public string? Cast { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string? Description { get; set; }

        public string? TrailerUrl { get; set; }

        public List<int> CategoryIds { get; set; } = new List<int>();
    }
}
