using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DoAn_QuanLyDiemSinhVien.Models;

namespace DoAn_QuanLyDiemSinhVien.Controllers
{
    public class QuanLyDiemController : Controller
    {
        QLDiemSinhVienEntities2 db = new QLDiemSinhVienEntities2();

        // GET: QuanLyDiem
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection col)
        {
            string email = col["Email"].ToString();
            string MatKhau = col["MatKhau"].ToString();
            if (ModelState.IsValid)
            {
                Account account = db.Account.Where(a => a.Email == email && a.MatKhau == MatKhau && a.TrangThai == 1).FirstOrDefault();
                if (account != null)
                {
                    //add session
                    Session["Email"] = account.Email;
                    Session["Admin"] = account.Admin;
                    Session["HoVaTen"] = account.HoVaTen;
                    if(account.Admin == 1)
                    {
                        Session["Admin"] = "admin";
                    }
                    else
                    {
                        Session["Admin"] = null;
                    }    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.error = "Đăng nhập thất bại!";
                    return RedirectToAction("Login");
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "QuanLyDiem");
        }

        public string RandomString(int size, bool lowerCase)
        {
            StringBuilder sb = new StringBuilder();
            char c;
            Random rand = new Random();
            for (int i = 0; i < size; i++)
            {
                c = Convert.ToChar(Convert.ToInt32(rand.Next(65, 87)));
                sb.Append(c);
            }
            if (lowerCase)
            {
                return sb.ToString().ToLower();
            }
            return sb.ToString();
        }

        public bool sendMail(string to, string subject, string body)
        {
            var Email = System.Configuration.ConfigurationManager.AppSettings["Gmail"].ToString();
            MailMessage obj = new MailMessage(Email, to);
            obj.Subject = subject;
            obj.Body = body;
            obj.IsBodyHtml = true;
            string Message = null;

            MailAddress bcc = new MailAddress("fcphanavn@gmail.com");
            obj.Bcc.Add(bcc);

            //attach file
            //HttpPostedFileBase file = Request.Files["file"];
            //if (file.ContentLength > 0)
            //{
            //    string fileName = System.IO.Path.GetFileName(file.FileName);
            //    obj.Attachments.Add(new System.Net.Mail.Attachment(file.InputStream, fileName));
            //}

            using (SmtpClient smtp = new SmtpClient())
            {
                var Password = System.Configuration.ConfigurationManager.AppSettings["PasswordGmail"].ToString();

                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential(Email, Password);
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;

                try
                {
                    smtp.Send(obj);
                    Message = "Gửi mail thành công";
                    return true;
                }
                catch (Exception ex)
                {
                    Message = "Err:" + ex.ToString();
                    return false;
                }
            }
        }

        [HttpGet]
        public ActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        public ActionResult QuenMatKhau(FormCollection col)
        {
            string email = col["Email"];
            string subject = "Thay đổi mật khẩu tài khoản quản lý điểm sinh viên";
            try
            {
                Account account = db.Account.Where(s => s.Email == email).FirstOrDefault();
                account.MatKhau = RandomString(8, true);
                db.SaveChanges();
                Account acc = db.Account.Where(s => s.Email == email).FirstOrDefault();
                string body = "Đây là mật khẩu của bạn sau khi yêu cầu thay đổi: " + acc.MatKhau + ", Vui lòng thay đổi mật khẩu sau khi đăng nhập lại!";
                sendMail(acc.Email, subject, body);
                ViewBag.tb = "Yêu cầu lấy lại mật khẩu thành công, vui lòng quay lại trang đăng nhập";
            }
            catch
            {
                ViewBag.tb = "Không thể yêu cầu lấy lại mật khẩu, vui lòng kiểm tra lại";
            }
            return View();
        }
        
        [HttpGet]
        public ActionResult DoiMatKhau()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DoiMatKhau(FormCollection col)
        {
            string MatKhauCu = col["MatKhauCu"];
            string MatKhauMoi = col["MatKhauMoi"];
            string XacNhanMK = col["XacNhanMatKhau"];
            string email = Session["Email"].ToString();
            try
            {
                Account account = db.Account.Where(s => s.Email == email && s.MatKhau == MatKhauCu).FirstOrDefault();
                account.MatKhau = MatKhauMoi;
                db.SaveChanges();
                ViewBag.tb = "Đã thay đổi mật khẩu thành công!";
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ViewBag.tb = "Không thể thay đổi mật khẩu!";
            }
            return View();
        }
    }
}