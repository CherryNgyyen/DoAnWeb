using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAn_QuanLyDiemSinhVien.Models
{
    public class DiemThi
    {
        public int ID { get; set; }
        public string TenSinhVien { get; set; }
        public string TenMonHoc { get; set; }
        public double? DiemSV { get; set; }
        public string MaSV { get; set; }
        public string MaMH { get; set; }
    }
}