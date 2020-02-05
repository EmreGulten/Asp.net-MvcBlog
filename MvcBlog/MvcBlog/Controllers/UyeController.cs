using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using MvcBlog.Models;

namespace MvcBlog.Controllers
{
    public class UyeController : Controller
    {
        MvcBlogDb db = new MvcBlogDb();
        // GET: Uye
        public ActionResult Index(int? id)
        {
            var uyes = db.Uyes.Where(x => x.UyeId == id).SingleOrDefault();
            if (Convert.ToInt32(Session["UyeId"]) != uyes.UyeId)
            {
                return HttpNotFound();
            }

            return View(uyes);
        }

        public ActionResult Login()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Login(Uye uye, string Sifre)
        {
            var m5pass = Crypto.Hash(Sifre, "MD5");
            var login = db.Uyes.Where(x => x.KullaniciAdi == uye.KullaniciAdi).SingleOrDefault();
            if (login.KullaniciAdi == uye.KullaniciAdi && login.Email == uye.Email && login.Sifre == m5pass)
            {
                Session["uyeid"] = login.UyeId;
                Session["kullaniciadi"] = login.KullaniciAdi;
                Session["yetkiid"] = login.YetkiId;

               

                return RedirectToAction("Index", "Home");


            }
            else
            {
                ViewBag.Uyari = "Kullanıcı Adı yada Şifre Yanlış";
                return View();
            }


        }


        public ActionResult Logout()
        {
            Session["uyeid"] = null;
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }


        public ActionResult Create()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Create(Uye uye, HttpPostedFileBase Foto, string Sifre)
        {
            var md5pass = Sifre;
            try
            {
                if (Foto != null)
                {
                    WebImage img = new WebImage(Foto.InputStream);
                    FileInfo fotoinfo = new FileInfo(Foto.FileName);

                    string newfoto = Guid.NewGuid().ToString() + fotoinfo.Extension;
                    img.Resize(150, 150);
                    img.Save("~/Upload/UyeFoto/" + newfoto);
                    uye.Foto = "/Upload/UyeFoto/" + newfoto;
                    uye.YetkiId = 2;

                    uye.Sifre = Crypto.Hash(md5pass, "MD5");
                    db.Uyes.Add(uye);
                    db.SaveChanges();
                    Session["uyeid"] = uye.UyeId;
                    Session["kullaniciadi"] = uye.KullaniciAdi;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("Fotoğraf", "Fotoğraf Seçiniz");
                }
            }
            catch (DbEntityValidationException dbValEx) // Hatanın tam olarak ne olduğunu getirir
            {
                var outputLines = new StringBuilder();
                foreach (var eve in dbValEx.EntityValidationErrors)
                {
                    outputLines.AppendFormat("{0}: Entity of type {1} in state {2} has the following validation errors:", DateTime.Now, eve.Entry.Entity.GetType().Name, eve.Entry);

                    foreach (var ve in eve.ValidationErrors)
                    {
                        outputLines.AppendFormat("- Property: {0}, Error: {1}"
                            , ve.PropertyName, ve.ErrorMessage);
                    }
                }

                //Tools.Notify(this, outputLines.ToString(),"error");
                throw new DbEntityValidationException(string.Format("Validation errorsrn {0} aaa {1}", outputLines.ToString()), dbValEx);

            }


            return View(uye);
        }

        public ActionResult Edit(int? id)
        {

            var uye2 = db.Uyes.FirstOrDefault(x => x.UyeId == id);
            var uye = db.Uyes.Where(x => x.UyeId == id).SingleOrDefault();

            if (Convert.ToInt32(Session["uyeid"]) != uye.UyeId)
            {
                return HttpNotFound();
            }

            return View(uye);
        }

        [HttpPost]
        public ActionResult Edit(Uye uye, int id, string Sifre, HttpPostedFileBase Foto)
        {
            if (ModelState.IsValid)
            {
                var md5pass = Sifre;
                var uyes = db.Uyes.Where(x => x.UyeId == id).SingleOrDefault();

                if (Foto != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(uyes.Foto)))
                    {
                        System.IO.File.Delete(Server.MapPath(uyes.Foto));
                    }
                    WebImage img = new WebImage(Foto.InputStream);
                    FileInfo fotoInfo = new FileInfo(Foto.FileName);

                    string newfoto = Guid.NewGuid().ToString() + fotoInfo.Extension;
                    img.Resize(150, 150);
                    img.Save("~/Upload/UyeFoto/" + newfoto);
                    uyes.Foto = "/Upload/UyeFoto/" + newfoto;
                }

