using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using MvcBlog.Models;
using PagedList.Mvc;
using PagedList;


namespace MvcBlog.Controllers
{
    public class AdminMakaleController : Controller
    {
        MvcBlogDb db = new MvcBlogDb();

        // GET: AdminMakale
        public ActionResult Index(int Page=1)
        {
            //int sayfaNo = Page ?? 1;
            var makales = db.Makales.OrderByDescending(x=>x.MakaleId).ToPagedList(Page, 5);
            return View(makales);
        }

        // GET: AdminMakale/Details/5
        public ActionResult Details(int? id)
        {
            if (id==null)
            {
                return HttpNotFound();
            }

            Makale makale = db.Makales.Find(id);
            if (makale==null)
            {
                return HttpNotFound();
            }
           
            return View(makale);
        }

        // GET: AdminMakale/Create
        public ActionResult Create()
        {
            ViewBag.KategoriId = new SelectList(db.Kategoris, "KategoriID", "KategoriAd");
            return View();
        }

        // POST: AdminMakale/Create
        [HttpPost]
        public ActionResult Create(Makale makale, string etiket, HttpPostedFileBase Foto)
        {

            if (ModelState.IsValid)
            {
                if (Foto != null)
                {
                    WebImage img = new WebImage(Foto.InputStream);
                    FileInfo fotoInfo = new FileInfo(Foto.FileName);

                    string newfoto = Guid.NewGuid().ToString() + fotoInfo.Extension;
                    img.Resize(800, 350);
                    img.Save("~/Upload/MakaleFoto/" + newfoto);
                    makale.Foto = "/Upload/MakaleFoto/" + newfoto;
                    

                }

                if (etiket != null)
                {
                    string[] etiketdizi = etiket.Split(',');
                    foreach (var s in etiketdizi)
                    {
                        var yenietiket = new Etiket { EtiketAdi = s };
                        db.Etikets.Add(yenietiket);
                        makale.Etikets.Add(yenietiket);

                    }
                }

                makale.UyeId = Convert.ToInt32(Session["uyeid"]);
                makale.Okunma = 1;
                db.Makales.Add(makale);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(makale);

        }

        // GET: AdminMakale/Edit/5
        public ActionResult Edit(int id)
        {
            var makale = db.Makales.Where(x => x.MakaleId == id).SingleOrDefault();
            if (makale == null)
            {
                return HttpNotFound();
            }

            ViewBag.KategoriId = new SelectList(db.Kategoris, "KategoriID", "KategoriAd", makale.KategoriId);

            return View(makale);
        }

        // POST: AdminMakale/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Makale makale, HttpPostedFileBase Foto)
        {
            try
            {
                var makales = db.Makales.Where(x => x.MakaleId == id).SingleOrDefault();
                if (Foto != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(makales.Foto)))
                    {
                        System.IO.File.Delete(Server.MapPath(makales.Foto));
                    }
                    WebImage img = new WebImage(Foto.InputStream);
                    FileInfo fotoInfo = new FileInfo(Foto.FileName);

                    string newfoto = Guid.NewGuid().ToString() + fotoInfo.Extension;
                    img.Resize(800, 350);
                    img.Save("~/Upload/MakaleFoto/" + newfoto);
                    makales.Foto = "/Upload/MakaleFoto/" + newfoto;
                    makales.Baslik = makale.Baslik;
                    makales.Icerik = makale.Icerik;
                    makales.KategoriId = makale.KategoriId;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View();
            }
            catch
            {
                ViewBag.KategoriId = new SelectList(db.Kategoris, "KategoriID", "KategoriAd", makale.KategoriId);
                return View(makale);
            }
        }

        // GET: AdminMakale/Delete/5
        public ActionResult Delete(int id)
        {
            var makales = db.Makales.Where(x => x.MakaleId == id).SingleOrDefault();

            if (makales == null)
            {
                return HttpNotFound();
            }

            return View(makales);
        }

        // POST: AdminMakale/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                var makales = db.Makales.Where(x => x.MakaleId == id).SingleOrDefault();
                if (makales == null)
                {
                    return HttpNotFound();
                }

                if (System.IO.File.Exists(Server.MapPath(makales.Foto)))
                {
                    System.IO.File.Delete(Server.MapPath(makales.Foto));

                }

                foreach (var yorum in makales.Yorums.ToList())
                {
                    db.Yorums.Remove(yorum);
                }

                foreach (var etiket in makales.Etikets.ToList())
                {
                    db.Etikets.Remove(etiket);
                }

                db.Makales.Remove(makales);
                db.SaveChanges();



                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
