using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DoAn_QuanLyDiemSinhVien.Models;

namespace DoAn_QuanLyDiemSinhVien.Controllers
{
    public class TraCuuDiemController : Controller
    {
        QLDiemSinhVienEntities2 db = new QLDiemSinhVienEntities2();

        public ActionResult TraCuu()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TraCuu(FormCollection col)
        {
            string mssv = col["mssv"].ToString();
            DateTime NgaySinh = DateTime.Parse(col["NgaySinh"]);
            var list = db.SinhVien.Where(s => s.MaSV == mssv && s.NgaySinh == NgaySinh).Select(s => s).ToList();
            if (list == null)
            {
                ViewBag.Error = "Vui lòng kiểm lại lại thông tin chính xác!";
                ViewBag.list = null;
            }
            else
            {
                ViewBag.list = list;
                Session["SinhVien"] = 1;
            }
            return View();
        }

        public ActionResult ChiTiet(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var diem = db.Diem.Where(s => s.MaSV == id).Select(s => s).ToList();
            if (diem == null)
            {
                return HttpNotFound();
            }
            return View(diem);
        }
    }
}