                uyes.AdSoyad = uye.AdSoyad;
                uyes.KullaniciAdi = uye.KullaniciAdi;
                uyes.Sifre = Crypto.Hash(md5pass, "MD5");
                uyes.Email = uye.Email;
                db.SaveChanges();
                Session["kullaniciadi"] = uye.KullaniciAdi;
                return RedirectToAction("Index", "Home", new { id = uyes.UyeId });

            }


            return View();
        }

        public ActionResult UyeProfil(int id)
        {
            var uye = db.Uyes.Where(x => x.UyeId == id).SingleOrDefault();
            return View(uye);
        }

        
        #region ŞifreSifirla




        //public ActionResult SifreSifirla(string id)
        //{
        //    using (db)
        //    {
        //        var user = db.Uyes.Where(x => x.SifreSifirla == id).FirstOrDefault();
        //        if (user != null)
        //        {
        //            ResetPasswordModel model = new ResetPasswordModel();
        //            model.ResetCode = id;
        //            return View(model);
        //        }
        //        else
        //        {
        //            return HttpNotFound();
        //        }
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult SifreSifirla(ResetPasswordModel model)
        //{
        //    var message = "";
        //    if (ModelState.IsValid)
        //    {
        //        using (db)
        //        {
        //            var user = db.Uyes.Where(x => x.SifreSifirla == model.ResetCode).FirstOrDefault();
        //            if (user !=null)
        //            {
        //                user.Sifre = Crypto.Hash(model.NewPassword);
        //                user.SifreSifirla = "";
        //                db.Configuration.ValidateOnSaveEnabled = false;
        //                db.SaveChanges();
        //                message = "new Passord updated successfully";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        message = "Something invalid";
        //    }
        //    ViewBag.Message = message;
        //    return View(model);
        //}


        #endregion

        public ActionResult SifreDegis(int? id)
        {
            var uyes = db.Uyes.Where(x => x.UyeId == id).SingleOrDefault();

            if (Convert.ToInt32(Session["uyeid"]) != uyes.UyeId)
            {
                return HttpNotFound();
            }

            ViewBag.Hata = "Üye bulunamadı";

            return View(uyes);
        }

        [HttpPost]
        public ActionResult SifreDegis(Uye uye, int id, string Sifre)
        {
            var user = db.Uyes.Where(x => x.UyeId ==id).FirstOrDefault();

            if (user.UyeId == id)
            {
                if (ModelState.IsValid)
                {
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    ViewBag.Basari = "İşlem Başarılı";
                    return RedirectToAction("Index", "Home");
                }
            }

           
            return View(user);
        }

        #region mailgonder
        //[NonAction]
        //public void SendVerificationLinkEmail(string emailID, string activationCode, string emailFor = "VerifyAccount")
        //{
        //    var verifyUrl = "/User/" + emailFor + "/" + activationCode;
        //    var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

        //    var fromEmail = new MailAddress("ukrf4tal0035@gmail.com", "Dotnet Awesome");
        //    var toEmail = new MailAddress(emailID);
        //    var fromEmailPassword = "emre886179++"; // Replace with actual password

        //    string subject = "";
        //    string body = "";
        //    if (emailFor == "VerifyAccount")
        //    {
        //        subject = "Your account is successfully created!";
        //        body = "aa";
        //        //"<br/><br/>We are excited to tell you that your Dotnet Awesome account is" +
        //        //   " successfully created. Please click on the below link to verify your account" +
        //        //   " <br/><br/><a href='" + link + "'>" + link + "</a> ";
        //    }
        //    else if (emailFor == "ResetPassword")
        //    {
        //        subject = "Reset Password";
        //        body = "aa";
        //        //"Hi,<br/>br/>We got request for reset your account password. Please click on the below link to reset your password" +
        //        //       "<br/><br/><a href=" + link + ">Reset Password link</a>";
        //    }


        //    var smtp = new SmtpClient
        //    {
        //        Host = "smtp.gmail.com",
        //        Port = 587,
        //        EnableSsl = true,
        //        DeliveryMethod = SmtpDeliveryMethod.Network,
        //        UseDefaultCredentials = false,
        //        Credentials = new NetworkCredential("ukrf4tal0035@gmail.com", "emre886179++")
        //    };

        //    using (var message = new MailMessage(fromEmail, toEmail)
        //    {
        //        Subject = subject,
        //        Body = body,
        //        IsBodyHtml = true
        //    })
        //        smtp.Send(message);
        //}

        #endregion

    }
}