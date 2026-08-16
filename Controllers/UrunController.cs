using System.IO.Compression;
using System.Runtime.CompilerServices;
using dotnet_store.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Controllers;

public class UrunController : Controller
{
    // Dependecy Injection => DI
    private readonly DataContext _context;
    public UrunController(DataContext context)
    {
        _context = context;
    }

    public ActionResult Index()
    {
        var urunler = _context.Urunler.Select(i => new UrunGetModel
        {
            Id = i.Id,
            UrunAdi = i.UrunAdi,
            Fiyat = i.Fiyat,
            Aktif = i.Aktif,
            Anasayfa = i.Anasayfa,
            KategoriAdi = i.Kategori.KategoriAdi,
            Resim = i.Resim
        }).ToList();
        return View(urunler);
    }
    // http://localhost:5162/urunler/telefon?q=apple
    //route params: url => value
    //query string: q   => value
    public ActionResult List(string url, string q)
    {
        var query = _context.Urunler.Where(i => i.Aktif); //Queryable

        if (!string.IsNullOrEmpty(url))
        {
            //filtreleme
            query = query.Where(i => i.Kategori.Url == url);
        }
        ;

        if (!string.IsNullOrEmpty(q))
        {
            //filtreleme
            query = query.Where(i => i.UrunAdi.ToLower().Contains(q.ToLower()));

            ViewData["q"] = q;
        }

        return View(query.ToList());
    }

    public ActionResult Details(int id)
    {
        var urun = _context.Urunler.FirstOrDefault(i => i.Id == id);
        // var urun = _context.Urunler.Find(id); 
        if (urun == null)
        {
            return RedirectToAction("Index", "Home");
        }
        ViewData["BenzerUrunler"] = _context.Urunler
                                            .Where(i => i.Aktif && i.KategoriId == urun.KategoriId && i.Id != urun.Id)
                                            .Take(4)
                                            .ToList();
        return View(urun);
    }

    public ActionResult Create()
    {
        // ViewBag.Kategoriler = _context.Kategoriler.ToList();
        ViewBag.Kategoriler = new SelectList(_context.Kategoriler.ToList(), "Id", "KategoriAdi");

        return View();
    }

    [HttpPost]
    public async Task<ActionResult> Create(UrunCreateModel model)
    {
        if(model.Resim == null || model.Resim.Length == 0)
        {
            ModelState.AddModelError("Resim", "Resim Seçmelisiniz");     
        }
        if (ModelState.IsValid)
        {

            var fileName = Path.GetRandomFileName() + ".jpg";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

            using(var stream = new FileStream(path, FileMode.Create))
            {
                await model.Resim!.CopyToAsync(stream);
            }

            var entity = new Urun
            {
                UrunAdi = model.UrunAdi,
                Aciklama = model.Aciklama,
                Fiyat = model.Fiyat ?? 0,
                Aktif = model.Aktif,
                Anasayfa = model.Anasayfa,
                KategoriId = (int)model.KategoriId!,
                Resim = fileName
            };
            _context.Urunler.Add(entity);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        return View(model);
    }

    public ActionResult Edit(int id)
    {
        var entity = _context.Urunler.Select(i => new UrunEditModel
        {
            Id = i.Id,
            UrunAdi = i.UrunAdi,
            Aciklama = i.Aciklama,
            Aktif = i.Aktif,
            Anasayfa = i.Anasayfa,
            Fiyat = i.Fiyat,
            KategoriId = i.KategoriId,
            ResimAdi = i.Resim
        }).FirstOrDefault(i => i.Id == id);

        ViewBag.Kategoriler = new SelectList(_context.Kategoriler.ToList(), "Id", "KategoriAdi");
        return View(entity);
    }


    [HttpPost]
    public async Task<ActionResult> Edit(int id, UrunEditModel model)
    {
        if(id != model.Id)
        {
            return RedirectToAction("Index");
        }

        var entity = _context.Urunler.FirstOrDefault(i => i.Id == model.Id);
        if(entity != null)
        {
            if(model.ResimDosyasi != null)
            {
                var fileName = Path.GetRandomFileName() + ".jpg";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                using(var stream = new FileStream(path, FileMode.Create))
                {
                    await model.ResimDosyasi!.CopyToAsync(stream);
                }   

                entity.Resim =fileName;
            }
            entity.UrunAdi =  model.UrunAdi;
            entity.Aciklama = model.Aciklama;
            entity.Fiyat = model.Fiyat;
            entity.Aktif = model.Aktif;
            entity.Anasayfa = model.Anasayfa;
            entity.KategoriId = model.KategoriId;

            _context.SaveChanges();

            TempData["Mesaj"] = $"{entity.UrunAdi} ürünü güncellendi";
            

            return RedirectToAction("Index");
        }
        return View(model);
    }
}