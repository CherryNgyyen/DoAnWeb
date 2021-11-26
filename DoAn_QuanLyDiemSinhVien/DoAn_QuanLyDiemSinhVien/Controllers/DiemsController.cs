using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using DoAn_QuanLyDiemSinhVien.Models;
using LinqToExcel;

namespace DoAn_QuanLyDiemSinhVien.Controllers
{
    public class DiemsController : Controller
    {
        private QLDiemSinhVienEntities2 db = new QLDiemSinhVienEntities2();

        // GET: Diems
        public ActionResult Index()
        {
            var diem = db.Diem.Include(d => d.MonHoc).Include(d => d.SinhVien);
            return View(diem.ToList());
        }

        // GET: Diems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Diem diem = db.Diem.Find(id);
            if (diem == null)
            {
                return HttpNotFound();
            }
            return View(diem);
        }

        // GET: Diems/Create
        public ActionResult Create()
        {
            ViewBag.MaMH = new SelectList(db.MonHoc, "MaMH", "TenMH");
            ViewBag.MaSV = new SelectList(db.SinhVien, "MaSV", "HoTen");
            return View();
        }

        // POST: Diems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,MaSV,MaMH,DiemSV")] Diem diem)
        {
            if (ModelState.IsValid)
            {
                db.Diem.Add(diem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaMH = new SelectList(db.MonHoc, "MaMH", "TenMH", diem.MaMH);
            ViewBag.MaSV = new SelectList(db.SinhVien, "MaSV", "HoTen", diem.MaSV);
            return View(diem);
        }

        // GET: Diems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Diem diem = db.Diem.Find(id);
            if (diem == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaMH = new SelectList(db.MonHoc, "MaMH", "TenMH", diem.MaMH);
            ViewBag.MaSV = new SelectList(db.SinhVien, "MaSV", "HoTen", diem.MaSV);
            return View(diem);
        }

        // POST: Diems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,MaSV,MaMH,DiemSV")] Diem diem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(diem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaMH = new SelectList(db.MonHoc, "MaMH", "TenMH", diem.MaMH);
            ViewBag.MaSV = new SelectList(db.SinhVien, "MaSV", "HoTen", diem.MaSV);
            return View(diem);
        }

        // GET: Diems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Diem diem = db.Diem.Find(id);
            if (diem == null)
            { 
                return HttpNotFound();
            }
            return View(diem);
        }

        // POST: Diems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Diem diem = db.Diem.Find(id);
            db.Diem.Remove(diem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public FileResult DownloadExcel()
        {
            string path = "/Doc/DiemThi.xlsx";
            return File(path, "application/vnd.ms-excel", "DiemThi.xlsx");
        }

        [HttpGet]
        public ActionResult UploadExcel()
        {
            return View();
        }

        [HttpPost]
        public JsonResult UploadExcel(HttpPostedFileBase FileUpload)
        {
            List<string> data = new List<string>();
            if (FileUpload != null)
            {
                // tdata.ExecuteCommand("truncate table OtherCompanyAssets");
                if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string filename = FileUpload.FileName;
                    string targetpath = Server.MapPath("~/Doc/");
                    FileUpload.SaveAs(targetpath + filename);
                    string pathToExcelFile = targetpath + filename;
                    var connectionString = "";
                    if (filename.EndsWith(".xls"))
                    {
                        connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                    }
                    else if (filename.EndsWith(".xlsx"))
                    {
                        connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                    }

                    var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
                    var ds = new DataSet();
                    adapter.Fill(ds, "ExcelTable");
                    DataTable dtable = ds.Tables["ExcelTable"];
                    string sheetName = "Sheet1";
                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var artistAlbums = from a in excelFile.Worksheet<DiemThi>(sheetName) select a;

                    //Lặp tất cả
                    foreach (var a in artistAlbums)
                    {
                        try
                        {
                            if (a.MaSV != "" && a.MaMH != "" && a.DiemSV != -1)
                            {
                                Diem diemthi = new Diem();
                                diemthi.MaSV = a.MaSV;
                                diemthi.MaMH = a.MaMH;
                                diemthi.DiemSV = a.DiemSV;
                                db.Diem.Add(diemthi);
                                db.SaveChanges();
                            }
                            else
                            {
                                data.Add("<ul>");
                                if (a.MaSV == "" || a.MaSV == null) data.Add("<li> Mã sinh viên </li>");
                                if (a.MaMH == "" || a.MaMH == null) data.Add("<li> Mã môn học </li>");
                                if (a.DiemSV == -1 || a.DiemSV == -2) data.Add("<li>Yêu cầu nhập điểm</li>");
                                data.Add("</ul>");
                                data.ToArray();
                                return Json(data, JsonRequestBehavior.AllowGet);
                            }
                        }
                        catch (DbEntityValidationException ex)
                        {
                            foreach (var entityValidationErrors in ex.EntityValidationErrors)
                            {
                                foreach (var validationError in entityValidationErrors.ValidationErrors)
                                {
                                    Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                                }
                            }
                        }
                    }
                    //deleting excel file from folder
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //alert message for invalid file format
                    data.Add("<ul>");
                    data.Add("<li>Only Excel file format is allowed</li>");
                    data.Add("</ul>");
                    data.ToArray();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                data.Add("<ul>");
                if (FileUpload == null) data.Add("<li>Please choose Excel file</li>");
                data.Add("</ul>");
                data.ToArray();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult TestExportExcel()
        {
            //var model = db.Diem.Include(d => d.MonHoc).Include(d => d.SinhVien).OrderByDescending(d => d.MaSV).ToList();
            //return View("View", model);
            return View();
        }

        [HttpPost]
        public ActionResult ExportToExcel()
        {
            var gridview = new GridView();
            gridview.DataSource = (from sv in db.SinhVien
                                  from monhoc in db.MonHoc
                                  from diem in db.Diem
                                  where sv.MaSV == diem.MaSV
                                  where diem.MaMH == monhoc.MaMH
                                  select new DiemThi()
                                  {
                                      ID = diem.ID,
                                      TenMonHoc = monhoc.TenMH,
                                      TenSinhVien = sv.HoTen,
                                      DiemSV = diem.DiemSV
                                  }).ToList();
            gridview.DataBind();
            Response.Clear();
            Response.Buffer = true;
            //Response.AddHeader("content-disposition",
            // "attachment;filename=GridViewExport.xls");
            Response.Charset = "utf-8";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=GridViewExport.xls");
            //Mã hóa chữ sang UTF8
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());

            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);

            for (int i = 0; i < gridview.Rows.Count; i++)
            {
                //Apply text style to each Row
                gridview.Rows[i].Attributes.Add("class", "textmode");
            }

            //Add màu nền cho header của file excel
            gridview.HeaderRow.BackColor = System.Drawing.Color.DarkBlue;
            //Màu chữ cho header của file excel
            gridview.HeaderStyle.ForeColor = System.Drawing.Color.White;

            gridview.HeaderRow.Cells[0].Text = "ID";
            gridview.HeaderRow.Cells[1].Text = "Tên sinh viên";
            gridview.HeaderRow.Cells[2].Text = "Tên môn học";
            gridview.HeaderRow.Cells[3].Text = "Điểm";
            gridview.RenderControl(hw);

            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
            var model = db.Diem.Include(d => d.MonHoc).Include(d => d.SinhVien).OrderBy(d => d.MaSV).ToList();
            return View("View", model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
